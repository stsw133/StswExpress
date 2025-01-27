using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace StswExpress;

/// <summary>
/// Represents a custom application class with additional functionality and customization options, 
/// including single-instance enforcement and inter-process communication for restoring a minimized or hidden instance.
/// </summary>
public class StswApp : Application
{
    /// <summary>
    /// Entry point that configures application settings, prevents multiple instances, 
    /// initializes resources, data templates, and translations, and sets global culture settings.
    /// </summary>
    /// <param name="e">The startup event arguments.</param>
    protected override void OnStartup(StartupEventArgs e)
    {
        if (!AllowMultipleInstances && CheckForExistingInstance())
        {
            Current.Shutdown();
            return;
        }

        base.OnStartup(e);

        InitializeResources();
        RegisterDataTemplates("Context", "View");
        Task.Run(StswTranslator.LoadTranslationsForCurrentLanguageAsync);

        /// global culture (does not work with converters)
        //Thread.CurrentThread.CurrentCulture = CultureInfo.CurrentCulture;
        //Thread.CurrentThread.CurrentUICulture = CultureInfo.CurrentCulture;
        //FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
    }

    /// <summary>
    /// Checks if another instance of the application is already running under the current user's session.
    /// If found, restores the main window of the existing instance.
    /// </summary>
    /// <returns><see langword="true"/> if an existing instance is found and restored; otherwise, <see langword="false"/>.</returns>
    private bool CheckForExistingInstance()
    {
        var thisProcessName = Process.GetCurrentProcess().ProcessName;
        var currentUser = Environment.UserName;

        var existingProcess = Process.GetProcessesByName(thisProcessName)
            .FirstOrDefault(x => x.Id != Environment.ProcessId && x.GetUser() == currentUser);

        if (existingProcess != null)
        {
            RestoreWindow(existingProcess);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Restores the window of an existing application instance.
    /// </summary>
    /// <param name="hWnd">The handle to the window.</param>
    private void RestoreWindow(Process process)
    {
        IntPtr hWnd = process.MainWindowHandle;

        if (hWnd != IntPtr.Zero)
        {
            ShowWindow(hWnd, SW_RESTORE);
            SetForegroundWindow(hWnd);
        }
        else
        {
            ActivateTrayWindow(process);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void ActivateTrayWindow(Process process)
    {
        foreach (ProcessThread thread in process.Threads)
        {
            EnumThreadWindows(thread.Id, (hWnd, lParam) =>
            {
                if (IsWindowVisible(hWnd))
                {
                    ShowWindow(hWnd, SW_RESTORE);
                    SetForegroundWindow(hWnd);
                    return false;
                }
                return true;
            }, IntPtr.Zero);
        }
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
    /// Dynamically registers data templates for each context-view pair within the application assembly.
    /// Maps each context type ending with "Context" to a view type ending with "View" if available.
    /// </summary>
    private static void RegisterDataTemplates(string contextSuffix, string viewSuffix)
    {
        if (Assembly.GetEntryAssembly()?.GetTypes() is Type[] types)
        {
            var typeDictionary = types.ToDictionary(x => x.FullName!);

            foreach (var context in types.Where(x => x.Name.EndsWith(contextSuffix)))
            {
                var viewName = $"{context.Namespace}.{context.Name[..^contextSuffix.Length]}{viewSuffix}";
                var dataTemplateKey = new DataTemplateKey(context);

                if (typeDictionary.TryGetValue(viewName, out var viewType) && !Current.Resources.Contains(dataTemplateKey))
                {
                    var dataTemplate = new DataTemplate(context) { VisualTree = new FrameworkElementFactory(viewType) };
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



    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool EnumThreadWindows(int dwThreadId, EnumThreadWndProc lpfn, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int SW_RESTORE = 9;
    private delegate bool EnumThreadWndProc(IntPtr hWnd, IntPtr lParam);
}
