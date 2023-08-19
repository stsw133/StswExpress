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
        if (!Resources.MergedDictionaries.Any(x => x is Theme))
            Current.Resources.MergedDictionaries.Add(new Theme()
            {
                Color = StswSettings.Default.Theme < 0 ? StswFn.GetWindowsTheme() : (ThemeColor)StswSettings.Default.Theme
            });

        if (!Resources.MergedDictionaries.Any(x => x.Source == new Uri("pack://application:,,,/StswExpress;component/Themes/Generic.xaml")))
            Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/StswExpress;component/Themes/Generic.xaml")
            });

        /// global commands
        var cmdFullscreen = new RoutedUICommand("Fullscreen", "Fullscreen", typeof(StswWindow), new InputGestureCollection() { new KeyGesture(Key.F11) });
        CommandManager.RegisterClassCommandBinding(typeof(StswWindow), new CommandBinding(cmdFullscreen, (s, e) => {
            if (s is StswWindow stsw)
                stsw.Fullscreen = !stsw.Fullscreen;
        }));

        /// global culture (does not work with converters)
        //Thread.CurrentThread.CurrentCulture = CultureInfo.CurrentCulture;
        //Thread.CurrentThread.CurrentUICulture = CultureInfo.CurrentCulture;
        //FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

        /// on exit
        Exit += (sender, e) => StswSettings.Default.Save();
    }
}
