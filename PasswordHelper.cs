using System;
using System.Security.Cryptography;

namespace WebApplicationM3
{
    public static class PasswordHelper
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100000;

        public static string Hash(string password)
        {
            byte[] salt = new byte[SaltSize];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(salt);

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations))
            {
                byte[] hash = pbkdf2.GetBytes(HashSize);
                byte[] result = new byte[SaltSize + HashSize];
                Buffer.BlockCopy(salt, 0, result, 0, SaltSize);
                Buffer.BlockCopy(hash, 0, result, SaltSize, HashSize);
                return Convert.ToBase64String(result);
            }
        }

        public static bool Verify(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash)) return false;

            byte[] data;
            try { data = Convert.FromBase64String(storedHash); }
            catch { return false; }

            if (data.Length != SaltSize + HashSize) return false;

            byte[] salt = new byte[SaltSize];
            Buffer.BlockCopy(data, 0, salt, 0, SaltSize);

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations))
            {
                byte[] hash = pbkdf2.GetBytes(HashSize);
                int diff = 0;
                for (int i = 0; i < HashSize; i++)
                    diff |= hash[i] ^ data[SaltSize + i];
                return diff == 0;
            }
        }
    }
}