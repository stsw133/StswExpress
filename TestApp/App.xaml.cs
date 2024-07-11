﻿global using StswExpress;
using System;
using System.IO;
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
        //StswTranslator.AvailableLanguages.Remove("");
        //StswTranslator.AvailableLanguages.Remove("pl");
        //StswSettings.Default.Language = "en";
        /// example for removing theme from config:
        //StswResources.AvailableThemes.Remove(StswTheme.Auto);
        //StswResources.AvailableThemes.Remove(StswTheme.Pink);
        //StswSettings.Default.Theme = (int)StswTheme.Dark;
    }

    private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) => StswLog.Write(StswInfoType.Error, e.Exception.ToString());

    private static void OpenHelp() => StswFn.OpenFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources\manual_en.pdf"));
}
