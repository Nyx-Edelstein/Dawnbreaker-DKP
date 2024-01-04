using Dawnbreaker_DKP.Repository;

namespace Dawnbreaker_DKP.Data.User_Data
{
    public class UserLoginData : DataItem
    {
        public string UserName { get; set; }
        public string RecoveryEmail { get; set; }
        public string SaltedHash { get; set; }
    }
}
