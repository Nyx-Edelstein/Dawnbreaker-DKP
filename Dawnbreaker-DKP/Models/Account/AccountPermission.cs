using System;
using System.ComponentModel.DataAnnotations;
using Dawnbreaker_DKP.Data;
using Dawnbreaker_DKP.Data.User_Data;

namespace Dawnbreaker_DKP.Models.Account
{
    public class AccountPermission
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Permissions { get; set; }

        public PermissionsLevel PermissionsLevel
        {
            get
            {
                var valid = Enum.TryParse(Permissions, true, out PermissionsLevel permissionsLevel);
                return valid ? permissionsLevel : PermissionsLevel.Default;
            }
        }
    }
}
