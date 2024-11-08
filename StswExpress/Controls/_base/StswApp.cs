using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace StswExpress;

/// <summary>
/// Represents a custom application class with additional functionality and customization options, 
/// including single-instance enforcement and inter-process communication for restoring a minimized or hidden instance.
/// </summary>
public class StswApp : Application
{
    private const string PipeName = "StswApp_SingleInstance_Pipe";

    /// <summary>
    /// Entry point that configures application settings, prevents multiple instances, 
    /// initializes resources, data templates, and translations, and sets global culture settings.
    /// </summary>
    /// <param name="e">The startup event arguments.</param>
    protected override void OnStartup(StartupEventArgs e)
    {
        if (!AllowMultipleInstances && CheckForExistingInstance())
        {
            SendRestoreCommandToExistingInstance();
            Current.Shutdown();
            return;
        }
        StartListeningForRestoreCommand();

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
    /// Checks if another instance of the application is already running under the current user's session.
    /// </summary>
    /// <returns><see langword="true"/> if an existing instance is found; otherwise, <see langword="false"/>.</returns>
    private bool CheckForExistingInstance()
    {
        var thisProcessName = Process.GetCurrentProcess().ProcessName;
        return Process.GetProcessesByName(thisProcessName).Any(x => x.Id != Environment.ProcessId && x.GetUser() == Environment.UserName);
    }

    /// <summary>
    /// Sends a restore command to the existing application instance using a named pipe, prompting it to restore its window.
    /// </summary>
    private void SendRestoreCommandToExistingInstance()
    {
        using (var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out))
        {
            try
            {
                client.Connect(200);
                using var writer = new StreamWriter(client);
                writer.WriteLine("Restore");
            }
            catch
            {
                // Log or handle exceptions, such as connection failure, if necessary
            }
        }
    }

    /// <summary>
    /// Asynchronously starts a named pipe server to listen for commands from additional application instances, 
    /// restoring the main window if a "Restore" command is received.
    /// </summary>
    private void StartListeningForRestoreCommand()
    {
        Task.Run(() =>
        {
            while (true)
            {
                using (var server = new NamedPipeServerStream(PipeName, PipeDirection.In))
                {
                    try
                    {
                        server.WaitForConnection();

                        using (var reader = new StreamReader(server))
                        {
                            var command = reader.ReadLine();
                            if (command == "Restore")
                            {
                                Current.Dispatcher.Invoke(() =>
                                {
                                    if (Current.MainWindow != null)
                                    {
                                        Current.MainWindow.Show();
                                        if (Current.MainWindow.WindowState == WindowState.Minimized)
                                            Current.MainWindow.WindowState = WindowState.Normal;
                                        Current.MainWindow.Activate();
                                    }
                                });
                            }
                        }
                    }
                    catch
                    {
                        // Handle exceptions if needed, such as IO errors when disconnecting
                    }
                }
            }
        });
    }

    /// <summary>
    /// Adds or updates the main theme resource dictionary in the application's MergedDictionaries collection.
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
    /// Configures language translations based on user settings, and loads them from the application's default translation file.
    /// Creates the file if it does not already exist.
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
    /// Dynamically registers data templates for each context-view pair within the application assembly.
    /// Maps each context type ending with "Context" to a view type ending with "View" if available.
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
    /// Virtual method called when the theme is changed, allowing derived classes to provide custom behavior on theme changes.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The new theme instance.</param>
    public virtual void OnThemeChanged(object? sender, StswTheme e)
    {
        //;
    }

    /// <summary>
    /// Gets the current application's main <see cref="StswWindow"/>, throwing an exception if it is not of the expected type.
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
}
