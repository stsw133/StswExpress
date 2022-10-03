using DynamicAero2;
using StswExpress;
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
        if (!Current.Resources.MergedDictionaries.Any(x => x is Theme))
            Current.Resources.MergedDictionaries.Add(new Theme());
        ((Theme)Current.Resources.MergedDictionaries.First(x => x is Theme)).Color = (ThemeColor)Settings.Default.Theme;
        
        var bindingHelp = new CommandBinding(Commands.Help, CmdHelp_Executed, CmdHelp_CanExecute);
        CommandManager.RegisterClassCommandBinding(typeof(Window), bindingHelp);

        Fn.AppDB = Global.DBs.FirstOrDefault();
    }
    private void CmdHelp_Executed(object sender, ExecutedRoutedEventArgs e) => Fn.OpenFile(AppDomain.CurrentDomain.BaseDirectory + @"/Resources/manual_en.pdf");
    private void CmdHelp_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
}
