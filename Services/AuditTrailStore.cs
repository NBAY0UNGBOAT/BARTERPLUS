using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using BarterPOS.Models;
using MongoDB.Driver;

namespace BarterPOS.Services
{
    public static class AuditTrailStore
    {
        private static readonly object FileLock = new();

        public static List<AuditTrailEntry> GetEntries()
        {
            lock (FileLock)
            {
                return LoadAll()
                    .OrderByDescending(entry => entry.Timestamp)
                    .ToList();
            }
        }

        public static int GetPendingCount()
        {
            lock (FileLock)
            {
                return LoadAll().Count(entry => !entry.IsSynced);
            }
        }

        public static SaveSyncResult Record(AuditTrailEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            bool synced = TrySyncToMongo(entry, out string syncError);

            entry.IsSynced = synced;
            entry.SyncedAt = synced ? DateTime.Now : null;
            entry.SyncError = synced ? string.Empty : syncError;

            lock (FileLock)
            {
                var entries = LoadAll();
                Upsert(entries, entry);
                SaveAll(entries);
            }

            return new SaveSyncResult
            {
                IsSynced = synced,
                Message = synced
                    ? "Audit trail entry saved and synced."
                    : $"Audit trail entry saved offline. {syncError}"
            };
        }

        public static SyncResult SyncPending()
        {
            var result = new SyncResult();

            lock (FileLock)
            {
                var entries = LoadAll();
                var pending = entries.Where(entry => !entry.IsSynced).ToList();
                result.PendingBeforeSync = pending.Count;

                foreach (var entry in pending)
                {
                    if (TrySyncToMongo(entry, out string syncError))
                    {
                        entry.IsSynced = true;
                        entry.SyncedAt = DateTime.Now;
                        entry.SyncError = string.Empty;
                        result.SyncedCount++;
                    }
                    else
                    {
                        entry.SyncError = syncError;
                        result.FailedCount++;
                        result.LastError = syncError;
                    }
                }

                SaveAll(entries);
            }

            return result;
        }

        public static void RecordSale(SaleTransaction transaction, string actor, string source = "POS")
        {
            Record(CreateEntry(
                actor,
                "Sale Completed",
                "Sale",
                transaction.TransactionId.ToString(),
                $"{transaction.TotalItems} item(s) for {transaction.NetAmount:C} via {transaction.PaymentMethod}",
                transaction.TerminalId,
                source));
        }

        public static void RecordTransactionUpdate(SaleTransaction transaction, string action, string actor, string details, string source = "POS")
        {
            Record(CreateEntry(
                actor,
                action,
                "Sale",
                transaction.TransactionId.ToString(),
                details,
                transaction.TerminalId,
                source));
        }

        public static void RecordCashDrawer(CashDrawerEntry entry, string actor, string source = "POS")
        {
            Record(CreateEntry(
                actor,
                entry.Type,
                "Cash Drawer",
                entry.Id,
                $"{entry.Amount:C} {entry.Note}".Trim(),
                entry.TerminalId,
                source));
        }

        public static void RecordReconciliation(EndOfDayReconciliation report, string actor, string source = "POS")
        {
            Record(CreateEntry(
                actor,
                "End-of-Day Reconciliation Saved",
                "Reconciliation",
                report.BusinessDate.ToString("yyyy-MM-dd"),
                $"Counted {report.CountedCash:C}, variance {report.Variance:C} ({report.VarianceLabel})",
                string.Empty,
                source));
        }

        private static AuditTrailEntry CreateEntry(
            string actor,
            string action,
            string entityType,
            string entityId,
            string details,
            string terminalId,
            string source)
        {
            return new AuditTrailEntry
            {
                Actor = actor,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details,
                TerminalId = terminalId,
                Source = source,
                Timestamp = DateTime.Now
            };
        }

        private static bool TrySyncToMongo(AuditTrailEntry entry, out string error)
        {
            error = string.Empty;

            if (!MongoDatabaseFactory.TryCreateDatabase(out IMongoDatabase? database, out error) || database == null)
            {
                return false;
            }

            try
            {
                var collection = database.GetCollection<AuditTrailEntry>("auditTrail");
                var idIndex = new CreateIndexModel<AuditTrailEntry>(
                    Builders<AuditTrailEntry>.IndexKeys.Ascending(e => e.Id),
                    new CreateIndexOptions { Unique = true });

                collection.Indexes.CreateOne(idIndex);
                collection.ReplaceOne(e => e.Id == entry.Id, entry, new ReplaceOptions { IsUpsert = true });
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static List<AuditTrailEntry> LoadAll()
        {
            string filePath = LocalApplicationStorage.GetDataFilePath("audit-trail.json");

            if (!File.Exists(filePath))
            {
                return new List<AuditTrailEntry>();
            }

            try
            {
                string json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<List<AuditTrailEntry>>(json, LocalApplicationStorage.JsonOptions)
                    ?? new List<AuditTrailEntry>();
            }
            catch
            {
                return new List<AuditTrailEntry>();
            }
        }

        private static void SaveAll(List<AuditTrailEntry> entries)
        {
            string filePath = LocalApplicationStorage.GetDataFilePath("audit-trail.json");
            string json = JsonSerializer.Serialize(entries.OrderBy(entry => entry.Timestamp), LocalApplicationStorage.JsonOptions);
            File.WriteAllText(filePath, json);
        }

        private static void Upsert(List<AuditTrailEntry> entries, AuditTrailEntry entry)
        {
            int index = entries.FindIndex(existing => existing.Id == entry.Id);

            if (index >= 0)
            {
                entries[index] = entry;
                return;
            }

            entries.Add(entry);
        }
    }
}
