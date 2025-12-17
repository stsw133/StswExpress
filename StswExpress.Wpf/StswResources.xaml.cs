using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace StswExpress.Wpf;
/// <summary>
/// Represents a resource manager for handling themes and application resources.
/// </summary>
public partial class StswResources : ResourceDictionary
{
    private static bool _themeSyncInProgress;

    /// <summary>
    /// Initializes a new instance of the <see cref="StswResources"/> class.
    /// </summary>
    public StswResources()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StswResources"/> class with a specified theme.
    /// </summary>
    /// <param name="theme">The theme to apply to the application.</param>
    public StswResources(string? theme)
    {
        InitializeComponent();
        CurrentTheme = theme;
    }

    /// <summary>
    /// Retrieves the instance of <see cref="StswResources"/> from the application's merged dictionaries.
    /// </summary>
    /// <returns>The <see cref="StswResources"/> instance if found; otherwise, <see langword="null"/>.</returns>
    public static StswResources? GetInstance() => Application.Current.Resources.MergedDictionaries.FirstOrDefault(x => x is StswResources) as StswResources;

    /// <summary>
    /// Gets the list of available themes that can be applied to the application.
    /// </summary>
    public static ObservableCollection<string?> AvailableThemes { get; set; } =
    [
        "Dark",
        "Halloween",
        "Light",
        "Pink",
        "Night",
        "Spring",
        "Summer",
        "Winter",
    ];

    /// <summary>
    /// Gets or sets the current theme of the application. When changed, updates the application theme.
    /// </summary>
    /// <example>
    /// Example usage:
    /// <code>
    /// StswResources.GetInstance().CurrentTheme = "Dark";
    /// </code>
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

    /// <summary>
    /// Occurs when the theme is changed, allowing custom brushes or settings to be applied.
    /// </summary>
    /// <example>
    /// Example usage:
    /// <code>
    /// StswResources.CustomThemeChanged += (sender, theme) =>
    /// {
    ///     Console.WriteLine($"Theme changed to {theme}");
    /// };
    /// </code>
    /// </example>
    public static event EventHandler<string?>? CustomThemeChanged;

    /// <summary>
    /// Updates the application's resource dictionary to use the selected theme and triggers theme change events.
    /// </summary>
    /// <param name="theme">The new theme to apply.</param>
    private void OnThemeChanged(string? theme)
    {
        if (MergedDictionaries.Count > 0)
        {
            try
            {
                MergedDictionaries[0] = new ResourceDictionary() { Source = new Uri($"/StswExpress.Wpf;component/Themes/Brushes/{theme}.xaml", UriKind.Relative) };
                CustomThemeChanged?.Invoke(this, theme);
            }
            catch
            {
                MergedDictionaries[0] = new ResourceDictionary() { Source = new Uri($"/StswExpress.Wpf;component/Themes/Brushes/Light.xaml", UriKind.Relative) };
            }
        }
    }

    /// <summary>
    /// Initializes and updates the application's main theme resource dictionary in the provided <see cref="ResourceDictionary"/>.
    /// If the theme resource dictionary already exists, it is replaced with a new instance reflecting the current theme setting.
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
