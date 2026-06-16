using System.Security.Cryptography;
using System.Text;

namespace StswExpress.Commons;

/// <summary>
/// Small compatibility helpers used by target frameworks that do not expose newer BCL convenience APIs.
/// </summary>
internal static class StswCompat
{
    public static string ToHexString(byte[] bytes)
    {
#if NET8_0_OR_GREATER
        return Convert.ToHexString(bytes);
#else
        if (bytes is null)
            throw new ArgumentNullException(nameof(bytes));

        var chars = new char[bytes.Length * 2];
        const string hex = "0123456789ABCDEF";
        for (var i = 0; i < bytes.Length; i++)
        {
            chars[i * 2] = hex[bytes[i] >> 4];
            chars[i * 2 + 1] = hex[bytes[i] & 0xF];
        }
        return new string(chars);
#endif
    }

    public static byte[] Sha256HashData(byte[] source)
    {
#if NET8_0_OR_GREATER
        return SHA256.HashData(source);
#else
        using var sha = SHA256.Create();
        return sha.ComputeHash(source);
#endif
    }

    public static void FillRandom(Span<byte> data)
    {
#if NET8_0_OR_GREATER
        RandomNumberGenerator.Fill(data);
#else
        var buffer = new byte[data.Length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(buffer);
        buffer.CopyTo(data);
#endif
    }

    public static byte[] Pbkdf2(string password, byte[] salt, int iterations, HashAlgorithmName hashAlgorithm, int outputLength)
    {
#if NET8_0_OR_GREATER
        return Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, hashAlgorithm, outputLength);
#else
        using var deriveBytes = new Rfc2898DeriveBytes(password, salt, iterations, hashAlgorithm);
        return deriveBytes.GetBytes(outputLength);
#endif
    }

    public static byte[] Pbkdf2(string password, ReadOnlySpan<byte> salt, int iterations, HashAlgorithmName hashAlgorithm, int outputLength)
        => Pbkdf2(password, salt.ToArray(), iterations, hashAlgorithm, outputLength);

    public static bool FixedTimeEquals(byte[] left, byte[] right)
    {
#if NET8_0_OR_GREATER
        return CryptographicOperations.FixedTimeEquals(left, right);
#else
        if (left is null || right is null || left.Length != right.Length)
            return false;

        var diff = 0;
        for (var i = 0; i < left.Length; i++)
            diff |= left[i] ^ right[i];
        return diff == 0;
#endif
    }

    public static int CombineHashCodes<T1, T2>(T1 value1, T2 value2)
    {
#if NET8_0_OR_GREATER
        return HashCode.Combine(value1, value2);
#else
        unchecked
        {
            var hash = 17;
            hash = hash * 31 + (value1?.GetHashCode() ?? 0);
            hash = hash * 31 + (value2?.GetHashCode() ?? 0);
            return hash;
        }
#endif
    }
    public static Task WriteAllTextAsync(string path, string contents)
    {
#if NET8_0_OR_GREATER
        return File.WriteAllTextAsync(path, contents);
#else
        return Task.Run(() => File.WriteAllText(path, contents));
#endif
    }

    public static Task<string> ReadAllTextAsync(string path)
    {
#if NET8_0_OR_GREATER
        return File.ReadAllTextAsync(path);
#else
        return Task.Run(() => File.ReadAllText(path));
#endif
    }

    public static Task<string> ReadAsStringAsync(HttpContent content, CancellationToken cancellationToken)
    {
#if NET8_0_OR_GREATER
        return content.ReadAsStringAsync(cancellationToken);
#else
        cancellationToken.ThrowIfCancellationRequested();
        return content.ReadAsStringAsync();
#endif
    }

}

internal static class StswGuard
{
    public static void ThrowIfNull(object? argument, string? paramName = null)
    {
        if (argument is null)
            throw new ArgumentNullException(paramName);
    }

    public static void ThrowIfNegativeOrZero(int value, string? paramName = null)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(paramName, value, "Value must be greater than zero.");
    }

    public static void ThrowIfNegative(int value, string? paramName = null)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(paramName, value, "Value must be non-negative.");
    }

    public static void ThrowIfLessThan(int value, int other, string? paramName = null)
    {
        if (value < other)
            throw new ArgumentOutOfRangeException(paramName, value, $"Value must be greater than or equal to {other}.");
    }

    public static void ThrowIfNullOrEmpty(string? argument, string? paramName = null)
    {
        if (string.IsNullOrEmpty(argument))
            throw new ArgumentException("Value cannot be null or empty.", paramName);
    }

    public static void ThrowIfNullOrWhiteSpace(string? argument, string? paramName = null)
    {
        if (string.IsNullOrWhiteSpace(argument))
            throw new ArgumentException("Value cannot be null or whitespace.", paramName);
    }
}
