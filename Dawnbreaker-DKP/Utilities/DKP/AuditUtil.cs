using System;
using System.Collections.Generic;
using System.Linq;
using Dawnbreaker_DKP.Data.DKP;
using Dawnbreaker_DKP.Data.DKP.Constants;
using Dawnbreaker_DKP.Repository;
using Dawnbreaker_DKP.Web.Models.DKP;
using Dawnbreaker_DKP.Web.Utilities.DKP.Interfaces;

namespace Dawnbreaker_DKP.Web.Utilities.DKP
{
    public class AuditUtil : IAuditUtil
    {
        public IRepository<PlayerRecord> PlayerRecordRepository { get; }
        public IRepository<DKPLedgerEntry> LedgerRepository { get; }
        public IRepository<RaidSession> RaidSessionRepository { get; }
        public IRepository<SessionParticipant> SessionParticipantRepository { get; }

        public AuditUtil
        (
            IRepository<PlayerRecord> playerRecordRepository,
            IRepository<DKPLedgerEntry> ledgerRepository,
            IRepository<RaidSession> raidSessionRepository,
            IRepository<SessionParticipant> sessionParticipantRepository)
        {
            PlayerRecordRepository = playerRecordRepository;
            LedgerRepository = ledgerRepository;
            RaidSessionRepository = raidSessionRepository;
            SessionParticipantRepository = sessionParticipantRepository;
        }

        public List<DKPTableRow> GetDKPTableData()
        {
            var playerRecords = PlayerRecordRepository.GetWhere(x => true)
                .ToList();

            var rows = playerRecords.Select(x => new DKPTableRow
            {
                PlayerName = x.PlayerName,
                Class = x.Class,
                RaidRoster = x.RaidRoster,
                DKPCurrent = x.DKPCurrent
            }).OrderBy(x => x.PlayerName).ToList();

            return rows;
        }

        public List<DKPViewModel> GetDKPLedgerData()
        {
            var ledgerEntries = LedgerRepository.GetWhere(x => true)
                .OrderByDescending(x => x.EntryTime)
                .Select(GetLedgerEntryDetails)
                .ToList();

            return ledgerEntries;
        }

        public void ResetToCap()
        {
            var playersOverCap = PlayerRecordRepository.GetWhere(x => x.DKPCurrent > DKPConstants.DKP_CAP);
            foreach (var player in playersOverCap)
            {
                var difference = DKPConstants.DKP_CAP - player.DKPCurrent;
                player.DKPCurrent = DKPConstants.DKP_CAP;
                PlayerRecordRepository.Upsert(player);
                LedgerRepository.Upsert(new DKPLedgerEntry
                {
                    EntryId = Guid.NewGuid(),
                    RaidSessionId = Guid.Empty,
                    PlayerName = player.PlayerName,
                    ItemId = -1,
                    EntryText = "Reset to Cap",
                    DKPAmount = difference,
                    EntryTime = DateTime.Now.Ticks
                });
            }
        }

        public void SquishDKP()
        {
            var players = PlayerRecordRepository.GetWhere(x => x.DKPCurrent > 0);
            foreach (var record in players)
            {
                var newAmount = (int)Math.Ceiling(record.DKPCurrent / 2.0);
                var difference =  newAmount - record.DKPCurrent;
                record.DKPCurrent = newAmount;
                PlayerRecordRepository.Upsert(record);
                LedgerRepository.Upsert(new DKPLedgerEntry
                {
                    EntryId = Guid.NewGuid(),
                    RaidSessionId = Guid.Empty,
                    PlayerName = record.PlayerName,
                    ItemId = -1,
                    EntryText = "DKP Squish",
                    DKPAmount = difference,
                    EntryTime = DateTime.Now.Ticks
                });
            }
        }

        private DKPViewModel GetLedgerEntryDetails(DKPLedgerEntry entry)
        {
            var details = new DKPViewModel
            {
                PlayerName = entry.PlayerName,
                ItemId = entry.ItemId,
                EntryText = entry.EntryText,
                DKPAmount = entry.DKPAmount,
                EntryTime = entry.EntryTime
            };

            var raidSession = RaidSessionRepository.GetWhere(x => x.RaidSessionId == entry.RaidSessionId)
                .FirstOrDefault();

            if (entry.RaidSessionId == Guid.Empty || raidSession == null)
            {
                details.Source = "Manual Adjustment";
            }
            else
            {
                details.Source = $"{raidSession.RaidName} - {raidSession.Date:MMM dd, yy}";
            }

            return details;
        }
    }
}
