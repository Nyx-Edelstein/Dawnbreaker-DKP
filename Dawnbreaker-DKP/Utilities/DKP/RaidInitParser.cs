using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Dawnbreaker_DKP.Data.DKP;
using Dawnbreaker_DKP.Web.Models.DKP;
using Dawnbreaker_DKP.Web.Utilities.DKP.Interfaces;

namespace Dawnbreaker_DKP.Web.Utilities.DKP
{
    public class RaidInitParser : IRaidInitParser
    {
        public RaidSessionComposite TryParse(SessionInitModel model, out bool parseSucceeded)
        {
            var participants = TryParse(model.SessionInitString);
            if (participants == null)
            {
                parseSucceeded = false;
                return null;
            };

            parseSucceeded = true;
            var raidSessionId = Guid.NewGuid();
            foreach (var participant in participants)
            {
                participant.RaidSessionId = raidSessionId;
            }

            return new RaidSessionComposite
            {
                RaidSession = new RaidSession
                {
                    RaidSessionId = raidSessionId,
                    RaidName = model.RaidName,
                    RaidRoster = model.RaidRoster,
                    Date = DateTime.Now,
                    Notes = string.Empty,
                    BossesKilled = string.Empty,
                    DKPAward = 0,
                    Open = true
                },
                Participants = participants
            };
        }

        private List<SessionParticipant> TryParse(string sessionInitString)
        {
            if (!sessionInitString.StartsWith("beginSessionInit:")) return null;
            if (!sessionInitString.EndsWith(":endSessionInit")) return null;
            if (sessionInitString.Count(c => c == ':') != 2) return null;

            sessionInitString = sessionInitString.Replace("beginSessionInit:", "").Replace(":endSessionInit", "");

            var participantNames = sessionInitString.Split(";", StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .Select(x => new SessionParticipant
                {
                    PlayerName = x
                }).Where(x => !string.IsNullOrWhiteSpace(x.PlayerName))
                .ToList();
            return participantNames.Any() ? participantNames : null;
        }
    }
}
