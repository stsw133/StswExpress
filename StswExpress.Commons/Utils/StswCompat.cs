using System.Security.Cryptography;
using System.Text;

namespace StswExpress.Commons;

/// <summary>
/// Small compatibility helpers used by target frameworks that do not expose newer BCL convenience APIs.
/// </summary>
public static class StswCompat
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
        if (hashAlgorithm == HashAlgorithmName.SHA1)
        {
            using var deriveBytes = new Rfc2898DeriveBytes(password, salt, iterations);
            return deriveBytes.GetBytes(outputLength);
        }

        return Pbkdf2Compat(password, salt, iterations, hashAlgorithm, outputLength);
#endif
	}

#if !NET8_0_OR_GREATER
    private static byte[] Pbkdf2Compat(string password, byte[] salt, int iterations, HashAlgorithmName hashAlgorithm, int outputLength)
    {
        if (password is null)
            throw new ArgumentNullException(nameof(password));
        if (salt is null)
            throw new ArgumentNullException(nameof(salt));
        if (iterations <= 0)
            throw new ArgumentOutOfRangeException(nameof(iterations));
        if (outputLength < 0)
            throw new ArgumentOutOfRangeException(nameof(outputLength));

        var passwordBytes = Encoding.UTF8.GetBytes(password);
        using var hmac = CreateHmac(hashAlgorithm, passwordBytes);
        var hashLength = hmac.HashSize / 8;
        var blockCount = (int)Math.Ceiling((double)outputLength / hashLength);
        var output = new byte[outputLength];
        var offset = 0;

        for (var blockIndex = 1; blockIndex <= blockCount; blockIndex++)
        {
            var block = Pbkdf2Block(hmac, salt, iterations, blockIndex);
            var count = Math.Min(hashLength, outputLength - offset);
            Buffer.BlockCopy(block, 0, output, offset, count);
            offset += count;
        }

        return output;
    }

    private static byte[] Pbkdf2Block(HMAC hmac, byte[] salt, int iterations, int blockIndex)
    {
        var blockIndexBytes = new[]
        {
            (byte)(blockIndex >> 24),
            (byte)(blockIndex >> 16),
            (byte)(blockIndex >> 8),
            (byte)blockIndex
        };
        var input = new byte[salt.Length + blockIndexBytes.Length];
        Buffer.BlockCopy(salt, 0, input, 0, salt.Length);
        Buffer.BlockCopy(blockIndexBytes, 0, input, salt.Length, blockIndexBytes.Length);

        var u = hmac.ComputeHash(input);
        var result = (byte[])u.Clone();

        for (var i = 1; i < iterations; i++)
        {
            u = hmac.ComputeHash(u);
            for (var j = 0; j < result.Length; j++)
                result[j] ^= u[j];
        }

        return result;
    }

    private static HMAC CreateHmac(HashAlgorithmName hashAlgorithm, byte[] key)
    {
        if (hashAlgorithm == HashAlgorithmName.SHA256)
            return new HMACSHA256(key);
        if (hashAlgorithm == HashAlgorithmName.SHA384)
            return new HMACSHA384(key);
        if (hashAlgorithm == HashAlgorithmName.SHA512)
            return new HMACSHA512(key);
        if (hashAlgorithm == HashAlgorithmName.SHA1)
            return new HMACSHA1(key);

        throw new CryptographicException($"Unsupported PBKDF2 hash algorithm: {hashAlgorithm.Name}.");
    }
#endif

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

    public static int Clamp(int value, int min, int max)
    {
#if NET8_0_OR_GREATER
        return Math.Clamp(value, min, max);
#else
        if (value < min)
            return min;
        if (value > max)
            return max;
        return value;
#endif
    }

    public static long Clamp(long value, long min, long max)
    {
#if NET8_0_OR_GREATER
        return Math.Clamp(value, min, max);
#else
        if (value < min)
            return min;
        if (value > max)
            return max;
        return value;
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

    public static long NextInt64(Random random, long minValue, long maxValue)
    {
#if NET8_0_OR_GREATER
        return random.NextInt64(minValue, maxValue);
#else
        if (random is null)
            throw new ArgumentNullException(nameof(random));
        if (minValue > maxValue)
            throw new ArgumentOutOfRangeException(nameof(minValue));
        if (minValue == maxValue)
            return minValue;

        var range = (decimal)maxValue - minValue;
        return minValue + (long)Math.Floor((decimal)random.NextDouble() * range);
#endif
    }

    public static Random SharedRandom { get; } = new Random();

    public static bool IsWindows()
    {
#if NET8_0_OR_GREATER
        return OperatingSystem.IsWindows();
#else
        return Environment.OSVersion.Platform == PlatformID.Win32NT;
#endif
    }

    public static bool PathExists(string path)
        => File.Exists(path) || Directory.Exists(path);

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

public static class StswGuard
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
