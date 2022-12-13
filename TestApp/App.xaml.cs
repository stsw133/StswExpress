using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace TestApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        StswFn.AppStart(this);
        
        var bindingHelp = new CommandBinding(StswCommands.Help, CmdHelp_Executed, CmdHelp_CanExecute);
        CommandManager.RegisterClassCommandBinding(typeof(Window), bindingHelp);

        StswFn.AppDB = Global.DBs.FirstOrDefault();
    }
    private void CmdHelp_Executed(object sender, ExecutedRoutedEventArgs e) => StswFn.OpenFile(AppDomain.CurrentDomain.BaseDirectory + @"/Resources/manual_en.pdf");
    private void CmdHelp_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
}
