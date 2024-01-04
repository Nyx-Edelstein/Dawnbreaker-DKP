using Dawnbreaker_DKP.Models.Account;

namespace Dawnbreaker_DKP.Utilities.UserData.Interfaces
{
    public interface IAccountRecovery
    {
        void TryInitiateRecovery(AccountPasswordRecovery model);
        bool TryRecover(AccountRecover model);
    }
}
