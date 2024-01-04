using System.Collections.Generic;
using Dawnbreaker_DKP.Data.User_Data;

namespace Dawnbreaker_DKP.Utilities.UserData.Interfaces
{
    public interface IUserPermissionLookup
    {
        bool UpdatePermissionFor(string userName, PermissionsLevel permissionsLevel);
        List<UserPermissions> GetUserPermissions();
        PermissionsLevel GetPermissionsForUser(string userName);
    }
}
