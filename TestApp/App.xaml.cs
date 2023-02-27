global using StswExpress;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace TestApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private void Application_Startup(object sender, StartupEventArgs e)
    {
        StswFn.AppStart(this, "mysaltkey", "myhashkey");
        CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(StswGlobalCommands.Help, (s, e) => StswFn.OpenFile(AppDomain.CurrentDomain.BaseDirectory + @"/Resources/manual_en.pdf")));

        MainWindow = new MainWindow();
        MainWindow.Show();
    }

    private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        StswLog.Write(e.Exception.ToString());
    }
}
