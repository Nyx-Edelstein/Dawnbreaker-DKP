using System;
using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Dawnbreaker_DKP.Data.User_Data;
using Dawnbreaker_DKP.Extensions;
using Dawnbreaker_DKP.Models.Account;
using Dawnbreaker_DKP.Utilities.UserData.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Dawnbreaker_DKP.Utilities.UserData
{
    public class UserIdentityAuthenticator : IUserIdentityAuthenticator
    {
        public IAccountActionExecutor AccountActionExecutor { get; }
        public IUserPermissionLookup UserPermissionLookup { get; }

        public UserIdentityAuthenticator(IAccountActionExecutor accountActionExecutor, IUserPermissionLookup userPermissionLookup)
        {
            AccountActionExecutor = accountActionExecutor;
            UserPermissionLookup = userPermissionLookup;
        }

        public Task GetUserIdentity(HttpContext httpContext, Func<Task> next)
        {
            var userName = httpContext.GetCurrentUser();

            //var userIPEntry = new UserIPEntry
            //{
            //    UserName = userName ?? "Guest",
            //    UserIP = httpContext.Connection.RemoteIpAddress.ToString(),
            //    LastSeen = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)
            //};

            //AccountActionExecutor.LogUserIP(userIPEntry);

            var authTicket = httpContext.GetUserAuthTicket();
            var authenticationAction = new AccountAuthenticate
            {
                Username = userName,
                AuthTicket = authTicket
            };

            var authenticated = !string.IsNullOrWhiteSpace(userName)
                && !string.IsNullOrWhiteSpace(authTicket)
                && AccountActionExecutor.TryAuthenticate(authenticationAction);
            if (authenticated)
            {
                var userRole = UserPermissionLookup.GetPermissionsForUser(userName);
                if (userRole == PermissionsLevel.Admin)
                {
                    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, userName),
                        new Claim(ClaimTypes.Role, PermissionsLevel.Admin.ToString()),
                        new Claim(ClaimTypes.Role, PermissionsLevel.Officer.ToString())
                    }));
                }
                else
                {
                    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, userName),
                        new Claim(ClaimTypes.Role, userRole.ToString())
                    }));
                }
            }
            else
            {
                httpContext.User = new ClaimsPrincipal(new ClaimsIdentity[0]);
            }

            return next();
        }
    }
}
