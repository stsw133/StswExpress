using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;

namespace StswExpress;

/// <summary>
/// Provides attached properties for customizing the appearance and behavior of WPF controls. 
/// Includes support for animations, ripple effects, borderless styling, and sub-control docking.
/// </summary>
/// <remarks>
/// This static class offers flexible property extensions that can be applied to various controls 
/// without modifying their base implementation.
/// </remarks>
[Stsw("0.7.0")]
public static class StswControl
{
    /// <summary>
    /// Enables or disables animations within the control.
    /// </summary>
    public static readonly DependencyProperty EnableAnimationsProperty
        = DependencyProperty.RegisterAttached(
            nameof(EnableAnimationsProperty)[..^8],
            typeof(bool),
            typeof(StswControl),
            new PropertyMetadata(true)
        );
    public static bool GetEnableAnimations(DependencyObject obj) => (bool)obj.GetValue(EnableAnimationsProperty);
    public static void SetEnableAnimations(DependencyObject obj, bool value) => obj.SetValue(EnableAnimationsProperty, value);

    /// <summary>
    /// Enables or disables a ripple effect triggered upon mouse click.
    /// The ripple effect is a circular animation starting from the click point and expanding outward.
    /// </summary>
    public static readonly DependencyProperty EnableRippleEffectProperty
        = DependencyProperty.RegisterAttached(
            nameof(EnableRippleEffectProperty)[..^8],
            typeof(bool),
            typeof(StswControl),
            new PropertyMetadata(false, OnEnableRippleEffectChanged)
        );
    public static bool GetEnableRippleEffect(DependencyObject obj) => (bool)obj.GetValue(EnableRippleEffectProperty);
    public static void SetEnableRippleEffect(DependencyObject obj, bool value) => obj.SetValue(EnableRippleEffectProperty, value);
    private static void OnEnableRippleEffectChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not Control stsw)
            return;

        if ((bool)e.NewValue)
            stsw.PreviewMouseDown += Button_PreviewMouseDown;
        else
            stsw.PreviewMouseDown -= Button_PreviewMouseDown;
    }
    private static void Button_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is Control control)
        {
            if (StswSettings.Default.EnableAnimations /*&& GetEnableAnimations(control)*/)
            {
                var point = e.GetPosition(control);
                var size = Math.Max(control.ActualWidth, control.ActualHeight);

                if ((control.Template.FindName("OPT_MainBorder", control) ?? control.Template.FindName("PART_MainBorder", control)) is Border border)
                {
                    var adornerLayer = AdornerLayer.GetAdornerLayer(control);
                    if (adornerLayer != null)
                    {
                        var rippleAdorner = new StswRippleAdorner(control, point, size, border);
                        adornerLayer.Add(rippleAdorner);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Enables or disables the system drop shadow effect for the control.
    /// </summary>
    public static readonly DependencyProperty EnableSystemDropShadowProperty
        = DependencyProperty.RegisterAttached(
            nameof(EnableSystemDropShadowProperty)[..^8],
            typeof(bool),
            typeof(StswControl),
            new PropertyMetadata(false, OnEnableSystemDropShadowChanged)
        );
    public static bool GetEnableSystemDropShadow(DependencyObject obj) => (bool)obj.GetValue(EnableSystemDropShadowProperty);
    public static void SetEnableSystemDropShadow(DependencyObject obj, bool value) => obj.SetValue(EnableSystemDropShadowProperty, value);
    private static void OnEnableSystemDropShadowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not bool enabled || !enabled)
            return;

        if (d is Window window)
        {
            window.SourceInitialized += (_, _) =>
            {
                var hwnd = new WindowInteropHelper(window).Handle;
                ApplyDropShadow(hwnd);
            };
        }
        else if (d is Popup popup)
        {
            popup.Opened += (_, __) =>
            {
                if (popup.Child is FrameworkElement child)
                    child.Dispatcher.BeginInvoke(() =>
                    {
                        if (PresentationSource.FromVisual(child) is HwndSource source)
                            ApplyDropShadow(source.Handle);
                    });
            };
        }
        else if (d is FrameworkElement fe)
        {
            fe.Loaded += (_, _) =>
            {
                if (PresentationSource.FromVisual(fe) is HwndSource source)
                    ApplyDropShadow(source.Handle);
            };
        }
    }

    private static void ApplyDropShadow(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero)
            return;

        var val = 2;
        DwmSetWindowAttribute(hwnd, DWMWA_NCRENDERING_POLICY, ref val, sizeof(int));

        var margins = new MARGINS
        {
            cxLeftWidth = 1,
            cxRightWidth = 1,
            cyTopHeight = 1,
            cyBottomHeight = 1
        };

        DwmExtendFrameIntoClientArea(hwnd, ref margins);
    }

    private const int DWMWA_NCRENDERING_POLICY = 2;

    [StructLayout(LayoutKind.Sequential)]
    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    /// <summary>
    /// Enables or disables borderless appearance for controls implementing <see cref="IStswCornerControl"/>.
    /// When enabled, the control's border thickness is set to zero, and corner rounding is disabled.
    /// </summary>
    public static readonly DependencyProperty IsBorderlessProperty
        = DependencyProperty.RegisterAttached(
            nameof(IsBorderlessProperty)[..^8],
            typeof(bool),
            typeof(StswControl),
            new PropertyMetadata(false, OnIsBorderlessChanged)
        );
    public static void SetIsBorderless(DependencyObject obj, bool value) => obj.SetValue(IsBorderlessProperty, value);
    private static void OnIsBorderlessChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not IStswCornerControl stsw)
            return;

        if ((bool)e.NewValue)
        {
            stsw.BorderThickness = new(0);
            stsw.CornerClipping = false;
            stsw.CornerRadius = new(0);
        }
        else if (Application.Current.TryFindResource(obj.GetType()) is Style defaultStyle)
        {
            var setters = defaultStyle.Setters.OfType<Setter>();
            stsw.BorderThickness = (Thickness?)setters.FirstOrDefault(x => x.Property.Name == nameof(stsw.BorderThickness))?.Value ?? new(0);
            stsw.CornerClipping = (bool?)setters.FirstOrDefault(x => x.Property.Name == nameof(stsw.CornerClipping))?.Value ?? false;
            stsw.CornerRadius = (CornerRadius?)setters.FirstOrDefault(x => x.Property.Name == nameof(stsw.CornerRadius))?.Value ?? new(0);
        }
    }

    /// <summary>
    /// Determines the docking position of sub-controls within a parent control.
    /// </summary>
    public static readonly DependencyProperty SubControlsDockProperty
        = DependencyProperty.RegisterAttached(
            nameof(SubControlsDockProperty)[..^8],
            typeof(Dock),
            typeof(StswControl),
            new PropertyMetadata(Dock.Right, OnSubControlsDockChanged)
        );
    public static void SetSubControlsDock(DependencyObject obj, Dock value) => obj.SetValue(SubControlsDockProperty, value);
    private static void OnSubControlsDockChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not Control control)
            return;

        if ((control.Template.FindName("OPT_SubControls", control) ?? control.Template.FindName("PART_SubControls", control)) is ItemsControl itemsControl)
            DockPanel.SetDock(itemsControl, (Dock?)e.NewValue ?? (Dock)SubControlsDockProperty.DefaultMetadata.DefaultValue);
    }
}
