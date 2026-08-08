using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace BarterPOS.Models
{
    public enum TransactionStatus
    {
        Completed,
        PartiallyRefunded,
        Refunded,
        Voided
    }

    public class SaleTransaction
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public int TransactionId { get; set; }
        public string TerminalId { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public DateTime CompletedAt { get; set; } = DateTime.Now;
        public string Cashier { get; set; } = string.Empty;
        public string CashierUsername { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerType { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public TransactionStatus Status { get; set; } = TransactionStatus.Completed;
        public List<SaleLineItem> Items { get; set; } = new();
        public List<SaleLineItem> RefundedItems { get; set; } = new();
        public int TotalItems { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal PercentageDiscount { get; set; }
        public decimal ManualDeduction { get; set; }
        public decimal LoyaltyCreditRedeemed { get; set; }
        public decimal LoyaltyPointsEarned { get; set; }
        public decimal NetAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal ChangeDue { get; set; }
        public bool WasCapturedOffline { get; set; }
        public bool IsSynced { get; set; }
        public DateTime? SyncedAt { get; set; }
        public string SyncError { get; set; } = string.Empty;

        [BsonIgnore]
        public string SyncStatus => IsSynced ? "Synced" : "Pending Sync";
    }
}
