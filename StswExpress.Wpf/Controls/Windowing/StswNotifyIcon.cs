using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace StswExpress.Wpf;
/// <summary>
/// A system tray icon control that supports context menus, notifications, and custom icons.
/// Allows minimizing the application to the system tray and displaying balloon tooltips.
/// </summary>
/// <remarks>
/// The control manages application state visibility and interaction with the system tray.
/// </remarks>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswNotifyIcon IconPath="pack://application:,,,/Assets/Icon.ico" Text="My Application" IsAlwaysVisible="True"/&gt;
/// </code>
/// </example>
[StswPlannedChanges(StswPlannedChanges.Refactor | StswPlannedChanges.NewFeatures, "Needs code cleanup and additional features like restoring instead when another app instance is started.")]
public class StswNotifyIcon : FrameworkElement
{
    private static readonly HashSet<WeakReference<StswNotifyIcon>> _loadedInstances = [];
    private NotifyIcon? _tray;
    private Window? _window;

    public StswNotifyIcon()
    {
        Loaded += OnLoaded;
        Loaded += Initialize;
        Unloaded += OnUnloaded;
        Unloaded += Cleanup;
    }

    #region Events & methods
    /// <summary>
    /// Registers the notify icon instance for identifier-based lookups.
    /// </summary>
    /// <param name="sender">The sender object.</param>
    /// <param name="e">The event arguments.</param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        foreach (var weakRef in _loadedInstances.ToList())
            if (weakRef.TryGetTarget(out StswNotifyIcon? notifyIcon) && ReferenceEquals(notifyIcon, this))
                return;

