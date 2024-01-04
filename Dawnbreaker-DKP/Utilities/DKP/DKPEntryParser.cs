using System;
using System.Linq;
using Dawnbreaker_DKP.Data.DKP;
using Dawnbreaker_DKP.Web.Models.DKP;
using Dawnbreaker_DKP.Web.Utilities.DKP.Interfaces;

namespace Dawnbreaker_DKP.Web.Utilities.DKP
{
    public class DKPEntryParser : IDKPEntryParser
    {
        public DKPLedgerEntry TryParse(DKPEntryModel model, out bool success)
        {
            var entry = TryParse(model.EntryString);
            if (entry == null)
            {
                success = false;
                return null;
            }

            success = true;
            entry.EntryId = Guid.NewGuid();
            entry.RaidSessionId = model.RaidSessionId;
            entry.EntryTime = DateTime.Now.Ticks;
            return entry;
        }

        private DKPLedgerEntry TryParse(string dkpEntryRaw)
        {
            if (!dkpEntryRaw.StartsWith("beginDKPEntry:") && !dkpEntryRaw.StartsWith("beginSKEntry:")) return null;
            if (!dkpEntryRaw.EndsWith(":endDKPEntry") && !dkpEntryRaw.EndsWith(":endSKEntry")) return null;
            if (dkpEntryRaw.Count(c => c == ':') != 2) return null;

            var isSKEntry = dkpEntryRaw.StartsWith("beginSKEntry:");

            var entryData = dkpEntryRaw.Replace("beginDKPEntry:", "")
                .Replace(":endDKPEntry", "")
                .Replace("beginSKEntry:", "")
                .Replace(":endSKEntry", "")
                .Split(";", StringSplitOptions.RemoveEmptyEntries);

            if (entryData.Length != 4) return null;

            string playerName = entryData[0];
            if (string.IsNullOrWhiteSpace(playerName)) return null;

            var itemIdParsed = int.TryParse(entryData[1], out var itemId);
            if (!itemIdParsed) return null;

            string entryText = entryData[2];
            if (string.IsNullOrWhiteSpace(entryText)) return null;

            var dkpAmountParsed = int.TryParse(entryData[3], out var dkpAmount);
            if (!dkpAmountParsed) return null;

            return new DKPLedgerEntry
            {
                PlayerName = playerName,
                ItemId = itemId,
                EntryText = entryText,
                DKPAmount = dkpAmount,
                IsSKEntry = isSKEntry
            };
        }
    }
}
