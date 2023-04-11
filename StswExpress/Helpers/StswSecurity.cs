using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace StswExpress;

/// <summary>
/// Provides methods for encryption and decryption of text.
/// </summary>
public static class StswSecurity
{
    private static string? key = null;
    /// <summary>
    /// Property to set the encryption key, which must be at least 16 characters long.
    /// </summary>
    [MinLength(16)]
    public static string Key { set => key = value; }

    /// <summary>
    /// Gets hashed <see cref="byte"/>[] using SHA256 algorithm.
    /// </summary>
    /// <param name="text">Text to hash.</param>
    /// <returns>Hashed text.</returns>
    public static byte[] GetHash(string text)
    {
        using (HashAlgorithm algorithm = SHA256.Create())
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(text));
    }

    /// <summary>
    /// Gets hashed <see cref="string"/> using SHA256 algorithm.
    /// </summary>
    /// <param name="text">Text to hash.</param>
    /// <returns>Hashed text.</returns>
    public static string GetHashString(string text)
    {
        var sb = new StringBuilder();
        foreach (byte b in GetHash(text))
            sb.Append(b.ToString("X2"));
        return sb.ToString();
    }

    /// <summary>
    /// Encrypts <see cref="string"/> using AES.
    /// </summary>
    /// <param name="text">Text to encrypt.</param>
    /// <returns>Encrypted text.</returns>
    /// <exception cref="ArgumentNullException"/>
    public static string Encrypt(string text)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(Key));

        if (string.IsNullOrEmpty(text))
            return text;

        var iv = new byte[16];
        byte[] array;

        using (var aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = iv;

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using (var memoryStream = new MemoryStream())
            using (var scryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            {
                using (var streamWriter = new StreamWriter(scryptoStream))
                    streamWriter.Write(text);

                array = memoryStream.ToArray();
            }
        }

        return Convert.ToBase64String(array);
    }

    /// <summary>
    /// Decrypts <see cref="string"/> using AES.
    /// </summary>
    /// <param name="text">Text to decrypt.</param>
    /// <returns>Decrypted text.</returns>
    /// <exception cref="ArgumentNullException"/>
    public static string Decrypt(string text)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(Key));

        if (string.IsNullOrEmpty(text))
            return text;

        var iv = new byte[16];
        var buffer = Convert.FromBase64String(text);

        using (var aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = iv;

            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using (var memoryStream = new MemoryStream(buffer))
            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
            {
                using (var streamReader = new StreamReader(cryptoStream))
                    return streamReader.ReadToEnd();
            }
        }
    }
}
