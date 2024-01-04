using Dawnbreaker_DKP.Data.DKP.Enum;

namespace Dawnbreaker_DKP.Web.Models.DKP
{
    public class DKPTableRow
    {
        public string PlayerName { get; set; }
        public Class Class { get; set; }
        public string RaidRoster { get; set; }
        public int DKPCurrent;
    }
}
