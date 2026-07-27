using System;
using System.Collections.Generic;
using System.Linq;
using BarterPOS.Models;
using MongoDB.Driver;

namespace BarterPOS.Services
{
    public class MongoUserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<AuditLogEntry> _auditLog;
        private readonly IMongoCollection<MongoCounter> _counters;

        public MongoUserRepository(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);

            _users = database.GetCollection<User>("users");
            _auditLog = database.GetCollection<AuditLogEntry>("auditLog");
            _counters = database.GetCollection<MongoCounter>("counters");

            EnsureUsernameKeys();
            EnsureIndexes();
            EnsureCounters();
        }

        public List<User> GetAllUsers() =>
            _users.Find(FilterDefinition<User>.Empty)
                .SortBy(u => u.Id)
                .ToList();

        public User? GetByUsername(string username) =>
            _users.Find(u => u.UsernameKey == NormalizeUsername(username) || u.Username == username)
                .FirstOrDefault();

        public User? GetById(int id) =>
            _users.Find(u => u.Id == id)
                .FirstOrDefault();

        public bool Register(User newUser, string plainPassword, out string error)
        {
            error = string.Empty;
            newUser.Username = newUser.Username.Trim();
            newUser.UsernameKey = NormalizeUsername(newUser.Username);

            if (GetByUsername(newUser.Username) != null)
            {
                error = "That username is already taken.";
                return false;
            }

            newUser.Id = GetNextId("users");
            newUser.PasswordHash = PasswordHasher.Hash(plainPassword);
            newUser.IsActive = true;
            newUser.CreatedAt = DateTime.Now;

            try
            {
                _users.InsertOne(newUser);
                return true;
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                error = "That username is already taken.";
                return false;
            }
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

            int userId = user.Id;
            DateTime loginTime = DateTime.Now;

            _users.UpdateOne(u => u.Id == userId, Builders<User>.Update
                .Set(u => u.LastLoginAt, loginTime)
                .Set(u => u.LastActivity, "Logged In"));
            AddAuditLog(user, "Logged In", user.Username);
            user.LastLoginAt = loginTime;
            user.LastActivity = "Logged In";

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

            var update = Builders<User>.Update
                .Set(u => u.IsActive, isActive)
                .Set(u => u.LastActivity, isActive ? "Account Activated" : "Account Disabled");

            if (isActive)
            {
                update = update
                    .Set(u => u.ReactivatedAt, DateTime.Now)
                    .Set(u => u.ReactivatedBy, performedBy);
            }
            else
            {
                update = update
                    .Set(u => u.DeactivatedAt, DateTime.Now)
                    .Set(u => u.DeactivatedBy, performedBy);
            }

            _users.UpdateOne(u => u.Id == userId, update);

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

            _users.UpdateOne(u => u.Id == userId, Builders<User>.Update
                .Set(u => u.Role, role)
                .Set(u => u.LastActivity, $"Role changed to {role}"));
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

            _users.UpdateOne(u => u.Id == userId, Builders<User>.Update
                .Set(u => u.PasswordHash, PasswordHasher.Hash(newPassword))
                .Set(u => u.LastActivity, "Password Reset"));
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

            _users.UpdateOne(u => u.Id == updatedUser.Id, Builders<User>.Update
                .Set(u => u.EmployeeID, updatedUser.EmployeeID.Trim())
                .Set(u => u.FullName, updatedUser.FullName.Trim())
                .Set(u => u.Email, updatedUser.Email.Trim())
                .Set(u => u.ContactNumber, updatedUser.ContactNumber.Trim())
                .Set(u => u.LastActivity, "Account Information Updated"));
            AddAuditLog(user, "Updated Account Information", performedBy);
            return true;
        }

        public List<AuditLogEntry> GetAuditLog() =>
            _auditLog.Find(FilterDefinition<AuditLogEntry>.Empty)
                .SortByDescending(a => a.Timestamp)
                .ToList();

        public List<AuditLogEntry> GetAuditLogForUser(int userId) =>
            _auditLog.Find(a => a.TargetUserId == userId)
                .SortByDescending(a => a.Timestamp)
                .ToList();

        private void EnsureIndexes()
        {
            var usernameIndex = new CreateIndexModel<User>(
                Builders<User>.IndexKeys.Ascending(u => u.UsernameKey),
                new CreateIndexOptions { Unique = true });

            _users.Indexes.CreateOne(usernameIndex);
            _auditLog.Indexes.CreateOne(new CreateIndexModel<AuditLogEntry>(
                Builders<AuditLogEntry>.IndexKeys.Descending(a => a.Timestamp)));
        }

        private void EnsureUsernameKeys()
        {
            var usersMissingKey = _users.Find(u => u.UsernameKey == null || u.UsernameKey == string.Empty).ToList();

            foreach (var user in usersMissingKey)
            {
                _users.UpdateOne(
                    u => u.Id == user.Id,
                    Builders<User>.Update.Set(u => u.UsernameKey, NormalizeUsername(user.Username)));
            }
        }

        private void EnsureCounters()
        {
            EnsureCounterAtLeast("users", GetMaxUserId());
            EnsureCounterAtLeast("auditLog", GetMaxAuditLogId());
        }

        private int GetNextId(string name)
        {
            var updatedCounter = _counters.FindOneAndUpdate(
                c => c.Id == name,
                Builders<MongoCounter>.Update.Inc(c => c.Value, 1),
                new FindOneAndUpdateOptions<MongoCounter>
                {
                    IsUpsert = true,
                    ReturnDocument = ReturnDocument.After
                });

            return updatedCounter.Value;
        }

        private void EnsureCounterAtLeast(string name, int minimumValue)
        {
            _counters.UpdateOne(
                c => c.Id == name,
                Builders<MongoCounter>.Update.Max(c => c.Value, minimumValue),
                new UpdateOptions { IsUpsert = true });
        }

        private int GetMaxUserId() =>
            _users.Find(FilterDefinition<User>.Empty)
                .SortByDescending(u => u.Id)
                .Limit(1)
                .ToList()
                .FirstOrDefault()?.Id ?? 0;

        private int GetMaxAuditLogId() =>
            _auditLog.Find(FilterDefinition<AuditLogEntry>.Empty)
                .SortByDescending(a => a.Id)
                .Limit(1)
                .ToList()
                .FirstOrDefault()?.Id ?? 0;

        private static string NormalizeUsername(string username) =>
            username.Trim().ToLowerInvariant();

        private void AddAuditLog(User user, string action, string performedBy)
        {
            _auditLog.InsertOne(new AuditLogEntry
            {
                Id = GetNextId("auditLog"),
                TargetUserId = user.Id,
                TargetUsername = user.Username,
                Action = action,
                PerformedBy = performedBy,
                Timestamp = DateTime.Now
            });
        }
    }
}
