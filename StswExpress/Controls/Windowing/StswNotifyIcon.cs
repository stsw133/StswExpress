using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Xml.Linq;

namespace StswExpress;

public class StswNotifyIcon : FrameworkElement
{
    public StswNotifyIcon()
    {
        Loaded += StswNotifyIcon_Loaded;
        Unloaded += StswNotifyIcon_Unloaded;
    }

    internal NotifyIcon? Tray;

    #region Events
    private Window? _window;
    
    /// StswNotifyIcon_Loaded
    private void StswNotifyIcon_Loaded(object sender, RoutedEventArgs e)
    {
        _window = ParentControl as Window ?? Window.GetWindow(this);
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

    /// IconFromPath
    private static Icon IconFromPath(string path) => new Icon(System.Windows.Application.GetResourceStream(new Uri(path)).Stream);

    /// StswWindow_StateChanged
    private void StswWindow_StateChanged(object? sender, EventArgs e)
    {
        if (_window != null && Tray != null && _window.WindowState == WindowState.Minimized)
        {
            _window.Hide();
            Tray.Visible = true;
        }
    }

    /// Tray_MouseDoubleClick
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

    /// Tray_MouseDown
    private void Tray_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right)
        {
            if (PresentationSource.FromVisual(ContextMenu) is HwndSource hwndSource)
                _ = SetForegroundWindow(hwndSource.Handle);
            ContextMenu.IsOpen = true;

            if (ParentControl is FrameworkElement frameworkElement)
                ContextMenu.DataContext = frameworkElement.DataContext;
        }
    }

    /// StswNotifyIcon_Loaded
    private void StswNotifyIcon_Unloaded(object sender, RoutedEventArgs e)
    {
        Tray?.Dispose();
        if (_window != null)
            _window.StateChanged -= StswWindow_StateChanged;
    }
    #endregion

    #region Main properties
    /// Icon
    public static readonly DependencyProperty IconProperty
        = DependencyProperty.Register(
            nameof(Icon),
            typeof(Icon),
            typeof(StswNotifyIcon),
            new PropertyMetadata(default(Icon), OnIconChanged)
        );
    public Icon Icon
    {
        get => (Icon)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    private static void OnIconChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswNotifyIcon stsw)
        {
            if (stsw.Tray != null)
                stsw.Tray.Icon = stsw.Icon;
        }
    }
    /// IconPath
    public static readonly DependencyProperty IconPathProperty
        = DependencyProperty.Register(
            nameof(IconPath),
            typeof(string),
            typeof(StswNotifyIcon),
            new PropertyMetadata(default(string), OnIconPathChanged)
        );
    public string IconPath
    {
        get => (string)GetValue(IconPathProperty);
        set => SetValue(IconPathProperty, value);
    }
    private static void OnIconPathChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswNotifyIcon stsw)
        {
            if (stsw.Tray != null)
                stsw.Tray.Icon = IconFromPath(stsw.IconPath);
        }
    }

    /// ParentControl
    public static readonly DependencyProperty ParentControlProperty
        = DependencyProperty.Register(
            nameof(ParentControl),
            typeof(UIElement),
            typeof(StswNotifyIcon)
        );
    public UIElement ParentControl
    {
        get => (UIElement)GetValue(ParentControlProperty);
        set => SetValue(ParentControlProperty, value);
    }

    /// Text
    public static readonly DependencyProperty TextProperty
        = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(StswNotifyIcon),
            new PropertyMetadata(default(string), OnTextChanged)
        );
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
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
