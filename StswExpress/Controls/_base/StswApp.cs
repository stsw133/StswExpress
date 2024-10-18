using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace StswExpress;

/// <summary>
/// Represents a custom application class with additional functionality and customization options.
/// </summary>
public class StswApp : Application
{
    /// <summary>
    /// Starting method that sets up various aspects of the application such as the theme, resources, commands, culture, and a callback for when the application exits.
    /// </summary>
    /// <param name="e">The event arguments for the startup event.</param>
    protected override void OnStartup(StartupEventArgs e)
    {
        /// close duplicated application
        if (!AllowMultipleInstances)
        {
            var thisProcessName = Process.GetCurrentProcess().ProcessName;
            var otherInstances = Process.GetProcesses().Where(x => x.ProcessName == thisProcessName && x.GetUser() == Environment.UserName);
            if (otherInstances.Count() > 1)
            {
                Current.Shutdown();

                if (otherInstances.FirstOrDefault(x => x.Id != Environment.ProcessId) is Process originalProcess)
                {
                    if (originalProcess.MainWindowHandle != IntPtr.Zero)
                    {
                        if (IsIconic(originalProcess.MainWindowHandle))
                            ShowWindow(originalProcess.MainWindowHandle, SW_RESTORE);

                        SetForegroundWindow(originalProcess.MainWindowHandle);
                    }
                }

                return;
            }
        }

        base.OnStartup(e);

        /// merged dictionaries (themes)
        var dict = Resources.MergedDictionaries.FirstOrDefault(x => x.Source == new Uri($"/{nameof(StswExpress)};component/StswResources.xaml", UriKind.Relative));
        dict ??= Resources.MergedDictionaries.FirstOrDefault(x => x is StswResources);
        Current.Resources.MergedDictionaries.Remove(dict);
        Current.Resources.MergedDictionaries.Add(new StswResources((StswTheme)StswSettings.Default.Theme));

        /// language
        StswTranslator.CurrentLanguage = string.IsNullOrEmpty(StswSettings.Default.Language) ? CultureInfo.InstalledUICulture.TwoLetterISOLanguageName : StswSettings.Default.Language;

        var trFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "translations.stsw.json");

        using (var stream = GetResourceStream(new Uri($"/{nameof(StswExpress)};component/Translator/Translations.json", UriKind.Relative)).Stream)
        using (var reader = new StreamReader(stream))
        {
            if (!Directory.Exists(Path.GetDirectoryName(trFileName)))
                Directory.CreateDirectory(Path.GetDirectoryName(trFileName)!);

            File.WriteAllText(trFileName, reader.ReadToEnd());
        }

        if (File.Exists(trFileName))
            Task.Run(() => StswTranslatorLanguagesLoader.Instance.AddFileAsync(trFileName));

        /// global culture (does not work with converters)
        //Thread.CurrentThread.CurrentCulture = CultureInfo.CurrentCulture;
        //Thread.CurrentThread.CurrentUICulture = CultureInfo.CurrentCulture;
        //FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
    }

    /// <summary>
    /// Method called when the theme is changed. Can be overridden to provide custom behavior.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The new theme.</param>
    public virtual void OnThemeChanged(object? sender, StswTheme e)
    {
        //;
    }

    /// <summary>
    /// Gets the current application's main <see cref="StswWindow"/>.
    /// </summary>
    public static StswWindow StswWindow => (StswWindow)Current.MainWindow;

    /// <summary>
    /// Gets or sets a value indicating whether running multiple instances of the application is allowed.
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
    private static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int SW_RESTORE = 9;
    private const int SW_MAXIMIZE = 3;
}
