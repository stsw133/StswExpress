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
        CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(StswCommands.Help, (s, e) => StswFn.OpenFile(AppDomain.CurrentDomain.BaseDirectory + @"/Resources/manual_en.pdf")));
        StswFn.AppDB = Global.DBs.FirstOrDefault();
    }
}
