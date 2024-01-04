using System;
using Dawnbreaker_DKP.Data.Utility;
using Dawnbreaker_DKP.Extensions;
using Dawnbreaker_DKP.Models.Account;
using Dawnbreaker_DKP.Repository;
using Dawnbreaker_DKP.Utilities.UserData;
using Dawnbreaker_DKP.Utilities.UserData.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Dawnbreaker_DKP.Controllers
{
    public class AccountController : Controller
    {
        public IAccountActionExecutor AccountActionExecutor { get; }
        public IAccountRecovery AccountRecovery { get; }

        public AccountController(IAccountActionExecutor accountActionExecutor, IAccountRecovery accountRecovery)
        {
            AccountActionExecutor = accountActionExecutor;
            AccountRecovery = accountRecovery;
        }

        public IActionResult Error() => View();
        private IActionResult RedirectToLast()
        {
            var lastPage = TempData.GetRedirect();
            if (lastPage == "/" || lastPage == "//") return RedirectToAction("Index", "Home");

            return lastPage != null && !lastPage.Contains("Account") && !lastPage.Contains("Error") ? (IActionResult)Redirect(lastPage) : RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register(string lastPage = null)
        {
            if (lastPage != null && lastPage != "/")
            {
                TempData.SetRedirect(lastPage);
            }

            if (User.Identity?.Name != null)
            {
                return RedirectToLast();
            }

            return User?.Identity?.IsAuthenticated == true ? RedirectToLast() : View();
        }

        [HttpPost]
        public IActionResult Register(AccountRegistrationRaw data)
        {
            if (!string.IsNullOrEmpty(data.ConfirmHuman))
            {
                LogActivity("Failed honeypot check on login", data);
                return RedirectToAction("TAC", "Home");
            }

            if (User.Identity?.Name != null)
            {
                return RedirectToLast();
            }

            if (!ModelState.IsValid)
            {
                TempData.AddError("Please correct validation errors.");
                return View();
            }

            var model = AccountRegistration.FromRaw(data);

            var userRegistered = AccountActionExecutor.TryExecute(model);
            if (userRegistered)
            {
                TempData.AddMessage($"Welcome to the site, {model.Username}!");
                return RedirectToLast();
            }

            TempData.AddError("That username is already taken.");
            return View();
        }

        [HttpGet]
        public IActionResult Login(string lastPage = null)
        {
            if (lastPage != null && lastPage != "/")
            {
                TempData.SetRedirect(lastPage);
            }

            return User?.Identity?.IsAuthenticated == true ? RedirectToLast() : View();
        }

        [HttpPost]
        public IActionResult Login(AccountLoginRaw data)
        {
            if (!string.IsNullOrEmpty(data.ConfirmHuman))
            {
                LogActivity("Failed honeypot check on login", data);
                return RedirectToAction("TAC", "Home");
            }

            if (!ModelState.IsValid)
            {
                TempData.AddError("Please correct validation errors.");
                return View();
            }

            var model = AccountLogin.FromRaw(data);

            var userLoggedIn = AccountActionExecutor.TryExecute(model);
            if (userLoggedIn) return RedirectToLast();

            TempData.AddError("Invalid login attempt.");
            return View();
        }

        [AcceptVerbs("Get", "Post")]
        public IActionResult Logout()
        {
            var currentUser = User?.Identity?.Name;
            if (currentUser == null) return RedirectToAction("Index", "Home");

            AccountActionExecutor.Logout(currentUser);

            TempData.AddMessage("You have been logged out.");

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(AccountChangePassword model)
        {
            if (!ModelState.IsValid)
            {
                TempData.AddError("Please correct validation errors.");
                return View();
            }

            var passwordChanged = AccountActionExecutor.TryExecute(model);
            if (passwordChanged)
            {
                TempData.AddMessage("Password has been updated.");
                return RedirectToLast();
            }

            TempData.AddError("Username or password is invalid.");
            return View();
        }

        [HttpGet]
        public IActionResult ChangeEmail()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChangeEmail(AccountChangeEmail model)
        {
            if (!ModelState.IsValid)
            {
                TempData.AddError("Please correct validation errors.");
                return View();
            }

            var changeEmailResult = AccountActionExecutor.TryExecute(model);
            if (changeEmailResult)
            {
                TempData.AddMessage("Recovery email has been updated.");
                return RedirectToLast();
            }

            TempData.AddError("Username or password is invalid.");
            return View();
        }

        [HttpGet]
        public IActionResult PasswordRecovery()
        {
            return View();
        }

        [HttpPost]
        public IActionResult PasswordRecovery(AccountPasswordRecovery model)
        {
            if (!ModelState.IsValid)
            {
                TempData.AddError("Please correct validation errors.");
                return View();
            }

            AccountRecovery.TryInitiateRecovery(model);

            //Show message regardless of success
            TempData.AddMessage("If the given email is valid for the specified user, a recovery email has been sent to that address. Please allow a couple minutes for the email to arrive.");
            return View("Recover");
        }

        [HttpGet]
        public IActionResult Recover()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Recover(AccountRecover model)
        {
            if (!ModelState.IsValid)
            {
                TempData.AddError("Please correct validation errors.");
                return View();
            }

            var passwordRecovered = AccountRecovery.TryRecover(model);
            if (passwordRecovered)
            {
                //Go ahead and submit a login request
                var loginRequest = new AccountLogin
                {
                    Username = model.Username,
                    Password = model.Password
                };
                var loginSucceeded = AccountActionExecutor.TryExecute(loginRequest);

                if (loginSucceeded)
                {
                    TempData.AddMessage("Your password has been updated.");
                    return RedirectToLast();
                }
            }

            TempData.AddError("Recovery attempt failed or ticket has expired.");
            return View();
        }

        [AcceptVerbs("Get", "Post")]
        public IActionResult ValidateStrongPassword(string password)
        {
            if (CommonPasswords.Contains(password))
            {
                return Json("This password is in a publicly accessible list of common passwords.");
            }

            password = password ?? "";

            var passwordStrength = 0.0;
            foreach (var c in password)
            {
                if (char.IsDigit(c)) passwordStrength += 3.322;
                else if (char.IsLower(c)) passwordStrength += 4.7;
                else if (char.IsUpper(c)) passwordStrength += 4.7;
                else passwordStrength += 5.04;
            }

            return passwordStrength >= 28 ? Json(true) : Json($"Minimum password strength: {Math.Round(passwordStrength, 1)}/28.0");
        }

        private void LogActivity(string details, dynamic data)
        {
            var logEntry = new SystemLog
            {
                Username = User.Identity?.Name ?? "Guest",
                IP = HttpContext.Connection.RemoteIpAddress.ToString(),
                Details = $"{DateTime.Now.ToString()}: {details}",
                Data = JsonConvert.SerializeObject(data)
            };

            RepositoryFactory<SystemLog>.SystemRepository().Upsert(logEntry);
        }
    }
}
