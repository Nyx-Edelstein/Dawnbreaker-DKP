using Dawnbreaker_DKP.Repository;

namespace Dawnbreaker_DKP.Data.Utility
{
    public class KeystoreEntry : DataItem
    {
        public string KeyName { get; set; }
        public string TokenValue { get; set; }
    }

    public static class KeyNames
    {
        public const string ADMIN_EMAIL_ADDRESS = @"ADMIN_EMAIL_ADDRESS";
        public const string ADMIN_EMAIL_ACCESS_TOKEN = @"ADMIN_EMAIL_ACCESS_TOKEN";
    }
}
