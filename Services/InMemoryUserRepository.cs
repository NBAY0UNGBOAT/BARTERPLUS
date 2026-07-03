using System;
using System.Collections.Generic;
using System.Linq;
using BarterPOS.Models;

namespace BarterPOS.Services
{
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users = new();
        private readonly List<AuditLogEntry> _auditLog = new();
        private int _nextUserId = 1;
        private int _nextAuditId = 1;

        public InMemoryUserRepository()
        {
            // Seed a default admin so the app is usable before Register/DB work lands.
            // Username: admin / Password: admin123
            _users.Add(new User
            {
                Id = _nextUserId++,
                EmployeeID = "EMP-0001",
                FullName = "System Administrator",
                Email = "admin@barterplus.local",
                Username = "admin",
                PasswordHash = PasswordHasher.Hash("admin123"),
                Role = "Admin",
                IsActive = true
            });
        }

        public List<User> GetAllUsers() => _users.ToList();

        public User? GetByUsername(string username) =>
            _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

        public User? GetById(int id) =>
            _users.FirstOrDefault(u => u.Id == id);

        public bool Register(User newUser, string plainPassword, out string error)
        {
            error = string.Empty;

            if (GetByUsername(newUser.Username) != null)
            {
                error = "That username is already taken.";
                return false;
            }

            newUser.Id = _nextUserId++;
            newUser.PasswordHash = PasswordHasher.Hash(plainPassword);
            newUser.IsActive = true;
            newUser.CreatedAt = DateTime.Now;

            _users.Add(newUser);
            return true;
        }

        public bool ValidateCredentials(string username, string plainPassword, out User? user, out string error)
        {
            error = string.Empty;
            user = GetByUsername(username);

            if (user == null || !PasswordHasher.Verify(plainPassword, user.PasswordHash))
            {
                error = "Invalid username or password.";
                user = null;
                return false;
            }

            if (!user.IsActive)
            {
                error = "This account has been deactivated. Please contact an administrator.";
                user = null;
                return false;
            }

            return true;
        }

        public bool SetActiveStatus(int userId, bool isActive, string performedBy, out string error)
        {
            error = string.Empty;
            var user = GetById(userId);

            if (user == null)
            {
                error = "User not found.";
                return false;
            }

            user.IsActive = isActive;

            if (isActive)
            {
                user.ReactivatedAt = DateTime.Now;
                user.ReactivatedBy = performedBy;
            }
            else
            {
                user.DeactivatedAt = DateTime.Now;
                user.DeactivatedBy = performedBy;
            }

            _auditLog.Add(new AuditLogEntry
            {
                Id = _nextAuditId++,
                TargetUserId = user.Id,
                TargetUsername = user.Username,
                Action = isActive ? "Activated" : "Deactivated",
                PerformedBy = performedBy,
                Timestamp = DateTime.Now
            });

            return true;
        }

        public List<AuditLogEntry> GetAuditLog() =>
            _auditLog.OrderByDescending(a => a.Timestamp).ToList();
    }
}
