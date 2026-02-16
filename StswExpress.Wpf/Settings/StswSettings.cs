using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace StswExpress.Wpf;

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
    /// Loads the shared settings from file.
    /// </summary>
    /// <param name="perMachine">If <see langword="true"/>, use machine-wide location, otherwise user-specific</param>
    /// <param name="readText">Function to read text from file</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Loaded settings</returns>
    private static async ValueTask<StswSettingsModel> LoadCoreAsync(bool perMachine, Func<string, CancellationToken, ValueTask<string>> readText, CancellationToken ct = default)
    {
        var path = GetSharedPath(perMachine);
        if (!File.Exists(path))
            return new StswSettingsModel();

        await Gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var json = await readText(path, ct).ConfigureAwait(false);
            return JsonSerializer.Deserialize<StswSettingsModel>(json, JsonOpts) ?? new StswSettingsModel();
        }
        finally { Gate.Release(); }
    }

    /// <summary>
    /// Loads the shared settings from file.
    /// </summary>
    /// <param name="perMachine">If <see langword="true"/>, use machine-wide location, otherwise user-specific</param>
    /// <returns>Loaded settings</returns>
    public static StswSettingsModel Load(bool perMachine = false) => LoadCoreAsync(perMachine, static (p, _) => new ValueTask<string>(File.ReadAllText(p))).GetAwaiter().GetResult();

    /// <summary>
    /// Loads the shared settings from file.
    /// </summary>
    /// <param name="perMachine">If <see langword="true"/>, use machine-wide location, otherwise user-specific</param>
    /// <returns>Loaded settings</returns>
    public static Task<StswSettingsModel> LoadAsync(bool perMachine = false) => LoadCoreAsync(perMachine, static (p, c) => new ValueTask<string>(File.ReadAllTextAsync(p, c))).AsTask();

    /// <summary>
    /// Saves the specified settings to the shared settings file.
    /// </summary>
    /// <param name="settings">Settings to save</param>
    /// <param name="perMachine">If <see langword="true"/>, use machine-wide location, otherwise user-specific</param>
    /// <returns>Awaitable task</returns>
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
