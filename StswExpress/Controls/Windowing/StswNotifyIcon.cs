using Microsoft.Win32;
using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace StswExpress;

/// <summary>
/// Represents a control for displaying a system tray icon with various properties for customization.
/// </summary>
public class StswNotifyIcon : FrameworkElement
{
    public StswNotifyIcon()
    {
        Loaded += StswNotifyIcon_Loaded;
        Unloaded += StswNotifyIcon_Unloaded;
    }
    internal NotifyIcon? Tray;

    #region Events & methods
    private Window? _window;

    /// <summary>
    /// Helper method to create an <see cref="Icon"/> from the specified path.
    /// </summary>
    public static Icon IconFromPath(string path) => new(System.Windows.Application.GetResourceStream(new Uri(path, UriKind.RelativeOrAbsolute)).Stream);

    /// <summary>
    /// Displays a notification balloon with the specified title, text, and icon.
    /// </summary>
    /// <param name="tipTitle">The title of the notification.</param>
    /// <param name="tipText">The text of the notification.</param>
    /// <param name="tipIcon">The icon type of the notification.</param>
    public void Notify(string tipTitle, string tipText, StswNotificationType tipIcon) => Tray?.ShowBalloonTip(3000, tipTitle, tipText, (ToolTipIcon)tipIcon);

    /// <summary>
    /// Handles the Exit event assigned to application.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void StswApplication_Exit(object sender, ExitEventArgs e) => Tray?.Dispose();

    /// <summary>
    /// Handles the Loaded event to initialize the <see cref="NotifyIcon"/>.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void StswNotifyIcon_Loaded(object sender, RoutedEventArgs e)
    {
        _window = ContextControl as Window ?? Window.GetWindow(this);
        if (_window != null)
        {
            _window.StateChanged += StswWindow_StateChanged;
            System.Windows.Application.Current.Exit += StswApplication_Exit;

            Tray = new()
            {
                Icon = Icon ?? IconFromPath(IconPath),
                Text = Text
            };
            Tray.BalloonTipClicked += Tray_BalloonTipClicked; /// run command
            Tray.MouseDoubleClick += Tray_MouseDoubleClick; /// show window
            Tray.MouseDown += Tray_MouseDown; /// show context menu
        }

        OnIsAlwaysVisibleChanged(this, new DependencyPropertyChangedEventArgs());
    }

    /// <summary>
    /// Handles the Unloaded event to clean up resources.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void StswNotifyIcon_Unloaded(object sender, RoutedEventArgs e)
    {
        Tray?.Dispose();
        if (_window != null)
            _window.StateChanged -= StswWindow_StateChanged;
        System.Windows.Application.Current.Exit -= StswApplication_Exit;
    }

    /// <summary>
    /// Handles the StateChanged event of the associated window to show/hide the window when minimized.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void StswWindow_StateChanged(object? sender, EventArgs e)
    {
        if (_window != null && Tray != null && _window.WindowState == WindowState.Minimized && IsEnabled)
        {
            if (!IsAlwaysVisible)
                _window.Hide();
            Tray.Visible = true;
        }
    }

    /// <summary>
    /// Handles the BalloonTipClicked event of the <see cref="NotifyIcon"/>.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void Tray_BalloonTipClicked(object? sender, EventArgs e)
    {
        if (_window != null && Tray != null)
        {
            _window.Show();
            _window.WindowState = WindowState.Normal;
            _window.Activate();

            if (!IsAlwaysVisible)
            {
                Thread.Sleep(100);
                Tray.Visible = false;
            }
        }
    }

    /// <summary>
    /// Handles the MouseDoubleClick event of the <see cref="NotifyIcon"/> to show the window when double-clicked.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void Tray_MouseDoubleClick(object? sender, MouseEventArgs e) => Tray_BalloonTipClicked(sender, e);

