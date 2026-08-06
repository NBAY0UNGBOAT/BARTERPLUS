using System;
using MongoDB.Bson.Serialization.Attributes;

namespace BarterPOS.Models
{
    public static class CashDrawerEntryTypes
    {
        public const string CashIn = "Cash In";
        public const string CashOut = "Cash Out";
        public const string CashSale = "Cash Sale";
        public const string RefundCash = "Cash Refund";
    }

    public class CashDrawerEntry
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string TerminalId { get; set; } = string.Empty;
        public string Cashier { get; set; } = string.Empty;
        public string CashierUsername { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Note { get; set; } = string.Empty;
        public int? RelatedTransactionId { get; set; }
        public bool IsSynced { get; set; }
        public DateTime? SyncedAt { get; set; }
        public string SyncError { get; set; } = string.Empty;

        [BsonIgnore]
        public decimal SignedAmount =>
            Type == CashDrawerEntryTypes.CashOut || Type == CashDrawerEntryTypes.RefundCash
                ? -Amount
                : Amount;

        [BsonIgnore]
        public string DisplayAmount => SignedAmount.ToString("C");
    }
}
