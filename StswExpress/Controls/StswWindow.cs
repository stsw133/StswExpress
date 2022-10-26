using DynamicAero2;
using System;
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
    static StswWindow() => DefaultStyleKeyProperty.OverrideMetadata(typeof(StswWindow), new FrameworkPropertyMetadata(typeof(StswWindow)));

    /// AllowToDarkMode
    public static readonly DependencyProperty AllowToDarkModeProperty
        = DependencyProperty.Register(
              nameof(AllowToDarkMode),
              typeof(bool),
              typeof(StswWindow),
              new PropertyMetadata(default(bool))
          );
    public bool AllowToDarkMode
    {
        get => (bool)GetValue(AllowToDarkModeProperty);
        set => SetValue(AllowToDarkModeProperty, value);
    }

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

    /// TitleBarBackground
    public static readonly DependencyProperty TitleBarBackgroundProperty
        = DependencyProperty.Register(
              nameof(TitleBarBackground),
              typeof(Brush),
              typeof(StswWindow),
              new PropertyMetadata(default(Brush))
          );
    public Brush TitleBarBackground
    {
        get => (Brush)GetValue(TitleBarBackgroundProperty);
        set => SetValue(TitleBarBackgroundProperty, value);
    }

    /// Events
    #region Events
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

    /// iSizeClick
    protected void iSizeClick(object sender, RoutedEventArgs e) => Settings.Default.iSize = 12;
    /// themeClick
    private void themeClick(int themeID)
    {
        if (!Application.Current.Resources.MergedDictionaries.Any(x => x is Theme))
            Application.Current.Resources.MergedDictionaries.Add(new Theme());
        var theme = (Theme)Application.Current.Resources.MergedDictionaries.First(x => x is Theme);

        theme.Color = (ThemeColor)themeID;
        Settings.Default.Theme = (int)theme.Color;
        Settings.Default.Save();
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
        CenterClick(sender, e);
    }
    /// DarkModeClick
    protected void DarkModeClick(object sender, RoutedEventArgs e) => themeClick(Settings.Default.Theme == 0 ? 1 : 0);
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
            Fn.OpenContextMenu(MenuItems);
            handled = true;
        }

        return IntPtr.Zero;
    }
    #endregion

    /// OnApplyTemplate
    public override void OnApplyTemplate()
    {
        var buttonsPanel = (StackPanel)GetTemplateChild("buttonsPanel");
        /// darkmodeButton
        var darkmodeButton = GetTemplateChild("darkmodeButton") as Button;
        if (darkmodeButton != null && AllowToDarkMode)
            darkmodeButton.Click += DarkModeClick;
        /// minimizeButton
        var minimizeButton = GetTemplateChild("minimizeButton") as Button;
        if (minimizeButton != null && ResizeMode != ResizeMode.NoResize)
            minimizeButton.Click += MinimizeClick;
        /// restoreButton
        var restoreButton = GetTemplateChild("restoreButton") as Button;
        if (restoreButton != null && ResizeMode.In(ResizeMode.CanResizeWithGrip, ResizeMode.CanResize))
            restoreButton.Click += RestoreClick;
        /// closeButton
        var closeButton = GetTemplateChild("closeButton") as Button;
        if (closeButton != null)
            closeButton.Click += CloseClick;

        var menuItems = (Grid)GetTemplateChild("menuItems");
        /// isizeMenuItem
        var isizeMenuItem = menuItems.ContextMenu.Items[0] as MenuItem;
        if (isizeMenuItem != null)
            isizeMenuItem.Click += iSizeClick;
        /// centerMenuItem
        var centerMenuItem = menuItems.ContextMenu.Items[2] as MenuItem;
        if (centerMenuItem != null)
            centerMenuItem.Click += CenterClick;
        /// defaultMenuItem
        var defaultMenuItem = menuItems.ContextMenu.Items[3] as MenuItem;
        if (defaultMenuItem != null && ResizeMode.In(ResizeMode.CanResizeWithGrip, ResizeMode.CanResize))
            defaultMenuItem.Click += DefaultClick;
        /// minimizeMenuItem
        var minimizeMenuItem = menuItems.ContextMenu.Items[4] as MenuItem;
        if (minimizeMenuItem != null && ResizeMode != ResizeMode.NoResize)
            minimizeMenuItem.Click += MinimizeClick;
        /// restoreMenuItem
        var restoreMenuItem = menuItems.ContextMenu.Items[5] as MenuItem;
        if (restoreMenuItem != null && ResizeMode.In(ResizeMode.CanResizeWithGrip, ResizeMode.CanResize))
            restoreMenuItem.Click += RestoreClick;
        /// closeMenuItem
        var closeMenuItem = menuItems.ContextMenu.Items[7] as MenuItem;
        if (closeMenuItem != null)
            closeMenuItem.Click += CloseClick;

        /// menuItems
        MenuItems = (FrameworkElement)GetTemplateChild("menuItems");
        /// moveRectangle
        var moveRectangle = (Label)GetTemplateChild("moveRectangle");
        moveRectangle.SizeChanged += MoveRectangle_SizeChanged;

        if (TitleBarBackground != null)
            moveRectangle.Background = TitleBarBackground;

        base.OnApplyTemplate();
        UpdateLayout();
    }

    /// MoveRectangle_SizeChanged
    private void MoveRectangle_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        var chrome = WindowChrome.GetWindowChrome(this);
        WindowChrome.SetWindowChrome(this, new WindowChrome()
        {
            CornerRadius = chrome.CornerRadius,
            CaptionHeight = ((FrameworkElement)sender).ActualHeight,
            GlassFrameThickness = chrome.GlassFrameThickness,
            ResizeBorderThickness = chrome.ResizeBorderThickness,
            UseAeroCaptionButtons = chrome.UseAeroCaptionButtons
        });
    }

    /// AddButtonToTitleBar
    protected Button? AddButtonToTitleBar(string text)
    {
        try
        {
            var buttonsPanel = Template.FindName("buttonsPanel", this) as StackPanel;
            if (buttonsPanel != null)
            {
                var button = new Button()
                {
                    Content = new OutlinedTextBlock()
                    {
                        Text = text
                    }
                };
                buttonsPanel.Children.Insert(0, button);
                return button;
            }
        }
        catch { }
        return null;
    }

    #region DLL
    [DllImport("user32")]
    internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

    [DllImport("User32")]
    internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam);
    #endregion
}