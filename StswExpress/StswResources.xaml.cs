using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace StswExpress;
/// <summary>
/// Represents a class for merging all <see cref="StswExpress"/> resource dictionaries.
/// </summary>
public partial class StswResources
{
    static StswResources()
    {
        AvailableThemes = Enum.GetValues(typeof(StswTheme)).Cast<StswTheme>().ToObservableCollection();
    }

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
    public StswResources(StswTheme theme)
    {
        InitializeComponent();
        Theme = theme;
    }

    /// <summary>
    /// Gets the instance of the <see cref="StswResources"/> class from the application's merged dictionaries.
    /// </summary>
    /// <returns>An instance of the <see cref="StswResources"/> class if found; otherwise, null.</returns>
    public static StswResources? GetInstance()
    {
        return Application.Current.Resources.MergedDictionaries.FirstOrDefault(x => x is StswResources) as StswResources;
    }

    /// <summary>
    /// Gets a list of available themes in the <see cref="StswConfig"/> control.
    /// </summary>
    public static ObservableCollection<StswTheme> AvailableThemes { get; }

    /// <summary>
    /// Gets or sets the selected theme for the application.
    /// </summary>
    public StswTheme Theme
    {
        get => theme;
        set
        {
            var newTheme = value < 0 ? StswFn.GetWindowsTheme() : value;

            if (theme == newTheme)
                return;

            theme = newTheme;
            SetTheme(newTheme);
        }
    }
    private StswTheme theme = StswTheme.Auto;

    /// <summary>
    /// Sets the specified theme by updating the resource dictionary.
    /// </summary>
    /// <param name="theme">The theme to set.</param>
    private void SetTheme(StswTheme theme)
    {
        if (MergedDictionaries.Count > 0)
        {
            MergedDictionaries[0] = new ResourceDictionary() { Source = new Uri($"/StswExpress;component/Themes/Brushes/{theme}.xaml", UriKind.Relative) };
            (Application.Current as StswApp)?.OnThemeChanged(this, theme);
        }
    }
}
