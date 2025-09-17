using System.IO;

namespace StswExpress.Commons.Tests;
public class StswMailboxesTests
{
    private class DummySecurity
    {
        public static string Encrypt(string input) => $"enc:{input}";
        public static string Decrypt(string input) => input.StartsWith("enc:") ? input.Substring(4) : string.Empty;
    }

    private class DummyMailbox : StswMailboxModel
    {
        public DummyMailbox(string name) => Name = name;
    }

    private string GetTempFilePath() => Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    public StswMailboxesTests()
    {
        // Patch StswSecurity for tests
        typeof(StswMailboxes)
            .GetField("StswSecurity", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
            ?.SetValue(null, typeof(DummySecurity));
    }

    [Fact]
    public void ExportList_WritesEncryptedFile()
    {
        var filePath = GetTempFilePath();
        StswMailboxes.Config.FilePath = filePath;
        var mailboxes = new List<StswMailboxModel> { new DummyMailbox("A"), new DummyMailbox("B") };

        StswMailboxes.ExportList(mailboxes);

        Assert.True(File.Exists(filePath));
        var content = File.ReadAllText(filePath);
        Assert.StartsWith("enc:", content);
    }

    [Fact]
    public async Task ExportListAsync_WritesEncryptedFile()
    {
        var filePath = GetTempFilePath();
        StswMailboxes.Config.FilePath = filePath;
        var mailboxes = new List<StswMailboxModel> { new DummyMailbox("A") };

        await StswMailboxes.ExportListAsync(mailboxes);

        Assert.True(File.Exists(filePath));
        var content = await File.ReadAllTextAsync(filePath);
        Assert.StartsWith("enc:", content);
    }

    [Fact]
    public void ImportList_ReturnsEmpty_WhenFileMissing()
    {
        var filePath = GetTempFilePath();
        if (File.Exists(filePath)) File.Delete(filePath);
        StswMailboxes.Config.FilePath = filePath;

        var result = StswMailboxes.ImportList();

        Assert.Empty(result);
    }

    [Fact]
    public async Task ImportListAsync_ReturnsEmpty_WhenFileMissing()
    {
        var filePath = GetTempFilePath();
        if (File.Exists(filePath)) File.Delete(filePath);
        StswMailboxes.Config.FilePath = filePath;

        var result = await StswMailboxes.ImportListAsync();

        Assert.Empty(result);
    }

    [Fact]
    public void ImportList_ReturnsDeserializedMailboxes()
    {
        var filePath = GetTempFilePath();
        StswMailboxes.Config.FilePath = filePath;
        var mailboxes = new List<StswMailboxModel> { new DummyMailbox("A"), new DummyMailbox("B") };
        var json = System.Text.Json.JsonSerializer.Serialize(mailboxes, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, DummySecurity.Encrypt(json));

        var result = StswMailboxes.ImportList().ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, m => m.Name == "A");
        Assert.Contains(result, m => m.Name == "B");
    }

    [Fact]
    public async Task ImportListAsync_ReturnsDeserializedMailboxes()
    {
        var filePath = GetTempFilePath();
        StswMailboxes.Config.FilePath = filePath;
        var mailboxes = new List<StswMailboxModel> { new DummyMailbox("A") };
        var json = System.Text.Json.JsonSerializer.Serialize(mailboxes, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(filePath, DummySecurity.Encrypt(json));

        var result = (await StswMailboxes.ImportListAsync()).ToList();

        Assert.Single(result);
        Assert.Equal("A", result[0].Name);
    }
}
