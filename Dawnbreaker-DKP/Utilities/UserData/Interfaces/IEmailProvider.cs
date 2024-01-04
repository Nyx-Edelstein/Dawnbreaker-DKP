namespace Dawnbreaker_DKP.Utilities.UserData.Interfaces
{
    public interface IEmailProvider
    {
        void SendRecoveryEmail(string userName, string recoveryEmail, string recoveryTicket);
    }
}
