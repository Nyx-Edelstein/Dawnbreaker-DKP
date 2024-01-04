using System;
using System.Collections.Generic;
using System.Linq;
using Dawnbreaker_DKP.Data.DKP;
using Dawnbreaker_DKP.Extensions;
using Dawnbreaker_DKP.Web.Models.DKP;
using Dawnbreaker_DKP.Web.Utilities.DKP.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dawnbreaker_DKP.Controllers
{
    [Authorize("IsOfficer")]
    public class RaidSessionsController : Controller
    {
        public IRaidSessionsUtil RaidSessionsUtil { get; }
        public IRaidInitParser RaidInitParser { get; }
        public IDKPEntryParser DKPEntryParser { get; }
        public IAuditUtil AuditUtil { get; }
        public IClassListUtil ClassListUtil { get; }

        public RaidSessionsController(IRaidSessionsUtil raidSessionsUtil, IRaidInitParser raidInitParser, IDKPEntryParser dkpEntryParser, IAuditUtil auditUtil, IClassListUtil classListUtil)
        {
            RaidSessionsUtil = raidSessionsUtil;
            RaidInitParser = raidInitParser;
            DKPEntryParser = dkpEntryParser;
            AuditUtil = auditUtil;
            ClassListUtil = classListUtil;
        }

        public IActionResult Error()
        {
            return View();
        }

        public IActionResult Index()
        {
            var sessions = RaidSessionsUtil.GetSessions().OrderByDescending(x => x.Date).ThenBy(x => x.RaidName).ToList();
            return View(sessions);
        }

        public IActionResult New(SessionInitModel model)
        {
            if (model == null)
            {
                TempData.AddError("Initialization string was not specified or failed to parse correctly.");
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(model.RaidName))
            {
                TempData.AddError("Raid name must be specified.");
                return RedirectToAction("Index");
            }

            var raidSessionData = RaidInitParser.TryParse(model, out var parseSucceeded);
            if (!parseSucceeded)
            {
                TempData.AddError("Initialization string was not specified or failed to parse correctly.");
                return RedirectToAction("Index");
            }

            var success = RaidSessionsUtil.TryAdd(raidSessionData);
            if (!success)
            {
                TempData.AddError("Error creating new raid session.");
                return RedirectToAction("Index");
            }

            TempData.AddMessage("Raid session successfully created.");
            return RedirectToAction("Manage", new { raidSessionid = raidSessionData.RaidSession.RaidSessionId });
        }

        public IActionResult Manage(Guid raidSessionId)
        {
            var session = RaidSessionsUtil.GetRaidSessionById(raidSessionId);
            ViewData["Participants"] = RaidSessionsUtil.GetRaidParticipants(raidSessionId);
            ViewData["SessionLedger"] = RaidSessionsUtil.GetSessionLedger(raidSessionId);
            ViewData["AddonData"] = GetAddonData();
            return View(session);
        }

        private string GetAddonData()
        {
            var data = AuditUtil.GetDKPTableData();
            var luaKVPs = data.Select(x => $"[\"{x.PlayerName}\"]=\"{x.DKPCurrent}\"");
            var luaTable = $"{{{string.Join(", ", luaKVPs)}}}";

            return luaTable;
        }

        public IActionResult ChangeStatus(ChangeParticipantStatusModel model)
        {
            if (string.IsNullOrWhiteSpace(model.ActivePlayers)
                && string.IsNullOrWhiteSpace(model.StandbyPlayers)
                && string.IsNullOrWhiteSpace(model.RemovePlayers))
            {
                TempData.AddError("No participant status changes specified.");
                return RedirectToAction("Manage", new {raidSessionId = model.RaidSessionId});
            }

            var activeParticipants = ParseParticipants(model.RaidSessionId, model.ActivePlayers);
            var standbyParticipants = ParseParticipants(model.RaidSessionId, model.StandbyPlayers, standby: true);
            var allParticipants = activeParticipants.Union(standbyParticipants);
            RaidSessionsUtil.ChangeParticipantStatuses(allParticipants);

            var toRemove = ParseParticipants(model.RaidSessionId, model.RemovePlayers);
            RaidSessionsUtil.RemoveParticipants(toRemove);

            TempData.AddMessage("Participant statuses successfully changed.");
            return RedirectToAction("Manage", new {raidSessionId = model.RaidSessionId});
        }

        private static List<SessionParticipant> ParseParticipants(Guid raidSessionId, string participantsStr, bool standby = false)
        {
            return (participantsStr ?? string.Empty).Split(",", StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Distinct()
                .Select(x => new SessionParticipant
                {
                    RaidSessionId = raidSessionId,
                    PlayerName = FirstCharToUpper(x),
                    Standby = standby,
                }).ToList();
        }

        private static string FirstCharToUpper(string input)
        {
            switch (input)
            {
                case null: return null;
                case "": return input;
                default: return input.First().ToString().ToUpper() + input.Substring(1);
            }
        }

        public IActionResult MakeLedgerEntry(DKPEntryModel model)
        {
            if (string.IsNullOrWhiteSpace(model.EntryString))
            {
                TempData.AddError("DKP entry data must be specified.");
                return RedirectToAction("Manage", new { raidSessionId = model.RaidSessionId });
            }

            var entry = DKPEntryParser.TryParse(model, out var parseSucceeded);
            if (!parseSucceeded)
            {
                TempData.AddError("DKP entry string was not specified or failed to parse correctly.");
                return RedirectToAction("Manage", new { raidSessionId = model.RaidSessionId });
            }

            entry.PlayerName = FirstCharToUpper(entry.PlayerName);
            RaidSessionsUtil.AddDKPEntry(entry);
            if (entry.IsSKEntry) ClassListUtil.HandleSKEntry(model.RaidSessionId, entry.PlayerName);

            TempData.AddMessage("DKP entry successfully added.");
            return RedirectToAction("Manage", new { raidSessionId = model.RaidSessionId });
        }

        public IActionResult RemoveLedgerEntry([FromQuery] Guid raidSessionId, [FromQuery] Guid entryId)
        {
            var success = RaidSessionsUtil.RemoveDKPEntry(entryId);
            if (success)
            {
                TempData.AddMessage("DKP entry removed.");
            }
            else
            {
                TempData.AddError("Error removing DKP entry.");
            }
            
            return RedirectToAction("Manage", new { raidSessionId = raidSessionId });
        }

        public IActionResult FinalizeSession(RaidSession session)
        {
            RaidSessionsUtil.CloseSession(session);
            TempData.AddMessage("Raid session closed.");
            return RedirectToAction("Index");
        }
    }
}
