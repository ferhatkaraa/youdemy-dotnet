using System.Security.Cryptography;
using System.Text;

namespace Youdemy.Helpers
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            return HashPassword(password).Equals(hashedPassword, StringComparison.OrdinalIgnoreCase);
        }
    }
}
