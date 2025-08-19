using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace StswExpress.Commons;
/// <summary>
/// Provides methods for encryption and decryption of text as well as hashing and secure string conversions.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// var password = "SuperSecretPassword123!";
/// var hashedPassword = StswSecurity.GetHashString(password);
/// 
/// StswSecurity.Key = "MySecureEncryptionKey";
/// string secretMessage = "This is secret message";
/// string encryptedMessage = StswSecurity.Encrypt(secretMessage);
/// string decryptedMessage = StswSecurity.Decrypt(encryptedMessage);
/// 
/// var password = "WeakPass1!";
/// var isStrong = StswSecurity.ValidatePasswordStrength(password);
/// 
/// var token = StswSecurity.GenerateRandomToken(32);
/// </code>
/// </example>
[StswInfo(null, "0.20.0")]
public static class StswSecurity
{
    private static byte[]? _manualKey;
    private static byte[]? _manualSalt;

    /// <summary>
    /// Optional: manually set the key (must be at least 16 characters). Overrides the default derived from AppName.
    /// </summary>
    [MinLength(16)]
    public static string Key
    {
        set
        {
            if (value.Length < 16)
                throw new ArgumentException("The key must be at least 16 characters long.", nameof(value));
            _manualKey = Encoding.UTF8.GetBytes(value.PadRight(32)[..32]);
        }
    }

    /// <summary>
    /// Returns the encryption key, derived from AppName or manually set.
    /// </summary>
    [StswInfo("0.20.0")]
    private static byte[] AesKey
    {
        get
        {
            if (_manualKey is not null)
                return _manualKey;

            var appName = StswFn.AppName() ?? "DefaultAppName";
            return SHA256.HashData(Encoding.UTF8.GetBytes(appName));
        }
    }

    /// <summary>
    /// Returns a static salt derived from the application name.
    /// </summary>
    [StswInfo("0.20.0")]
    public static byte[] Salt
    {
        get
        {
            if (_manualSalt is not null)
                return _manualSalt;

            var appName = StswFn.AppName() ?? "DefaultAppName";
            return SHA256.HashData(Encoding.UTF8.GetBytes("Salt_" + appName));
        }
    }

    /// <summary>
    /// Sets a manual salt for hashing operations.
    /// </summary>
    /// <param name="salt">The salt to set, must be at least 16 bytes long.</param>
    /// <exception cref="ArgumentException">Thrown when the salt is less than 16 bytes.</exception>
    [StswInfo("0.20.0")]
    public static void SetSalt(ReadOnlySpan<byte> salt)
    {
        if (salt.Length < 16)
            throw new ArgumentException("Salt must be at least 16 bytes.", nameof(salt));
        _manualSalt = salt.ToArray();
    }

    #region Hashing
    /// <summary>
    /// Computes the hash of data using the specified hashing algorithm.
    /// </summary>
    /// <param name="source">The data to hash.</param>
    /// <param name="algorithmFactory">A factory function to create the hashing algorithm instance.</param>
    /// <returns>The hash of the data.</returns>
    [StswInfo("0.14.0")]
    public static byte[] ComputeHash(byte[] source, Func<HashAlgorithm> algorithmFactory)
    {
        ArgumentNullException.ThrowIfNull(algorithmFactory);
        ArgumentNullException.ThrowIfNull(source);

        using var algorithm = algorithmFactory();
        return algorithm.ComputeHash(source);
    }

    /// <summary>
    /// Computes the hash of data using the SHA256 algorithm.
    /// </summary>
    /// <param name="source">The data to hash.</param>
    /// <returns>The hash of the data.</returns>
    public static byte[] ComputeHash(byte[] source) => ComputeHash(source, SHA256.Create);

    /// <summary>
    /// Gets a hashed byte array using the specified hashing algorithm.
    /// </summary>
    /// <param name="text">The text to hash.</param>
    /// <param name="algorithmFactory">A factory function to create the hashing algorithm instance.</param>
    /// <returns>A byte array containing the hashed text.</returns>
    [StswInfo("0.14.0")]
    public static byte[] GetHash(string text, Func<HashAlgorithm> algorithmFactory)
    {
        ArgumentNullException.ThrowIfNull(text);
        return ComputeHash(Encoding.UTF8.GetBytes(text), algorithmFactory);
    }

    /// <summary>
    /// Gets a hashed byte array using the SHA256 algorithm.
    /// </summary>
    /// <param name="text">The text to hash.</param>
    /// <returns>A byte array containing the hashed text.</returns>
    public static byte[] GetHash(string text) => GetHash(text, SHA256.Create);

