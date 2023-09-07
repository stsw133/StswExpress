using System;
using System.Drawing;
using System.Runtime.InteropServices;
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
    /// Handles the Loaded event to initialize the NotifyIcon.
    /// </summary>
    private void StswNotifyIcon_Loaded(object sender, RoutedEventArgs e)
    {
        _window = ContextControl as Window ?? Window.GetWindow(this);
        if (_window != null)
        {
            _window.StateChanged += StswWindow_StateChanged;

            Tray = new()
            {
                Icon = Icon ?? IconFromPath(IconPath),
                Text = Text,
            };
            Tray.MouseDoubleClick += Tray_MouseDoubleClick;
            Tray.MouseDown += Tray_MouseDown;
        }
    }

    /// <summary>
    /// Helper method to create an Icon from the specified path.
    /// </summary>
    private static Icon IconFromPath(string path) => new(System.Windows.Application.GetResourceStream(new Uri(path, UriKind.RelativeOrAbsolute)).Stream);

    /// <summary>
    /// Handles the StateChanged event of the associated Window to show/hide the window when minimized.
    /// </summary>
    private void StswWindow_StateChanged(object? sender, EventArgs e)
    {
        if (_window != null && Tray != null && _window.WindowState == WindowState.Minimized)
        {
            _window.Hide();
            Tray.Visible = true;
        }
    }

    /// <summary>
    /// Handles the MouseDoubleClick event of the NotifyIcon to show the window when double-clicked.
    /// </summary>
    private void Tray_MouseDoubleClick(object? sender, MouseEventArgs e)
    {
        if (_window != null && Tray != null)
        {
            _window.Show();
            _window.WindowState = WindowState.Normal;
            _window.Activate();
            System.Threading.Thread.Sleep(100);
            Tray.Visible = false;
        }
    }

    /// <summary>
    /// Handles the MouseDown event of the NotifyIcon to show the context menu when right-clicked.
    /// </summary>
    private void Tray_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right)
        {
            if (PresentationSource.FromVisual(ContextMenu) is HwndSource hwndSource)
                _ = SetForegroundWindow(hwndSource.Handle);
            ContextMenu.IsOpen = true;

            if (ContextControl is FrameworkElement frameworkElement)
                ContextMenu.DataContext = frameworkElement.DataContext;
        }
    }

    /// <summary>
    /// Handles the Unloaded event to clean up resources.
    /// </summary>
    private void StswNotifyIcon_Unloaded(object sender, RoutedEventArgs e)
    {
        Tray?.Dispose();
        if (_window != null)
            _window.StateChanged -= StswWindow_StateChanged;
    }
    #endregion

    #region Main properties
    /// <summary>
    /// Gets or sets the parent UI element of the NotifyIcon control which will serve as DataContext source.
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
    /// Gets or sets the Icon to be displayed in the NotifyIcon control.
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
    /// Gets or sets the path to the icon file to be displayed in the NotifyIcon control.
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
    /// Gets or sets the text to be displayed as a tooltip for the NotifyIcon.
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
    #endregion

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern IntPtr SetForegroundWindow(IntPtr hwnd);
}
