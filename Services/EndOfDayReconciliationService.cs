    using System;
using System.Linq;
using BarterPOS.Models;

namespace BarterPOS.Services
{
    // Pure computation: reads today's transactions + cash drawer entries and produces a report.
    // Does not persist anything — see EndOfDayReconciliationStore for saving a finalized report.
    public static class EndOfDayReconciliationService
    {
        public static EndOfDayReconciliation BuildReport(DateTime businessDate, decimal countedCash)
        {
            DateTime dayStart = businessDate.Date;
            DateTime dayEnd = dayStart.AddDays(1);

            var transactionsForDay = TransactionRecordStore.GetTransactions()
                .Where(t => t.CompletedAt >= dayStart && t.CompletedAt < dayEnd)
                .ToList();

            var drawerEntriesForDay = CashDrawerStore.GetEntries()
                .Where(e => e.Timestamp >= dayStart && e.Timestamp < dayEnd)
                .ToList();

            var activeTransactions = transactionsForDay
                .Where(t => t.Status != TransactionStatus.Voided)
                .ToList();

            var report = new EndOfDayReconciliation
            {
                BusinessDate = dayStart,
                ReconciledBy = Session.CurrentUser?.FullName is { Length: > 0 } fullName
                    ? fullName
                    : Session.CurrentUser?.Username ?? "Unknown",
                ReconciledByUsername = Session.CurrentUser?.Username ?? string.Empty,
                CountedCash = countedCash,

                TransactionCount = activeTransactions.Count,
                VoidedCount = transactionsForDay.Count(t => t.Status == TransactionStatus.Voided),
                RefundedCount = transactionsForDay.Count(t =>
                    t.Status == TransactionStatus.Refunded || t.Status == TransactionStatus.PartiallyRefunded),

                GrossSales = activeTransactions.Sum(t => t.GrossAmount),
                TotalDiscounts = activeTransactions.Sum(t => t.PercentageDiscount + t.ManualDeduction),
                NetSales = activeTransactions.Sum(t => t.NetAmount),

                PaymentMethodBreakdown = activeTransactions
                    .GroupBy(t => string.IsNullOrWhiteSpace(t.PaymentMethod) ? "Unspecified" : t.PaymentMethod)
                    .Select(g => new PaymentMethodTotal
                    {
                        PaymentMethod = g.Key,
                        TransactionCount = g.Count(),
                        GrossAmount = g.Sum(t => t.GrossAmount),
                        NetAmount = g.Sum(t => t.NetAmount)
                    })
                    .OrderByDescending(p => p.NetAmount)
                    .ToList()
            };

            report.CashSalesTotal = drawerEntriesForDay
                .Where(e => e.Type == CashDrawerEntryTypes.CashSale)
                .Sum(e => e.Amount);

            report.CashRefundsTotal = drawerEntriesForDay
                .Where(e => e.Type == CashDrawerEntryTypes.RefundCash)
                .Sum(e => e.Amount);

            report.CashInTotal = drawerEntriesForDay
                .Where(e => e.Type == CashDrawerEntryTypes.CashIn)
                .Sum(e => e.Amount);

            report.CashOutTotal = drawerEntriesForDay
                .Where(e => e.Type == CashDrawerEntryTypes.CashOut)
                .Sum(e => e.Amount);

            // Net cash movement for the day only (no opening float tracked in this system).
            report.ExpectedCash = drawerEntriesForDay.Sum(e => e.SignedAmount);

            return report;
        }
    }
}