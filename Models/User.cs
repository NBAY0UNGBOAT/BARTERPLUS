using System;
using MongoDB.Bson.Serialization.Attributes;

namespace BarterPOS.Models
{
    public class User
    {
        public int Id { get; set; }
        public string EmployeeID { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string UsernameKey { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // Admin, Manager, Employee, Cashier

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastLoginAt { get; set; }
        public string LastActivity { get; set; } = "No activity yet";
        public DateTime? DeactivatedAt { get; set; }
        public string? DeactivatedBy { get; set; }
        public DateTime? ReactivatedAt { get; set; }
        public string? ReactivatedBy { get; set; }

        [BsonIgnore]
        public string StatusDisplay => IsActive ? "Active" : "Inactive";

        [BsonIgnore]
        public string LastLoginDisplay => LastLoginAt?.ToString("MMM dd, yyyy h:mm tt") ?? "Never";
    }
}
