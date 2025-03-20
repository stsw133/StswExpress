using System.Text.Json;

namespace StswExpress.Commons;
/// <summary>
/// Provides functionality for managing email configurations, including methods for importing and exporting these configurations with encryption.
/// </summary>
public static class StswMailboxes
{
    /// <summary>
    /// Gets the configuration settings for managing email configurations.
    /// </summary>
    public static StswMailboxesConfig Config { get; } = new();

    /// <summary>
    /// Gets or sets the default instance of the email configuration currently in use by the application, if only one is required.
    /// </summary>
    public static StswMailboxModel? Default { get; set; }

    /// <summary>
    /// Exports the specified collection of email configurations to an encrypted file.
    /// </summary>
    /// <param name="collection">The collection of <see cref="StswMailboxModel"/> objects to export.</param>
    public static void ExportList(IEnumerable<StswMailboxModel> collection)
    {
        var serializedData = JsonSerializer.Serialize(collection, new JsonSerializerOptions { WriteIndented = true });
        var encryptedData = StswSecurity.Encrypt(serializedData);
        File.WriteAllText(Config.FilePath, encryptedData);
    }

    /// <summary>
    /// Asynchronously exports the specified collection of email configurations to an encrypted file.
    /// </summary>
    /// <param name="collection">The collection of <see cref="StswMailboxModel"/> objects to export.</param>
    public static async Task ExportListAsync(IEnumerable<StswMailboxModel> collection)
    {
        var serializedData = JsonSerializer.Serialize(collection, new JsonSerializerOptions { WriteIndented = true });
        var encryptedData = StswSecurity.Encrypt(serializedData);
        await File.WriteAllTextAsync(Config.FilePath, encryptedData);
    }

    /// <summary>
    /// Imports and decrypts email configurations from a file, returning a collection of <see cref="StswMailboxModel"/> objects.
    /// </summary>
    /// <returns>An enumerable collection of <see cref="StswMailboxModel"/> objects representing the imported configurations.</returns>
    public static IEnumerable<StswMailboxModel> ImportList()
    {
        if (!File.Exists(Config.FilePath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Config.FilePath)!);
            File.Create(Config.FilePath).Dispose();
            return [];
        }

        var encryptedData = File.ReadAllText(Config.FilePath);
        var decryptedData = StswSecurity.Decrypt(encryptedData);
        if (string.IsNullOrEmpty(decryptedData))
            return [];

        return JsonSerializer.Deserialize<List<StswMailboxModel>>(decryptedData) ?? [];
    }

    /// <summary>
    /// Asynchronously imports and decrypts email configurations from a file, returning a collection of <see cref="StswMailboxModel"/> objects.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, with a result of an enumerable collection of <see cref="StswMailboxModel"/> objects.</returns>
    public static async Task<IEnumerable<StswMailboxModel>> ImportListAsync()
    {
        if (!File.Exists(Config.FilePath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Config.FilePath)!);
            await File.Create(Config.FilePath).DisposeAsync();
            return [];
        }

        var encryptedData = await File.ReadAllTextAsync(Config.FilePath);
        var decryptedData = StswSecurity.Decrypt(encryptedData);
        if (string.IsNullOrEmpty(decryptedData))
            return [];

        return JsonSerializer.Deserialize<List<StswMailboxModel>>(decryptedData) ?? [];
    }
}
