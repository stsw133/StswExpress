using Microsoft.Win32;
using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace StswExpress;
/// <summary>
/// A system tray icon control that supports context menus, notifications, and custom icons.
/// Allows minimizing the application to the system tray and displaying balloon tooltips.
/// </summary>
/// <remarks>
/// The control manages application state visibility and interaction with the system tray.
/// </remarks>
[StswInfo("0.1.0", Changes = StswPlannedChanges.Refactor | StswPlannedChanges.NewFeatures)]
public class StswNotifyIcon : FrameworkElement
{
    private NotifyIcon? _tray;
    private Window? _window;

    public StswNotifyIcon()
    {
        Loaded += Initialize;
        Unloaded += Cleanup;
    }

    #region Events & methods
    /// <summary>
    /// Initializes the <see cref="NotifyIcon"/> instance and sets up event handlers for tray icon actions and application state changes.
    /// </summary>
    /// <param name="sender">The sender object triggering the event.</param>
    /// <param name="e">Event arguments.</param>
    private void Initialize(object? sender, RoutedEventArgs e)
    {
        _window = ContextControl as Window ?? Window.GetWindow(this);
        if (_window == null)
            return;

        _window.StateChanged += HandleWindowStateChange;
        System.Windows.Application.Current.Exit += OnApplicationExit;

        if (Icon == null && IconPath == null)
            throw new Exception($"{nameof(Icon)} or {nameof(IconPath)} cannot be null!");

        _tray = new()
        {
            Icon = Icon ?? LoadIcon(IconPath),
            Text = Text,
            Visible = true
        };

        _tray.BalloonTipClicked += (_, _) => ShowWindow();
        _tray.MouseDoubleClick += (_, _) => ShowWindow();
        _tray.MouseDown += (_, e) =>
        {
            if (e.Button == MouseButtons.Right)
                ShowContextMenu();
        };

        UpdateIconVisibility();
    }

    /// <summary>
    /// Cleans up resources related to the <see cref="NotifyIcon"/> instance and detaches event handlers when the control is unloaded.
    /// </summary>
    /// <param name="sender">The sender object triggering the event.</param>
    /// <param name="e">Event arguments.</param>
    private void Cleanup(object? sender, RoutedEventArgs e)
    {
        _tray?.Dispose();
        if (_window != null)
            _window.StateChanged -= HandleWindowStateChange;
        System.Windows.Application.Current.Exit -= OnApplicationExit;
    }

    /// <summary>
    /// Handles window state changes, hiding the window when minimized if <see cref="IsAlwaysVisible"/> is false.
    /// Ensures the tray icon remains visible.
    /// </summary>
    /// <param name="sender">The sender object triggering the event.</param>
    /// <param name="e">Event arguments.</param>
    private void HandleWindowStateChange(object? sender, EventArgs e)
    {
        if (_window == null || _tray == null || !IsEnabled)
            return;

        if (_window.WindowState == WindowState.Minimized && !IsAlwaysVisible)
            _window.Hide();

        _tray.Visible = true;
    }

    /// <summary>
    /// Loads an <see cref="System.Drawing.Icon"/> from a specified file path, enabling dynamic icon assignment.
    /// </summary>
    /// <param name="path">Path to the icon file.</param>
    /// <returns>An <see cref="Icon"/> instance loaded from the specified file.</returns>
    private static Icon LoadIcon(string path)
    {
        using var stream = System.Windows.Application.GetResourceStream(new Uri(path, UriKind.RelativeOrAbsolute)).Stream;
        return new Icon(stream);
    }

    /// <summary>
    /// Displays a notification balloon with a specified title, text, and icon in the system tray.
    /// </summary>
    /// <param name="title">The notification title text.</param>
    /// <param name="text">The notification content text.</param>
    /// <param name="icon">The icon type for the notification balloon.</param>
    public void Notify(string? title, string? text, ToolTipIcon? icon)
    {
        var timeout = ((int?)Registry.GetValue("HKEY_CURRENT_USER\\Control Panel\\Accessibility", "MessageDuration", 5) ?? 5) * 1000;
        if (!string.IsNullOrEmpty(text))
            _tray?.ShowBalloonTip(timeout, title ?? string.Empty, text, icon ?? ToolTipIcon.None);
    }

