using System;
using Dawnbreaker_DKP.Repository;

namespace Dawnbreaker_DKP.Data.DKP
{
    public class RaidSession : DataItem
    {
        public Guid RaidSessionId { get; set; }
        public string RaidName { get; set; }
        public string RaidRoster { get; set; }
        public DateTime Date { get; set; }
        public string Notes { get; set; }
        public string BossesKilled { get; set; }
        public int DKPAward { get; set; }
        public bool Open { get; set; }
    }
}
