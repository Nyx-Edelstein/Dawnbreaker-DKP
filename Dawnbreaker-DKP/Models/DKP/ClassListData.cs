using System.Collections.Generic;
using Dawnbreaker_DKP.Data.DKP;

namespace Dawnbreaker_DKP.Web.Models.DKP
{
    public class ClassListData
    {
        public string RaidRoster { get; set; }
        public string Class { get; set; }
        public List<ClassListEntry> Players { get; set; }
    }
}
