using System;
using System.ComponentModel.DataAnnotations;
using Dawnbreaker_DKP.Data.User_Data;

namespace Dawnbreaker_DKP.Models.Account
{
    public class AccountPasswordRecovery
    {
        [Required]
        public string Username { get; set; }

        [Required] [EmailAddress]
        public string RecoveryEmail { get; set; }

        public bool IsValid(UserLoginData existingLoginData)
        {
            return existingLoginData != null && string.Equals(RecoveryEmail, existingLoginData.RecoveryEmail, StringComparison.OrdinalIgnoreCase);
        }
    }
}
