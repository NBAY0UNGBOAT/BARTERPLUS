using System;
using MongoDB.Driver;

namespace BarterPOS.Services
{
    internal static class MongoDatabaseFactory
    {
        private static readonly object SyncStateLock = new();
        private static DateTime? _syncSuspendedUntil;

        public static bool IsConfigured =>
            !string.IsNullOrWhiteSpace(AppConfig.GetMongoSettings().ConnectionString);

        public static bool CanAttemptSync(out string error)
        {
            lock (SyncStateLock)
            {
                if (_syncSuspendedUntil.HasValue && _syncSuspendedUntil.Value > DateTime.Now)
                {
                    error = $"Remote sync is temporarily unavailable. Retry after {_syncSuspendedUntil.Value:h:mm:ss tt}.";
                    return false;
                }
            }

            error = string.Empty;
            return true;
        }

        public static void MarkSyncSuccess()
        {
            lock (SyncStateLock)
            {
                _syncSuspendedUntil = null;
            }
        }

        public static void MarkSyncFailure()
        {
            lock (SyncStateLock)
            {
                _syncSuspendedUntil = DateTime.Now.AddSeconds(30);
            }
        }

        public static bool TryCreateDatabase(out IMongoDatabase? database, out string error)
        {
            database = null;
            error = string.Empty;

            MongoSettings mongoSettings = AppConfig.GetMongoSettings();

            if (string.IsNullOrWhiteSpace(mongoSettings.ConnectionString))
            {
                error = "MongoDB is not configured.";
                return false;
            }

            try
            {
                MongoClientSettings clientSettings = MongoClientSettings.FromConnectionString(mongoSettings.ConnectionString);
                clientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(2);
                clientSettings.ConnectTimeout = TimeSpan.FromSeconds(2);
                clientSettings.SocketTimeout = TimeSpan.FromSeconds(5);

                var client = new MongoClient(clientSettings);
                database = client.GetDatabase(mongoSettings.DatabaseName);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}
