using System;
using MongoDB.Bson.Serialization.Attributes;

namespace BarterPOS.Models
{
    public class AuditTrailEntry
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Actor { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string TerminalId { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public bool IsSynced { get; set; }
        public DateTime? SyncedAt { get; set; }
        public string SyncError { get; set; } = string.Empty;

        [BsonIgnore]
        public string TimestampDisplay => Timestamp.ToString("MMM dd, yyyy h:mm tt");

        [BsonIgnore]
        public string DisplayText
        {
            get
            {
                string entity = string.IsNullOrWhiteSpace(EntityType)
                    ? string.Empty
                    : string.IsNullOrWhiteSpace(EntityId)
                        ? EntityType
                        : $"{EntityType} #{EntityId}";

                string detail = string.IsNullOrWhiteSpace(Details)
                    ? string.Empty
                    : $" - {Details}";

                string actor = string.IsNullOrWhiteSpace(Actor) ? "Unknown" : Actor;

                return string.IsNullOrWhiteSpace(entity)
                    ? $"{TimestampDisplay} - {Action} by {actor}{detail}"
                    : $"{TimestampDisplay} - {Action} on {entity} by {actor}{detail}";
            }
        }
    }
}