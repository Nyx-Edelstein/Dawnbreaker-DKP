using System.Linq;
using Dawnbreaker_DKP.Extensions;
using Dawnbreaker_DKP.Web.Models.DKP;
using Dawnbreaker_DKP.Web.Utilities.DKP.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dawnbreaker_DKP.Controllers
{
    public class HomeController : Controller
    {
        private IAuditUtil AuditUtil { get; set; }
        private IImportUtil ImportUtil { get; set; }

        public HomeController(IAuditUtil auditUtil, IImportUtil importUtil)
        {
            AuditUtil = auditUtil;
            ImportUtil = importUtil;
        }

        public IActionResult Index()
        {
            var dkpTableData = AuditUtil.GetDKPTableData();
            return View(dkpTableData);
        }

        public IActionResult Ledger([FromQuery] bool itemsOnly, [FromQuery] string sourceFilter)
        {
            var ledgerData = AuditUtil.GetDKPLedgerData();

            if (itemsOnly)
            {
                ledgerData = ledgerData.Where(x => x.ItemId > 0).ToList();
            }

            if (!string.IsNullOrWhiteSpace(sourceFilter))
            {
                ledgerData = ledgerData.Where(x => ContainsStandardized(x.Source, sourceFilter)).ToList();
            }

            var order = ledgerData.Count;
            foreach (var vm in ledgerData)
            {
                vm.Order = order;
                order -= 1;
            }

            ledgerData = ledgerData.Take(500).ToList();

            return View(ledgerData);
        }

        private static bool ContainsStandardized(string a, string b)
        {
            if (string.IsNullOrWhiteSpace(a)) return false;
            return a.Trim().ToLowerInvariant().Contains(b.Trim().ToLowerInvariant());
        }

        [Authorize("IsAdmin")]
        public IActionResult Import()
        {
            return View(new ImportData());
        }

        [Authorize("IsAdmin")]
        public IActionResult ImportData(ImportData importData)
        {
            if (string.IsNullOrWhiteSpace(importData?.Data))
            {
                TempData.AddError("Error: No input data to parse.");
                return RedirectToAction("Import");
            }

            var success = ImportUtil.TryImport(importData.Data);
            if (success)
            {
                TempData.AddMessage("Data imported successfully.");
            }
            else
            {
                TempData.AddError("Error: Data could not be imported.");
            }

            return RedirectToAction("Import");
        }

        [Authorize("IsOfficer")]
        public IActionResult ResetToCap()
        {
            AuditUtil.ResetToCap();
            TempData.AddMessage("Players over cap have been reset to cap.");
            return RedirectToAction("Index");
        }

        //[Authorize("IsAdmin")]
        //public IActionResult DKPSquish()
        //{
        //    AuditUtil.SquishDKP();
        //    TempData.AddMessage("DKP squish successful.");
        //    return RedirectToAction("Index");
        //}

        public IActionResult TAC()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
