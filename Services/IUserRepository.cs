using System.Collections.Generic;
using BarterPOS.Models;

namespace BarterPOS.Services
{
    public interface IUserRepository
    {
        List<User> GetAllUsers();
        User? GetByUsername(string username);
        User? GetById(int id);
        string GetNextEmployeeId();

        bool Register(User newUser, string plainPassword, out string error);
        bool ValidateCredentials(string username, string plainPassword, out User? user, out string error);

        bool SetActiveStatus(int userId, bool isActive, string performedBy, out string error);
        bool ChangeRole(int userId, string role, string performedBy, out string error);
        bool ResetPassword(int userId, string newPassword, string performedBy, out string error);
        bool UpdateUser(User updatedUser, string performedBy, out string error);
        bool DeleteUser(int userId, string performedBy, out string error);

        List<AuditLogEntry> GetAuditLog();
        List<AuditLogEntry> GetAuditLogForUser(int userId);
    }
}
