using System;
using System.Collections.Generic;
using System.Linq;
using Dawnbreaker_DKP.Data.DKP;
using Dawnbreaker_DKP.Data.DKP.Enum;
using Dawnbreaker_DKP.Repository;
using Dawnbreaker_DKP.Web.Models.DKP;
using Dawnbreaker_DKP.Web.Utilities.DKP.Interfaces;

namespace Dawnbreaker_DKP.Web.Utilities.DKP
{
    public class PlayerManagementUtil : IPlayerManagementUtil
    {
        private IRepository<PlayerRecord> PlayerRecordRepository { get; set; }
        private IRepository<DKPLedgerEntry> DKPLedgerRepository { get; set; }
        private IRepository<RaidSession> RaidSessionRepository { get; set; }
        private IRepository<SessionParticipant> SessionParticipantRepository { get; set; }

        public PlayerManagementUtil
        (
            IRepository<PlayerRecord> playerRecordRepository,
            IRepository<DKPLedgerEntry> dkpLedgerRepository,
            IRepository<RaidSession> raidSessionRepository,
            IRepository<SessionParticipant> sessionParticipantRepository)
        {
            PlayerRecordRepository = playerRecordRepository;
            DKPLedgerRepository = dkpLedgerRepository;
            RaidSessionRepository = raidSessionRepository;
            SessionParticipantRepository = sessionParticipantRepository;
        }

        public List<string> GetAllPlayerNames()
        {
            var players = PlayerRecordRepository.GetWhere(x => true)
                .OrderBy(x => x.PlayerName)
                .Select(x => x.PlayerName)
                .ToList();

            return players;
        }

        public PlayerDetails GetPlayerDetails(string playerName)
        {
            var playerRecord = PlayerRecordRepository.GetWhere(x => x.PlayerName == playerName)
                .FirstOrDefault();

            var playerDKP = playerRecord?.DKPCurrent ?? 0;
            var playerClass = playerRecord?.Class ?? Class.Unknown;
            var playerSpec = playerRecord?.Spec ?? string.Empty;
            var playerRaidRoster = playerRecord?.RaidRoster ?? string.Empty;

            var playerLedgerEntries = DKPLedgerRepository.GetWhere(x => x.PlayerName == playerName)
                .OrderByDescending(x => x.EntryTime)
                .Select(GetLedgerEntryDetails)
                .ToList();

            return new PlayerDetails
            {
                PlayerName = playerName,
                Class = playerClass,
                Spec = playerSpec,
                RaidRoster = playerRaidRoster,
                DKPCurrent = playerDKP,
                PlayerLedger = playerLedgerEntries
            };
        }

        public bool UpdateDetails(PlayerDetailsUpdateModel data)
        {
            var playerRecord = PlayerRecordRepository.GetWhere(x => x.PlayerName == data.PlayerName).FirstOrDefault();
            if (playerRecord == null) return false;

            Enum.TryParse<Class>(data.Class, true, out var playerClass);
            playerRecord.Class = playerClass;

            playerRecord.Spec = data.Spec;
            playerRecord.RaidRoster = data.RaidRoster;

            return PlayerRecordRepository.Upsert(playerRecord);
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


        public bool UpdateDKP(PlayerDKPUpdateModel data)
        {
            var existingRecord = PlayerRecordRepository.GetWhere(x => x.PlayerName == data.PlayerName)
                .FirstOrDefault();

            var previousTotal = 0;
            if (existingRecord == null)
            {
                PlayerRecordRepository.Upsert(new PlayerRecord
                {
                    PlayerName = data.PlayerName,
                    DKPCurrent = data.NewDKPTotal
                });
            }
            else
            {
                previousTotal = existingRecord.DKPCurrent;
                existingRecord.DKPCurrent = data.NewDKPTotal;
                PlayerRecordRepository.Upsert(existingRecord);
            }

            DKPLedgerRepository.Upsert(new DKPLedgerEntry
            {
                EntryId = Guid.NewGuid(),
                RaidSessionId = Guid.Empty,
                PlayerName = data.PlayerName,
                DKPAmount = data.NewDKPTotal - previousTotal,
                ItemId = -1,
                EntryText = data.DKPAdjustmentReason,
                EntryTime = DateTime.Now.Ticks
            });
            return true;
        }

        public bool DeletePlayer(string playerName)
        {
            var playerRecord = PlayerRecordRepository.GetWhere(x => x.PlayerName == playerName);
            if (playerRecord == null) return false;

            PlayerRecordRepository.RemoveWhere(x => x.PlayerName == playerName);
            SessionParticipantRepository.RemoveWhere(x => x.PlayerName == playerName);
            DKPLedgerRepository.RemoveWhere(x => x.PlayerName == playerName);
            return true;
        }

        public List<InactivePlayer> GetInactivePlayers()
        {
            var allPlayers = PlayerRecordRepository.GetWhere(x => x.DKPCurrent > 0);

            var inactivePlayers = new List<InactivePlayer>();
            foreach (var player in allPlayers)
            {
                var lastPositiveGain = DKPLedgerRepository.GetWhere(x => x.PlayerName == player.PlayerName && x.DKPAmount > 0)
                    .OrderByDescending(x => x.EntryTime)
                    .FirstOrDefault();

                if (lastPositiveGain == null) continue;
                var lastGainTime = new DateTime(lastPositiveGain.EntryTime);
                var daysElapsed = (DateTime.Now - lastGainTime).Days;

                if (daysElapsed < 14) continue;

                var lastEntry = DKPLedgerRepository.GetWhere(x => x.PlayerName == player.PlayerName)
                    .OrderByDescending(x => x.EntryTime)
                    .First();

                inactivePlayers.Add(new InactivePlayer
                {
                    PlayerName = player.PlayerName,
                    DKP = player.DKPCurrent,
                    LastActive = new DateTime(lastPositiveGain.EntryTime),
                    LastDKPChange = new DateTime(lastEntry.EntryTime),
                });
            }

            return inactivePlayers.OrderBy(x => x.LastDKPChange).ToList();
        }
    }
}
