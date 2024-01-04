using System;
using System.Collections.Generic;
using Dawnbreaker_DKP.Data.DKP;
using Dawnbreaker_DKP.Web.Models.DKP;

namespace Dawnbreaker_DKP.Web.Utilities.DKP.Interfaces
{
    public interface IRaidSessionsUtil
    {
        List<RaidSession> GetSessions();
        RaidSession GetRaidSessionById(Guid raidSessionId);
        bool TryAdd(RaidSessionComposite raidSessionData);
        List<SessionParticipantComposite> GetRaidParticipants(Guid raidSessionId);
        void ChangeParticipantStatuses(IEnumerable<SessionParticipant> allParticipants);
        void RemoveParticipants(List<SessionParticipant> toRemove);
        List<DKPLedgerEntry> GetSessionLedger(Guid raidSessionId);
        void AddDKPEntry(DKPLedgerEntry entry);
        bool RemoveDKPEntry(Guid entryId);
        void CloseSession(RaidSession session);
    }
}
