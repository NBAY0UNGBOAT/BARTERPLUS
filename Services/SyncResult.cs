namespace BarterPOS.Services
{
    public class SyncResult
    {
        public int PendingBeforeSync { get; set; }
        public int SyncedCount { get; set; }
        public int FailedCount { get; set; }
        public string LastError { get; set; } = string.Empty;

        public bool HasFailures => FailedCount > 0;
    }

    public class SaveSyncResult
    {
        public bool IsSynced { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
