using System;
using System.Security.Cryptography;
using System.Text;

namespace Core.Entities
{
    public class Password
    {
        public int UserId { get; set; }

        public string Cipher { get; set; }
        public string Encrypted { get; set; }

        public Password()
        {
            Cipher = string.Empty;
            Encrypted = string.Empty;
        }

        public static string Hash(string password, string cipher)
        {
            int keySize = 128;
            int iterations = 350000;
            var hashAlgorithm = HashAlgorithmName.SHA512;

            var hash = Rfc2898DeriveBytes.Pbkdf2(
               Encoding.UTF8.GetBytes(password),
               Convert.FromHexString(cipher),
               iterations,
               hashAlgorithm,
               keySize
            );

            return Convert.ToHexString(hash!);
        }

        public static string Hash(string password, out byte[] cipher)
        {
            int keySize = 128;
            int iterations = 350000;
            var hashAlgorithm = HashAlgorithmName.SHA512;
            cipher = RandomNumberGenerator.GetBytes(keySize);

            var hash = Rfc2898DeriveBytes.Pbkdf2(
               Encoding.UTF8.GetBytes(password),
               cipher,
               iterations,
               hashAlgorithm,
               keySize
            );

            return Convert.ToHexString(hash!);
        }
    }
}
