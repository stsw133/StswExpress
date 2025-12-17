using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using System.Collections.ObjectModel;

namespace StswExpress.Avalonia;
/// <summary>
/// Represents a resource manager for handling themes and application resources.
/// </summary>
public partial class StswResources : ResourceDictionary
{
    private static bool _themeSyncInProgress;

    static StswResources()
    {
        CurrentThemeProperty.Changed.AddClassHandler<StswResources>(OnCurrentThemeChanged);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StswResources"/> class.
    /// </summary>
    public StswResources()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StswResources"/> class with a specified theme.
    /// </summary>
    /// <param name="theme">The theme to apply to the application.</param>
    public StswResources(string? theme) : this()
    {
        CurrentTheme = theme;
    }

    /// <summary>
    /// Retrieves the instance of <see cref="StswResources"/> from the application's merged dictionaries.
    /// </summary>
    /// <returns>The <see cref="StswResources"/> instance if found; otherwise, <see langword="null"/>.</returns>
    public static StswResources? GetInstance() => Application.Current?.Resources.MergedDictionaries.FirstOrDefault(x => x is StswResources) as StswResources;

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
        get => GetValue(CurrentThemeProperty);
        set => SetValue(CurrentThemeProperty, value);
    }
    public static readonly StyledProperty<string?> CurrentThemeProperty = AvaloniaProperty.Register<StswResources, string?>(nameof(CurrentTheme));
    private static void OnCurrentThemeChanged(StswResources resources, AvaloniaPropertyChangedEventArgs e)
    {
        var newTheme = string.IsNullOrEmpty((string?)e.NewValue) ? "Light" : (string?)e.NewValue;

        if (resources.CurrentTheme == newTheme)
            return;

        resources.CurrentTheme = newTheme;
        if (!_themeSyncInProgress)
        {
            try
            {
                _themeSyncInProgress = true;
                StswApp.Settings.SyncThemeFromResources(newTheme);
            }
            finally { _themeSyncInProgress = false; }
        }
        resources.OnThemeChanged(newTheme);
    }

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
            var assemblyName = typeof(StswResources).Assembly.GetName().Name;
            try
            {
                MergedDictionaries[0] = new ResourceInclude(new Uri($"avares://{assemblyName}/Themes/Brushes/{theme}.axaml"));
                CustomThemeChanged?.Invoke(this, theme);
            }
            catch
            {
                MergedDictionaries[0] = new ResourceInclude(new Uri($"avares://{assemblyName}/Themes/Brushes/Light.axaml"));
            }
        }
    }

    /// <summary>
    /// Initializes and updates the application's main theme resource dictionary in the provided <see cref="ResourceDictionary"/>.
    /// If the theme resource dictionary already exists, it is replaced with a new instance reflecting the current theme setting.
    /// </summary>
    /// <param name="application">The <see cref="Application"/> whose resources should be updated.</param>
    internal static void InitializeResources(Application application)
    {
        var resources = application.Resources;
        var dictIndex = resources.MergedDictionaries
            .Select((x, index) => new { x, index })
            .FirstOrDefault(d => d.x is StswResources)?.index;

        if (dictIndex.HasValue)
            resources.MergedDictionaries[dictIndex.Value] = new StswResources();
        else
            resources.MergedDictionaries.Add(new StswResources());
    }

    /// <summary>
    /// Synchronizes the current theme from application settings to the resource manager.
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
