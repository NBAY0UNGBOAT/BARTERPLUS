using System.Security.Cryptography;
using System.Text;

namespace BarterPOS.Services
{
    public static class PasswordHasher
    {
        public static string Hash(string plainPassword)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(plainPassword);
            byte[] hash = sha256.ComputeHash(bytes);
            return System.Convert.ToBase64String(hash);
        }

        public static bool Verify(string plainPassword, string storedHash)
        {
            return Hash(plainPassword) == storedHash;
        }
    }
}
