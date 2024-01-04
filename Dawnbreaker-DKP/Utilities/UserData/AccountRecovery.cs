using System;
using System.Linq;
using Dawnbreaker_DKP.Data.User_Data;
using Dawnbreaker_DKP.Repository;
using Dawnbreaker_DKP.Utility;
using Dawnbreaker_DKP.Models.Account;
using Dawnbreaker_DKP.Utilities.UserData.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Dawnbreaker_DKP.Utilities.UserData
{
    public class AccountRecovery : IAccountRecovery
    {
        private IHttpContextAccessor HttpContextAccessor { get; }
        private IRepository<UserLoginData> UserLoginDataRepository { get; }
        private IRepository<UserAuthData> UserAuthDataRepository { get; }
        private IRepository<UserRecoveryToken> UserRecoveryTokenRepository { get; }
        private IEmailProvider EmailProvider { get; }

        private HttpContext HttpContext => HttpContextAccessor?.HttpContext;

        public AccountRecovery(IHttpContextAccessor httpContextAccessor,
            IRepository<UserLoginData> userLoginDataRepository,
            IRepository<UserAuthData> userAuthDataRepository,
            IRepository<UserRecoveryToken> userRecoveryTokenRepository,
            IEmailProvider emailProvider)
        {
            HttpContextAccessor = httpContextAccessor;
            UserLoginDataRepository = userLoginDataRepository;
            UserAuthDataRepository = userAuthDataRepository;
            UserRecoveryTokenRepository = userRecoveryTokenRepository;
            EmailProvider = emailProvider;
        }

        private static bool Matches(string a, string b)
        {
            return string.Equals(a, b, StringComparison.InvariantCultureIgnoreCase);
        }

        public void TryInitiateRecovery(AccountPasswordRecovery model)
        {
            //Check against existing login data
            var existingLoginData = UserLoginDataRepository.GetWhere(x => Matches(x.UserName, model.Username)).FirstOrDefault();
            if (!model.IsValid(existingLoginData)) return;

            //If valid, expire logins, generate a secret, and send an email containing the secret
            Logout(model.Username);

            var secret = RNG.RandomIdentifier(32);
            var token = new UserRecoveryToken
            { 
                Username = model.Username,
                HashedSecret = BCrypt.HashPassword(secret, BCrypt.GenerateSalt()),
                Expiry = DateTime.UtcNow + TimeSpan.FromHours(1)
            };
            UserRecoveryTokenRepository.Upsert(token);

            EmailProvider.SendRecoveryEmail(model.Username, model.RecoveryEmail, secret);
        }

        public bool TryRecover(AccountRecover model)
        {
            //Check against existing recovery token
            var existingRecoveryToken = UserRecoveryTokenRepository.GetWhere(x => Matches(x.Username, model.Username) && x.Expiry >= DateTime.UtcNow).FirstOrDefault();
            if (existingRecoveryToken == null) return false;

            //Check that the provided recovery token is valid
            var isValid = BCrypt.CheckPassword(model.RecoveryToken, existingRecoveryToken.HashedSecret);
            if (!isValid) return false;

            //If valid, update password
            var loginInfo = UserLoginDataRepository.GetWhere(x => Matches(x.UserName, model.Username)).FirstOrDefault();
            if (loginInfo == null) return false;

            loginInfo.SaltedHash = BCrypt.HashPassword(model.Password, BCrypt.GenerateSalt());
            return UserLoginDataRepository.Upsert(loginInfo);
        }

        public const string USER_IDENTITY_KEY = "USER_IDENTITY";
        private void Logout(string currentUser)
        {
            UserAuthDataRepository.RemoveWhere(x => Matches(x.Username, currentUser));
            UserRecoveryTokenRepository.RemoveWhere(x => Matches(x.Username, currentUser));
            HttpContext?.Response.Cookies.Delete(USER_IDENTITY_KEY);
        }
    }
}
