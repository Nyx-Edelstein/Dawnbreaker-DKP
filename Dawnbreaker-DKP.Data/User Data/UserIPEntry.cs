using Dawnbreaker_DKP.Repository;

namespace Dawnbreaker_DKP.Data.User_Data
{
    public class UserIPEntry : DataItem
    {
        public string UserName { get; set; }
        public string UserIP { get; set; }
        public string LastSeen { get; set; }
    }
}
