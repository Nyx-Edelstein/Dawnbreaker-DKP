using Dawnbreaker_DKP.Data.DKP;
using Dawnbreaker_DKP.Web.Models.DKP;

namespace Dawnbreaker_DKP.Web.Utilities.DKP.Interfaces
{
    public interface IDKPEntryParser
    {
        DKPLedgerEntry TryParse(DKPEntryModel model, out bool success);
    }
}
