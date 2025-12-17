using System.Text.Json;

namespace StswExpress.Commons;

/// <summary>
/// Global application settings
/// </summary>
public sealed class StswGlobalSettings
{
    public bool AnimationsEnabled { get; set; } = true;
    public string? Language { get; set; } = null; //null=auto
    public string? Theme { get; set; } = null; //null=auto
    public double UiScale { get; set; } = 1.0;
}

/// <summary>
/// Store for global application settings
/// </summary>
public static class StswSettingsStore
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly SemaphoreSlim Gate = new(1, 1);

    /// <summary>
    /// Get path to shared settings file
    /// </summary>
    /// <param name="perMachine">If <see langword="true"/>, use machine-wide location, otherwise user-specific</param>
    /// <returns>Full path to settings file</returns>
    public static string GetSharedPath(bool perMachine = false)
    {
        var root = Environment.GetFolderPath(
            perMachine ? Environment.SpecialFolder.CommonApplicationData
                       : Environment.SpecialFolder.ApplicationData);

        return Path.Combine(root, "StswExpress", "global-settings.json");
    }

    /// <summary>
    /// Load global settings
    /// </summary>
    /// <param name="perMachine">If <see langword="true"/>, use machine-wide location, otherwise user-specific</param>
    /// <returns>Loaded settings</returns>
    public static async Task<StswGlobalSettings> LoadAsync(bool perMachine = false)
    {
        var path = GetSharedPath(perMachine);
        if (!File.Exists(path))
            return new StswGlobalSettings();

        await Gate.WaitAsync().ConfigureAwait(false);
        try
        {
            var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
            return JsonSerializer.Deserialize<StswGlobalSettings>(json, JsonOpts)
                   ?? new StswGlobalSettings();
        }
        finally { Gate.Release(); }
    }

    /// <summary>
    /// Save global settings
    /// </summary>
    /// <param name="settings">Settings to save</param>
    /// <param name="perMachine">If <see langword="true"/>, use machine-wide location, otherwise user-specific</param>
    /// <returns>Asynchronous task</returns>
    public static async Task SaveAsync(StswGlobalSettings settings, bool perMachine = false)
    {
        var path = GetSharedPath(perMachine);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        var tmp = path + ".tmp";
        var json = JsonSerializer.Serialize(settings, JsonOpts);

        await Gate.WaitAsync().ConfigureAwait(false);
        try
        {
            await File.WriteAllTextAsync(tmp, json).ConfigureAwait(false);

            if (File.Exists(path))
                File.Replace(tmp, path, null);
            else
                File.Move(tmp, path);
        }
        finally
        {
            try { if (File.Exists(tmp)) File.Delete(tmp); } catch { /* ignore */ }
            Gate.Release();
        }
    }
}
