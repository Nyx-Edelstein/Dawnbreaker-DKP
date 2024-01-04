using System;
using System.Collections.Generic;
using System.Linq;
using Dawnbreaker_DKP.Data.User_Data;
using Dawnbreaker_DKP.Repository;
using Dawnbreaker_DKP.Utilities.UserData.Interfaces;

namespace Dawnbreaker_DKP.Utilities.UserData
{
    public class UserPermissionLookup : IUserPermissionLookup
    {
        private IRepository<UserLoginData> UserLoginDataRepository { get; }
        private IRepository<UserPermissions> UserPermissionsRepository { get; }

        public UserPermissionLookup(IRepository<UserLoginData> userLoginDataRepository, IRepository<UserPermissions> userPermissionsRepository)
        {
            UserLoginDataRepository = userLoginDataRepository;
            UserPermissionsRepository = userPermissionsRepository;
        }

        private static bool Matches(string a, string b)
        {
            return string.Equals(a, b, StringComparison.InvariantCultureIgnoreCase);
        }

        public List<UserPermissions> GetUserPermissions()
        {
            var availableUsers = UserLoginDataRepository.GetWhere(x => true)
                .Select(x => x.UserName)
                .OrderBy(x => x)
                .ToList();

            var existingUserPermissions = UserPermissionsRepository.GetWhere(x => true);

            var userPermissions = availableUsers.Select(userName => new UserPermissions
            {
                UserName = userName,
                PermissionsLevel = existingUserPermissions.FirstOrDefault(x => Matches(x.UserName, userName))?.PermissionsLevel ?? PermissionsLevel.Default
            }).ToList();

            return userPermissions;
        }

        public PermissionsLevel GetPermissionsForUser(string userName)
        {
            return UserPermissionsRepository.GetWhere(x => Matches(x.UserName, userName))
                .FirstOrDefault()?.PermissionsLevel ?? PermissionsLevel.Default;
        }

        public bool UpdatePermissionFor(string userName, PermissionsLevel newPermissionsLevel)
        {
            var existingPermissionsData = UserPermissionsRepository.GetWhere(x => Matches(x.UserName, userName)).FirstOrDefault();
            if (existingPermissionsData != null)
            {
                existingPermissionsData.PermissionsLevel = newPermissionsLevel;
                return UserPermissionsRepository.Upsert(existingPermissionsData);
            };

            var newPermissionsData = new UserPermissions
            {
                UserName = userName,
                PermissionsLevel = newPermissionsLevel
            };
            return UserPermissionsRepository.Upsert(newPermissionsData);
        }
    }
}
