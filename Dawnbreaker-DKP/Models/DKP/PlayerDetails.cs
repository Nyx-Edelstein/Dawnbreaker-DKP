using System.Collections.Generic;
using Dawnbreaker_DKP.Data.DKP.Enum;

namespace Dawnbreaker_DKP.Web.Models.DKP
{
    public class PlayerDetails
    {
        public string PlayerName { get; set; }
        public Class Class { get; set; }
        public string Spec { get; set; }
        public string RaidRoster { get; set; }
        public int DKPCurrent { get; set; }
        public List<DKPViewModel> PlayerLedger { get; set; }
    }
}
