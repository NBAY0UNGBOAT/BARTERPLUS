using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using BarterPOS.Models;
using MongoDB.Driver;

namespace BarterPOS.Services
{
    public static class CashDrawerStore
    {
        private static readonly object FileLock = new();

        public static decimal GetCurrentBalance()
        {
            lock (FileLock)
            {
                return LoadAll().Sum(e => e.SignedAmount);
            }
        }

        public static string GetLastActivityText()
        {
            lock (FileLock)
            {
                var latest = LoadAll()
                    .OrderByDescending(e => e.Timestamp)
                    .FirstOrDefault();

                if (latest == null)
                {
                    return "No drawer activity yet";
                }

                return $"{latest.Type} {latest.SignedAmount:C} at {latest.Timestamp:h:mm tt}";
            }
        }

        public static int GetPendingCount()
        {
            lock (FileLock)
            {
                return LoadAll().Count(e => !e.IsSynced);
            }
        }

        public static SaveSyncResult AddEntry(CashDrawerEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            entry.Amount = Math.Abs(entry.Amount);
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
                    ? "Cash drawer entry saved and synced."
                    : $"Cash drawer entry saved offline. {syncError}"
            };
        }

        public static SyncResult SyncPending()
        {
            var result = new SyncResult();

            lock (FileLock)
            {
                var entries = LoadAll();
                var pending = entries.Where(e => !e.IsSynced).ToList();
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

        private static bool TrySyncToMongo(CashDrawerEntry entry, out string error)
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
                var entries = database.GetCollection<CashDrawerEntry>("cashDrawerEntries");
                var idIndex = new CreateIndexModel<CashDrawerEntry>(
                    Builders<CashDrawerEntry>.IndexKeys.Ascending(e => e.Id),
                    new CreateIndexOptions { Unique = true });

                entries.Indexes.CreateOne(idIndex);
                entries.ReplaceOne(e => e.Id == entry.Id, entry, new ReplaceOptions { IsUpsert = true });
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

        private static List<CashDrawerEntry> LoadAll()
        {
            string filePath = LocalApplicationStorage.GetDataFilePath("cash-drawer.json");

            if (!File.Exists(filePath))
            {
                return new List<CashDrawerEntry>();
            }

            try
            {
                string json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<List<CashDrawerEntry>>(json, LocalApplicationStorage.JsonOptions)
                    ?? new List<CashDrawerEntry>();
            }
            catch
            {
                return new List<CashDrawerEntry>();
            }
        }

        private static void SaveAll(List<CashDrawerEntry> entries)
        {
            string filePath = LocalApplicationStorage.GetDataFilePath("cash-drawer.json");
            string json = JsonSerializer.Serialize(entries.OrderBy(e => e.Timestamp), LocalApplicationStorage.JsonOptions);
            File.WriteAllText(filePath, json);
        }

        private static void Upsert(List<CashDrawerEntry> entries, CashDrawerEntry entry)
        {
            int index = entries.FindIndex(e => e.Id == entry.Id);

            if (index >= 0)
            {
                entries[index] = entry;
                return;
            }

            entries.Add(entry);
        }
    }
}
