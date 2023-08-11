using System;
using System.Collections.ObjectModel;
using System.Linq;
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
public class StswWindow : Window
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
    private double defaultHeight, defaultWidth;
    private FrameworkElement? partFullscreenPanel, partTitleBar;
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
        /// Button: minimize
        if (GetTemplateChild("PART_ButtonMinimize") is Button btnMinimize)
            btnMinimize.Click += MinimizeClick;
        /// Button: restore
        if (GetTemplateChild("PART_ButtonRestore") is Button btnRestore)
            btnRestore.Click += RestoreClick;
        /// Button: close
        if (GetTemplateChild("PART_ButtonClose") is Button btnClose)
            btnClose.Click += CloseClick;

        /// Fullscreen Button: minimize
        if (GetTemplateChild("PART_FsButtonMinimize") is Button btnFsMinimize)
            btnFsMinimize.Click += MinimizeClick;
        /// Fullscreen Button: restore
        if (GetTemplateChild("PART_FsButtonRestore") is Button btnFsRestore)
            btnFsRestore.Click += FullscreenClick;
        /// Fullscreen Button: close
        if (GetTemplateChild("PART_FsButtonClose") is Button btnFsClose)
            btnFsClose.Click += CloseClick;

        /// Menu: scaling
        if (GetTemplateChild("PART_MenuScaling") is MenuItem mniScaling)
            mniScaling.Click += (s, e) => StswSettings.Default.iSize = 1;
        if (GetTemplateChild("PART_MenuScalingSlider") is Slider sliScaling)
            sliScaling.ValueChanged += (s, e) => UpdateChrome();
        /// Menu: theme
        if (GetTemplateChild("PART_MenuTheme") is MenuItem mniTheme)
            foreach (MenuItem mni in mniTheme.Items)
                mni.Click += (s, e) => ThemeClick(mniTheme.Items.IndexOf(mni) - 1);
        /// Menu: fullscreen
        if (GetTemplateChild("PART_MenuFullscreen") is MenuItem mniFullscreen)
            mniFullscreen.Click += FullscreenClick;
        /// Menu: center
        if (GetTemplateChild("PART_MenuCenter") is MenuItem mniCenter)
            mniCenter.Click += CenterClick;
        /// Menu: default
        if (GetTemplateChild("PART_MenuDefault") is MenuItem mniDefault)
            mniDefault.Click += DefaultClick;
        /// Menu: minimize
        if (GetTemplateChild("PART_MenuMinimize") is MenuItem mniMinimize)
            mniMinimize.Click += MinimizeClick;
        /// Menu: restore
        if (GetTemplateChild("PART_MenuRestore") is MenuItem mniRestore)
            mniRestore.Click += RestoreClick;
        /// Menu: close
        if (GetTemplateChild("PART_MenuClose") is MenuItem mniClose)
            mniClose.Click += CloseClick;

        /// Chrome change
        if (GetTemplateChild("PART_TitleBar") is FrameworkElement fmeTitlebar)
        {
            fmeTitlebar.SizeChanged += (s, e) => UpdateChrome();
            fmeTitlebar.IsVisibleChanged += (s, e) => UpdateChrome();
            partTitleBar = fmeTitlebar;
        }
        StateChanged += (s, e) => UpdateChrome();

        /// Fullscreen panel
        if (GetTemplateChild("PART_FullscreenPanel") is FrameworkElement fmeFullscreen)
            partFullscreenPanel = fmeFullscreen;
        MouseMove += OnMouseMove;

        base.OnApplyTemplate();
        //UpdateLayout();
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
        else if (chrome != null && partTitleBar is not null)
        {
            chrome.CornerRadius = new CornerRadius(CornerRadius.TopLeft * iSize, CornerRadius.TopRight * iSize, CornerRadius.BottomRight * iSize, CornerRadius.BottomLeft * iSize);
            chrome.CaptionHeight = (partTitleBar.ActualHeight - 2) * iSize >= 0 ? (partTitleBar.ActualHeight - 2) * iSize : 0;
            chrome.GlassFrameThickness = new Thickness(0);
            chrome.ResizeBorderThickness = new Thickness(WindowState == WindowState.Maximized ? 0 : 5 * iSize);
            chrome.UseAeroCaptionButtons = false;

            WindowChrome.SetWindowChrome(this, chrome);
        }
        else if (partTitleBar is not null)
        {
            chrome = new WindowChrome()
            {
                CornerRadius = new CornerRadius(CornerRadius.TopLeft * iSize, CornerRadius.TopRight * iSize, CornerRadius.BottomRight * iSize, CornerRadius.BottomLeft * iSize),
                CaptionHeight = (partTitleBar.ActualHeight - 2) * iSize >= 0 ? (partTitleBar.ActualHeight - 2) * iSize : 0,
                GlassFrameThickness = new Thickness(0),
                ResizeBorderThickness = new Thickness(WindowState == WindowState.Maximized ? 0 : 5 * iSize),
                UseAeroCaptionButtons = false
            };

            WindowChrome.SetWindowChrome(this, chrome);
        }
    }

    /// <summary>
    /// Event handler to show/hide the fullscreen panel based on mouse movement.
    /// </summary>
    private void OnMouseMove(object? sender, MouseEventArgs e)
    {
        if (Fullscreen && partFullscreenPanel is not null)
        {
            var pos = Mouse.GetPosition(this);
            if (pos.Y <= 10 || pos.Y < partFullscreenPanel.ActualHeight)
                partFullscreenPanel.Visibility = Visibility.Visible;
            else
                partFullscreenPanel.Visibility = Visibility.Collapsed;
        }
    }

    /// <summary>
    /// Event handler for changing the theme based on the clicked menu item.
    /// </summary>
    private static void ThemeClick(int themeID)
    {
        if (!Application.Current.Resources.MergedDictionaries.Any(x => x is Theme))
            Application.Current.Resources.MergedDictionaries.Add(new Theme());
        var theme = (Theme)Application.Current.Resources.MergedDictionaries.First(x => x is Theme);

        if (themeID < 0)
            theme.Color = StswFn.GetWindowsTheme();
        else
            theme.Color = (ThemeColor)themeID;

        StswSettings.Default.Theme = themeID;
    }

    /// <summary>
    /// Event handler for the fullscreen button click to toggle fullscreen mode.
    /// </summary>
    private void FullscreenClick(object? sender, RoutedEventArgs e) => Fullscreen = !Fullscreen;

    /// <summary>
    /// Event handler for the center button click to center the window on the screen.
    /// </summary>
    protected void CenterClick(object? sender, RoutedEventArgs e)
    {
        if (WindowState == WindowState.Maximized)
            WindowState = WindowState.Normal;

        if (_hwndSource != null)
        {
            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            var monitor = MonitorFromWindow(_hwndSource.Handle, MONITOR_DEFAULTTONEAREST);
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
    /// Event handler for the default button click to reset the window size to its default.
    /// </summary>
    protected void DefaultClick(object? sender, RoutedEventArgs e)
    {
        Height = defaultHeight;
        Width = defaultWidth;
        //CenterClick(sender, e);
    }

    /// <summary>
    /// Event handler for the minimize button click to minimize the window.
    /// </summary>
    protected void MinimizeClick(object? sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    /// <summary>
    /// Event handler for the restore button click to toggle between normal and maximized window state.
    /// </summary>
    protected void RestoreClick(object? sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;

    /// <summary>
    /// Event handler for the close button click to close the window.
    /// </summary>
    protected void CloseClick(object? sender, RoutedEventArgs e) => Close();
    #endregion

    #region Main properties
    /// <summary>
    /// Gets or sets the collection of UI elements used in the custom window's title bar.
    /// </summary>
    public ObservableCollection<UIElement> Components
    {
        get => (ObservableCollection<UIElement>)GetValue(ComponentsProperty);
        set => SetValue(ComponentsProperty, value);
    }
    public static readonly DependencyProperty ComponentsProperty
        = DependencyProperty.Register(
            nameof(Components),
            typeof(ObservableCollection<UIElement>),
            typeof(StswWindow)
        );

    /// <summary>
    /// Gets or sets the content of the custom window dialog.
    /// </summary>
    public StswContentDialogModel? ContentDialogBinding
    {
        get => (StswContentDialogModel?)GetValue(ContentDialogProperty);
        set => SetValue(ContentDialogProperty, value);
    }
    public static readonly DependencyProperty ContentDialogProperty
        = DependencyProperty.Register(
            nameof(ContentDialogBinding),
            typeof(StswContentDialogModel),
            typeof(StswWindow)
        );

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
            if (stsw.partFullscreenPanel is not null && stsw.partTitleBar is not null)
            {
                if (stsw.ResizeMode.In(ResizeMode.NoResize, ResizeMode.CanMinimize))
                    return;

                if (stsw.Fullscreen)
                {
                    stsw.preFullscreenState = stsw.WindowState;

                    if (stsw.WindowState == WindowState.Maximized)
                        stsw.WindowState = WindowState.Minimized;

                    stsw.partTitleBar.Visibility = Visibility.Collapsed;
                    stsw.WindowState = WindowState.Maximized;
                }
                else
                {
                    stsw.partTitleBar.Visibility = Visibility.Visible;
                    stsw.partFullscreenPanel.Visibility = Visibility.Collapsed;

                    if (stsw.preFullscreenState == WindowState.Maximized)
                        stsw.WindowState = WindowState.Minimized;

                    stsw.WindowState = stsw.preFullscreenState;
                }
                stsw.Focus();

                stsw.FullscreenChanged?.Invoke(stsw, EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// Gets or sets the subtitle text of the custom window.
    /// </summary>
    public string SubTitle
    {
        get => (string)GetValue(SubTitleProperty);
        set => SetValue(SubTitleProperty, value);
    }
    public static readonly DependencyProperty SubTitleProperty
        = DependencyProperty.Register(
            nameof(SubTitle),
            typeof(string),
            typeof(StswWindow)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the corner radius of the custom window.
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
                stsw.AllowsTransparency = stsw.CornerRadius.TopLeft > 0
                                       || stsw.CornerRadius.TopRight > 0
                                       || stsw.CornerRadius.BottomLeft > 0
                                       || stsw.CornerRadius.BottomRight > 0;
        }
    }
    #endregion

    #region OnInitialized
    private HwndSource? _hwndSource;

    /// OnInitialized
    protected override void OnInitialized(EventArgs e)
    {
        SourceInitialized += OnSourceInitialized; /// Avoid hiding task bar upon maximization
        Loaded += OnLoaded; /// Hide default context menu and show custom
        Closed += OnClosed; /// Hide default context menu and show custom
        base.OnInitialized(e);

        defaultHeight = Height;
        defaultWidth = Width;
    }

    /// OnSourceInitialized
    void OnSourceInitialized(object? sender, EventArgs e)
    {
        _hwndSource = (HwndSource)PresentationSource.FromVisual(this);
        IntPtr handle = new WindowInteropHelper(this).Handle;
        HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WindowProc));
    }
    #endregion

    #region Hide default context menu and show custom
    private HwndSourceHook? _hook;

    /// OnLoaded
    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (PresentationSource.FromVisual(this) is HwndSource hwndSource)
        {
            _hook = new HwndSourceHook(WndProc);
            hwndSource.AddHook(_hook);
        }
    }

    /// OnClosed
    private void OnClosed(object? sender, EventArgs e)
    {
        if (PresentationSource.FromVisual(this) is HwndSource hwndSource)
            hwndSource.RemoveHook(_hook);
    }

    /// WndProc
    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == 0xa4 && wParam.ToInt32() == 0x02 || msg == 165)
        {
            if (partTitleBar != null)
                partTitleBar.ContextMenu.IsOpen = true;
            handled = true;
        }
        return IntPtr.Zero;
    }
    #endregion

    #region Avoid hiding task bar upon maximization
    /// WindowProc
    private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        switch (msg)
        {
            case 0x0024:
                if (!Fullscreen)
                    WmGetMinMaxInfo(hwnd, lParam);
                handled = false;
                break;
        }
        return (IntPtr)0;
    }

    /// WmGetMinMaxInfo
    private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
    {
        if (Marshal.PtrToStructure(lParam, typeof(MINMAXINFO)) is MINMAXINFO mmi)
        {
            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            var monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero)
            {
                var monitorInfo = new MONITORINFO();
                GetMonitorInfo(monitor, monitorInfo);
                var rcWorkArea = monitorInfo.rcWork;
                var rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
                mmi.ptMaxTrackSize.x = Math.Abs(rcWorkArea.Width);
                mmi.ptMaxTrackSize.y = Math.Abs(rcWorkArea.Height);
            }

            Marshal.StructureToPtr(mmi, lParam, true);
        }
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
    #endregion

    [DllImport("user32")]
    internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

    [DllImport("User32")]
    internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);
}
