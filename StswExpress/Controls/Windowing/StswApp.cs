using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// Represents a custom application class with additional functionality and customization options.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
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
        Current.Resources.MergedDictionaries.Add(new StswResources((StswTheme)StswSettings.Default.Theme));

        /// language
        StswTranslator.Instance.CurrentLanguage = string.IsNullOrEmpty(StswSettings.Default.Language) ? CultureInfo.InstalledUICulture.TwoLetterISOLanguageName : StswSettings.Default.Language;

        var trFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "translations.stsw.json");

        using (var stream = GetResourceStream(new Uri($"pack://application:,,,/{nameof(StswExpress)};component//Translator/Translations.json", UriKind.RelativeOrAbsolute)).Stream)
        using (var reader = new StreamReader(stream))
        {
            if (!Directory.Exists(Path.GetDirectoryName(trFileName)))
                Directory.CreateDirectory(Path.GetDirectoryName(trFileName)!);

            File.WriteAllText(trFileName, reader.ReadToEnd());
        }

        if (File.Exists(trFileName))
            StswTranslatorLanguagesLoader.Instance.AddFile(trFileName);

        /// global commands
        var commandBinding = new RoutedUICommand("Help", "Help", typeof(StswWindow), new InputGestureCollection() { new KeyGesture(Key.F1) });
        CommandManager.RegisterClassCommandBinding(typeof(StswWindow), new CommandBinding(commandBinding, (s, e) => OpenHelp?.Invoke()));

        /// global culture (does not work with converters)
        //Thread.CurrentThread.CurrentCulture = CultureInfo.CurrentCulture;
        //Thread.CurrentThread.CurrentUICulture = CultureInfo.CurrentCulture;
        //FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

        /// on exit
        //Exit += (sender, e) => StswSettings.Default.Save();
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
