using System;

namespace Dawnbreaker_DKP.Web.Models.DKP
{
    public class ChangeParticipantStatusModel
    {
        public Guid RaidSessionId { get; set; }
        public string ActivePlayers { get; set; }
        public string StandbyPlayers { get; set; }
        public string RemovePlayers { get; set; }
    }
}
