using Dawnbreaker_DKP.Repository;

namespace Dawnbreaker_DKP.Data.User_Data
{
    public class UserPoints : DataItem
    {
        public string Username { get; set; }
        public int CurrentPoints { get; set; }
        public int PendingPoints { get; set; }
        public int TotalAccruedPoints { get; set; }
    }
}
