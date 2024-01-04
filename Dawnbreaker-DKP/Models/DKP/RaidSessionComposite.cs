using System.Collections.Generic;
using Dawnbreaker_DKP.Data.DKP;

namespace Dawnbreaker_DKP.Web.Models.DKP
{
    public class RaidSessionComposite
    {
        public RaidSession RaidSession { get; set; }
        public List<SessionParticipant> Participants { get; set; }
    }
}
