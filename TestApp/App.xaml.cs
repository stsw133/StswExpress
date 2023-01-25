global using StswExpress;
using System;
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
        StswFn.AppStart(this, "stsw");
        CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(StswGlobalCommands.Help, (s, e) => StswFn.OpenFile(AppDomain.CurrentDomain.BaseDirectory + @"/Resources/manual_en.pdf")));
    }
}
