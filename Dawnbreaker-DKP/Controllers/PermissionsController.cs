using Dawnbreaker_DKP.Extensions;
using Dawnbreaker_DKP.Models.Account;
using Dawnbreaker_DKP.Utilities;
using Dawnbreaker_DKP.Utilities.UserData.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dawnbreaker_DKP.Controllers
{
    [Authorize("IsAdmin")]
    public class PermissionsController : Controller
    {
        public IUserPermissionLookup UserPermissionsLookup { get; }

        public PermissionsController(IUserPermissionLookup userLookup)
        {
            UserPermissionsLookup = userLookup;
        }

        public IActionResult Error()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Update()
        {
            var users = UserPermissionsLookup.GetUserPermissions();
            ViewData["Users"] = users;
            return View();
        }

        [HttpPost]
        public IActionResult Update(AccountPermission model)
        {
            if (!ModelState.IsValid)
            {
                TempData.AddError("Please correct validation errors.");
                return View();
            }

            UserPermissionsLookup.UpdatePermissionFor(model.Username, model.PermissionsLevel);
            return Update();
        }
    }
}