    /// <summary>
    /// Handles application exit, releasing any resources tied to the <see cref="NotifyIcon"/> instance.
    /// </summary>
    /// <param name="sender">The sender object triggering the event.</param>
    /// <param name="e">Event arguments.</param>
    private void OnApplicationExit(object? sender, ExitEventArgs e) => _tray?.Dispose();

    /// <summary>
    /// Displays the context menu for the system tray icon, setting its data context from the control’s context, if available.
    /// </summary>
    private void ShowContextMenu()
    {
        if (ContextMenu == null)
            return;

        if (PresentationSource.FromVisual(ContextMenu) is HwndSource hwndSource)
            _ = SetForegroundWindow(hwndSource.Handle);

        ContextMenu.DataContext = ContextControl is FrameworkElement fe ? fe.DataContext : null;
        ContextMenu.IsOpen = true;
    }

    /// <summary>
    /// Restores the main window from a minimized state, brings it to the foreground, and optionally hides the tray icon.
    /// </summary>
    private void ShowWindow()
    {
        if (_window == null || _tray == null)
            return;

        _window.Show();
        _window.WindowState = WindowState.Normal;
        _window.Activate();

        if (!IsAlwaysVisible)
            Task.Delay(100).ContinueWith(_ => _tray.Visible = false);
    }

    /// <summary>
    /// Adjusts the system tray icon visibility based on the window state and the value of <see cref="IsAlwaysVisible"/>.
    /// </summary>
    private void UpdateIconVisibility() => _tray!.Visible = IsAlwaysVisible || _window?.IsVisible != true;
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the parent UI element that acts as the data context source for the <see cref="NotifyIcon"/>.
    /// </summary>
    public UIElement ContextControl
    {
        get => (UIElement)GetValue(ContextControlProperty);
        set => SetValue(ContextControlProperty, value);
    }
    public static readonly DependencyProperty ContextControlProperty
        = DependencyProperty.Register(
            nameof(ContextControl),
            typeof(UIElement),
            typeof(StswNotifyIcon)
        );

    /// <summary>
    /// Gets or sets the <see cref="Icon"/> to be displayed in the <see cref="NotifyIcon"/> control.
    /// </summary>
    public Icon Icon
    {
        get => (Icon)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    public static readonly DependencyProperty IconProperty
        = DependencyProperty.Register(
            nameof(Icon),
            typeof(Icon),
            typeof(StswNotifyIcon),
            new PropertyMetadata(default(Icon), OnIconChanged)
        );
    private static void OnIconChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswNotifyIcon stsw)
            return;

