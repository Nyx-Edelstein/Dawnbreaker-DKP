using System.ComponentModel.DataAnnotations;
using Dawnbreaker_DKP.Data;
using Dawnbreaker_DKP.Data.User_Data;
using Dawnbreaker_DKP.Utilities;
using Dawnbreaker_DKP.Utilities.UserData;

namespace Dawnbreaker_DKP.Models.Account
{
    public class AccountChangeEmail : IAccountAction
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Required] [EmailAddress]
        public string NewEmail { get; set; }

        public bool CanExecute(UserLoginData existingLoginData)
        {
            return existingLoginData != null && BCrypt.CheckPassword(Password, existingLoginData.SaltedHash);
        }

        public UserLoginData Update(UserLoginData existingLoginData)
        {
            existingLoginData.RecoveryEmail = NewEmail;
            return existingLoginData;
        }
    }
}
