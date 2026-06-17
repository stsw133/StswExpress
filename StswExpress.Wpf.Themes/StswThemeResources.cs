using System;
using StswExpress.Wpf;

namespace StswExpress.Wpf.Themes;

/// <summary>
/// Registers additional theme dictionaries shipped by <c>StswExpress.Wpf.Themes</c>.
/// </summary>
public static class StswThemeResources
{
    /// <summary>
    /// Adds all additional StswExpress WPF themes to <see cref="StswResources.AvailableThemes"/> and registers their resource dictionaries.
    /// </summary>
    public static void Register()
    {
        RegisterTheme("Halloween");
        RegisterTheme("Night");
        RegisterTheme("Pink");
        RegisterTheme("Spring");
        RegisterTheme("Summer");
        RegisterTheme("Winter");
    }

    private static void RegisterTheme(string theme) => StswResources.RegisterTheme(theme, new Uri($"/StswExpress.Wpf.Themes;component/Themes/Brushes/{theme}.xaml", UriKind.Relative));
}
