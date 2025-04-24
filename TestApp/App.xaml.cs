global using StswExpress;
global using StswExpress.Commons;
using System;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace TestApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : StswApp
{
    public static ICommand HelpCommand { get; } = new RoutedUICommand("Help", "Help", typeof(StswWindow), [new KeyGesture(Key.F1)]);

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        CommandManager.RegisterClassCommandBinding(typeof(StswWindow), new CommandBinding(HelpCommand, (_, _) => OpenHelp()));

        /// example for overriding security key:
        //StswSecurity.Key = "myOwnStswHashKey";
        /// example for removing language from config:
        //StswTranslator.AvailableLanguages = new() { { "en", "English" } };
        //StswTranslator.CurrentLanguage = "en"; //or `string.Empty` to get default language
        /// example for removing theme from config:
        //StswResources.AvailableThemes = ["Light", "Dark"];
        //StswResources.CurrentTheme = "Light"; //or `string.Empty` to get default theme

        //var test = SQLService.DbCurrent.ExecuteStoredProcedure("[dbo].[proc_AnalizaObrotow]", new
        //{
        //    Typ = "PH",
        //    ZS = false,
        //    DataOd = "2025-04-15",
        //    DataDo = "2025-04-15",
        //    Kody = "11111",
        //    Akronimy = ""
        //});
    }

    private async void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        try
        {
            await StswMessageDialog.Show(e.Exception, "Unhandled exception", true);
        }
        catch
        {

        }
    }

    private static async void OpenHelp()
    {
        if (Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"Resources\help_{StswSettings.Default.Language.ToLower()}.pdf") is string helpPath && File.Exists(helpPath))
            StswFn.OpenFile(helpPath);
        else if (Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources\help_en.pdf") is string helpPathEN && File.Exists(helpPathEN))
            StswFn.OpenFile(helpPathEN);
        else
            await StswMessageDialog.Show("No help file is available!", "Information", null, StswDialogButtons.OK, StswDialogImage.Information, false);
    }
}
