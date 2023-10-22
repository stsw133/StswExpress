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
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
    }

    /// <summary>
    /// Gets hashed <see cref="string"/> using SHA256 algorithm.
    /// </summary>
    /// <param name="text">Text to hash.</param>
    /// <returns>Hashed text.</returns>
    public static string GetHashString(string text)
    {
        using var sha256 = SHA256.Create();
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
        return BitConverter.ToString(bytes).Replace("-", string.Empty);
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

        using var aesAlg = Aes.Create();
        aesAlg.Key = Encoding.UTF8.GetBytes(key);
        aesAlg.IV = new byte[16];
        var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        using var msEncrypt = new MemoryStream();
        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        {
            using var swEncrypt = new StreamWriter(csEncrypt);
            swEncrypt.Write(text);
        }
        return Convert.ToBase64String(msEncrypt.ToArray());
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

        using var aesAlg = Aes.Create();
        aesAlg.Key = Encoding.UTF8.GetBytes(key);
        aesAlg.IV = new byte[16]; // Initialization Vector
        var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        using var msDecrypt = new MemoryStream(Convert.FromBase64String(text));
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);
        return srDecrypt.ReadToEnd();
    }
}
