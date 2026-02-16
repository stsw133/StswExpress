using StswExpress.Avalonia.Settings;
using System.Text.Json;

namespace StswExpress.Avalonia;

/// <summary>
/// Store for global application settings
/// </summary>
internal static class StswSettings
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
    public static async Task<StswSettingsModel> LoadAsync(bool perMachine = false)
    {
        var path = GetSharedPath(perMachine);
        if (!File.Exists(path))
            return new StswSettingsModel();

        await Gate.WaitAsync().ConfigureAwait(false);
        try
        {
            var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
            return JsonSerializer.Deserialize<StswSettingsModel>(json, JsonOpts)
                   ?? new StswSettingsModel();
        }
        finally { Gate.Release(); }
    }

    /// <summary>
    /// Save global settings
    /// </summary>
    /// <param name="settings">Settings to save</param>
    /// <param name="perMachine">If <see langword="true"/>, use machine-wide location, otherwise user-specific</param>
    /// <returns>Asynchronous task</returns>
    public static async Task SaveAsync(StswSettingsModel settings, bool perMachine = false)
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
