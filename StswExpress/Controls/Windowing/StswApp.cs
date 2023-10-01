using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// Represents a custom application class with additional functionality and customization options.
/// </summary>
public class StswApp : Application
{
    /// <summary>
    /// Starting method that sets up various aspects of the application such as the theme, resources, commands, culture, and a callback for when the application exits.
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        /// hash key
        //StswSecurity.Key = hashKey;

        /// import databases
        //StswDatabase.ImportDatabases();
        //StswDatabase.CurrentDatabase = StswDatabase.AllDatabases.FirstOrDefault() ?? new();

        /// merged dictionaries
        var dict = Resources.MergedDictionaries.FirstOrDefault(x => x.Source == new Uri("/StswExpress;component/StswResources.xaml", UriKind.RelativeOrAbsolute));
        dict ??= Resources.MergedDictionaries.FirstOrDefault(x => x is StswResources);
        Current.Resources.MergedDictionaries.Remove(dict);
        Current.Resources.MergedDictionaries.Add(new StswResources(Settings.Default.Theme < 0 ? StswFn.GetWindowsTheme() : (StswTheme)Settings.Default.Theme));

        /// global commands
        var commandBinding = new RoutedUICommand("Help", "Help", typeof(StswWindow), new InputGestureCollection() { new KeyGesture(Key.F1) });
        CommandManager.RegisterClassCommandBinding(typeof(StswWindow), new CommandBinding(commandBinding, (s, e) => OpenHelp?.Invoke()));

        /// global culture (does not work with converters)
        //Thread.CurrentThread.CurrentCulture = CultureInfo.CurrentCulture;
        //Thread.CurrentThread.CurrentUICulture = CultureInfo.CurrentCulture;
        //FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

        /// on exit
        Exit += (sender, e) => Settings.Default.Save();
    }

    /// <summary>
    /// Current application's main StswWindow.
    /// </summary>
    public static StswWindow StswWindow => (StswWindow)Current.MainWindow;

    /// <summary>
    /// Open current application's help.
    /// </summary>
    public static Action? OpenHelp;
}
