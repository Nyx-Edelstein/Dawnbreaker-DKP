using Dawnbreaker_DKP.Web.Models.DKP;

namespace Dawnbreaker_DKP.Web.Utilities.DKP.Interfaces
{
    public interface IRaidInitParser
    {
        RaidSessionComposite TryParse(SessionInitModel model, out bool parseSucceeded);
    }
}
