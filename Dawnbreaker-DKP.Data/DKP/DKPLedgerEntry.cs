using System;
using Dawnbreaker_DKP.Repository;

namespace Dawnbreaker_DKP.Data.DKP
{
    public class DKPLedgerEntry : DataItem
    {
        public Guid EntryId { get; set; }
        public Guid RaidSessionId { get; set; }
        public string PlayerName { get; set; }
        public int ItemId { get; set; }
        public string EntryText { get; set; }
        public int DKPAmount { get; set; }
        public long EntryTime { get; set; }
        public bool IsSKEntry { get; set; }
    }
}
