using Dawnbreaker_DKP.Extensions;
using Dawnbreaker_DKP.Web.Utilities.DKP.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dawnbreaker_DKP.Web.Controllers
{
    public class PlayerController : Controller
    {
        private IPlayerManagementUtil PlayerManagementUtil { get; set; }

        public PlayerController(IPlayerManagementUtil playerManagementUtil)
        {
            PlayerManagementUtil = playerManagementUtil;
        }

        public IActionResult Details([FromQuery] string playerName)
        {
            var playerDetails = PlayerManagementUtil.GetPlayerDetails(playerName);
            if (playerDetails == null)
            {
                TempData.AddError($"Player data for \"{playerName}\" not found.");
                return RedirectToAction("Index", "Home");
            }

            return View(playerDetails);
        }
    }
}