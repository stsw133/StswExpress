global using StswExpress;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace TestApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private void Application_Startup(object sender, StartupEventArgs e)
    {
        StswFn.AppStart(this, "myOwnStswHashKey");

        StswDatabase.ImportDatabases();
        StswDatabase.CurrentDatabase = StswDatabase.AllDatabases.FirstOrDefault() ?? new();

        MainWindow = new MainWindow();
        MainWindow.Show();
    }

    private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        StswLog.Write(StswLogType.Error, e.Exception.ToString());
    }
}
