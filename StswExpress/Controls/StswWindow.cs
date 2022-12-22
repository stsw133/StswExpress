using DynamicAero2;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;

namespace StswExpress;

public class StswWindow : Window
{
    private double DefaultHeight, DefaultWidth;

    /// Constructors
    public StswWindow() => SetValue(CustomControlsProperty, new ObservableCollection<UIElement>());
    static StswWindow() => DefaultStyleKeyProperty.OverrideMetadata(typeof(StswWindow), new FrameworkPropertyMetadata(typeof(StswWindow)));

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
    /// SubIcon
    public static readonly DependencyProperty SubIconProperty
        = DependencyProperty.Register(
              nameof(SubIcon),
              typeof(ImageSource),
              typeof(StswWindow),
              new PropertyMetadata(default(ImageSource))
          );
    public ImageSource SubIcon
    {
        get => (ImageSource)GetValue(SubIconProperty);
        set => SetValue(SubIconProperty, value);
    }

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

    #region OnInitialized
    private HwndSource _hwndSource;
    protected override void OnInitialized(EventArgs e)
    {
        SourceInitialized += OnSourceInitialized;
        Loaded += OnLoaded;
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

    #region Menu
    /// iSizeClick
    protected void iSizeClick(object sender, RoutedEventArgs e) => Settings.Default.iSize = 12;

    /// ChangeTheme
    private void ChangeTheme(int themeID)
    {
        if (!Application.Current.Resources.MergedDictionaries.Any(x => x is Theme))
            Application.Current.Resources.MergedDictionaries.Add(new Theme());
        var theme = (Theme)Application.Current.Resources.MergedDictionaries.First(x => x is Theme);

        theme.Color = (ThemeColor)themeID;
        Settings.Default.Theme = (int)theme.Color;
    }

    /// TitleBarColorClick
    protected void TitleBarColorClick(object sender, RoutedEventArgs e) => Settings.Default.TitleBarColor = new ColorConverter().ConvertToString(SystemParameters.WindowGlassColor);

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
        CenterClick(sender, e);
    }

    /// DarkModeClick
    protected void DarkModeClick(object sender, RoutedEventArgs e) => ChangeTheme(Settings.Default.Theme == 0 ? 1 : 0);

    /// MinimizeClick
    protected void MinimizeClick(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    /// RestoreClick
    protected void RestoreClick(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;

    /// CloseClick
    protected void CloseClick(object sender, RoutedEventArgs e) => Close();
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

    #region Hide default context menu and show custom
    private FrameworkElement MenuItems;
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        IntPtr windowhandle = new WindowInteropHelper(this).Handle;
        HwndSource hwndSource = HwndSource.FromHwnd(windowhandle);
        hwndSource.AddHook(new HwndSourceHook(WndProc));
    }

    private const uint WM_SYSTEMMENU = 0xa4;
    private const uint WP_SYSTEMMENU = 0x02;

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (((msg == WM_SYSTEMMENU) && (wParam.ToInt32() == WP_SYSTEMMENU)) || msg == 165)
        {
            StswFn.OpenContextMenu(MenuItems);
            handled = true;
        }

        return IntPtr.Zero;
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

        var menuItems = (Grid)GetTemplateChild("menuItems");
        /// interfaceMenuItem
        if (menuItems.ContextMenu.Items[0] is MenuItem interfaceMenuItem)
        {
            /// isizeMenuItem
            if (interfaceMenuItem.Items[0] is MenuItem isizeMenuItem)
                isizeMenuItem.Click += iSizeClick;
            /// themeMenuItem
            if (interfaceMenuItem.Items[1] is MenuItem themeMenuItem)
            {
                /// theme0MenuItem
                if (themeMenuItem.Items[0] is MenuItem theme0MenuItem)
                    theme0MenuItem.Click += (s, e) => ChangeTheme(0);
                /// theme1MenuItem
                if (themeMenuItem.Items[1] is MenuItem theme1MenuItem)
                    theme1MenuItem.Click += (s, e) => ChangeTheme(1);
            }
            /// titlebarcolorMenuItem
            if (interfaceMenuItem.Items[2] is MenuItem titlebarcolorMenuItem)
                titlebarcolorMenuItem.Click += TitleBarColorClick;
        }
        /// centerMenuItem
        if (menuItems.ContextMenu.Items[2] is MenuItem centerMenuItem)
            centerMenuItem.Click += CenterClick;
        /// defaultMenuItem
        if (menuItems.ContextMenu.Items[3] is MenuItem defaultMenuItem && ResizeMode.In(ResizeMode.CanResizeWithGrip, ResizeMode.CanResize))
            defaultMenuItem.Click += DefaultClick;
        /// minimizeMenuItem
        if (menuItems.ContextMenu.Items[4] is MenuItem minimizeMenuItem && ResizeMode != ResizeMode.NoResize)
            minimizeMenuItem.Click += MinimizeClick;
        /// restoreMenuItem
        if (menuItems.ContextMenu.Items[5] is MenuItem restoreMenuItem && ResizeMode.In(ResizeMode.CanResizeWithGrip, ResizeMode.CanResize))
            restoreMenuItem.Click += RestoreClick;
        /// closeMenuItem
        if (menuItems.ContextMenu.Items[7] is MenuItem closeMenuItem)
            closeMenuItem.Click += CloseClick;

        /// menuItems
        MenuItems = (FrameworkElement)GetTemplateChild("menuItems");

        /// Chrome change
        MoveRectangle = (Label)GetTemplateChild("moveRectangle");
        MoveRectangle.SizeChanged += MoveRectangle_SizeChanged;
        StateChanged += StswWindow_StateChanged;

        base.OnApplyTemplate();
        UpdateLayout();
    }

    /// Chrome change
    private FrameworkElement MoveRectangle;
    private void MoveRectangle_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        var chrome = WindowChrome.GetWindowChrome(this);
        WindowChrome.SetWindowChrome(this, new WindowChrome()
        {
            CornerRadius = chrome.CornerRadius,
            CaptionHeight = MoveRectangle.ActualHeight - (WindowState == WindowState.Maximized ? 7 : 0),
            GlassFrameThickness = chrome.GlassFrameThickness,
            ResizeBorderThickness = chrome.ResizeBorderThickness,
            UseAeroCaptionButtons = chrome.UseAeroCaptionButtons
        });
    }
    private void StswWindow_StateChanged(object? sender, EventArgs e) => MoveRectangle_SizeChanged(MoveRectangle, null);

    #region DLL
    [DllImport("user32")]
    internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

    [DllImport("User32")]
    internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam);
    #endregion
}