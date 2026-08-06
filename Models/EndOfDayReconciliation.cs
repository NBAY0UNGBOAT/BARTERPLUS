using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace BarterPOS.Models
{
    public class PaymentMethodTotal
    {
        public string PaymentMethod { get; set; } = string.Empty;
        public int TransactionCount { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal NetAmount { get; set; }
    }

    public class EndOfDayReconciliation
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        // The calendar day this reconciliation covers (date-only, time component ignored).
        public DateTime BusinessDate { get; set; }

        public DateTime ReconciledAt { get; set; } = DateTime.Now;
        public string ReconciledBy { get; set; } = string.Empty;
        public string ReconciledByUsername { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        // Sales summary (Voided transactions excluded from totals below).
        public int TransactionCount { get; set; }
        public int VoidedCount { get; set; }
        public int RefundedCount { get; set; }
        public decimal GrossSales { get; set; }
        public decimal TotalDiscounts { get; set; }
        public decimal NetSales { get; set; }
        public List<PaymentMethodTotal> PaymentMethodBreakdown { get; set; } = new();

        // Cash drawer summary — net movement for BusinessDate only (no opening float tracked).
        public decimal CashSalesTotal { get; set; }
        public decimal CashRefundsTotal { get; set; }
        public decimal CashInTotal { get; set; }
        public decimal CashOutTotal { get; set; }
        public decimal ExpectedCash { get; set; }
        public decimal CountedCash { get; set; }

        public bool IsSynced { get; set; }
        public DateTime? SyncedAt { get; set; }
        public string SyncError { get; set; } = string.Empty;

        [BsonIgnore]
        public decimal Variance => CountedCash - ExpectedCash;

        [BsonIgnore]
        public bool IsBalanced => Math.Abs(Variance) < 0.01m;

        [BsonIgnore]
        public string VarianceLabel => IsBalanced ? "Balanced" : (Variance > 0 ? "Over" : "Short");

        [BsonIgnore]
        public string SyncStatus => IsSynced ? "Synced" : "Pending Sync";
    }
}