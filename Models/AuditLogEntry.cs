using System;

namespace BarterPOS.Models
{
    public class AuditLogEntry
    {
        public int Id { get; set; }
        public int TargetUserId { get; set; }
        public string TargetUsername { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // "Activated" or "Deactivated"
        public string PerformedBy { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public string DisplayText =>
            $"{Timestamp:yyyy-MM-dd HH:mm} - {TargetUsername} was {Action} by {PerformedBy}";
    }
}
