using Dawnbreaker_DKP.Data;
using Dawnbreaker_DKP.Data.User_Data;

namespace Dawnbreaker_DKP.Models.Account
{
    public interface IAccountAction
    {
        string Username { get; }
        bool CanExecute(UserLoginData existingLoginData);
        UserLoginData Update(UserLoginData existingLoginData);
    }
}
