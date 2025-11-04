using System;
using System.Security.Cryptography;

namespace CertificationTracker.Helpers
{
    public static class PasswordHasher
    {
        // PBKDF2 settings
        private const int SaltSize = 16; // 128 bit
        private const int KeySize = 32;  // 256 bit
        private const int Iterations = 100_000;

        public static void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
        {
            salt = RandomNumberGenerator.GetBytes(SaltSize);
            using var deriveBytes = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            hash = deriveBytes.GetBytes(KeySize);
        }

        public static bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
        {
            using var deriveBytes = new Rfc2898DeriveBytes(password, storedSalt, Iterations, HashAlgorithmName.SHA256);
            var computed = deriveBytes.GetBytes(KeySize);
            return CryptographicOperations.FixedTimeEquals(computed, storedHash);
        }
    }
}
