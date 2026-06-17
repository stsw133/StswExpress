using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace StswExpress.Wpf;

/// </summary>
public partial class StswResources : ResourceDictionary
{
    private static bool _themeSyncInProgress;

    /// </summary>
    public StswResources()
    {
        InitializeComponent();
    }

    /// <param name="theme">The theme to apply to the application.</param>
    public StswResources(string? theme)
    {
        InitializeComponent();
        CurrentTheme = theme;
    }

    /// </summary>
    /// <returns>The <see cref="StswResources"/> instance if found; otherwise, <see langword="null"/>.</returns>
    public static StswResources? GetInstance() => Application.Current.Resources.MergedDictionaries.FirstOrDefault(x => x is StswResources) as StswResources;

    /// </summary>
    public static ObservableCollection<string?> AvailableThemes { get; set; } =
    ];

    private static readonly Dictionary<string, Uri> ThemeSources = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Dark"] = new("/StswExpress.Wpf;component/Themes/Brushes/Dark.xaml", UriKind.Relative),
        ["Light"] = new("/StswExpress.Wpf;component/Themes/Brushes/Light.xaml", UriKind.Relative),
    };

    /// <summary>
    /// Registers an application theme resource dictionary so it can be selected by configuration UI.
    /// </summary>
    /// <param name="theme">The theme name displayed in <see cref="AvailableThemes"/>.</param>
    /// <param name="source">The resource dictionary source for the theme brushes.</param>
    public static void RegisterTheme(string theme, Uri source)
    {
        if (string.IsNullOrWhiteSpace(theme))
            throw new ArgumentException("Theme name cannot be empty.", nameof(theme));

        ThemeSources[theme] = source ?? throw new ArgumentNullException(nameof(source));

        if (!AvailableThemes.Any(x => string.Equals(x, theme, StringComparison.OrdinalIgnoreCase)))
            AvailableThemes.Add(theme);
    }

    /// <summary>
    /// </example>
    public string? CurrentTheme
    {
        get => _currentTheme;
        set
        {
            var newTheme = string.IsNullOrEmpty(value) ? StswFnUI.GetWindowsTheme() : value;

            if (_currentTheme == newTheme)
                return;

            _currentTheme = newTheme;

            if (!_themeSyncInProgress)
            {
                try
                {
                    _themeSyncInProgress = true;
                    StswApp.Settings.SyncThemeFromResources(newTheme);
                }
                finally { _themeSyncInProgress = false; }
            }
            OnThemeChanged(newTheme);
        }
    }
    private string? _currentTheme;

    public static event EventHandler<string?>? CustomThemeChanged;

    /// <param name="theme">The new theme to apply.</param>
    private void OnThemeChanged(string? theme)
    {
        if (MergedDictionaries.Count > 0)
        {
            try
            {
                if (!ThemeSources.TryGetValue(theme ?? string.Empty, out var source))
                    throw new InvalidOperationException($"Theme '{theme}' is not registered.");

                MergedDictionaries[0] = new ResourceDictionary() { Source = source };
                CustomThemeChanged?.Invoke(this, theme);
            }
            catch
            {
                MergedDictionaries[0] = new ResourceDictionary() { Source = new Uri($"/StswExpress.Wpf;component/Themes/Brushes/Light.xaml", UriKind.Relative) };
            }
        }

    /// <param name="resources">The <see cref="ResourceDictionary"/> to update with the current theme.</param>
    internal static void InitializeResources(ResourceDictionary resources)
    {
        var existingDictionary = resources.MergedDictionaries
            .Select((x, index) => new { x, index })
            .FirstOrDefault(d => d.x is StswResources);

        if (existingDictionary?.x is StswResources stswResources)
        {
            if (string.IsNullOrEmpty(stswResources.CurrentTheme))
                stswResources.CurrentTheme = StswApp.Settings.Theme;

            resources.MergedDictionaries[existingDictionary.index] = stswResources;
        }
        else
        {
            resources.MergedDictionaries.Add(new StswResources(StswApp.Settings.Theme));
        }
    }

    /// <summary>
    /// Synchronizes the theme from application settings to the <see cref="StswResources"/> instance.
    /// </summary>
    /// <param name="theme">The theme to synchronize.</param>
    internal static void SyncThemeFromSettings(string? theme)
    {
        if (_themeSyncInProgress)
            return;

        var instance = GetInstance();
        if (instance is null)
            return;

        try
        {
            _themeSyncInProgress = true;
            instance.CurrentTheme = theme;
        }
        finally { _themeSyncInProgress = false; }
    }
}
    /// </summary>
    /// <param name="resources">The <see cref="ResourceDictionary"/> to update with the current theme.</param>
    internal static void InitializeResources(ResourceDictionary resources)
    {
        var existingDictionary = resources.MergedDictionaries
            .Select((x, index) => new { x, index })
            .FirstOrDefault(d => d.x is StswResources);

        if (existingDictionary?.x is StswResources stswResources)
        {
            if (string.IsNullOrEmpty(stswResources.CurrentTheme))
                stswResources.CurrentTheme = StswApp.Settings.Theme;

            resources.MergedDictionaries[existingDictionary.index] = stswResources;
        }
        else
        {
            resources.MergedDictionaries.Add(new StswResources(StswApp.Settings.Theme));
        }
    }

    /// <summary>
    /// Synchronizes the theme from application settings to the <see cref="StswResources"/> instance.
    /// </summary>
    /// <param name="theme">The theme to synchronize.</param>
    internal static void SyncThemeFromSettings(string? theme)
    {
        if (_themeSyncInProgress)
            return;

        var instance = GetInstance();
        if (instance is null)
            return;

        try
        {
            _themeSyncInProgress = true;
            instance.CurrentTheme = theme;
        }
        finally { _themeSyncInProgress = false; }
    }
}
