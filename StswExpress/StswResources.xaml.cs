using System;
using System.Linq;
using System.Windows;

namespace StswExpress;

/// <summary>
/// A class for merging all StswExpress's resource dictionaries.
/// </summary>
public partial class StswResources
{
    public StswResources()
    {
        InitializeComponent();
    }
    public StswResources(StswTheme theme)
    {
        InitializeComponent();
        Theme = theme;
    }

    /// <summary>
    /// Selected theme for application.
    /// </summary>
    public StswTheme Theme
    {
        get => theme;
        set
        {
            var newTheme = StswSettings.Default.Theme < 0 ? StswFn.GetWindowsTheme() : value;

            if (theme == newTheme)
                return;

            theme = newTheme;
            SetTheme(newTheme);
            ThemeChanged?.Invoke(this, newTheme);
        }
    }
    private StswTheme theme = StswTheme.Auto;

    /// <summary>
    /// 
    /// </summary>
    public static StswResources? GetInstance() => Application.Current.Resources.MergedDictionaries.FirstOrDefault(x => x is StswResources) as StswResources;

    /// <summary>
    /// Method for selecting Theme.
    /// </summary>
    private void SetTheme(StswTheme theme) => MergedDictionaries[0] = new ResourceDictionary() { Source = new Uri($"/StswExpress;component/Themes/Brushes/{theme}.xaml", UriKind.Relative) };

    /// <summary>
    /// Occurs when the Theme changes.
    /// </summary>
    public event EventHandler<StswTheme>? ThemeChanged;
}

/// <summary>
/// Enumeration for <see cref="StswResources"/>'s type.
/// </summary>
public enum StswTheme
{
    Auto = -1,
    Light,
    Dark,
    Pink
}