    /// <summary>
    /// Gets a hashed string using the specified hashing algorithm.
    /// </summary>
    /// <param name="text">The text to hash.</param>
    /// <param name="algorithmFactory">A factory function to create the hashing algorithm instance.</param>
    /// <returns>A string containing the hashed text.</returns>
    [StswInfo("0.14.0", "0.20.0")]
    public static string GetHashString(string text, Func<HashAlgorithm> algorithmFactory) => Convert.ToHexString(GetHash(text, algorithmFactory));

    /// <summary>
    /// Gets a hashed string using the SHA256 algorithm.
    /// </summary>
    /// <param name="text">The text to hash.</param>
    /// <returns>A byte array containing the hashed text.</returns>
    [StswInfo(null, "0.20.0")]
    public static string GetHashString(string text) => Convert.ToHexString(GetHash(text));

    /// <summary>
    /// Hashes a password using PBKDF2 with a static salt and 100,000 iterations.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>A Base64-encoded string representing the hashed password.</returns>
    public static string HashPassword(string password)
    {
        var pbkdf2 = new Rfc2898DeriveBytes(password, Salt, 100_000, HashAlgorithmName.SHA256);
        return Convert.ToBase64String(pbkdf2.GetBytes(32));
    }

    /// <summary>
    /// Hashes a password using PBKDF2 with a specified number of iterations, salt size, and key size.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <param name="iterations">The number of iterations for the PBKDF2 algorithm.</param>
    /// <param name="saltSize">The size of the salt in bytes.</param>
    /// <param name="keySize">The size of the key in bytes.</param>
    /// <returns>A string containing the hashed password in the format "pbkdf2-sha256$iterations$salt$hash".</returns>
    /// <exception cref="ArgumentNullException">Thrown when the password is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the salt size is less than 16 bytes.</exception>
    [StswInfo("0.20.0")]
    public static string HashPassword(string password, int iterations = 310_000, int saltSize = 16, int keySize = 32)
    {
        ArgumentNullException.ThrowIfNull(password);
        ArgumentOutOfRangeException.ThrowIfLessThan(saltSize, 16);

        Span<byte> salt = stackalloc byte[saltSize];
        RandomNumberGenerator.Fill(salt);

        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, keySize);
        return $"pbkdf2-sha256${iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Verifies a password against a stored hash.
    /// </summary>
    /// <param name="password">The password to verify.</param>
    /// <param name="stored">The stored hash to verify against.</param>
    /// <returns><see langword="true"/> if the password matches the stored hash; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the password or stored hash is <see langword="null"/>.</exception>
    [StswInfo("0.20.0")]
    public static bool VerifyPassword(string password, string stored)
    {
        ArgumentNullException.ThrowIfNull(password);
        ArgumentNullException.ThrowIfNull(stored);

        var parts = stored.Split('$');
        if (parts.Length != 4 || !parts[0].Equals("pbkdf2-sha256", StringComparison.Ordinal))
            return false;

        if (!int.TryParse(parts[1], out var iter) || iter <= 0)
            return false;

        byte[] salt, expected;
        try
        {
            salt = Convert.FromBase64String(parts[2]);
            expected = Convert.FromBase64String(parts[3]);
        }
        catch { return false; }

        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iter, HashAlgorithmName.SHA256, expected.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
    #endregion

    #region Encryption/Decryption
    /// <summary>
    /// Encrypts a string using AES encryption.
    /// </summary>
    /// <param name="text">The text to encrypt.</param>
    /// <returns>The encrypted text as a Base64 string.</returns>
    [StswInfo(null, "0.20.0")]
    public static string Encrypt(string text)
    {
        using var aesAlg = Aes.Create();
        aesAlg.Key = AesKey;
        aesAlg.GenerateIV();

        using var msEncrypt = new MemoryStream();
        msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);

        using var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
        using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        using var swEncrypt = new StreamWriter(csEncrypt);
        swEncrypt.Write(text);
        swEncrypt.Flush();
        csEncrypt.FlushFinalBlock();

        return Convert.ToBase64String(msEncrypt.ToArray());
    }