        if (stsw._tray != null)
            stsw._tray.Icon = stsw.Icon;
    }

    /// <summary>
    /// Gets or sets the path to the icon file to be displayed in the <see cref="NotifyIcon"/> control.
    /// </summary>
    public string IconPath
    {
        get => (string)GetValue(IconPathProperty);
        set => SetValue(IconPathProperty, value);
    }
    public static readonly DependencyProperty IconPathProperty
        = DependencyProperty.Register(
            nameof(IconPath),
            typeof(string),
            typeof(StswNotifyIcon),
            new PropertyMetadata(default(string), OnIconPathChanged)
        );
    private static void OnIconPathChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswNotifyIcon stsw)
            return;

        if (stsw._tray != null)
            stsw._tray.Icon = stsw.Icon ?? LoadIcon(stsw.IconPath);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the <see cref="NotifyIcon"/> remains visible even when the associated window is minimized.
    /// </summary>
    [StswInfo("0.7.0")]
    public bool IsAlwaysVisible
    {
        get => (bool)GetValue(IsAlwaysVisibleProperty);
        set => SetValue(IsAlwaysVisibleProperty, value);
    }
    public static readonly DependencyProperty IsAlwaysVisibleProperty
        = DependencyProperty.Register(
            nameof(IsAlwaysVisible),
            typeof(bool),
            typeof(StswNotifyIcon),
            new PropertyMetadata(default(bool), OnIsAlwaysVisibleChanged)
        );
    private static void OnIsAlwaysVisibleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswNotifyIcon stsw)
            return;

        if (stsw._tray != null)
            stsw.UpdateIconVisibility();
    }

    /// <summary>
    /// Gets or sets the text to be displayed as a tooltip for the icon in the <see cref="NotifyIcon"/> control.
    /// </summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    public static readonly DependencyProperty TextProperty
        = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(StswNotifyIcon),
            new PropertyMetadata(default(string), OnTextChanged)
        );
    private static void OnTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswNotifyIcon stsw)
            return;

        if (stsw._tray != null)
            stsw._tray.Text = stsw.Text;
    }

    /// <summary>
    /// Gets or sets the notification tip model, containing title, text, and icon information, displayed in the system tray.
    /// </summary>
    public StswNotifyIconTip Tip
    {
        get => (StswNotifyIconTip)GetValue(TipProperty);
        set => SetValue(TipProperty, value);
    }
    public static readonly DependencyProperty TipProperty
        = DependencyProperty.Register(
            nameof(Tip),
            typeof(StswNotifyIconTip),
            typeof(StswNotifyIcon),
            new PropertyMetadata(default(StswNotifyIconTip), OnTipChanged)
        );
    private static void OnTipChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswNotifyIcon stsw)
            return;

        stsw.Notify(stsw.Tip.TipTitle, stsw.Tip.TipText, stsw.Tip.TipIcon);
    }
    #endregion

    #region Static instance
    private static NotifyIcon? _staticTray;

    /// <summary>
    /// Initializes a static instance of the <see cref="NotifyIcon"/> with a specified icon and tooltip text, making it available globally.
    /// </summary>
    /// <param name="icon">Icon to be displayed in the tray.</param>
    /// <param name="text">Tooltip text for the tray icon.</param>
    [StswInfo("0.7.0")]
    private static void InitStaticIcon(Icon? icon, string? text)
    {
        if (_staticTray == null)
        {
            _staticTray = new();
            _staticTray.Click += (_, _) => _staticTray.Visible = false;
            _staticTray.MouseClick += (_, _) => _staticTray.Visible = false;
            System.Windows.Application.Current.Exit += (_, _) => _staticTray?.Dispose();
        }
        _staticTray.Icon = icon ?? Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly()!.ManifestModule.Name);
        _staticTray.Text = text ?? StswFn.AppName();
    }

    /// <summary>
    /// Displays a static notification balloon with a specified title, text, and icon in the system tray.
    /// Allows setting a custom icon and tooltip text.
    /// </summary>
    /// <param name="tipTitle">Title text for the notification.</param>
    /// <param name="tipText">Notification content text.</param>
    /// <param name="tipIcon">Icon type for the notification balloon.</param>
    /// <param name="icon">Icon displayed in the system tray.</param>
    /// <param name="text">Tooltip text for the tray icon.</param>
    [StswInfo("0.7.0")]
    public static async Task Notify(string? tipTitle, string? tipText, ToolTipIcon? tipIcon, Icon? icon = null, string? text = null)
    {
        InitStaticIcon(icon, text);
        _staticTray!.Visible = true;

        var timeout = ((int?)Registry.GetValue("HKEY_CURRENT_USER\\Control Panel\\Accessibility", "MessageDuration", 5) ?? 5) * 1000;
        await Task.Run(() => _staticTray?.ShowBalloonTip(timeout, tipTitle ?? string.Empty, tipText ?? string.Empty, tipIcon ?? ToolTipIcon.None));
    }
    #endregion

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern IntPtr SetForegroundWindow(IntPtr hwnd);

    ~StswNotifyIcon()
    {
        _tray?.Dispose();
    }
}

/* usage:

<se:StswNotifyIcon IconPath="pack://application:,,,/Resources/Icon.ico" Text="My Application" IsAlwaysVisible="True"/>

*/
