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
    public class ClassListUtil : IClassListUtil
    {
        private IRepository<PlayerRecord> PlayerRecordRepository { get; set; }
        private IRepository<ClassListEntry> ClassListRepository { get; set; }
        private IRepository<RaidSession> RaidSessionRepository { get; set; }
        private IRepository<SessionParticipant> SessionParticipantRepository { get; }

        public ClassListUtil
        (
            IRepository<PlayerRecord> playerRecordRepository,
            IRepository<ClassListEntry> classListRepository,
            IRepository<RaidSession> raidSessionRepository,
            IRepository<SessionParticipant> sessionParticipantRepository
        )
        {
            PlayerRecordRepository = playerRecordRepository;
            ClassListRepository = classListRepository;
            RaidSessionRepository = raidSessionRepository;
            SessionParticipantRepository = sessionParticipantRepository;
        }

        public List<ClassListData> GetClassListData(string raidRoster, string classList)
        {
            var allData = ClassListRepository.GetWhere(x =>
                (string.IsNullOrWhiteSpace(raidRoster) || x.RaidRoster == raidRoster) &&
                (string.IsNullOrWhiteSpace(classList) || x.Class == classList));

            var classListData = allData.GroupBy(x => new { x.RaidRoster, x.Class })
                .Select(x => new ClassListData
                {
                    RaidRoster = x.First().RaidRoster,
                    Class = x.First().Class,
                    Players = x.OrderBy(y => y.Position).ToList()
                }).OrderBy(x => x.RaidRoster)
                .ThenBy(x => x.Class)
                .ToList();

            return classListData;
        }

        public bool AddEntry(ClassListEntry data)
        {
            var playerExists = PlayerRecordRepository.GetWhere(x => x.PlayerName == data.PlayerName).Any();
            if (!playerExists)
            {
                return false;
            }

            var existingEntry = ClassListRepository.GetWhere(x => x.PlayerName == data.PlayerName).FirstOrDefault();
            if (existingEntry != null)
            {
                return true;
            }

            var position = ClassListRepository.GetWhere(x => x.RaidRoster == data.RaidRoster && x.Class == data.Class).Count + 1;
            data.Position = position;

            return ClassListRepository.Upsert(data);
        }

        public bool MovePlayerToBottom(string raidRoster, string classType, string playerName)
        {
            if (string.IsNullOrWhiteSpace(raidRoster) || string.IsNullOrWhiteSpace(classType) || string.IsNullOrWhiteSpace(playerName))
            {
                return false;
            }

            var classRaidList = ClassListRepository.GetWhere(x => x.RaidRoster == raidRoster && x.Class == classType)
                .OrderBy(x => x.PlayerName == playerName)
                .ThenBy(x => x.Position)
                .ToArray();

            for (int i = 0; i < classRaidList.Count(); i++)
            {
                classRaidList[i].Position = i+1;
            }

            foreach (var entry in classRaidList)
            {
                ClassListRepository.Upsert(entry);
            }

            return true;
        }

        public bool HandleSKEntry(Guid raidSessionId, string playerName)
        {
            if (raidSessionId == Guid.Empty || string.IsNullOrWhiteSpace(playerName)) return false;

            var raidSession = RaidSessionRepository.GetWhere(x => x.RaidSessionId == raidSessionId).FirstOrDefault();
            if (raidSession == null) return false;

            var raidRoster = raidSession.RaidRoster;
            if (string.IsNullOrWhiteSpace(raidRoster)) return false;

            var playerRecord = PlayerRecordRepository.GetWhere(x => x.PlayerName == playerName).FirstOrDefault();
            if (playerRecord == null) return false;

            var classType = playerRecord.Class;
            if (classType == Class.Unknown) return false;

            var sameClassParticipants = SessionParticipantRepository.GetWhere(x => x.RaidSessionId == raidSessionId)
                .Select(x => ClassListRepository.GetWhere(y => y.PlayerName == x.PlayerName).FirstOrDefault())
                .Where(x => x != null && x.RaidRoster == raidRoster && x.Class == classType.ToString())
                .OrderBy(x => x.Position)
                .ToList();
            if (!sameClassParticipants.Any()) return false;
            if (!sameClassParticipants.Select(x => x.PlayerName).Contains(playerName)) return false;

            MovePlayerSpecial(sameClassParticipants, playerName);
            return true;
        }

        private void MovePlayerSpecial(List<ClassListEntry> sameClassParticipants, string playerName)
        {
            var activePositions = sameClassParticipants.Select(x => x.Position).ToArray();
            var orderedParticipants = sameClassParticipants.OrderBy(x => x.PlayerName == playerName).ThenBy(x => x.Position).ToArray();

            for (int i = 0; i < sameClassParticipants.Count; i++)
            {
                orderedParticipants[i].Position = activePositions[i];
            }

            foreach (var entry in orderedParticipants)
            {
                ClassListRepository.Upsert(entry);
            }
        }
    }
}
