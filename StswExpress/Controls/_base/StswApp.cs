using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;

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
    /// <param name="e">The event arguments</param>
    protected override void OnStartup(StartupEventArgs e)
    {
        /// close duplicated application
        if (!AllowMultipleInstances)
        {
            var thisProcessName = Process.GetCurrentProcess().ProcessName;
            var otherInstances = Process.GetProcesses().Where(x => x.ProcessName == thisProcessName && StswFn.GetProcessUser(x) == Environment.UserName);
            if (otherInstances.Count() > 1)
            {
                Current.Shutdown();

                if (otherInstances.FirstOrDefault(x => x.Id != Process.GetCurrentProcess().Id) is Process originalProcess)
                {
                    if (originalProcess.MainWindowHandle != IntPtr.Zero)
                        SetForegroundWindow(originalProcess.MainWindowHandle);
                }

                return;
            }
        }

        base.OnStartup(e);

        /// hash key
        //StswSecurity.Key = hashKey;

        /// import databases
        //StswDatabase.ImportDatabases();
        //StswDatabase.CurrentDatabase = StswDatabase.AllDatabases.FirstOrDefault() ?? new();

        /// merged dictionaries (themes)
        var dict = Resources.MergedDictionaries.FirstOrDefault(x => x.Source == new Uri("/StswExpress;component/StswResources.xaml", UriKind.RelativeOrAbsolute));
        dict ??= Resources.MergedDictionaries.FirstOrDefault(x => x is StswResources);
        Current.Resources.MergedDictionaries.Remove(dict);
        Current.Resources.MergedDictionaries.Add(new StswResources((StswTheme)StswSettings.Default.Theme));

        /// language
        StswTranslator.CurrentLanguage = string.IsNullOrEmpty(StswSettings.Default.Language) ? CultureInfo.InstalledUICulture.TwoLetterISOLanguageName : StswSettings.Default.Language;

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

        /// global culture (does not work with converters)
        //Thread.CurrentThread.CurrentCulture = CultureInfo.CurrentCulture;
        //Thread.CurrentThread.CurrentUICulture = CultureInfo.CurrentCulture;
        //FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public virtual void OnThemeChanged(object? sender, StswTheme e)
    {
        //;
    }

    /// <summary>
    /// Current application's main StswWindow.
    /// </summary>
    public static StswWindow StswWindow => (StswWindow)Current.MainWindow;

    /// <summary>
    /// Gets or sets if running multiple instances of application is allowed or not.
    /// </summary>
    public bool AllowMultipleInstances
    {
        get => _allowMultipleInstances;
        set
        {
            VerifyAccess();
            _allowMultipleInstances = value;
        }
    }
    private bool _allowMultipleInstances = true;



    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
}
