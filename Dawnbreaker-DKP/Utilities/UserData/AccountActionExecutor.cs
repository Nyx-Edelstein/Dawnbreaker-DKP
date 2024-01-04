using System;
using System.Collections.Generic;
using System.Linq;
using Dawnbreaker_DKP.Data.User_Data;
using Dawnbreaker_DKP.Repository;
using Dawnbreaker_DKP.Models.Account;
using Dawnbreaker_DKP.Utilities.UserData.Interfaces;
using Dawnbreaker_DKP.Utility;
using Microsoft.AspNetCore.Http;

namespace Dawnbreaker_DKP.Utilities.UserData
{
    internal class AccountActionExecutor : IAccountActionExecutor
    {
        private IHttpContextAccessor HttpContextAccessor { get; }
        private IRepository<UserLoginData> UserLoginDataRepository { get; }
        private IRepository<UserAuthData> UserAuthDataRepository { get; }
        private IRepository<UserPermissions> UserPermissionsRepository { get; }

        private HttpContext HttpContext => HttpContextAccessor?.HttpContext;

        public AccountActionExecutor(IHttpContextAccessor httpContextAccessor,
            IRepository<UserLoginData> userLoginDataRepository,
            IRepository<UserAuthData> userAuthDataRepository,
            IRepository<UserPermissions> userPermissionsRepository)
        {
            HttpContextAccessor = httpContextAccessor;
            UserLoginDataRepository = userLoginDataRepository;
            UserAuthDataRepository = userAuthDataRepository;
            UserPermissionsRepository = userPermissionsRepository;
        }

        private static bool Matches(string a, string b)
        {
            return string.Equals(a, b, StringComparison.InvariantCultureIgnoreCase);
        }

        public bool TryExecute(IAccountAction accountAction)
        {
            //Get existing data
            var existingLoginData = UserLoginDataRepository.GetWhere(x => Matches(x.UserName, accountAction.Username)).FirstOrDefault();

            //Ensure the action is valid
            if (!accountAction.CanExecute(existingLoginData)) return false;

            //Save updated login data if necessary
            var updatedLoginData = accountAction.Update(existingLoginData);
            var updateSucceeded = updatedLoginData == null || UserLoginDataRepository.Upsert(updatedLoginData);
            if (!updateSucceeded) return false;

            //If this is a login, generate an auth ticket for the user
            if (accountAction is AccountLogin || accountAction is AccountRegistration)
            {
                return GenerateAuthTicket(accountAction.Username);
            }

            return true;
        }

        public bool TryAuthenticate(AccountAuthenticate authAction)
        {
            //Remove expired auth data
            UserAuthDataRepository.RemoveWhere(x => x.AuthTicketExpiry < DateTime.UtcNow);

            //Get existing data
            var existingAuthDataList = UserAuthDataRepository.GetWhere(x => Matches(x.Username, authAction.Username));

            //Find if any are valid
            var authData = existingAuthDataList.FirstOrDefault(authAction.IsValid);
            if (authData == null) return false;

            //Determine if a new auth ticket needs to be generated. If not, nothing else to do.
            if (authData.AuthTicketExpiry >= DateTime.UtcNow)
                return true;

            return GenerateAuthTicket(authAction.Username);
        }

        private bool GenerateAuthTicket(string username)
        {
            //Generate new auth ticket and persist it to cookies (unhashed) and db (hashed)
            var authTicket = RNG.RandomIdentifier(32);
            var authTicketExpiry = DateTime.UtcNow.Add(TimeSpan.FromDays(14));

            //Save unhashed auth data to cookies
            var identity = new UserIdentity
            {
                Username = username,
                AuthTicket = authTicket,
                AuthTicketExpiry = authTicketExpiry,
            };
            var cookieSaved = SetUserIdentity(identity);

            //Save hashed to DB
            var newAuthData = new UserAuthData
            {
                Username = username,
                AuthTicket = BCrypt.HashPassword(authTicket, BCrypt.GenerateSalt()),
                AuthTicketExpiry = authTicketExpiry
            };
            var authDataUpdateSucceeded = UserAuthDataRepository.Upsert(newAuthData);

            //Return true only if everything succeeded
            return authDataUpdateSucceeded && cookieSaved;
        }

        public void Logout(string currentUser)
        {
            UserAuthDataRepository.RemoveWhere(x => Matches(x.Username, currentUser));
            HttpContext?.Response.Cookies.Delete(USER_IDENTITY_KEY);
        }

        //public bool LogUserIP(UserIPEntry userIPEntry)
        //{
        //    var existingEntries = UserIPEntryRepository
        //        .GetWhere(x => Matches(x.UserName, userIPEntry.UserName) || x.UserIP == userIPEntry.UserIP)
        //        ?? new List<UserIPEntry>();

        //    if (existingEntries.Any(x => x.IsBanned)) return true;

        //    UserIPEntryRepository.RemoveWhere(x => Matches(x.UserName, userIPEntry.UserName) && x.UserIP == userIPEntry.UserIP);
        //    UserIPEntryRepository.Upsert(userIPEntry);
        //    return false;
        //}


        public const string USER_IDENTITY_KEY = "USER_IDENTITY";
        private bool SetUserIdentity(UserIdentity identity)
        {
            if (HttpContext == null) return false;

            if (HttpContext.Request.Cookies.ContainsKey(USER_IDENTITY_KEY))
                HttpContext.Response.Cookies.Delete(USER_IDENTITY_KEY);

            var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(identity);
            HttpContext.Response.Cookies.Append(USER_IDENTITY_KEY, serialized, new CookieOptions
            {
                Expires = identity.AuthTicketExpiry
            });

            return true;
        }
    }
}
