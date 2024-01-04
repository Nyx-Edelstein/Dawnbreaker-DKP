using System;
using Dawnbreaker_DKP.Data;
using Dawnbreaker_DKP.Data.User_Data;
using Dawnbreaker_DKP.Utilities.UserData;

namespace Dawnbreaker_DKP.Models.Account
{
    public class AccountAuthenticate
    {
        public string Username { get; set; }
        public string AuthTicket { get; set; }

        public bool IsValid(UserAuthData authData)
        {
            return authData != null
                && authData.AuthTicketExpiry >= DateTime.UtcNow
                && authData.Username == Username
                && BCrypt.CheckPassword(AuthTicket, authData.AuthTicket);
        }
    }
}
