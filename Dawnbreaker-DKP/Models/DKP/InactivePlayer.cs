using System;

namespace Dawnbreaker_DKP.Web.Models.DKP
{
    public class InactivePlayer
    {
        public string PlayerName { get; set; }
        public int DKP { get; set; }
        public DateTime LastActive { get; set; }
        public DateTime LastDKPChange { get; set; }
    }
}
