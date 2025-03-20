using System.Text.Json;

namespace StswExpress.Commons;

/// <summary>
/// Provides configuration settings for managing database connections, including methods for importing and exporting these connections with encryption.
/// </summary>
public static class StswDatabases
{
    /// <summary>
    /// Gets the configuration settings for managing database connections.
    /// </summary>
    public static StswDatabasesConfig Config { get; } = new();

    /// <summary>
    /// Gets or sets the default instance of the database connection currently in use by the application, if only one is required.
    /// </summary>
    public static StswDatabaseModel? Default { get; set; }

    /// <summary>
    /// Exports the specified collection of database connections to an encrypted file.
    /// </summary>
    /// <param name="collection">The collection of <see cref="StswDatabaseModel"/> objects to export.</param>
    public static void ExportList(IEnumerable<StswDatabaseModel> collection)
    {
        var serializedData = JsonSerializer.Serialize(collection, new JsonSerializerOptions { WriteIndented = true });
        var encryptedData = StswSecurity.Encrypt(serializedData);
        File.WriteAllText(Config.FilePath, encryptedData);
    }

    /// <summary>
    /// Asynchronously exports the specified collection of database connections to an encrypted file.
    /// </summary>
    /// <param name="collection">The collection of <see cref="StswDatabaseModel"/> objects to export.</param>
    public static async Task ExportListAsync(IEnumerable<StswDatabaseModel> collection)
    {
        var serializedData = JsonSerializer.Serialize(collection, new JsonSerializerOptions { WriteIndented = true });
        var encryptedData = StswSecurity.Encrypt(serializedData);
        await File.WriteAllTextAsync(Config.FilePath, encryptedData);
    }

    /// <summary>
    /// Imports and decrypts database connections from a file, returning a collection of <see cref="StswDatabaseModel"/> objects.
    /// </summary>
    /// <returns>An enumerable collection of <see cref="StswDatabaseModel"/> objects representing the imported connections.</returns>
    public static IEnumerable<StswDatabaseModel> ImportList()
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

        return JsonSerializer.Deserialize<List<StswDatabaseModel>>(decryptedData) ?? [];
    }

    /// <summary>
    /// Asynchronously imports and decrypts database connections from a file, returning a collection of <see cref="StswDatabaseModel"/> objects.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, with a result of an enumerable collection of <see cref="StswDatabaseModel"/> objects.</returns>
    public static async Task<IEnumerable<StswDatabaseModel>> ImportListAsync()
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

        return JsonSerializer.Deserialize<List<StswDatabaseModel>>(decryptedData) ?? [];
    }
}
