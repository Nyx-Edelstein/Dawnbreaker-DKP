using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Dawnbreaker_DKP.Utilities.UserData.Interfaces
{
    public interface IUserIdentityAuthenticator
    {
        Task GetUserIdentity(HttpContext httpContext, Func<Task> next);
    }
}
