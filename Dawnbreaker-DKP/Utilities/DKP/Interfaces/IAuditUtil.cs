using System.Collections.Generic;
using Dawnbreaker_DKP.Web.Models.DKP;

namespace Dawnbreaker_DKP.Web.Utilities.DKP.Interfaces
{
    public interface IAuditUtil
    {
        List<DKPTableRow> GetDKPTableData();
        List<DKPViewModel> GetDKPLedgerData();
        void ResetToCap();
        void SquishDKP();
    }
}
