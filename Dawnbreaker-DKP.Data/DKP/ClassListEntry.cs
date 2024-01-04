using Dawnbreaker_DKP.Repository;

namespace Dawnbreaker_DKP.Data.DKP
{
    public class ClassListEntry : DataItem
    {
        public string RaidRoster { get; set; }
        public string Class { get; set; }
        public string PlayerName { get; set; }
        public int Position { get; set; }
    }
}
