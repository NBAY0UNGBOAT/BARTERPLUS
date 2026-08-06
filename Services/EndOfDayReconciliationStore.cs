using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using BarterPOS.Models;
using MongoDB.Driver;

namespace BarterPOS.Services
{
    public static class EndOfDayReconciliationStore
    {
        private static readonly object FileLock = new();

        public static List<EndOfDayReconciliation> GetReports()
        {
            lock (FileLock)
            {
                return LoadAll()
                    .OrderByDescending(r => r.BusinessDate)
                    .ToList();
            }
        }

        // Returns the saved reconciliation for a given calendar day, if one has already been recorded.
        public static EndOfDayReconciliation? GetReportForDate(DateTime businessDate)
        {
            lock (FileLock)
            {
                return LoadAll()
                    .FirstOrDefault(r => r.BusinessDate.Date == businessDate.Date);
            }
        }

        public static int GetPendingCount()
        {
            lock (FileLock)
            {
                return LoadAll().Count(r => !r.IsSynced);
            }
        }

        public static SaveSyncResult Save(EndOfDayReconciliation report)
        {
            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            bool synced = TrySyncToMongo(report, out string syncError);

            report.IsSynced = synced;
            report.SyncedAt = synced ? DateTime.Now : null;
            report.SyncError = synced ? string.Empty : syncError;

            lock (FileLock)
            {
                var reports = LoadAll();
                Upsert(reports, report);
                SaveAll(reports);
            }

            return new SaveSyncResult
            {
                IsSynced = synced,
                Message = synced
                    ? "End-of-day reconciliation saved and synced."
                    : $"End-of-day reconciliation saved offline. {syncError}"
            };
        }

        public static SyncResult SyncPending()
        {
            var result = new SyncResult();

            lock (FileLock)
            {
                var reports = LoadAll();
                var pending = reports.Where(r => !r.IsSynced).ToList();
                result.PendingBeforeSync = pending.Count;

                foreach (var report in pending)
                {
                    if (TrySyncToMongo(report, out string syncError))
                    {
                        report.IsSynced = true;
                        report.SyncedAt = DateTime.Now;
                        report.SyncError = string.Empty;
                        result.SyncedCount++;
                    }
                    else
                    {
                        report.SyncError = syncError;
                        result.FailedCount++;
                        result.LastError = syncError;
                    }
                }

                SaveAll(reports);
            }

            return result;
        }

        private static bool TrySyncToMongo(EndOfDayReconciliation report, out string error)
        {
            error = string.Empty;

            if (!MongoDatabaseFactory.CanAttemptSync(out error))
            {
                return false;
            }

            if (!MongoDatabaseFactory.TryCreateDatabase(out IMongoDatabase? database, out error) || database == null)
            {
                return false;
            }

            try
            {
                var reports = database.GetCollection<EndOfDayReconciliation>("endOfDayReconciliations");
                var idIndex = new CreateIndexModel<EndOfDayReconciliation>(
                    Builders<EndOfDayReconciliation>.IndexKeys.Ascending(r => r.Id),
                    new CreateIndexOptions { Unique = true });

                reports.Indexes.CreateOne(idIndex);
                reports.ReplaceOne(r => r.Id == report.Id, report, new ReplaceOptions { IsUpsert = true });
                MongoDatabaseFactory.MarkSyncSuccess();
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                MongoDatabaseFactory.MarkSyncFailure();
                return false;
            }
        }

        private static List<EndOfDayReconciliation> LoadAll()
        {
            string filePath = LocalApplicationStorage.GetDataFilePath("eod-reconciliations.json");

            if (!File.Exists(filePath))
            {
                return new List<EndOfDayReconciliation>();
            }

            try
            {
                string json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<List<EndOfDayReconciliation>>(json, LocalApplicationStorage.JsonOptions)
                    ?? new List<EndOfDayReconciliation>();
            }
            catch
            {
                return new List<EndOfDayReconciliation>();
            }
        }

        private static void SaveAll(List<EndOfDayReconciliation> reports)
        {
            string filePath = LocalApplicationStorage.GetDataFilePath("eod-reconciliations.json");
            string json = JsonSerializer.Serialize(reports.OrderBy(r => r.BusinessDate), LocalApplicationStorage.JsonOptions);
            File.WriteAllText(filePath, json);
        }

        private static void Upsert(List<EndOfDayReconciliation> reports, EndOfDayReconciliation report)
        {
            int index = reports.FindIndex(r => r.Id == report.Id);

            if (index >= 0)
            {
                reports[index] = report;
                return;
            }

            reports.Add(report);
        }
    }
}