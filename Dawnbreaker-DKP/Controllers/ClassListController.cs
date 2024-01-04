using Dawnbreaker_DKP.Data.DKP;
using Dawnbreaker_DKP.Data.DKP.Enum;
using Dawnbreaker_DKP.Extensions;
using Dawnbreaker_DKP.Web.Utilities.DKP.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dawnbreaker_DKP.Web.Controllers
{
    public class ClassListController : Controller
    {
        private IClassListUtil ClassListUtil { get; set; }

        public ClassListController(IClassListUtil classListUtil)
        {
            ClassListUtil = classListUtil;
        }

        public IActionResult Details([FromQuery] string raidRoster, [FromQuery] string classList)
        {
            var data = ClassListUtil.GetClassListData(raidRoster, classList);
            return View(data);
        }

        [Authorize("IsOfficer")]
        public IActionResult NewEntry(ClassListEntry data)
        {
            if (data == null)
            {
                TempData.AddError($"Invalid data.");
                return RedirectToAction("Details");
            }

            if (string.IsNullOrWhiteSpace(data.RaidRoster))
            {
                TempData.AddError("Raid roster must be specified.");
                return RedirectToAction("Details");
            }

            if (string.IsNullOrWhiteSpace(data.Class) || data.Class == Class.Unknown.ToString())
            {
                TempData.AddError("Class must be specified.");
                return RedirectToAction("Details");
            }

            if (string.IsNullOrWhiteSpace(data.PlayerName))
            {
                TempData.AddError("Player name must be specified.");
                return RedirectToAction("Details");
            }

            var success = ClassListUtil.AddEntry(data);
            if (!success)
            {
                TempData.AddError("Player not found.");
                return RedirectToAction("Details");
            }

            TempData.AddMessage("Entry successfully added.");
            return RedirectToAction("Details");
        }

        public IActionResult MovePlayerToBottom(string raidroster, string classtype, string playername)
        {
            var success = ClassListUtil.MovePlayerToBottom(raidroster, classtype, playername);
            if (!success)
            {
                TempData.AddError("Error occured when attempting to move player to bottom of list.");
                return RedirectToAction("Details");
            }

            return RedirectToAction("Details");
        }
    }
}