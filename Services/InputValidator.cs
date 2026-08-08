using System.Linq;
using System.Net.Mail;

namespace BarterPOS.Services
{
    internal static class InputValidator
    {
        public static bool IsValidEmployeeId(string employeeId) =>
            !string.IsNullOrWhiteSpace(employeeId)
            && employeeId.Length >= 3
            && employeeId.Length <= 20
            && employeeId.All(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_');

        public static bool IsValidPersonName(string name) =>
            !string.IsNullOrWhiteSpace(name)
            && name.Trim().Length >= 2
            && name.Trim().Length <= 80;

        public static bool IsValidUsername(string username) =>
            !string.IsNullOrWhiteSpace(username)
            && username.Length >= 3
            && username.Length <= 30
            && username.All(ch => char.IsLetterOrDigit(ch) || ch == '_' || ch == '.' || ch == '-');

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return true;
            }

            try
            {
                _ = new MailAddress(email.Trim());
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidContactNumber(string contactNumber)
        {
            if (string.IsNullOrWhiteSpace(contactNumber))
            {
                return true;
            }

            string cleaned = new(contactNumber.Where(ch => !char.IsWhiteSpace(ch) && ch != '-' && ch != '(' && ch != ')').ToArray());
            return cleaned.Length is >= 7 and <= 15
                && cleaned.All(ch => char.IsDigit(ch) || ch == '+')
                && cleaned.Count(ch => ch == '+') <= 1
                && (cleaned.IndexOf('+') is -1 or 0);
        }
    }
}