    /// <summary>
    /// Decrypts a string using AES encryption.
    /// </summary>
    /// <param name="text">The text to decrypt.</param>
    /// <returns>The decrypted text.</returns>
    /// <exception cref="CryptographicException">Thrown if the ciphertext is too short or decryption fails.</exception>
    [StswInfo(null, "0.20.0")]
    public static string Decrypt(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        var fullCipher = Convert.FromBase64String(text);
        if (fullCipher.Length < 16)
            throw new CryptographicException("Ciphertext too short.");

        using var aesAlg = Aes.Create();
        aesAlg.Key = AesKey;

        var iv = new byte[16];
        Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
        aesAlg.IV = iv;

        using var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
        using var msDecrypt = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);
        return srDecrypt.ReadToEnd();
    }

    private const int NonceSize = 12;  // 96-bit nonce
    private const int TagSize = 16;  // 128-bit tag

    /// <summary>
    /// Encrypts a string using AES-GCM encryption.
    /// </summary>
    /// <param name="plaintext">The plaintext to encrypt.</param>
    /// <returns>The encrypted text as a Base64 string in the format nonce|cipher|tag.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the plaintext is null.</exception>"
    [StswInfo("0.20.0")]
    public static string EncryptGcm(string plaintext)
    {
        ArgumentNullException.ThrowIfNull(plaintext);
        var key = AesKey;

        Span<byte> nonce = stackalloc byte[NonceSize];
        RandomNumberGenerator.Fill(nonce);

        var plainBytes = Encoding.UTF8.GetBytes(plaintext);
        var cipher = new byte[plainBytes.Length];
        Span<byte> tag = stackalloc byte[TagSize];

        using var gcm = new AesGcm(key, TagSize);
        gcm.Encrypt(nonce, plainBytes, cipher, tag);

        var output = new byte[nonce.Length + cipher.Length + tag.Length];
        Buffer.BlockCopy(nonce.ToArray(), 0, output, 0, nonce.Length);
        Buffer.BlockCopy(cipher, 0, output, nonce.Length, cipher.Length);
        Buffer.BlockCopy(tag.ToArray(), 0, output, nonce.Length + cipher.Length, tag.Length);

        return Convert.ToBase64String(output);
    }

    /// <summary>
    /// Decrypts a string using AES-GCM encryption.
    /// </summary>
    /// <param name="b64">The Base64-encoded string to decrypt, in the format nonce|cipher|tag.</param>
    /// <returns>The decrypted plaintext.</returns>
    /// <exception cref="CryptographicException">Thrown if the ciphertext is too short or decryption fails.</exception>
    [StswInfo("0.20.0")]
    public static string DecryptGcm(string b64)
    {
        if (string.IsNullOrEmpty(b64))
            return string.Empty;

        var data = Convert.FromBase64String(b64);
        if (data.Length < NonceSize + TagSize)
            throw new CryptographicException("Ciphertext too short.");

        var key = AesKey;

        var nonce = new ReadOnlySpan<byte>(data, 0, NonceSize);
        var tag = new ReadOnlySpan<byte>(data, data.Length - TagSize, TagSize);
        var cipher = new ReadOnlySpan<byte>(data, NonceSize, data.Length - NonceSize - TagSize);

        var plain = new byte[cipher.Length];
        using var gcm = new AesGcm(key, TagSize);
        gcm.Decrypt(nonce, cipher, tag, plain);

        return Encoding.UTF8.GetString(plain);
    }
    #endregion

    /// <summary>
    /// Generates a random token of the specified length.
    /// </summary>
    /// <param name="length">The length of the token to generate.</param>
    /// <returns>A randomly generated token.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the length is less than or equal to zero.</exception>
    [StswInfo("0.9.0", "0.20.0")]
    public static string GenerateRandomToken(int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);

        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^*()-_=+[]{};:,.?/";
        var result = new char[length];

        var n = chars.Length;
        var threshold = 256 / n * n;

        Span<byte> buffer = stackalloc byte[1];
        var i = 0;
        using var rng = RandomNumberGenerator.Create();

        while (i < length)
        {
            rng.GetBytes(buffer);
            byte b = buffer[0];
            if (b >= threshold) continue;
            result[i++] = chars[b % n];
        }

        return new string(result);
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
    /// <returns><see langword="true"/> if the password meets all criteria; otherwise, <see langword="false"/>.</returns>
    [StswInfo("0.9.0")]
    public static bool ValidatePasswordStrength(string password, int minLength = 8, bool requireUppercase = true, bool requireLowercase = true, bool requireDigit = true, bool requireSpecialChar = true)
    {
        if (string.IsNullOrEmpty(password) || password.Length < minLength)
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
