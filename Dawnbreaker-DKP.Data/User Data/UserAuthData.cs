using System;
using Dawnbreaker_DKP.Repository;

namespace Dawnbreaker_DKP.Data.User_Data
{
    public class UserAuthData : DataItem
    {
        public string Username { get; set; }
        public string AuthTicket { get; set; }
        public DateTime AuthTicketExpiry { get; set; }
    }
}
