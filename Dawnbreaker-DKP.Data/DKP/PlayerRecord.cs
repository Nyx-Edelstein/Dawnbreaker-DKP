using Dawnbreaker_DKP.Data.DKP.Enum;
using Dawnbreaker_DKP.Repository;

namespace Dawnbreaker_DKP.Data.DKP
{
    public class PlayerRecord : DataItem
    {
        public string PlayerName { get; set; }
        public Class Class { get; set; }
        public string Spec { get; set; }
        public string RaidRoster { get; set; }
        public int DKPCurrent { get; set; }
    }
}
