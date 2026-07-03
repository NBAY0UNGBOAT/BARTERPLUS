using System;

namespace BarterPOS.Models
{
    public class User
    {
        public int Id { get; set; }
        public string EmployeeID { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // Admin, Manager, Employee, Cashier

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? DeactivatedAt { get; set; }
        public string? DeactivatedBy { get; set; }
        public DateTime? ReactivatedAt { get; set; }
        public string? ReactivatedBy { get; set; }

        public string StatusDisplay => IsActive ? "Active" : "Inactive";
    }
}
