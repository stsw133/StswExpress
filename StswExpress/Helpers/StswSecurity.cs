using System;
using System.IO;
using System.Net;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace StswExpress;

public static class StswSecurity
{
    private static string? hashKey = null;
    public static string HashKey { set => hashKey = value; }

    /// New secure string
    public static SecureString NewSecureString(string text) => new NetworkCredential(string.Empty, text).SecurePassword;

    /// Secure string to byte array
    public static byte[] SecureStringToBytea(SecureString text) => Encoding.UTF8.GetBytes(new NetworkCredential(string.Empty, text).Password);

    /// Generate salt
    private static byte[] GenerateSalt()
    {
        using var generator = RandomNumberGenerator.Create();
        var salt = new byte[16];
        generator.GetBytes(salt);
        return salt;
    }

    /// Generate hash
    public static byte[] GenerateHash(SecureString password)
    {
        using var deriveBytes = new Rfc2898DeriveBytes(SecureStringToBytea(password), new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 }, 1000);
        return deriveBytes.GetBytes(16);
    }

    /// Encrypt text
    public static string Encrypt(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var clearBytes = Encoding.Unicode.GetBytes(text);
        using (var encryptor = Aes.Create())
        {
            var pdb = new Rfc2898DeriveBytes(hashKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            encryptor.Key = pdb.GetBytes(32);
            encryptor.IV = pdb.GetBytes(16);
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(clearBytes, 0, clearBytes.Length);
                    cs.Close();
                }
                text = Convert.ToBase64String(ms.ToArray());
            }
        }
        return text;
    }

    /// Decrypt text
    public static string Decrypt(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        text = text.Replace(" ", "+");
        var cipherBytes = Convert.FromBase64String(text);
        using (var encryptor = Aes.Create())
        {
            var pdb = new Rfc2898DeriveBytes(hashKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            encryptor.Key = pdb.GetBytes(32);
            encryptor.IV = pdb.GetBytes(16);
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(cipherBytes, 0, cipherBytes.Length);
                    cs.Close();
                }
                text = Encoding.Unicode.GetString(ms.ToArray());
            }
        }
        return text;
    }
}
