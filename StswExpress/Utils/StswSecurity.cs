﻿using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace StswExpress;
/// <summary>
/// Provides methods for encryption and decryption of text as well as hashing and secure string conversions.
/// </summary>
public static class StswSecurity
{
    /// <summary>
    /// Sets the encryption key, which must be at least 16 characters long.
    /// </summary>
    [MinLength(16)]
    public static string Key
    {
        set
        {
            if (value.Length < 16)
                throw new ArgumentException("The key must be at least 16 characters long.");
            key = value;
        }
    }
    private static string? key = "".PadRight(16, StswFn.AppName() ?? " ");

    /// <summary>
    /// Computes the hash of data using the specified hashing algorithm.
    /// </summary>
    /// <param name="algorithmFactory">A factory function to create the hashing algorithm instance.</param>
    /// <param name="source">The data to hash.</param>
    /// <returns>The hash of the data.</returns>
    public static byte[] ComputeHash(Func<HashAlgorithm> algorithmFactory, byte[] source)
    {
        ArgumentNullException.ThrowIfNull(algorithmFactory);
        ArgumentNullException.ThrowIfNull(source);

        using var algorithm = algorithmFactory();
        return algorithm.ComputeHash(source);
    }

    /// <summary>
    /// Gets a hashed byte array using the specified hashing algorithm.
    /// </summary>
    /// <param name="algorithmFactory">A factory function to create the hashing algorithm instance.</param>
    /// <param name="text">The text to hash.</param>
    /// <returns>A byte array containing the hashed text.</returns>
    public static byte[] GetHash(Func<HashAlgorithm> algorithmFactory, string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        return ComputeHash(algorithmFactory, Encoding.UTF8.GetBytes(text));
    }

    /// <summary>
    /// Gets a hashed string using the specified hashing algorithm.
    /// </summary>
    /// <param name="algorithmFactory">A factory function to create the hashing algorithm instance.</param>
    /// <param name="text">The text to hash.</param>
    /// <returns>A string containing the hashed text.</returns>
    public static string GetHashString(Func<HashAlgorithm> algorithmFactory, string text)
    {
        var hash = GetHash(algorithmFactory, text);
        return BitConverter.ToString(hash).Replace("-", string.Empty);
    }

    /// <summary>
    /// Gets a hashed byte array using the SHA256 algorithm.
    /// </summary>
    /// <param name="text">The text to hash.</param>
    /// <returns>A byte array containing the hashed text.</returns>
    public static byte[] GetHash(string text) => SHA256.HashData(Encoding.UTF8.GetBytes(text));

    /// <summary>
    /// Gets a hashed string using the SHA256 algorithm.
    /// </summary>
    /// <param name="text">The text to hash.</param>
    /// <returns>A string containing the hashed text.</returns>
    public static string GetHashString(string text) => BitConverter.ToString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).Replace("-", string.Empty);

    /// <summary>
    /// Encrypts a string using AES encryption.
    /// </summary>
    /// <param name="text">The text to encrypt.</param>
    /// <returns>The encrypted text as a Base64 string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the encryption key is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the encryption key is less than 16 characters long.</exception>
    public static string Encrypt(string text)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(Key), "The encryption key cannot be null.");
        if (key.Length < 16)
            throw new ArgumentException("The encryption key must be at least 16 characters long.", nameof(Key));

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
    /// Decrypts a string using AES encryption.
    /// </summary>
    /// <param name="text">The text to decrypt.</param>
    /// <returns>The decrypted text.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the encryption key is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the encryption key is less than 16 characters long.</exception>
    public static string Decrypt(string text)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(Key), "The encryption key cannot be null.");
        if (key.Length < 16)
            throw new ArgumentException("The encryption key must be at least 16 characters long.", nameof(Key));

        using var aesAlg = Aes.Create();
        aesAlg.Key = Encoding.UTF8.GetBytes(key);
        aesAlg.IV = new byte[16];
        var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        using var msDecrypt = new MemoryStream(Convert.FromBase64String(text));
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);
        return srDecrypt.ReadToEnd();
    }

    /// <summary>
    /// Converts a string to a SecureString.
    /// </summary>
    /// <param name="text">The text to secure.</param>
    /// <returns>A SecureString containing the text.</returns>
    public static SecureString GetSecureString(string text)
    {
        if (text == null)
            throw new ArgumentNullException(nameof(text), "The text to secure cannot be null.");

        var secureString = new SecureString();
        foreach (char c in text)
            secureString.AppendChar(c);

        secureString.MakeReadOnly();
        return secureString;
    }

    /// <summary>
    /// Generates a random token of the specified length.
    /// </summary>
    /// <param name="length">The length of the token to generate.</param>
    /// <returns>A randomly generated token.</returns>
    public static string GenerateRandomToken(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^*()-_=+[]{};:,.?/";
        var token = new char[length];
        var data = new byte[length];

        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(data);

        for (int i = 0; i < token.Length; i++)
        {
            var rnd = data[i] % chars.Length;
            token[i] = chars[rnd];
        }

        return new string(token);
    }

    /// <summary>
    /// Validates the strength of a password based on given criteria.
    /// </summary>
    /// <param name="password">The password to validate.</param>
    /// <param name="minLength">The minimum length of the password.</param>
    /// <param name="requireUppercase">Whether the password must contain uppercase letters.</param>
    /// <param name="requireLowercase">Whether the password must contain lowercase letters.</param>
    /// <param name="requireDigit">Whether the password must contain digits.</param>
    /// <param name="requireSpecialChar">Whether the password must contain special characters.</param>
    /// <returns>True if the password meets the specified criteria; otherwise, false.</returns>
    public static bool ValidatePasswordStrength(string password, int minLength = 8, bool requireUppercase = true, bool requireLowercase = true, bool requireDigit = true, bool requireSpecialChar = true)
    {
        if (password.Length < minLength)
            return false;

        if (requireUppercase && !password.Any(char.IsUpper))
            return false;

        if (requireLowercase && !password.Any(char.IsLower))
            return false;

        if (requireDigit && !password.Any(char.IsDigit))
            return false;

        if (requireSpecialChar && !password.Any(ch => !char.IsLetterOrDigit(ch)))
            return false;

        return true;
    }
}

/* usage:

var sha256Hash = StswSecurity.GetHashString(() => SHA256.Create(), "Hello, World!");

*/
