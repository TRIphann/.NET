using System.Security.Cryptography;
using System.Text;

namespace QLDuLichRBAC_Upgrade.Utils
{
    /// <summary>
    /// Helper class cho xác th?c và b?o m?t
    /// </summary>
    public static class AuthHelper
    {
        /// <summary>
        /// Hash m?t kh?u b?ng SHA256
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("M?t kh?u không ???c ?? tr?ng", nameof(password));

            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(bytes).Replace("-", "").ToUpperInvariant();
        }

        /// <summary>
        /// Sanitize input ?? tránh XSS
        /// </summary>
        public static string SanitizeInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            return input
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#x27;")
                .Replace("/", "&#x2F;")
                .Trim();
        }
    }
}
