using System;
using System.Collections.Generic;
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
		"Light",
	];

    /// <summary>
    /// 
    /// </summary>
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
