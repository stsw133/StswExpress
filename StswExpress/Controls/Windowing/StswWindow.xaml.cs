using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Shell;

namespace StswExpress;

/// <summary>
/// Represents a custom window control with additional functionality and customization options.
/// </summary>
public class StswWindow : Window, IStswCornerControl
{
    public StswWindow()
    {
        var commandBinding = new RoutedUICommand(nameof(Fullscreen), nameof(Fullscreen), GetType(), new InputGestureCollection() { new KeyGesture(Key.F11) });
        CommandBindings.Add(new CommandBinding(commandBinding, (s, e) => Fullscreen = !Fullscreen));
    }
    static StswWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswWindow), new FrameworkPropertyMetadata(typeof(StswWindow)));
    }

    #region Events & methods
    private StswWindowBar? windowBar;
    private WindowState preFullscreenState;

    /// <summary>
    /// Occurs when the Fullscreen property is changed.
    /// </summary>
    public event EventHandler? FullscreenChanged;

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// Menu: config
        if (GetTemplateChild("PART_iConfig") is MenuItem mniConfig)
            mniConfig.Click += (s, e) => StswConfig.Show(this);
        /// Menu: default
        if (GetTemplateChild("PART_MenuDefault") is MenuItem mniDefault)
            mniDefault.Click += DefaultClick;
        /// Menu: minimize
        if (GetTemplateChild("PART_MenuMinimize") is MenuItem mniMinimize)
            mniMinimize.Click += MinimizeClick;
        /// Menu: restore
        if (GetTemplateChild("PART_MenuRestore") is MenuItem mniRestore)
            mniRestore.Click += RestoreClick;
        /// Menu: fullscreen
        if (GetTemplateChild("PART_MenuFullscreen") is MenuItem mniFullscreen)
            mniFullscreen.Click += FullscreenClick;
        /// Menu: close
        if (GetTemplateChild("PART_MenuClose") is MenuItem mniClose)
            mniClose.Click += CloseClick;

        /// Chrome change
        if (GetTemplateChild("PART_WindowBar") is StswWindowBar windowBar)
        {
            windowBar.SizeChanged += (s, e) => UpdateChrome();
            if (windowBar.Parent is StswSidePanel windowBarPanel)
                windowBarPanel.ExpandMode = Fullscreen ? StswCompactibility.Collapsed : StswCompactibility.Full;
            this.windowBar = windowBar;
        }
        StateChanged += (s, e) => UpdateChrome();
    }

    /// <summary>
    /// Updates the custom window chrome based on the current state and settings.
    /// </summary>
    private void UpdateChrome()
    {
        var chrome = WindowChrome.GetWindowChrome(this);
        var iSize = StswSettings.Default.iSize;

        if (Fullscreen)
        {
            WindowChrome.SetWindowChrome(this, null);
        }
        else if (windowBar != null)
        {
            var max = WindowState == WindowState.Maximized;
            var cr = CornerRadius;

            chrome ??= new WindowChrome();
            chrome.CornerRadius = new CornerRadius(cr.TopLeft * iSize, cr.TopRight * iSize, cr.BottomRight * iSize, cr.BottomLeft * iSize);
            chrome.CaptionHeight = (windowBar.ActualHeight - (max ? 0 : 2)) * iSize >= 0 ? (windowBar.ActualHeight - (max ? 0 : 2)) * iSize : 0;
            chrome.GlassFrameThickness = new Thickness(0);
            chrome.ResizeBorderThickness = new Thickness(max ? 0 : 5 * iSize);
            chrome.UseAeroCaptionButtons = false;

            WindowChrome.SetWindowChrome(this, chrome);
        }
    }

    /// <summary>
    /// Event handler for the fullscreen button click to toggle fullscreen mode.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    protected void FullscreenClick(object? sender, RoutedEventArgs e) => Fullscreen = !Fullscreen;

    /// <summary>
    /// Event handler for the default button click to reset the window size to its default.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    protected void DefaultClick(object? sender, RoutedEventArgs e)
    {
        if (WindowState == WindowState.Maximized)
            WindowState = WindowState.Normal;

        Height = DefaultHeight;
        Width = DefaultWidth;

        /// center the window on the screen
        if (PresentationSource.FromVisual(this) is HwndSource hwndSource)
        {
            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            var monitor = MonitorFromWindow(hwndSource.Handle, MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero)
            {
                var monitorInfo = new MONITORINFO();
                GetMonitorInfo(monitor, monitorInfo);
                var rcWorkArea = monitorInfo.rcWork;
                Left = (rcWorkArea.Width - Width) / 2 + rcWorkArea.left;
                Top = (rcWorkArea.Height - Height) / 2 + rcWorkArea.top;
            }
        }
    }

    /// <summary>
    /// Event handler for the minimize button click to minimize the window.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    internal void MinimizeClick(object? sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    /// <summary>
    /// Event handler for the restore button click to toggle between normal and maximized window state.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    internal void RestoreClick(object? sender, RoutedEventArgs e)
    {
        if (Fullscreen)
            Fullscreen = false;
        else
            WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
    }

    /// <summary>
    /// Event handler for the close button click to close the window.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    internal void CloseClick(object? sender, RoutedEventArgs e) => Close();
    #endregion

    #region Main properties
    /// <summary>
    /// Gets or sets a value indicating whether the window is in fullscreen mode.
    /// </summary>
    public bool Fullscreen
    {
        get => (bool)GetValue(FullscreenProperty);
        set => SetValue(FullscreenProperty, value);
    }
    public static readonly DependencyProperty FullscreenProperty
        = DependencyProperty.Register(
            nameof(Fullscreen),
            typeof(bool),
            typeof(StswWindow),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnFullscreenChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnFullscreenChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswWindow stsw)
        {
            if (stsw.windowBar != null)
            {
                if (stsw.ResizeMode.In(ResizeMode.NoResize, ResizeMode.CanMinimize))
                    return;

                if (stsw.Fullscreen)
                {
                    stsw.preFullscreenState = stsw.WindowState;

                    if (stsw.WindowState == WindowState.Maximized)
                        stsw.WindowState = WindowState.Minimized;

                    if (stsw.windowBar.Parent is StswSidePanel windowBarPanel)
                        windowBarPanel.ExpandMode = StswCompactibility.Collapsed;

                    stsw.WindowState = WindowState.Maximized;
                }
                else
                {
                    if (stsw.windowBar.Parent is StswSidePanel windowBarPanel)
                        windowBarPanel.ExpandMode = StswCompactibility.Full;

                    if (stsw.preFullscreenState == WindowState.Maximized)
                        stsw.WindowState = WindowState.Minimized;

                    stsw.WindowState = stsw.preFullscreenState;
                }
                stsw.Activate();
                stsw.Focus();

                stsw.FullscreenChanged?.Invoke(stsw, EventArgs.Empty);
            }
        }
    }
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets a value indicating whether corner clipping is enabled for the control.
    /// When set to <see langword="true"/>, content within the control's border area is clipped to match
    /// the border's rounded corners, preventing elements from protruding beyond the border.
    /// </summary>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty
        = DependencyProperty.Register(
            nameof(CornerClipping),
            typeof(bool),
            typeof(StswWindow)
        );

    /// <summary>
    /// Gets or sets the degree to which the corners of the control's border are rounded by defining
    /// a radius value for each corner independently. This property allows users to control the roundness
    /// of corners, and large radius values are smoothly scaled to blend from corner to corner.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswWindow),
            new PropertyMetadata(default(CornerRadius), OnCornerRadiusChanged)
        );
    public static void OnCornerRadiusChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswWindow stsw)
        {
            if (!stsw.IsLoaded)
            {
                var cr = stsw.CornerRadius;
                stsw.AllowsTransparency = stsw.AllowsTransparency || (cr.TopLeft + cr.TopRight + cr.BottomLeft + cr.BottomRight) > 0;
            }
        }
    }

    /// <summary>
    /// Gets or sets the default height of the custom window.
    /// </summary>
    public double DefaultHeight
    {
        get => (double)GetValue(DefaultHeightProperty);
        set => SetValue(DefaultHeightProperty, value);
    }
    public static readonly DependencyProperty DefaultHeightProperty
        = DependencyProperty.Register(
            nameof(DefaultHeight),
            typeof(double),
            typeof(StswWindow)
        );

    /// <summary>
    /// Gets or sets the default width of the custom window.
    /// </summary>
    public double DefaultWidth
    {
        get => (double)GetValue(DefaultWidthProperty);
        set => SetValue(DefaultWidthProperty, value);
    }
    public static readonly DependencyProperty DefaultWidthProperty
        = DependencyProperty.Register(
            nameof(DefaultWidth),
            typeof(double),
            typeof(StswWindow)
        );
    #endregion

    #region Advanced logic
    /// OnSourceInitialized
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        IntPtr handle = new WindowInteropHelper(this).Handle;
        if (PresentationSource.FromVisual(this) is HwndSource hwndSource)
            hwndSource.AddHook(new HwndSourceHook(WndProc));

        if (DefaultHeight == 0)
            DefaultHeight = Height;
        if (DefaultWidth == 0)
            DefaultWidth = Width;
    }

    /// WndProc
    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        /// Avoid hiding task bar upon maximization
        if (msg == 0x24)
        {
            if (!Fullscreen)
                WmGetMinMaxInfo(hwnd, lParam);
            handled = false;
        }
        /// Hide default context menu and show custom
        else if (msg == 0xa4 && wParam.ToInt32() == 0x02 || msg == 0xa5)
        {
            if (windowBar != null)
                windowBar.ContextMenu.IsOpen = true;
            handled = true;
        }
        return IntPtr.Zero;
    }

    /// WmGetMinMaxInfo
    private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
    {
        if (Marshal.PtrToStructure(lParam, typeof(MINMAXINFO)) is not MINMAXINFO mmi)
            return;

        var monitor = MonitorFromWindow(hwnd, 0x02);
        if (monitor == IntPtr.Zero)
            return;

        var monitorInfo = new MONITORINFO();
        if (!GetMonitorInfo(monitor, monitorInfo))
            return;

        var rcWorkArea = monitorInfo.rcWork;
        var rcMonitorArea = monitorInfo.rcMonitor;
        mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
        mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
        mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
        mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
        mmi.ptMaxTrackSize.x = Math.Abs(rcWorkArea.Width);
        mmi.ptMaxTrackSize.y = Math.Abs(rcWorkArea.Height);

        Marshal.StructureToPtr(mmi, lParam, true);
    }

    /// POINT
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;
        public POINT(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    /// MINMAXINFO
    [StructLayout(LayoutKind.Sequential)]
    public struct MINMAXINFO
    {
        public POINT ptReserved;
        public POINT ptMaxSize;
        public POINT ptMaxPosition;
        public POINT ptMinTrackSize;
        public POINT ptMaxTrackSize;
    };

    /// MONITORINFO
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class MONITORINFO
    {
        public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
        public RECT rcMonitor = new RECT();
        public RECT rcWork = new RECT();
        public int dwFlags = 0;
    }

    /// RECT
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct RECT
    {
        public int left, top, right, bottom;
        public static readonly RECT Empty = new RECT();
        public readonly int Width => Math.Abs(right - left);
        public readonly int Height => bottom - top;

        public RECT(int left, int top, int right, int bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public RECT(RECT rcSrc)
        {
            left = rcSrc.left;
            top = rcSrc.top;
            right = rcSrc.right;
            bottom = rcSrc.bottom;
        }

        public readonly bool IsEmpty => left >= right || top >= bottom;

        public override readonly string ToString() => this == Empty ? "RECT {Empty}" : $"RECT {{ left : {left} / top : {top} / right : {right} / bottom : {bottom} }}";
        public override readonly bool Equals(object? obj) => obj is Rect && this == (RECT)obj;
        public override readonly int GetHashCode() => left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
        public static bool operator ==(RECT rect1, RECT rect2) => rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom;
        public static bool operator !=(RECT rect1, RECT rect2) => !(rect1 == rect2);
    }

    [DllImport("user32")]
    internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

    [DllImport("User32")]
    internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);
    #endregion
}