    /// <summary>
    /// Handles the MouseDown event of the <see cref="NotifyIcon"/> to show the context menu when right-clicked.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void Tray_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right && ContextMenu != null)
        {
            if (PresentationSource.FromVisual(ContextMenu) is HwndSource hwndSource)
                _ = SetForegroundWindow(hwndSource.Handle);
            ContextMenu.IsOpen = true;

            if (ContextControl is FrameworkElement frameworkElement)
                ContextMenu.DataContext = frameworkElement.DataContext;
        }
    }
    #endregion

    #region Main properties
    /// <summary>
    /// Gets or sets the parent UI element of the <see cref="NotifyIcon"/> control which will serve as data context source.
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
        if (obj is StswNotifyIcon stsw)
        {
            if (stsw.Tray != null)
                stsw.Tray.Icon = stsw.Icon;
        }
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
        if (obj is StswNotifyIcon stsw)
        {
            if (stsw.Tray != null)
                stsw.Tray.Icon = IconFromPath(stsw.IconPath);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the <see cref="NotifyIcon"/> control should always be visible, even when the associated window is minimized.
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
    private static void OnIsAlwaysVisibleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswNotifyIcon stsw)
        {
            if (stsw.Tray != null)
            {
                if (stsw.IsAlwaysVisible)
                    stsw.Tray.Visible = true;
                else
                    stsw.Tray.Visible = stsw._window?.IsVisible != true;
            }
        }
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
        if (obj is StswNotifyIcon stsw)
        {
            if (stsw.Tray != null)
                stsw.Tray.Text = stsw.Text;
        }
    }

    /// <summary>
    /// Gets or sets the tip model to be displayed as a notification when the <see cref="NotifyIcon"/> control is shown.
    /// </summary>
    public StswNotifyIconTipModel? Tip
    {
        get => (StswNotifyIconTipModel?)GetValue(TipProperty);
        set => SetValue(TipProperty, value);
    }
    public static readonly DependencyProperty TipProperty
        = DependencyProperty.Register(
            nameof(Tip),
            typeof(StswNotifyIconTipModel),
            typeof(StswNotifyIcon),
            new PropertyMetadata(default(StswNotifyIconTipModel?), OnTipChanged)
        );
    private static void OnTipChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswNotifyIcon stsw)
        {
            if (stsw.Tip != null)
                stsw.Notify(stsw.Tip.TipTitle, stsw.Tip.TipText, stsw.Tip.TipIcon);
        }
    }
    #endregion

    #region Static instance
    private static NotifyIcon? _tray;

    /// <summary>
    /// Initializes the static instance of the <see cref="NotifyIcon"/> control with the specified icon and text.
    /// </summary>
    /// <param name="icon">The icon to be displayed in the system tray.</param>
    /// <param name="text">The text to be displayed as a tooltip for the icon in the notification panel.</param>
    private static void Init(Icon? icon, string? text)
    {
        if (_tray == null)
        {
            _tray = new();
            _tray.Click += (s, e) => _tray.Visible = false;
            _tray.MouseClick += (s, e) => _tray.Visible = false;
            System.Windows.Application.Current.Exit += (s, e) => _tray?.Dispose();
        }
        _tray.Icon = icon ?? Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly()!.ManifestModule.Name);
        _tray.Text = text ?? StswFn.AppName();
    }

    /// <summary>
    /// Displays a notification balloon with the specified title, text, and icon.
    /// </summary>
    /// <param name="tipTitle">The title of the notification.</param>
    /// <param name="tipText">The text of the notification.</param>
    /// <param name="tipIcon">The icon type of the notification.</param>
    /// <param name="icon">The icon to be displayed in the system tray.</param>
    /// <param name="text">The text to be displayed as a tooltip for the icon in the notification panel.</param>
    public static async Task Notify(string? tipTitle, string? tipText, StswNotificationType? tipIcon, Icon? icon = null, string? text = null)
    {
        Init(icon, text);

        _tray!.Visible = true;
        await Task.Run(() =>
        {
            var timeout = ((int?)Registry.GetValue("HKEY_CURRENT_USER\\Control Panel\\Accessibility", "MessageDuration", 5) ?? 5) * 1000;
            _tray?.ShowBalloonTip(timeout, tipTitle ?? string.Empty, tipText ?? string.Empty, (ToolTipIcon?)tipIcon ?? ToolTipIcon.None);
        });
    }
    #endregion

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern IntPtr SetForegroundWindow(IntPtr hwnd);

    ~StswNotifyIcon()
    {
        Tray?.Dispose();
    }
}

/// <summary>
/// Data model for <see cref="StswNotifyIcon"/>'s tip.
/// </summary>
public class StswNotifyIconTipModel
{
    /// <summary>
    /// Gets or sets the title of the notification tip.
    /// </summary>
    public string TipTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the text of the notification tip.
    /// </summary>
    public string TipText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the icon type of the notification tip.
    /// </summary>
    public StswNotificationType TipIcon { get; set; } = StswNotificationType.None;
}
