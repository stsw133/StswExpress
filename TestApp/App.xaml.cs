global using StswExpress;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace TestApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : StswApp
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        StswSecurity.Key = "myOwnStswHashKey";

        StswDatabase.ImportDatabases();
        StswDatabase.CurrentDatabase = StswDatabase.AllDatabases.FirstOrDefault() ?? new();

        OpenHelp = () => StswFn.OpenFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources\manual_en.pdf"));

        /// example for removing language from config:
        //StswTranslator.AvailableLanguages.Remove("pl");
        //StswSettings.Default.Language = "en";
        /// example for removing theme from config:
        //StswResources.AvailableThemes.Remove(StswTheme.Pink);
        //StswSettings.Default.Theme = (int)StswTheme.Dark;
    }

    private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        StswLog.Write(StswLogType.Error, e.Exception.ToString());
    }
}
