using Dawnbreaker_DKP.Extensions;
using Dawnbreaker_DKP.Web.Models.DKP;
using Dawnbreaker_DKP.Web.Utilities.DKP.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dawnbreaker_DKP.Web.Controllers
{
    [Authorize("IsOfficer")]
    public class PlayerManagementController : Controller
    {
        private IPlayerManagementUtil PlayerManagementUtil { get; set; }

        public PlayerManagementController(IPlayerManagementUtil playerManagementUtil)
        {
            PlayerManagementUtil = playerManagementUtil;
        }

        public IActionResult Index()
        {
            ViewData["InactivePlayers"] = PlayerManagementUtil.GetInactivePlayers();
            var players = PlayerManagementUtil.GetAllPlayerNames();
            return View(players);
        }

        public IActionResult Edit([FromQuery] string playerName)
        {
            var playerDetails = PlayerManagementUtil.GetPlayerDetails(playerName);
            if (playerDetails == null)
            {
                TempData.AddError($"Player data for \"{playerName}\" not found.");
                return RedirectToAction("Index");
            }

            return View(playerDetails);
        }

        public IActionResult UpdateDetails(PlayerDetailsUpdateModel model)
        {
            var errors = false;

            var updateDetailsSuccess = PlayerManagementUtil.UpdateDetails(model);
            if (!updateDetailsSuccess)
            {
                TempData.AddError($"Error updating details for \"{model.PlayerName}\".");
                errors = true;
            }

            if (errors)
            {
                return RedirectToAction("Edit", new { playerName = model.PlayerName });
            }

            TempData.AddMessage($"Player details successfully updated.");
            return RedirectToAction("Edit", new { playerName = model.PlayerName });
        }

        public IActionResult UpdateDKP(PlayerDKPUpdateModel model)
        {
            var success = PlayerManagementUtil.UpdateDKP(model);
            if (!success)
            {
                TempData.AddError($"Error updating DKP for \"{model.PlayerName}\".");
                return RedirectToAction("Edit", new { playerName = model.PlayerName });
            }

            TempData.AddMessage($"DKP successfully updated.");
            return RedirectToAction("Edit", new { playerName = model.PlayerName });
        }

        public IActionResult Delete(PlayerDetailsUpdateModel data)
        {
            var playerName = data.PlayerName;
            if (string.IsNullOrWhiteSpace(playerName))
            {
                TempData.AddError("Could not find player.");
                return RedirectToAction("Index");
            }

            var success = PlayerManagementUtil.DeletePlayer(playerName);
            if (!success)
            {
                TempData.AddError($"Error deleting player: {playerName}.");
                return RedirectToAction("Index");
            }

            TempData.AddMessage("Player has been deleted.");
            return RedirectToAction("Index");
        }
    }
}