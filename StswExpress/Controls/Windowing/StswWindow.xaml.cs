using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Shell;

namespace StswExpress;
/// <summary>
/// A custom window control with extended functionality, including fullscreen toggle,
/// window state management, and support for custom window chrome.
/// Provides additional features such as window centering, default size restoration, 
/// and integration with custom UI components.
/// </summary>
/// <remarks>
/// This control enhances the standard WPF window by enabling fullscreen mode, 
/// managing custom window chrome, and supporting additional UI elements in the title bar.
/// </remarks>
public class StswWindow : Window, IStswCornerControl
{
    public StswWindow()
    {
        SetValue(ComponentsProperty, new ObservableCollection<UIElement>());
    }
    static StswWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswWindow), new FrameworkPropertyMetadata(typeof(StswWindow)));
    }

    #region Events & methods
    internal StswWindowBar? _windowBar;
    private WindowState preFullscreenState;
    private double defaultHeight, defaultWidth;

    /// <summary>
    /// Event that occurs when the <see cref="Fullscreen"/> property changes.
    /// </summary>
    public event EventHandler? FullscreenChanged;

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// chrome change
        if (GetTemplateChild("PART_WindowBar") is StswWindowBar windowBar)
        {
            windowBar.SizeChanged += (_, _) => UpdateChrome();
            if (windowBar.Parent is StswSidePanel windowBarPanel)
                windowBarPanel.IsAlwaysVisible = !Fullscreen;
            _windowBar = windowBar;
        }
        StateChanged += (_, _) => UpdateChrome();
    }

    /// <summary>
    /// Centers the window on the screen based on the monitor's available work area.
    /// Ensures proper alignment even with multiple monitors.
    /// </summary>
    private void Center()
    {
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
    /// Restores the window to its default size and repositions it at the center of the screen.
    /// </summary>
    public void Default()
    {
        if (WindowState == WindowState.Maximized)
            WindowState = WindowState.Normal;

        Height = defaultHeight;
        Width = defaultWidth;
        Center();
    }

    /// <summary>
    /// Handles transitioning into and out of fullscreen mode, managing window visibility and state changes.
    /// </summary>
    /// <param name="enteringFullscreen"><see langword="true"/> if entering fullscreen, <see langword="false"/> if exiting.</param>
    private void HandleEnteringFullscreen(bool enteringFullscreen)
    {
        if (enteringFullscreen)
        {
            preFullscreenState = WindowState;
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;

            if (_windowBar?.Parent is StswSidePanel windowBarPanel)
                windowBarPanel.IsAlwaysVisible = false;

            WindowState = WindowState.Maximized;
        }
        else
        {
            if (_windowBar?.Parent is StswSidePanel windowBarPanel)
                windowBarPanel.IsAlwaysVisible = true;

            if (preFullscreenState == WindowState.Maximized)
                WindowState = WindowState.Normal;

            WindowState = preFullscreenState;
        }

        Activate();
    }

    /// <summary>
    /// Updates the custom window chrome properties, including corner radius, caption height, and resize border thickness.
    /// </summary>
    private void UpdateChrome()
    {
        var chrome = WindowChrome.GetWindowChrome(this);
        var iSize = StswSettings.Default.iSize;

        if (Fullscreen)
        {
            WindowChrome.SetWindowChrome(this, null);
        }
        else if (_windowBar != null)
        {
            var max = WindowState == WindowState.Maximized;
            var cr = CornerRadius;

            chrome ??= new WindowChrome();
            chrome.CornerRadius = new CornerRadius(cr.TopLeft * iSize, cr.TopRight * iSize, cr.BottomRight * iSize, cr.BottomLeft * iSize);
            chrome.CaptionHeight = (_windowBar.ActualHeight - (max ? 0 : 2)) * iSize >= 0 ? (_windowBar.ActualHeight - (max ? 0 : 2)) * iSize : 0;
            chrome.GlassFrameThickness = new Thickness(0);
            chrome.ResizeBorderThickness = new Thickness(max ? 0 : 5 * iSize);
            chrome.UseAeroCaptionButtons = false;

            WindowChrome.SetWindowChrome(this, chrome);
        }
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the collection of elements used in the window's title bar.
    /// Allows adding custom UI components such as buttons or labels.
    /// </summary>
    public IList Components
    {
        get => (IList)GetValue(ComponentsProperty);
        set => SetValue(ComponentsProperty, value);
    }
    public static readonly DependencyProperty ComponentsProperty
        = DependencyProperty.Register(
            nameof(Components),
            typeof(IList),
            typeof(StswWindow)
        );

    /// <summary>
    /// Gets or sets the presentation mode for the config dialog, allowing customization of its appearance and behavior.
    /// </summary>
    public StswPresentationMode? ConfigPresentationMode
    {
        get => (StswPresentationMode?)GetValue(ConfigPresentationModeProperty);
        set => SetValue(ConfigPresentationModeProperty, value);
    }
    public static readonly DependencyProperty ConfigPresentationModeProperty
        = DependencyProperty.Register(
            nameof(ConfigPresentationMode),
            typeof(StswPresentationMode?),
            typeof(StswWindow)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the window is in fullscreen mode.
    /// When enabled, the window takes up the entire screen, hiding the taskbar and borders.
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
            if (stsw._windowBar != null)
            {
                if (stsw.ResizeMode.In(ResizeMode.NoResize, ResizeMode.CanMinimize))
                    return;

                stsw.HandleEnteringFullscreen(stsw.Fullscreen);

                /// event for non MVVM programming
                stsw.FullscreenChanged?.Invoke(stsw, new StswValueChangedEventArgs<bool?>((bool?)e.OldValue, (bool?)e.NewValue));
            }
        }
    }
    #endregion

    #region Style properties
    /// <inheritdoc/>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty
        = DependencyProperty.Register(
            nameof(CornerClipping),
            typeof(bool),
            typeof(StswWindow),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <inheritdoc/>
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
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender, OnCornerRadiusChanged)
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
    #endregion

    #region Advanced logic
    /// <inheritdoc/>
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        IntPtr handle = new WindowInteropHelper(this).Handle;
        if (PresentationSource.FromVisual(this) is HwndSource hwndSource)
            hwndSource.AddHook(new HwndSourceHook(WndProc));

        if (defaultHeight == 0)
            defaultHeight = Height;
        if (defaultWidth == 0)
            defaultWidth = Width;
    }

    /// <summary>
    /// Processes window messages, including handling fullscreen and maximization behavior.
    /// Ensures that the taskbar remains visible when maximizing the window.
    /// </summary>
    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        handled = msg switch
        {
            0x0024 when !Fullscreen => false,
            0x00A4 or 0x00A5 when wParam.ToInt32() == 0x02 && _windowBar != null => _windowBar.ContextMenu.IsOpen = true,
            0x0100 when wParam.ToInt32() == 0x7A => Fullscreen = !Fullscreen,
            _ => handled
        };

        /// avoid hiding task bar upon maximization
        if (msg == 0x24 && !Fullscreen)
            WmGetMinMaxInfo(hwnd, lParam);

        return IntPtr.Zero;
    }

    /// <summary>
    /// Updates the window's maximized size to prevent covering the taskbar.
    /// Adjusts positioning and dimensions based on the monitor's work area.
    /// </summary>
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

    [StructLayout(LayoutKind.Sequential)]
    public struct MINMAXINFO
    {
        public POINT ptReserved;
        public POINT ptMaxSize;
        public POINT ptMaxPosition;
        public POINT ptMinTrackSize;
        public POINT ptMaxTrackSize;
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class MONITORINFO
    {
        public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
        public RECT rcMonitor = new();
        public RECT rcWork = new();
        public int dwFlags = 0;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT(int x, int y)
    {
        public int x = x;
        public int y = y;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct RECT
    {
        public int left, top, right, bottom;
        public static readonly RECT Empty = new();
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

        public override readonly string ToString() => this == Empty ? "RECT {Empty}" : $"RECT {{ left: {left}, top: {top}, right: {right}, bottom: {bottom} }}";
        public override readonly bool Equals(object? obj) => obj is RECT rect && this == rect;
        public override readonly int GetHashCode() => HashCode.Combine(left, top, right, bottom);
        public static bool operator ==(RECT rect1, RECT rect2) => rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom;
        public static bool operator !=(RECT rect1, RECT rect2) => !(rect1 == rect2);
    }
    #endregion

    [DllImport("user32")]
    internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

    [DllImport("User32")]
    internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);
}

/* usage:

<se:StswWindow Title="My Application" Width="800" Height="600">
    <Grid>
        <TextBlock Text="Welcome to the custom window!" HorizontalAlignment="Center" VerticalAlignment="Center"/>
    </Grid>
</se:StswWindow>

*/
