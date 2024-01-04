using System;

namespace Dawnbreaker_DKP.Models.Account
{
    public class UserIdentity
    {
        public string Username { get; set; }
        public string AuthTicket { get; set; }
        public DateTime AuthTicketExpiry { get; set; }
    }
}
