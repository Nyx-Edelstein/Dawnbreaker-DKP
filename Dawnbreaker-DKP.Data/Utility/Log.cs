using Dawnbreaker_DKP.Repository;

namespace Dawnbreaker_DKP.Data.Utility
{
    public class SystemLog : DataItem
    {
        public string Username { get; set; }
        public string IP { get; set; }
        public string Details { get; set; }
        public string Data { get; set; }
    }
}
