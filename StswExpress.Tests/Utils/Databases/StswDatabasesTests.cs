using System.IO;
using System.Text.Json;

namespace StswExpress.Commons.Tests;
public class StswDatabasesTests
{
    private class FakeSecurity
    {
        public static string Encrypt(string input) => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(input));
        public static string Decrypt(string input) => System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(input));
    }

    private string GetTempFilePath() => Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

    private StswDatabaseModel GetSampleModel() =>
        new StswDatabaseModel("server", 1234, "db", "user", "pass") { Name = "TestConn", Type = StswDatabaseType.MSSQL };

    public StswDatabasesTests()
    {
        // Patch StswSecurity for testing
        typeof(StswDatabases).GetField("StswSecurity", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
            ?.SetValue(null, new { Encrypt = (Func<string, string>)FakeSecurity.Encrypt, Decrypt = (Func<string, string>)FakeSecurity.Decrypt });
    }

    [Fact]
    public void ExportList_CreatesEncryptedFile()
    {
        var tempFile = GetTempFilePath();
        StswDatabases.Config.FilePath = tempFile;
        var models = new[] { GetSampleModel() };

        StswDatabases.ExportList(models);

        Assert.True(File.Exists(tempFile));
        var fileContent = File.ReadAllText(tempFile);
        Assert.False(string.IsNullOrEmpty(fileContent));
        File.Delete(tempFile);
    }

    [Fact]
    public async Task ExportListAsync_CreatesEncryptedFileAsync()
    {
        var tempFile = GetTempFilePath();
        StswDatabases.Config.FilePath = tempFile;
        var models = new[] { GetSampleModel() };

        await StswDatabases.ExportListAsync(models);

        Assert.True(File.Exists(tempFile));
        var fileContent = File.ReadAllText(tempFile);
        Assert.False(string.IsNullOrEmpty(fileContent));
        File.Delete(tempFile);
    }

    [Fact]
    public void ImportList_ReturnsModels_WhenFileExists()
    {
        var tempFile = GetTempFilePath();
        StswDatabases.Config.FilePath = tempFile;
        var models = new[] { GetSampleModel() };
        var json = JsonSerializer.Serialize(models, new JsonSerializerOptions { WriteIndented = true });
        var encrypted = FakeSecurity.Encrypt(json);
        File.WriteAllText(tempFile, encrypted);

        var result = StswDatabases.ImportList();

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("TestConn", ((List<StswDatabaseModel>)result)[0].Name);
        File.Delete(tempFile);
    }

    [Fact]
    public async Task ImportListAsync_ReturnsModels_WhenFileExists()
    {
        var tempFile = GetTempFilePath();
        StswDatabases.Config.FilePath = tempFile;
        var models = new[] { GetSampleModel() };
        var json = JsonSerializer.Serialize(models, new JsonSerializerOptions { WriteIndented = true });
        var encrypted = FakeSecurity.Encrypt(json);
        await File.WriteAllTextAsync(tempFile, encrypted);

        var result = await StswDatabases.ImportListAsync();

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("TestConn", ((List<StswDatabaseModel>)result)[0].Name);
        File.Delete(tempFile);
    }

    [Fact]
    public void ImportList_ReturnsEmpty_WhenFileDoesNotExist()
    {
        var tempFile = GetTempFilePath();
        StswDatabases.Config.FilePath = tempFile;
        if (File.Exists(tempFile)) File.Delete(tempFile);

        var result = StswDatabases.ImportList();

        Assert.Empty(result);
        File.Delete(tempFile);
    }

    [Fact]
    public async Task ImportListAsync_ReturnsEmpty_WhenFileDoesNotExist()
    {
        var tempFile = GetTempFilePath();
        StswDatabases.Config.FilePath = tempFile;
        if (File.Exists(tempFile)) File.Delete(tempFile);

        var result = await StswDatabases.ImportListAsync();

        Assert.Empty(result);
        File.Delete(tempFile);
    }

    [Fact]
    public void ImportList_ReturnsEmpty_WhenDecryptionFails()
    {
        var tempFile = GetTempFilePath();
        StswDatabases.Config.FilePath = tempFile;
        File.WriteAllText(tempFile, "notbase64");

        var result = StswDatabases.ImportList();

        Assert.Empty(result);
        File.Delete(tempFile);
    }

    [Fact]
    public async Task ImportListAsync_ReturnsEmpty_WhenDecryptionFails()
    {
        var tempFile = GetTempFilePath();
        StswDatabases.Config.FilePath = tempFile;
        await File.WriteAllTextAsync(tempFile, "notbase64");

        var result = await StswDatabases.ImportListAsync();

        Assert.Empty(result);
        File.Delete(tempFile);
    }
}
