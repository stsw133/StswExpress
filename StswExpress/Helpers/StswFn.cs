﻿using Microsoft.Win32;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace StswExpress;

public static class StswFn
{
    /// App: name & version & name + version & copyright
    public static string? AppName() => Assembly.GetEntryAssembly()?.GetName().Name;
    public static string? AppVersion() => Assembly.GetEntryAssembly()?.GetName().Version?.ToString()?.TrimEnd(".0").TrimEnd(".0").TrimEnd(".0");
    public static string AppNameAndVersion => $"{AppName()} {(AppVersion() != "1" ? AppVersion() : string.Empty)}";
    public static string? AppCopyright => $"{FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).LegalCopyright}";

    /// App: database connection & mail config
    public static StswDB? AppDB { get; set; } = new();
    public static StswMC? AppMC { get; set; } = new();

    /// Starting functions that should be placed in constructor of App class (if you want to have light and dark theme)
    public static void AppStart(Application app, string saltKey, string hashKey)
    {
        StswSecurity.SaltKey = saltKey;
        StswSecurity.HashKey = hashKey;

        if (!app.Resources.MergedDictionaries.Any(x => x is Theme))
            app.Resources.MergedDictionaries.Add(new Theme());
        ((Theme)app.Resources.MergedDictionaries.First(x => x is Theme)).Color = Settings.Default.Theme < 0 ? (ThemeColor)GetWindowsTheme() : (ThemeColor)Settings.Default.Theme;

        app.Exit += (sender, e) => Settings.Default.Save();
    }

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