        _loadedInstances.Add(new WeakReference<StswNotifyIcon>(this));
    }

    /// <summary>
    /// Unregisters the notify icon instance when it is unloaded.
    /// </summary>
    /// <param name="sender">The sender object.</param>
    /// <param name="e">The event arguments.</param>
    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        foreach (var weakRef in _loadedInstances.ToList())
            if (!weakRef.TryGetTarget(out StswNotifyIcon? notifyIcon) || ReferenceEquals(notifyIcon, this))
            {
                _loadedInstances.Remove(weakRef);
                break;
            }
    }

    /// <summary>
    /// Initializes the <see cref="NotifyIcon"/> instance and sets up event handlers for tray icon actions and application state changes.
    /// </summary>
    /// <param name="sender">The sender object.</param>
    /// <param name="e">The event arguments.</param>
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
    /// <param name="sender">The sender object.</param>
    /// <param name="e">The event arguments.</param>
    private void Cleanup(object? sender, RoutedEventArgs e)
    {
        _tray?.Dispose();
        if (_window != null)
            _window.StateChanged -= HandleWindowStateChange;
        System.Windows.Application.Current.Exit -= OnApplicationExit;
    }

    /// <summary>
    /// Handles application exit, releasing any resources tied to the <see cref="NotifyIcon"/> instance.
    /// </summary>
    /// <param name="sender">The sender object.</param>
    /// <param name="e">The event arguments.</param>
    private void OnApplicationExit(object? sender, ExitEventArgs e) => _tray?.Dispose();

    /// <summary>
    /// Retrieves a notify icon instance based on the provided identifier.
    /// </summary>
    /// <param name="notifyIconIdentifier">The identifier of the notify icon.</param>
    /// <returns>A matching <see cref="StswNotifyIcon"/> instance.</returns>
    internal static StswNotifyIcon GetInstance(object? notifyIconIdentifier)
    {
        if (_loadedInstances.Count == 0)
            throw new InvalidOperationException($"No loaded {nameof(StswNotifyIcon)} instances.");

        var targets = new List<StswNotifyIcon>();
        foreach (var instance in _loadedInstances.ToList())
        {
            if (instance.TryGetTarget(out var notifyIcon))
            {
                object? identifier = null;

                if (notifyIcon.CheckAccess())
                    identifier = notifyIcon.Identifier;
                else
                    identifier = notifyIcon.Dispatcher.Invoke(() => notifyIcon.Identifier);

                if (Equals(notifyIconIdentifier, identifier))
                    targets.Add(notifyIcon);
            }
            else _loadedInstances.Remove(instance);
        }

        if (targets.Count == 0)
            throw new InvalidOperationException($"No loaded {nameof(StswNotifyIcon)} have an {nameof(Identifier)} property matching {nameof(notifyIconIdentifier)} ('{notifyIconIdentifier}') argument.");
        if (targets.Count > 1)
            throw new InvalidOperationException($"Multiple viable {nameof(StswNotifyIcon)}s. Specify a unique Identifier on each {nameof(StswNotifyIcon)}, especially where multiple Windows are a concern.");

        return targets[0];
    }

    /// <summary>
    /// Handles window state changes, hiding the window when minimized if <see cref="IsAlwaysVisible"/> is false.
    /// Ensures the tray icon remains visible.
    /// </summary>
    /// <param name="sender">The sender object.</param>
    /// <param name="e">The event arguments.</param>
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
    /// Adjusts the system tray icon visibility based on the window state and the value of <see cref="IsAlwaysVisible"/>.
    /// </summary>
    private void UpdateIconVisibility() => _tray!.Visible = IsAlwaysVisible || _window?.IsVisible != true;

    /// <summary>
    /// Displays a balloon notification for the <see cref="StswNotifyIcon"/> identified by <paramref name="notifyIconIdentifier"/>.
    /// </summary>
    /// <param name="title">Title of the notification.</param>
    /// <param name="text">Content text of the notification.</param>
    /// <param name="icon">Notification icon type.</param>
    /// <param name="identifier">Identifier of the notify icon instance.</param>
    public static void Show(string? title, string? text, ToolTipIcon? icon, object? identifier) => GetInstance(identifier).Show(title, text, icon);

    /// <summary>
    /// Displays a notification balloon with a specified title, text, and icon in the system tray.
    /// </summary>
    /// <param name="title">The notification title text.</param>
    /// <param name="text">The notification content text.</param>
    /// <param name="icon">The icon type for the notification balloon.</param>
    public void Show(string? title, string? text, ToolTipIcon? icon)
    {
        if (!CheckAccess())
        {
            Dispatcher.Invoke(() => Show(title, text, icon));
            return;
        }

        if (_tray is null)
            throw new InvalidOperationException($"{nameof(StswNotifyIcon)} is not initialized.");

        var timeout = ((int?)Registry.GetValue("HKEY_CURRENT_USER\\Control Panel\\Accessibility", "MessageDuration", 5) ?? 5) * 1000;
        if (!string.IsNullOrEmpty(text))
        {
            var shouldRestoreVisibility = !_tray.Visible;
            _tray.Visible = true;

            _tray.ShowBalloonTip(timeout, title ?? string.Empty, text, icon ?? ToolTipIcon.None);

            if (shouldRestoreVisibility)
                UpdateIconVisibility();
        }
    }

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
    private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var stsw = (StswNotifyIcon)d;
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
    private static void OnIconPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var stsw = (StswNotifyIcon)d;
        if (stsw._tray != null)
            stsw._tray.Icon = stsw.Icon ?? LoadIcon(stsw.IconPath);
    }

    /// <summary>
    /// Gets or sets the identifier that allows static access to the notify icon instance.
    /// </summary>
    public object? Identifier
    {
        get => GetValue(IdentifierProperty);
        set => SetValue(IdentifierProperty, value);
    }
    public static readonly DependencyProperty IdentifierProperty
        = DependencyProperty.Register(
            nameof(Identifier),
            typeof(object),
            typeof(StswNotifyIcon)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the <see cref="NotifyIcon"/> remains visible even when the associated window is minimized.
    /// </summary>
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
    private static void OnIsAlwaysVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var stsw = (StswNotifyIcon)d;
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
    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var stsw = (StswNotifyIcon)d;
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
    private static void OnTipChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var stsw = (StswNotifyIcon)d;
        stsw.Notify(stsw.Tip.TipTitle, stsw.Tip.TipText, stsw.Tip.TipIcon);
    }
    #endregion

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern IntPtr SetForegroundWindow(IntPtr hwnd);

    ~StswNotifyIcon()
    {
        _tray?.Dispose();
    }
}
