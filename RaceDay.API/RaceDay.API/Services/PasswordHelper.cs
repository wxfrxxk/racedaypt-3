using System.Security.Cryptography;

namespace RaceDay.API.Services
{
    // PBKDF2 (SHA-256, 100 000 iterations) hashing used by the seeder.
    // Format matches AuthController: base64(salt):base64(hash)
    public static class PasswordHelper
    {
        public static string Hash(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);
            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }
    }
}
