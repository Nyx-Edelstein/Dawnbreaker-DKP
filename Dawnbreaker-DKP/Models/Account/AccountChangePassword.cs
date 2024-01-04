using System.ComponentModel.DataAnnotations;
using Dawnbreaker_DKP.Data;
using Dawnbreaker_DKP.Data.User_Data;
using Dawnbreaker_DKP.Utilities;
using Dawnbreaker_DKP.Utilities.UserData;
using Microsoft.AspNetCore.Mvc;

namespace Dawnbreaker_DKP.Models.Account
{
    public class AccountChangePassword : IAccountAction
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string OldPassword { get; set; }

        [Required] [Remote("ValidateStrongPassword", "Account")]
        public string Password { get; set; }

        public bool CanExecute(UserLoginData existingLoginData)
        {
            if (existingLoginData == null) return false;

            return BCrypt.CheckPassword(OldPassword, existingLoginData.SaltedHash);
        }

        public UserLoginData Update(UserLoginData existingLoginData)
        {
            existingLoginData.SaltedHash = BCrypt.HashPassword(Password, BCrypt.GenerateSalt());
            return existingLoginData;
        }
    }
}
