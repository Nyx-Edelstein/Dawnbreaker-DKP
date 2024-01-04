using System.ComponentModel.DataAnnotations;
using Dawnbreaker_DKP.Data.User_Data;
using Dawnbreaker_DKP.Utilities.UserData;
using Microsoft.AspNetCore.Mvc;

namespace Dawnbreaker_DKP.Models.Account
{
    public class AccountRegistrationRaw
    {
        [Required] [RegularExpression("^[a-zA-Z 0-9_\\'\\-]*$", ErrorMessage = "Username can contain only alphanumeric characters, spaces, dashes, underscores, and apostrophes.")]
        public string Username { get; set; }

        [Required] [Remote("ValidateStrongPassword", "Account")]
        public string Password { get; set; }

        public string ConfirmHuman { get; set; }

        [Required] [EmailAddress]
        public string RecoveryEmail { get; set; }
    }

    public class AccountRegistration : IAccountAction
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [Remote("ValidateStrongPassword", "Account")]
        public string Password { get; set; }

        [Required]
        [EmailAddress]
        public string RecoveryEmail { get; set; }

        [Required]
        public bool Subscribed { get; set; }

        public static AccountRegistration FromRaw(AccountRegistrationRaw data)
        {
            return new AccountRegistration
            {
                Username = data.Username,
                Password = data.Password,
                RecoveryEmail = data.RecoveryEmail,
            };
        }

        public bool CanExecute(UserLoginData existingLoginData)
        {
            return Username != "Guest" && existingLoginData == null;
        }

        public UserLoginData Update(UserLoginData existingLoginData)
        {
            return new UserLoginData
            {
                UserName = Username,
                RecoveryEmail = RecoveryEmail,
                SaltedHash = BCrypt.HashPassword(Password, BCrypt.GenerateSalt())
            };
        }
    }
}
