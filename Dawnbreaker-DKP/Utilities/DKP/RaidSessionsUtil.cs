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
    public class RaidSessionsUtil : IRaidSessionsUtil
    {
        public IRepository<RaidSession> RaidSessionRepository { get; }
        public IRepository<SessionParticipant> SessionParticipantRepository { get; }
        public IRepository<PlayerRecord> PlayerRecordRepository { get; }
        public IRepository<DKPLedgerEntry> LedgerRepository { get; }

        public RaidSessionsUtil
        (
            IRepository<RaidSession> raidSessionRepository,
            IRepository<SessionParticipant> sessionParticipantRepository,
            IRepository<PlayerRecord> playerRecordRepository,
            IRepository<DKPLedgerEntry> ledgerRepository)
        {
            RaidSessionRepository = raidSessionRepository;
            SessionParticipantRepository = sessionParticipantRepository;
            PlayerRecordRepository = playerRecordRepository;
            LedgerRepository = ledgerRepository;
        }

        public List<RaidSession> GetSessions()
        {
            return RaidSessionRepository.GetWhere(x => true);
        }

        public RaidSession GetRaidSessionById(Guid raidSessionId)
        {
            return RaidSessionRepository.GetWhere(x => x.RaidSessionId == raidSessionId).FirstOrDefault();
        }

        public List<SessionParticipantComposite> GetRaidParticipants(Guid raidSessionId)
        {
            var sessionParticipants = SessionParticipantRepository.GetWhere(x => x.RaidSessionId == raidSessionId)
                .Select(p => new SessionParticipantComposite
                {
                    Participant = p,
                    DKP = PlayerRecordRepository.GetWhere(x => x.PlayerName == p.PlayerName).FirstOrDefault()?.DKPCurrent ?? 0
                }).ToList();

            return sessionParticipants;
        }

        public bool TryAdd(RaidSessionComposite data)
        {
            var existing = RaidSessionRepository.GetWhere(x => x.RaidSessionId == data.RaidSession.RaidSessionId);
            if (existing.Any()) return false;

            RaidSessionRepository.Upsert(data.RaidSession);

            foreach (var participant in data.Participants)
                UpsertSessionParticipant(participant);

            return true;
        }

        private void UpsertSessionParticipant(SessionParticipant participant)
        {
            var existingEntry = SessionParticipantRepository
                .GetWhere(x => x.RaidSessionId == participant.RaidSessionId && x.PlayerName == participant.PlayerName)
                .FirstOrDefault();

            if (existingEntry == null)
            {
                EnsurePlayerDKP(participant.RaidSessionId, participant.PlayerName);
                SessionParticipantRepository.Upsert(participant);
            }
            else
            {
                existingEntry.Standby = participant.Standby;
                SessionParticipantRepository.Upsert(existingEntry);
            }    
        }

        private void EnsurePlayerDKP(Guid raidSessionId, string playerName)
        {
            var existingPlayer = PlayerRecordRepository.GetWhere(x => x.PlayerName == playerName).FirstOrDefault();
            if (existingPlayer != null) return;

            PlayerRecordRepository.Upsert(new PlayerRecord
            {
                PlayerName = playerName,
                DKPCurrent = DKPConstants.DKP_INITIAL
            });
            LedgerRepository.Upsert(new DKPLedgerEntry
            {
                EntryId = Guid.NewGuid(),
                RaidSessionId = raidSessionId,
                PlayerName = playerName,
                DKPAmount = DKPConstants.DKP_INITIAL,
                ItemId = -1,
                EntryText = "New Raider Initial DKP",
                EntryTime = DateTime.Now.Ticks
            });
        }

        public void ChangeParticipantStatuses(IEnumerable<SessionParticipant> participants)
        {
            foreach (var participant in participants)
            {
                UpsertSessionParticipant(participant);
            }
        }

        public void RemoveParticipants(List<SessionParticipant> toRemove)
        {
            foreach (var participant in toRemove)
            {
                SessionParticipantRepository.RemoveWhere(x => x.RaidSessionId == participant.RaidSessionId && x.PlayerName == participant.PlayerName);
            }
        }

        public List<DKPLedgerEntry> GetSessionLedger(Guid raidSessionId)
        {
            return LedgerRepository.GetWhere(x => x.RaidSessionId == raidSessionId)
                .OrderByDescending(x => x.EntryTime)
                .ToList();
        }

        public void AddDKPEntry(DKPLedgerEntry entry)
        {
            var existingEntry = LedgerRepository.GetWhere(x => x.EntryId == entry.EntryId).FirstOrDefault();
            if (existingEntry != null) return;

            EnsurePlayerDKP(entry.RaidSessionId, entry.PlayerName);

            var playerRecord = PlayerRecordRepository.GetWhere(x => x.PlayerName == entry.PlayerName).First();
            playerRecord.DKPCurrent += entry.DKPAmount;
            PlayerRecordRepository.Upsert(playerRecord);
            LedgerRepository.Upsert(entry);
        }

        public bool RemoveDKPEntry(Guid entryId)
        {
            var existingEntry = LedgerRepository.GetWhere(x => x.EntryId == entryId).FirstOrDefault();
            if (existingEntry == null) return false;

            LedgerRepository.RemoveWhere(x => x.EntryId == entryId);

            var playerRecord = PlayerRecordRepository.GetWhere(x => x.PlayerName == existingEntry.PlayerName).FirstOrDefault();
            if (playerRecord == null) return true;

            playerRecord.DKPCurrent -= existingEntry.DKPAmount;
            PlayerRecordRepository.Upsert(playerRecord);

            return true;
        }

        public void CloseSession(RaidSession session)
        {
            var existing = RaidSessionRepository.GetWhere(x => x.RaidSessionId == session.RaidSessionId).FirstOrDefault();
            if (existing == null) return;

            existing.Notes = session.Notes;
            existing.BossesKilled = session.BossesKilled;
            existing.DKPAward = session.DKPAward;
            existing.Open = false;
            RaidSessionRepository.Upsert(existing);

            if (session.DKPAward <= 0) return;

            var sessionParticipants = SessionParticipantRepository.GetWhere(x => x.RaidSessionId == session.RaidSessionId);
            foreach (var participant in sessionParticipants)
            {
                var playerRecord = PlayerRecordRepository.GetWhere(x => x.PlayerName == participant.PlayerName).First();

                int actualDKPEarned;
                if (playerRecord.DKPCurrent >= DKPConstants.DKP_CAP)
                {
                    continue;
                }
                if (playerRecord.DKPCurrent + session.DKPAward >= DKPConstants.DKP_CAP)
                {
                    actualDKPEarned = DKPConstants.DKP_CAP - playerRecord.DKPCurrent;
                    playerRecord.DKPCurrent = DKPConstants.DKP_CAP;
                }
                else
                {
                    actualDKPEarned = session.DKPAward;
                    playerRecord.DKPCurrent += session.DKPAward;
                }

                PlayerRecordRepository.Upsert(playerRecord);

                LedgerRepository.Upsert(new DKPLedgerEntry
                {
                    EntryId = Guid.NewGuid(),
                    RaidSessionId = session.RaidSessionId,
                    PlayerName = participant.PlayerName,
                    DKPAmount = actualDKPEarned,
                    ItemId = 0,
                    EntryText = participant.Standby ? "Raid Participation (Standby)" : "Raid Participation",
                    EntryTime = DateTime.Now.Ticks
                });
            }
        }
    }
}
