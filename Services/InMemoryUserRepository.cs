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

            newUser.Username = newUser.Username.Trim();
            newUser.UsernameKey = newUser.Username.ToLowerInvariant();
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

            user.LastLoginAt = DateTime.Now;
            user.LastActivity = "Logged In";
            AddAuditLog(user, "Logged In", user.Username);

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

            user.LastActivity = isActive ? "Account Activated" : "Account Disabled";
            AddAuditLog(user, isActive ? "Activated Account" : "Disabled Account", performedBy);

            return true;
        }

        public bool ChangeRole(int userId, string role, string performedBy, out string error)
        {
            error = string.Empty;
            var user = GetById(userId);

            if (user == null)
            {
                error = "User not found.";
                return false;
            }

            user.Role = role;
            user.LastActivity = $"Role changed to {role}";
            AddAuditLog(user, $"Changed Role to {role}", performedBy);
            return true;
        }

        public bool ResetPassword(int userId, string newPassword, string performedBy, out string error)
        {
            error = string.Empty;
            var user = GetById(userId);

            if (user == null)
            {
                error = "User not found.";
                return false;
            }

            user.PasswordHash = PasswordHasher.Hash(newPassword);
            user.LastActivity = "Password Reset";
            AddAuditLog(user, "Reset Password", performedBy);
            return true;
        }

        public bool UpdateUser(User updatedUser, string performedBy, out string error)
        {
            error = string.Empty;
            var user = GetById(updatedUser.Id);

            if (user == null)
            {
                error = "User not found.";
                return false;
            }

            user.EmployeeID = updatedUser.EmployeeID.Trim();
            user.FullName = updatedUser.FullName.Trim();
            user.Email = updatedUser.Email.Trim();
            user.ContactNumber = updatedUser.ContactNumber.Trim();
            user.LastActivity = "Account Information Updated";
            AddAuditLog(user, "Updated Account Information", performedBy);
            return true;
        }

        public List<AuditLogEntry> GetAuditLog() =>
            _auditLog.OrderByDescending(a => a.Timestamp).ToList();

        public List<AuditLogEntry> GetAuditLogForUser(int userId) =>
            _auditLog.Where(a => a.TargetUserId == userId)
                .OrderByDescending(a => a.Timestamp)
                .ToList();

        private void AddAuditLog(User user, string action, string performedBy)
        {
            _auditLog.Add(new AuditLogEntry
            {
                Id = _nextAuditId++,
                TargetUserId = user.Id,
                TargetUsername = user.Username,
                Action = action,
                PerformedBy = performedBy,
                Timestamp = DateTime.Now
            });
        }
    }
}
