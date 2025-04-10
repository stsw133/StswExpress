﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// Represents a custom application class with additional functionality and customization options, 
/// including single-instance enforcement, inter-process communication for restoring a minimized instance,
/// automatic registration of data templates, and resource initialization.
/// </summary>
/// <remarks>
/// If <see cref="AllowMultipleInstances"/> is set to <see langword="false"/>, only one instance of the application
/// is allowed per user session. If a second instance is launched, it will attempt to bring the existing instance to the foreground.
/// </remarks>
public class StswApp : Application
{
    /// <summary>
    /// Entry point that configures application settings, prevents multiple instances if needed, 
    /// initializes resources, loads translations asynchronously, and registers data templates if enabled.
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

        EventManager.RegisterClassHandler(typeof(StswWindow), Keyboard.PreviewKeyDownEvent, new KeyEventHandler(GlobalPreviewKeyDownHandler));
        Task.Run(StswTranslator.LoadTranslationsForCurrentLanguageAsync);
        StswResources.InitializeResources(Resources);
        if (IsRegisterDataTemplatesEnabled)
            RegisterDataTemplates(ContextSuffix, ViewSuffix);
    }

    /// <summary>
    /// Attempts to find and activate a system tray window if the main window is hidden or not directly accessible.
    /// </summary>
    /// <param name="process">The process instance of the running application.</param>
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
    /// Checks if another instance of the application is already running under the current user's session.
    /// If found, attempts to restore the main window of the existing instance.
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
    /// Handles the global preview key down event.
    /// Toggles the fullscreen mode of the window when the F11 key is pressed.
    /// </summary>
    /// <param name="sender">The source of the event, typically a window.</param>
    /// <param name="e">The key event arguments containing information about the key press.</param>
    private void GlobalPreviewKeyDownHandler(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F11)
        {
            if (sender is StswWindow stswWindow)
            {
                stswWindow.Fullscreen = !stswWindow.Fullscreen;
                e.Handled = true;
            }
        }
    }

    /// <summary>
    /// Restores the main window of an existing application instance, bringing it to the foreground.
    /// If the window is minimized, it is restored.
    /// </summary>
    /// <param name="process">The process instance of the running application.</param>
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
    /// Dynamically registers data templates for each context-view pair within the application assembly.
    /// Maps each context type ending with <see cref="ContextSuffix"/> to a view type ending with <see cref="ViewSuffix"/> if available.
    /// </summary>
    /// <param name="contextSuffix">The suffix used to identify context classes.</param>
    /// <param name="viewSuffix">The suffix used to identify view classes.</param>
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
    /// Gets the current application's main <see cref="StswWindow"/> instance.
    /// Throws an exception if the main window is not of the expected type.
    /// </summary>
    public static StswWindow StswWindow => Current.MainWindow as StswWindow ?? throw new InvalidOperationException($"Main window is not of type {nameof(StswWindow)}.");

    /// <summary>
    /// Gets or sets a value indicating whether running multiple instances of the application is allowed.
    /// When set to <see langword="false"/>, a second instance will attempt to bring the first instance to the foreground and then shut down.
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

    /// <summary>
    /// Gets or sets a value indicating whether automatic registration of data templates is enabled.
    /// </summary>
    public bool IsRegisterDataTemplatesEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the suffix used to identify context types when registering data templates.
    /// Defaults to "Context".
    /// </summary>
    public string ContextSuffix { get; set; } = "Context";

    /// <summary>
    /// Gets or sets the suffix used to identify view types when registering data templates.
    /// Defaults to "View".
    /// </summary>
    public string ViewSuffix { get; set; } = "View";



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
