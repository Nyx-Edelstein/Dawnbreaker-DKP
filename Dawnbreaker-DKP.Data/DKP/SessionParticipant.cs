using System;
using Dawnbreaker_DKP.Repository;

namespace Dawnbreaker_DKP.Data.DKP
{
    public class SessionParticipant : DataItem
    {
        public Guid RaidSessionId { get; set; }
        public string PlayerName { get; set; }
        public bool Standby { get; set; }
    }
}
