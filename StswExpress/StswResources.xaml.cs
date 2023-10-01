using System;
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
            if (theme == value)
                return;
            theme = value;
            SetTheme(theme);
        }
    }
    private StswTheme theme = StswTheme.Default;

    /// <summary>
    /// Method for selecting Theme.
    /// </summary>
    private void SetTheme(StswTheme theme) => MergedDictionaries[0] = new ResourceDictionary() { Source = new Uri($"/StswExpress;component/Themes/Brushes/{theme}.xaml", UriKind.Relative) };
}

/// <summary>
/// Enumeration for <see cref="StswResources"/>'s type.
/// </summary>
public enum StswTheme
{
    Default = -1,
    Light,
    Dark,
    Pink
}
