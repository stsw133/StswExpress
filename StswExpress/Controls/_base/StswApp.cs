using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace StswExpress;

/// <summary>
/// Represents a custom application class with additional functionality and customization options.
/// </summary>
public class StswApp : Application
{
    // <summary>
    /// Starting method that sets up various aspects of the application, including preventing duplicate instances,
    /// initializing resources, registering data templates, setting translations, and configuring global culture settings.
    /// </summary>
    /// <param name="e">The event arguments for the startup event.</param>
    protected override void OnStartup(StartupEventArgs e)
    {
        /// close duplicated application
        if (!AllowMultipleInstances)
        {
            var thisProcessName = Process.GetCurrentProcess().ProcessName;
            var otherInstance = Process.GetProcessesByName(thisProcessName).FirstOrDefault(x => x.Id != Environment.ProcessId && x.GetUser() == Environment.UserName);

            if (otherInstance != null)
            {
                Current.Shutdown();

                if (otherInstance.MainWindowHandle != IntPtr.Zero)
                {
                    if (IsIconic(otherInstance.MainWindowHandle))
                        ShowWindow(otherInstance.MainWindowHandle, SW_RESTORE);

                    SetForegroundWindow(otherInstance.MainWindowHandle);
                }

                return;
            }
        }

        base.OnStartup(e);

        InitializeResources();
        RegisterDataTemplates();
        InitializeTranslations();

        /// global culture (does not work with converters)
        //Thread.CurrentThread.CurrentCulture = CultureInfo.CurrentCulture;
        //Thread.CurrentThread.CurrentUICulture = CultureInfo.CurrentCulture;
        //FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
    }

    /// <summary>
    /// Initializes application resources by adding or replacing the main theme resource dictionary in the MergedDictionaries collection.
    /// </summary>
    private void InitializeResources()
    {
        var themeUri = new Uri($"/{nameof(StswExpress)};component/StswResources.xaml", UriKind.Relative);
        var dictIndex = Resources.MergedDictionaries
            .Select((x, index) => new { x, index })
            .FirstOrDefault(d => d.x.Source == themeUri)?.index;

        if (dictIndex.HasValue)
            Resources.MergedDictionaries[dictIndex.Value] = new StswResources((StswTheme)StswSettings.Default.Theme);
        else
            Resources.MergedDictionaries.Add(new StswResources((StswTheme)StswSettings.Default.Theme));
    }

    /// <summary>
    /// Sets up language translations based on user settings and loads them from the application's default translation file.
    /// If the target file path doesn't exist, it will be created.
    /// </summary>
    private void InitializeTranslations()
    {
        StswTranslator.CurrentLanguage = string.IsNullOrEmpty(StswSettings.Default.Language)
            ? CultureInfo.InstalledUICulture.TwoLetterISOLanguageName
            : StswSettings.Default.Language;

        var trFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "translations.stsw.json");

        using (var stream = GetResourceStream(new Uri($"/{nameof(StswExpress)};component/Translator/Translations.json", UriKind.Relative)).Stream)
        using (var reader = new StreamReader(stream))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(trFileName)!);
            File.WriteAllText(trFileName, reader.ReadToEnd());
        }

        if (File.Exists(trFileName))
            Task.Run(() => StswTranslatorLanguagesLoader.Instance.AddFileAsync(trFileName));
    }

    /// <summary>
    /// Registers data templates dynamically for each context-view pair found in the application assembly.
    /// Searches for types ending with "Context" and generates corresponding <see cref="DataTemplate"/>s
    /// by mapping each context to a view with the same base name but ending with "View".
    /// Adds each <see cref="DataTemplate"/> to the application's resources if it is not already present.
    /// </summary>
    private void RegisterDataTemplates()
    {
        if (Assembly.GetEntryAssembly()?.GetTypes() is Type[] types)
        {
            var typeDictionary = types.ToDictionary(x => x.FullName!);

            foreach (var context in types.Where(x => x.Name.EndsWith("Context")))
            {
                var viewName = context.Namespace + "." + context.Name[..^7] + "View";
                var dataTemplateKey = new DataTemplateKey(context);

                if (typeDictionary.TryGetValue(viewName, out var viewType) && !Current.Resources.Contains(dataTemplateKey))
                {
                    var dataTemplate = new DataTemplate
                    {
                        DataType = context,
                        VisualTree = new FrameworkElementFactory(viewType)
                    };
                    Current.Resources.Add(dataTemplateKey, dataTemplate);
                }
            }
        }
    }

    /// <summary>
    /// Method called when the theme is changed. Can be overridden to provide custom behavior on theme changes.
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
    public static StswWindow StswWindow => Current.MainWindow as StswWindow ?? throw new InvalidOperationException($"Main window is not of type {nameof(StswWindow)}.");

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
