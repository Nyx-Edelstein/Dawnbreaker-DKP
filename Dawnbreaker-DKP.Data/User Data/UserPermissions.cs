using Dawnbreaker_DKP.Repository;

namespace Dawnbreaker_DKP.Data.User_Data
{
    public class UserPermissions : DataItem
    {
        public string UserName { get; set; }
        public PermissionsLevel PermissionsLevel { get; set; }
    }

    public enum PermissionsLevel
    {
        Default = 0,
        Officer = 1,
        Admin = 2
    }
}
