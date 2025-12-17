using System.Text.Json;

namespace StswExpress.Avalonia;

/// <summary>
/// Global application settings
/// </summary>
public sealed class StswSettings : StswObservableObject
{
    private bool _languageSyncInProgress;
    private bool _themeSyncInProgress;

    /// <summary>
    /// Synchronizes the current theme with the specified resource theme name.
    /// </summary>
    /// <param name="theme">The name of the theme to apply, or null to clear the current theme.</param>
    internal void SyncThemeFromResources(string? theme)
    {
        if (_themeSyncInProgress)
            return;

        try
        {
            _themeSyncInProgress = true;
            Theme = theme;
        }
        finally { _themeSyncInProgress = false; }
    }

    /// <summary>
    /// Synchronizes the current language setting with the specified language value from the translator.
    /// </summary>
    /// <param name="language">The language code to synchronize with, or null to clear the current language setting.</param>
    internal void SyncLanguageFromTranslator(string? language)
    {
        if (_languageSyncInProgress)
            return;

        try
        {
            _languageSyncInProgress = true;
            Language = language;
        }
        finally { _languageSyncInProgress = false; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether UI animations are enabled.
    /// </summary>
    public bool AnimationsEnabled
    {
        get => _animationsEnabled;
        set => SetProperty(ref _animationsEnabled, value);
    }
    private bool _animationsEnabled = true;

    /// <summary>
    /// Gets or sets the preferred application language (e.g. "en").
    /// </summary>
    public string? Language
    {
        get => _language;
        set
        {
            if (SetProperty(ref _language, value) && !_languageSyncInProgress)
            {
                try
                {
                    _languageSyncInProgress = true;
                    StswTranslator.SyncLanguageFromSettings(value);
                }
                finally { _languageSyncInProgress = false; }
            }
        }
    }
    private string? _language;

    /// <summary>
    /// Gets or sets the application theme (e.g. "Light", "Dark").
    /// </summary>
    public string? Theme
    {
        get => _theme;
        set
        {
            if (SetProperty(ref _theme, value) && !_themeSyncInProgress)
            {
                try
                {
                    _themeSyncInProgress = true;
                    StswResources.SyncThemeFromSettings(value);
                }
                finally { _themeSyncInProgress = false; }
            }
        }
    }
    private string? _theme;

    /// <summary>
    /// Gets or sets the UI scale factor.
    /// </summary>
    public double UiScale
    {
        get => _uiScale;
        set => SetProperty(ref _uiScale, value);
    }
    private double _uiScale = 1.0;
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
    public static async Task<StswSettings> LoadAsync(bool perMachine = false)
    {
        var path = GetSharedPath(perMachine);
        if (!File.Exists(path))
            return new StswSettings();

        await Gate.WaitAsync().ConfigureAwait(false);
        try
        {
            var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
            return JsonSerializer.Deserialize<StswSettings>(json, JsonOpts)
                   ?? new StswSettings();
        }
        finally { Gate.Release(); }
    }

    /// <summary>
    /// Save global settings
    /// </summary>
    /// <param name="settings">Settings to save</param>
    /// <param name="perMachine">If <see langword="true"/>, use machine-wide location, otherwise user-specific</param>
    /// <returns>Asynchronous task</returns>
    public static async Task SaveAsync(StswSettings settings, bool perMachine = false)
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
