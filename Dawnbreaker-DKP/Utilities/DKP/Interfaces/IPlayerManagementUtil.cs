using System.Collections.Generic;
using Dawnbreaker_DKP.Web.Models.DKP;

namespace Dawnbreaker_DKP.Web.Utilities.DKP.Interfaces
{
    public interface IPlayerManagementUtil
    {
        List<string> GetAllPlayerNames();
        PlayerDetails GetPlayerDetails(string playerName);
        bool UpdateDetails(PlayerDetailsUpdateModel data);
        bool UpdateDKP(PlayerDKPUpdateModel data);
        bool DeletePlayer(string playerName);
        List<InactivePlayer> GetInactivePlayers();
    }
}
