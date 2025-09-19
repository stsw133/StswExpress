using System.Security.Cryptography;
using System.Text;

namespace StswExpress.Commons.Tests.Utils.Security;
public class StswSecurityTests
{
    [Fact]
    public void Key_SetLessThan16Chars_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => StswSecurity.Key = "shortkey");
    }

    [Fact]
    public void Key_SetValidKey_SetsManualKey()
    {
        StswSecurity.Key = "1234567890123456";
        var key = typeof(StswSecurity).GetProperty("AesKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!.GetValue(null) as byte[];
        Assert.NotNull(key);
        Assert.Equal(32, key!.Length);
    }

    [Fact]
    public void Salt_DefaultSalt_IsDerivedFromAppName()
    {
        var salt = StswSecurity.Salt;
        Assert.NotNull(salt);
        Assert.True(salt.Length >= 16);
    }

    [Fact]
    public void SetSalt_LessThan16Bytes_ThrowsArgumentException()
    {
        var salt = new byte[8];
        Assert.Throws<ArgumentException>(() => StswSecurity.SetSalt(salt));
    }

    [Fact]
    public void SetSalt_ValidSalt_SetsManualSalt()
    {
        var salt = new byte[16];
        StswSecurity.SetSalt(salt);
        Assert.Equal(salt, StswSecurity.Salt);
    }

    [Fact]
    public void ComputeHash_UsesSHA256ByDefault()
    {
        var data = Encoding.UTF8.GetBytes("test");
        var hash = StswSecurity.ComputeHash(data);
        Assert.Equal(32, hash.Length);
    }

    [Fact]
    public void GetHashString_ReturnsHexString()
    {
        var hex = StswSecurity.GetHashString("test");
        Assert.Equal(64, hex.Length);
        Assert.True(hex.All(c => Uri.IsHexDigit(c)));
    }

    [Fact]
    public void HashPassword_Default_ReturnsBase64String()
    {
        var hash = StswSecurity.HashPassword("password");
        var bytes = Convert.FromBase64String(hash);
        Assert.Equal(32, bytes.Length);
    }

    [Fact]
    public void HashPassword_CustomFormat_ReturnsExpectedFormat()
    {
        var hash = StswSecurity.HashPassword("password", 310_000, 16, 32);
        var parts = hash.Split('$');
        Assert.Equal(4, parts.Length);
        Assert.Equal("pbkdf2-sha256", parts[0]);
        Assert.True(int.TryParse(parts[1], out var iter) && iter == 310_000);
        Assert.True(Convert.FromBase64String(parts[2]).Length == 16);
        Assert.True(Convert.FromBase64String(parts[3]).Length == 32);
    }

    [Fact]
    public void VerifyPassword_ValidAndInvalidCases()
    {
        var hash = StswSecurity.HashPassword("mypassword", 310_000, 16, 32);
        Assert.True(StswSecurity.VerifyPassword("mypassword", hash));
        Assert.False(StswSecurity.VerifyPassword("wrongpassword", hash));
        Assert.False(StswSecurity.VerifyPassword("mypassword", "invalid$format"));
    }

    [Fact]
    public void EncryptDecrypt_RoundTrip_ReturnsOriginal()
    {
        StswSecurity.Key = "1234567890123456";
        var original = "SecretMessage";
        var encrypted = StswSecurity.Encrypt(original);
        var decrypted = StswSecurity.Decrypt(encrypted);
        Assert.Equal(original, decrypted);
    }

    [Fact]
    public void EncryptGcmDecryptGcm_RoundTrip_ReturnsOriginal()
    {
        StswSecurity.Key = "1234567890123456";
        var original = "SecretMessageGCM";
        var encrypted = StswSecurity.EncryptGcm(original);
        var decrypted = StswSecurity.DecryptGcm(encrypted);
        Assert.Equal(original, decrypted);
    }

    [Fact]
    public void Decrypt_EmptyOrNull_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, StswSecurity.Decrypt(null));
        Assert.Equal(string.Empty, StswSecurity.Decrypt(""));
    }

    [Fact]
    public void Decrypt_InvalidCipher_ThrowsCryptographicException()
    {
        Assert.Throws<CryptographicException>(() => StswSecurity.Decrypt("short"));
    }

    [Fact]
    public void DecryptGcm_EmptyOrNull_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, StswSecurity.DecryptGcm(null));
        Assert.Equal(string.Empty, StswSecurity.DecryptGcm(""));
    }

    [Fact]
    public void DecryptGcm_InvalidCipher_ThrowsCryptographicException()
    {
        Assert.Throws<CryptographicException>(() => StswSecurity.DecryptGcm("short"));
    }

    [Fact]
    public void GenerateRandomToken_LengthIsCorrectAndCharsValid()
    {
        var token = StswSecurity.GenerateRandomToken(32);
        Assert.Equal(32, token.Length);
        const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^*()-_=+[]{};:,.?/";
        Assert.All(token, c => Assert.Contains(c, validChars));
    }

    [Fact]
    public void GenerateRandomToken_LengthZero_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => StswSecurity.GenerateRandomToken(0));
    }

    [Theory]
    [InlineData("Abcdef1!", true)]
    [InlineData("abcdef1!", false)]
    [InlineData("ABCDEF1!", false)]
    [InlineData("Abcdefgh", false)]
    [InlineData("Abcdef1", false)]
    [InlineData("Abcdef1!", true)]
    [InlineData("A1!", false)]
    public void ValidatePasswordStrength_VariousCases(string password, bool expected)
    {
        Assert.Equal(expected, StswSecurity.ValidatePasswordStrength(password));
    }
}
