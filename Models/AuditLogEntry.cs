using System;
using MongoDB.Bson.Serialization.Attributes;

namespace BarterPOS.Models
{
    public class AuditLogEntry
    {
        public int Id { get; set; }
        public int TargetUserId { get; set; }
        public string TargetUsername { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string PerformedBy { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [BsonIgnore]
        public string TimestampDisplay => Timestamp.ToString("MMM dd, yyyy h:mm tt");

        [BsonIgnore]
        public string DisplayText =>
            $"{TimestampDisplay} - {TargetUsername}: {Action} by {PerformedBy}";
    }
}
