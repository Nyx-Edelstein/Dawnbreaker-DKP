using Dawnbreaker_DKP.Models.Account;

namespace Dawnbreaker_DKP.Utilities.UserData.Interfaces
{
    public interface IAccountActionExecutor
    {
        bool TryExecute(IAccountAction accountAction);
        bool TryAuthenticate(AccountAuthenticate authenticationAction);
        void Logout(string currentUser);
    }
}
