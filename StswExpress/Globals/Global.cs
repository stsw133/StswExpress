using StswExpress.Models;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace StswExpress.Globals
{
    public static class Global
    {
        /// App version
        public static string AppVersion()
        {
            int major = Assembly.GetEntryAssembly().GetName().Version.Major,
                minor = Assembly.GetEntryAssembly().GetName().Version.Minor,
                build = Assembly.GetEntryAssembly().GetName().Version.Build,
                revis = Assembly.GetEntryAssembly().GetName().Version.Revision;

            if (build > 0)
                return $"{major}.{minor}.{build}.{revis}";
            else if (revis > 0)
                return $"{major}.{minor}.{build}";
            else if (minor > 0)
                return $"{major}.{minor}";
            else
                return $"{major}";
        }

        /// App name + version
        public static string AppName => $"{Assembly.GetEntryAssembly().GetName().Name} {AppVersion()}";

        /// Chosen database
        public static M_Database AppDatabase { get; set; } = new M_Database();

        /// Logged user
        public static M_User AppUser { get; set; } = new M_User();

        #region Crypto
        public static string Encrypt(string inputString)
        {
            if (inputString.Length == 0)
                return string.Empty;

            var saltStringBytes = Generate256BitsOfRandomEntropy();
            var ivStringBytes = Generate256BitsOfRandomEntropy();
            var plainTextBytes = Encoding.UTF8.GetBytes(inputString);
            using (var password = new Rfc2898DeriveBytes(Properties.HashKey, saltStringBytes, 1000))
            {
                var keyBytes = password.GetBytes(16);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 128;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                var cipherTextBytes = saltStringBytes;
                                cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }

        public static string Decrypt(string inputString)
        {
            if (inputString.Length == 0)
                return string.Empty;

            var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(inputString);
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(16).ToArray();
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(16).Take(16).ToArray();
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip(16 * 2).Take(cipherTextBytesWithSaltAndIv.Length - (16 * 2)).ToArray();
            using (var password = new Rfc2898DeriveBytes(Properties.HashKey, saltStringBytes, 1000))
            {
                var keyBytes = password.GetBytes(16);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 128;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                var plainTextBytes = new byte[cipherTextBytes.Length];
                                var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                            }
                        }
                    }
                }
            }
        }

        private static byte[] Generate256BitsOfRandomEntropy()
        {
            var randomBytes = new byte[16];
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }

        public static string Hash(string inputString)
        {
            byte[] data = Encoding.ASCII.GetBytes(inputString);
            data = new SHA256Managed().ComputeHash(data);
            return Encoding.ASCII.GetString(data);
        }
        #endregion

        #region Proxy
        public class BindingProxy : Freezable
        {
            protected override Freezable CreateInstanceCore()
            {
                return new BindingProxy();
            }

            public object Data
            {
                get { return (object)GetValue(DataProperty); }
                set { SetValue(DataProperty, value); }
            }

            public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));
        }
        #endregion
    }
}
