using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using BarterPOS.Models;
using MongoDB.Driver;

namespace BarterPOS.Services
{
    public static class TransactionRecordStore
    {
        private const int FirstTransactionId = 1001;
        private static readonly object FileLock = new();

        public static int GetNextTransactionId()
        {
            lock (FileLock)
            {
                int latestTransactionId = LoadAll()
                    .Select(t => t.TransactionId)
                    .DefaultIfEmpty(FirstTransactionId - 1)
                    .Max();

                return Math.Max(FirstTransactionId, latestTransactionId + 1);
            }
        }

        public static int GetPendingCount()
        {
            lock (FileLock)
            {
                return LoadAll().Count(t => !t.IsSynced);
            }
        }

        public static List<SaleTransaction> GetTransactions()
        {
            lock (FileLock)
            {
                return LoadAll()
                    .OrderByDescending(t => t.CompletedAt)
                    .ToList();
            }
        }

        public static SaveSyncResult Save(SaleTransaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            bool synced = TrySyncToMongo(transaction, out string syncError);

            transaction.IsSynced = synced;
            transaction.WasCapturedOffline = !synced;
            transaction.SyncedAt = synced ? DateTime.Now : null;
            transaction.SyncError = synced ? string.Empty : syncError;

            lock (FileLock)
            {
                var transactions = LoadAll();
                Upsert(transactions, transaction);
                SaveAll(transactions);
            }

            return new SaveSyncResult
            {
                IsSynced = synced,
                Message = synced
                    ? "Transaction saved and synced."
                    : $"Transaction saved offline. {syncError}"
            };
        }

        public static void UpdateTransaction(SaleTransaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            lock (FileLock)
            {
                var transactions = LoadAll();
                Upsert(transactions, transaction);
                SaveAll(transactions);
            }
        }

        public static SyncResult SyncPending()
        {
            var result = new SyncResult();

            lock (FileLock)
            {
                var transactions = LoadAll();
                var pending = transactions.Where(t => !t.IsSynced).ToList();
                result.PendingBeforeSync = pending.Count;

                foreach (var transaction in pending)
                {
                    if (TrySyncToMongo(transaction, out string syncError))
                    {
                        transaction.IsSynced = true;
                        transaction.SyncedAt = DateTime.Now;
                        transaction.SyncError = string.Empty;
                        result.SyncedCount++;
                    }
                    else
                    {
                        transaction.SyncError = syncError;
                        result.FailedCount++;
                        result.LastError = syncError;
                    }
                }

                SaveAll(transactions);
            }

            return result;
        }

        public static bool RefundItems(
            SaleTransaction transaction,
            List<SaleLineItem> refundedItems)
        {
            if (transaction == null || refundedItems.Count == 0)
            {
                return false;
            }

            foreach (var refund in refundedItems)
            {
                var existingRefund = transaction.RefundedItems
                    .FirstOrDefault(i => i.Code == refund.Code);

                if (existingRefund == null)
                {
                    transaction.RefundedItems.Add(new SaleLineItem
                    {
                        Code = refund.Code,
                        Name = refund.Name,
                        UnitPrice = refund.UnitPrice,
                        Quantity = refund.Quantity,
                        Subtotal = refund.UnitPrice * refund.Quantity
                    });
                }
                else
                {
                    existingRefund.Quantity += refund.Quantity;
                    existingRefund.Subtotal =
                        existingRefund.UnitPrice * existingRefund.Quantity;
                }
            }

            bool fullyRefunded = true;

            foreach (var item in transaction.Items)
            {
                int refundedQty = transaction.RefundedItems
                    .Where(r => r.Code == item.Code)
                    .Sum(r => r.Quantity);

                if (refundedQty < item.Quantity)
                {
                    fullyRefunded = false;
                    break;
                }
            }

            transaction.Status = fullyRefunded
                ? TransactionStatus.Refunded
                : TransactionStatus.PartiallyRefunded;

            UpdateTransaction(transaction);

            return true;
        }
        
        private static bool TrySyncToMongo(SaleTransaction transaction, out string error)
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
                var transactions = database.GetCollection<SaleTransaction>("transactions");
                var idIndex = new CreateIndexModel<SaleTransaction>(
                    Builders<SaleTransaction>.IndexKeys.Ascending(t => t.Id),
                    new CreateIndexOptions { Unique = true });

                transactions.Indexes.CreateOne(idIndex);
                transactions.ReplaceOne(t => t.Id == transaction.Id, transaction, new ReplaceOptions { IsUpsert = true });
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

        private static List<SaleTransaction> LoadAll()
        {
            string filePath = LocalApplicationStorage.GetDataFilePath("transactions.json");

            if (!File.Exists(filePath))
            {
                return new List<SaleTransaction>();
            }

            try
            {
                string json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<List<SaleTransaction>>(json, LocalApplicationStorage.JsonOptions)
                    ?? new List<SaleTransaction>();
            }
            catch
            {
                return new List<SaleTransaction>();
            }
        }

        private static void SaveAll(List<SaleTransaction> transactions)
        {
            string filePath = LocalApplicationStorage.GetDataFilePath("transactions.json");
            string json = JsonSerializer.Serialize(transactions.OrderBy(t => t.TransactionId), LocalApplicationStorage.JsonOptions);
            File.WriteAllText(filePath, json);
        }

        private static void Upsert(List<SaleTransaction> transactions, SaleTransaction transaction)
        {
            int index = transactions.FindIndex(t => t.Id == transaction.Id);

            if (index >= 0)
            {
                transactions[index] = transaction;
                return;
            }

            transactions.Add(transaction);
        }
    }
}
