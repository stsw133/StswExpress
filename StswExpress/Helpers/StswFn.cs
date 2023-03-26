using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace StswExpress;

public static class StswFn
{
    /// App: name & version & name + version & copyright
    public static string? AppName() => Assembly.GetEntryAssembly()?.GetName().Name;
    public static string? AppVersion() => Assembly.GetEntryAssembly()?.GetName().Version?.ToString()?.TrimEnd(".0").TrimEnd(".0").TrimEnd(".0");
    public static string AppNameAndVersion => $"{AppName()} {(AppVersion() != "1" ? AppVersion() : string.Empty)}";
    public static string? AppCopyright => Assembly.GetEntryAssembly()?.Location is string location ? FileVersionInfo.GetVersionInfo(location).LegalCopyright : null;

    /// App: database connection & mail config
    public static StswDatabase? AppDB { get; set; } = new();
    public static StswMailConfig? AppMC { get; set; } = new();

    /// Starting functions that should be placed in constructor of App class (if you want to have light and dark theme)
    public static void AppStart(Application app, string saltKey, string hashKey)
    {
        /// hash keys
        StswSecurity.SaltKey = saltKey;
        StswSecurity.HashKey = hashKey;

        /// merged dictionaries
        if (!app.Resources.MergedDictionaries.Any(x => x is Theme))
            app.Resources.MergedDictionaries.Add(new Theme());
        ((Theme)app.Resources.MergedDictionaries.First(x => x is Theme)).Color = Settings.Default.Theme < 0 ? (ThemeColor)GetWindowsTheme() : (ThemeColor)Settings.Default.Theme;

        if (!app.Resources.MergedDictionaries.Any(x => x.Source == new Uri("pack://application:,,,/StswExpress;component/Themes/Generic.xaml")))
            app.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("pack://application:,,,/StswExpress;component/Themes/Generic.xaml") });

        /// global commands
        CommandManager.RegisterClassCommandBinding(typeof(StswWindow), new CommandBinding(StswGlobalCommands.Fullscreen, (s, e) => {
            if (s is StswWindow stsw)
                stsw.Fullscreen = !stsw.Fullscreen;
        }));

        /// global culture (does not work with converters)
        //Thread.CurrentThread.CurrentCulture = CultureInfo.CurrentCulture;
        //Thread.CurrentThread.CurrentUICulture = CultureInfo.CurrentCulture;
        //FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

        /// on exit
        app.Exit += (sender, e) => Settings.Default.Save();
    }

    /// Calculate string into double
    public static double CalculateString(string expression) => Convert.ToDouble(new System.Data.DataTable().Compute(expression, string.Empty));

    /// Gets system theme
    public static int GetWindowsTheme()
    {
        var theme = 0;

        try
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
            {
                var registryValueObject = key?.GetValue("AppsUseLightTheme");
                if (registryValueObject == null)
                    return 0;

                var registryValue = (int)registryValueObject;

                //if (SystemParameters.HighContrast)
                //    theme = 2;

                theme = registryValue > 0 ? 0 : 1;
            }

            return theme;
        }
        catch
        {
            return theme;
        }
    }

    /// Opens context menu of a framework element.
    public static void OpenContextMenu(object sender)
    {
        if (sender is FrameworkElement f)
        {
            f.ContextMenu.PlacementTarget = f;
            f.ContextMenu.IsOpen = true;
        }
    }

    /// Opens file from path.
    public static void OpenFile(string path)
    {
        var process = new Process();
        process.StartInfo.FileName = path;
        process.StartInfo.UseShellExecute = true;
        process.StartInfo.Verb = "open";
        process.Start();
    }
}
