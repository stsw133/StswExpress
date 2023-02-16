using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Shell;

namespace StswExpress;

public class StswWindow : Window
{
    private double DefaultHeight, DefaultWidth;
    private FrameworkElement TitleBar;
    private StswHeader MoveRectangle;

    /// Constructors
    public StswWindow()
    {
        SetValue(CustomControlsProperty, new ObservableCollection<UIElement>()); /// without this controls move into newly opened window
    }
    static StswWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswWindow), new FrameworkPropertyMetadata(typeof(StswWindow)));
    }

    #region Properties
    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswWindow),
            new FrameworkPropertyMetadata(default(CornerRadius),
              FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
              CornerRadiusChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static void CornerRadiusChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswWindow window && !window.IsLoaded)
            window.AllowsTransparency = window.CornerRadius.TopLeft != 0;
    }

    /// CustomControls
    public static readonly DependencyProperty CustomControlsProperty
        = DependencyProperty.Register(
            nameof(CustomControls),
            typeof(ObservableCollection<UIElement>),
            typeof(StswWindow),
            new PropertyMetadata(default(ObservableCollection<UIElement>))
        );
    public ObservableCollection<UIElement> CustomControls
    {
        get => (ObservableCollection<UIElement>)GetValue(CustomControlsProperty);
        set => SetValue(CustomControlsProperty, value);
    }
    /*
    /// Fullscreen
    public static readonly DependencyProperty FullscreenProperty
        = DependencyProperty.Register(
            nameof(Fullscreen),
            typeof(bool),
            typeof(StswWindow),
            new PropertyMetadata(default(bool))
        );
    public bool Fullscreen
    {
        get => (bool)GetValue(FullscreenProperty);
        set => SetValue(FullscreenProperty, value);
    }
    */
    /// SubTitle
    public static readonly DependencyProperty SubTitleProperty
        = DependencyProperty.Register(
            nameof(SubTitle),
            typeof(string),
            typeof(StswWindow),
            new PropertyMetadata(default(string))
        );
    public string SubTitle
    {
        get => (string)GetValue(SubTitleProperty);
        set => SetValue(SubTitleProperty, value);
    }
    #endregion

    #region OnInitialized
    private HwndSource _hwndSource;

    protected override void OnInitialized(EventArgs e)
    {
        SourceInitialized += OnSourceInitialized;
        Loaded += OnLoaded;
        Closed += OnClosed;
        base.OnInitialized(e);

        DefaultHeight = Height;
        DefaultWidth = Width;
    }
    void OnSourceInitialized(object sender, EventArgs e)
    {
        _hwndSource = (HwndSource)PresentationSource.FromVisual(this);
        IntPtr handle = (new WindowInteropHelper(this)).Handle;
        HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WindowProc));
    }
    #endregion

    #region Hide default context menu and show custom
    private HwndSourceHook _hook;
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (PresentationSource.FromVisual(this) is HwndSource hwndSource)
        {
            _hook = new HwndSourceHook(WndProc);
            hwndSource.AddHook(_hook);
        }
    }
    private void OnClosed(object sender, EventArgs e)
    {
        if (PresentationSource.FromVisual(this) is HwndSource hwndSource)
            hwndSource.RemoveHook(_hook);
    }

    private const uint WM_SYSTEMMENU = 0xa4;
    private const uint WP_SYSTEMMENU = 0x02;

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if ((msg == WM_SYSTEMMENU && wParam.ToInt32() == WP_SYSTEMMENU) || msg == 165)
        {
            TitleBar.ContextMenu.IsOpen = true;
            handled = true;
        }
        return IntPtr.Zero;
    }
    #endregion

    #region Avoid hiding task bar upon maximization
    private static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        switch (msg)
        {
            case 0x0024:
                WmGetMinMaxInfo(hwnd, lParam);
                handled = false;
                break;
        }
        return (IntPtr)0;
    }

    private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
    {
        var mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));
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
        public RECT rcMonitor = new RECT();
        public RECT rcWork = new RECT();
        public int dwFlags = 0;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct RECT
    {
        public int left, top, right, bottom;
        public static readonly RECT Empty = new RECT();
        public int Width => Math.Abs(right - left);
        public int Height => bottom - top;

        public RECT(int left, int top, int right, int bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public RECT(RECT rcSrc)
        {
            this.left = rcSrc.left;
            this.top = rcSrc.top;
            this.right = rcSrc.right;
            this.bottom = rcSrc.bottom;
        }

        public bool IsEmpty => left >= right || top >= bottom;

        public override string ToString() => this == Empty ? "RECT {Empty}" : $"RECT {{ left : {left} / top : {top} / right : {right} / bottom : {bottom} }}";
        public override bool Equals(object obj) => obj is Rect && this == (RECT)obj;
        public override int GetHashCode() => left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
        public static bool operator ==(RECT rect1, RECT rect2) => rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom;
        public static bool operator !=(RECT rect1, RECT rect2) => !(rect1 == rect2);
    }
    #endregion

    /// OnApplyTemplate
    public override void OnApplyTemplate()
    {
        /// minimizeButton
        if (GetTemplateChild("minimizeButton") is Button minimizeButton && ResizeMode != ResizeMode.NoResize)
            minimizeButton.Click += MinimizeClick;
        /// restoreButton
        if (GetTemplateChild("restoreButton") is Button restoreButton && ResizeMode.In(ResizeMode.CanResizeWithGrip, ResizeMode.CanResize))
            restoreButton.Click += RestoreClick;
        /// closeButton
        if (GetTemplateChild("closeButton") is Button closeButton)
            closeButton.Click += CloseClick;

        TitleBar = (FrameworkElement)GetTemplateChild("titleBar");

        /// interfaceMenuItem
        if (TitleBar.ContextMenu.Items[0] is MenuItem interfaceMenuItem)
        {
            /// isizeMenuItem
            if (interfaceMenuItem.Items[0] is MenuItem isizeMenuItem)
                isizeMenuItem.Click += iSizeClick;
            /// themeMenuItem
            if (interfaceMenuItem.Items[1] is MenuItem themeMenuItem)
            {
                /// themeAutoMenuItem
                if (themeMenuItem.Items[0] is MenuItem themeAutoMenuItem)
                    themeAutoMenuItem.Click += (s, e) => ChangeTheme(-1);
                /// theme0MenuItem
                if (themeMenuItem.Items[1] is MenuItem theme0MenuItem)
                    theme0MenuItem.Click += (s, e) => ChangeTheme(0);
                /// theme1MenuItem
                if (themeMenuItem.Items[2] is MenuItem theme1MenuItem)
                    theme1MenuItem.Click += (s, e) => ChangeTheme(1);
            }
            /// hideSubTitle
            if (interfaceMenuItem.Items[2] is MenuItem hideSubTitle)
                hideSubTitle.Click += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(SubTitle))
                        MoveRectangle.SubTexts[0].Visibility = Settings.Default.ShowSubTitle ? Visibility.Visible : Visibility.Collapsed;
                };
        }
        /// centerMenuItem
        if (TitleBar.ContextMenu.Items[2] is MenuItem centerMenuItem)
            centerMenuItem.Click += CenterClick;
        /// defaultMenuItem
        if (TitleBar.ContextMenu.Items[3] is MenuItem defaultMenuItem && ResizeMode.In(ResizeMode.CanResizeWithGrip, ResizeMode.CanResize))
            defaultMenuItem.Click += DefaultClick;
        /// minimizeMenuItem
        if (TitleBar.ContextMenu.Items[4] is MenuItem minimizeMenuItem && ResizeMode != ResizeMode.NoResize)
            minimizeMenuItem.Click += MinimizeClick;
        /// restoreMenuItem
        if (TitleBar.ContextMenu.Items[5] is MenuItem restoreMenuItem && ResizeMode.In(ResizeMode.CanResizeWithGrip, ResizeMode.CanResize))
            restoreMenuItem.Click += RestoreClick;
        /// closeMenuItem
        if (TitleBar.ContextMenu.Items[7] is MenuItem closeMenuItem)
            closeMenuItem.Click += CloseClick;

        /// Chrome change
        MoveRectangle = (StswHeader)GetTemplateChild("moveRectangle");
        MoveRectangle.SizeChanged += MoveRectangle_SizeChanged;
        StateChanged += (s, e) => MoveRectangle_SizeChanged(MoveRectangle, null);

        base.OnApplyTemplate();
        UpdateLayout();
    }

    /// Chrome change
    private void MoveRectangle_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        var chrome = WindowChrome.GetWindowChrome(this);
        WindowChrome.SetWindowChrome(this, new WindowChrome()
        {
            CornerRadius = chrome.CornerRadius,
            CaptionHeight = TitleBar.ActualHeight - (WindowState == WindowState.Maximized ? 2 : 0),
            GlassFrameThickness = chrome.GlassFrameThickness,
            ResizeBorderThickness = chrome.ResizeBorderThickness,
            UseAeroCaptionButtons = chrome.UseAeroCaptionButtons
        });
    }

    #region ContextMenu
    /// iSizeClick
    protected void iSizeClick(object sender, RoutedEventArgs e) => Settings.Default.iSize = 12;

    /// ChangeTheme
    private void ChangeTheme(int themeID)
    {
        if (!Application.Current.Resources.MergedDictionaries.Any(x => x is Theme))
            Application.Current.Resources.MergedDictionaries.Add(new Theme());
        var theme = (Theme)Application.Current.Resources.MergedDictionaries.First(x => x is Theme);

        if (themeID < 0)
            theme.Color = (ThemeColor)StswFn.GetWindowsTheme();
        else
            theme.Color = (ThemeColor)themeID;
        Settings.Default.Theme = themeID;
    }

    /// CenterClick
    protected void CenterClick(object sender, RoutedEventArgs e)
    {
        if (WindowState == WindowState.Maximized)
            WindowState = WindowState.Normal;

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

    /// DefaultClick
    protected void DefaultClick(object sender, RoutedEventArgs e)
    {
        Height = DefaultHeight;
        Width = DefaultWidth;
        //CenterClick(sender, e);
    }

    /// MinimizeClick
    protected void MinimizeClick(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    /// RestoreClick
    protected void RestoreClick(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;

    /// CloseClick
    protected void CloseClick(object sender, RoutedEventArgs e) => Close();
    #endregion

    [DllImport("user32")]
    internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

    [DllImport("User32")]
    internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);
}
