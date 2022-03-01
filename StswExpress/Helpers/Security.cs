using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace StswExpress
{
    public static class Security
    {
        private static string hashKey = "stsw";
        public static string HashKey { set => hashKey = value; }

        /// New secure string
        public static SecureString NewSecureString(string text) => new NetworkCredential(string.Empty, text).SecurePassword;

        /// Secure string to byte array
        public static byte[] SecureStringToBytea(SecureString text) => Encoding.UTF8.GetBytes(new NetworkCredential(string.Empty, text).Password);

        /// Generate salt
        private static byte[] GenerateSalt()
        {
            var bytes = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(bytes);
            return bytes;
        }

        /// Generate hash
        public static byte[] GenerateHash(SecureString password)
        {
            using var deriveBytes = new Rfc2898DeriveBytes(SecureStringToBytea(password), GenerateSalt(), 1000);
            return deriveBytes.GetBytes(16);
        }

        /// Encrypt text
        public static string Encrypt(string text)
        {
            if (text.Length == 0)
                return string.Empty;

            var saltStringBytes = GenerateSalt();
            var ivStringBytes = GenerateSalt();
            var plainTextBytes = Encoding.UTF8.GetBytes(text);
            using var password = new Rfc2898DeriveBytes(hashKey, saltStringBytes, 1000);
            var keyBytes = password.GetBytes(16);
            using var symmetricKey = new RijndaelManaged();
            symmetricKey.BlockSize = 128;
            symmetricKey.Mode = CipherMode.CBC;
            symmetricKey.Padding = PaddingMode.PKCS7;
            using var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes);
            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            var cipherTextBytes = saltStringBytes;
            cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
            cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
            memoryStream.Close();
            cryptoStream.Close();
            return Convert.ToBase64String(cipherTextBytes);
        }

        /// Decrypt text
        public static string Decrypt(string text)
        {
            if (text.Length == 0)
                return string.Empty;

            var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(text);
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(16).ToArray();
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(16).Take(16).ToArray();
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip(16 * 2).Take(cipherTextBytesWithSaltAndIv.Length - (16 * 2)).ToArray();
            using var password = new Rfc2898DeriveBytes(hashKey, saltStringBytes, 1000);
            var keyBytes = password.GetBytes(16);
            using var symmetricKey = new RijndaelManaged();
            symmetricKey.BlockSize = 128;
            symmetricKey.Mode = CipherMode.CBC;
            symmetricKey.Padding = PaddingMode.PKCS7;
            using var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes);
            using var memoryStream = new MemoryStream(cipherTextBytes);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            var plainTextBytes = new byte[cipherTextBytes.Length];
            var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            memoryStream.Close();
            cryptoStream.Close();
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
        }
    }
}
