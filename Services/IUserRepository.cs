using System.Collections.Generic;
using BarterPOS.Models;

namespace BarterPOS.Services
{
    public interface IUserRepository
    {
        List<User> GetAllUsers();
        User? GetByUsername(string username);
        User? GetById(int id);

        bool Register(User newUser, string plainPassword, out string error);
        bool ValidateCredentials(string username, string plainPassword, out User? user, out string error);

        bool SetActiveStatus(int userId, bool isActive, string performedBy, out string error);

        List<AuditLogEntry> GetAuditLog();
    }
}
