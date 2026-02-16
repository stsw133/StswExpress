namespace StswExpress.Wpf;

/// <summary>
/// Global application settings
/// </summary>
public sealed class StswSettingsModel : StswObservableObject
{
    private bool _languageSyncInProgress;
    private bool _themeSyncInProgress;

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
}
