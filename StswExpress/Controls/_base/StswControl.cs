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
public static class StswControl
{
    #region Alignment
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
    public static void SetSubControlsDock(DependencyObject d, Dock value) => d.SetValue(SubControlsDockProperty, value);
    private static void OnSubControlsDockChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Control control)
            return;

        if ((control.Template.FindName("OPT_SubControls", control) ?? control.Template.FindName("PART_SubControls", control)) is ItemsControl itemsControl)
            DockPanel.SetDock(itemsControl, (Dock?)e.NewValue ?? (Dock)SubControlsDockProperty.DefaultMetadata.DefaultValue);
    }
    #endregion

    #region Animations
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
    public static bool GetEnableAnimations(DependencyObject d) => (bool)d.GetValue(EnableAnimationsProperty);
    public static void SetEnableAnimations(DependencyObject d, bool value) => d.SetValue(EnableAnimationsProperty, value);

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
    public static bool GetEnableRippleEffect(DependencyObject d) => (bool)d.GetValue(EnableRippleEffectProperty);
    public static void SetEnableRippleEffect(DependencyObject d, bool value) => d.SetValue(EnableRippleEffectProperty, value);
    private static void OnEnableRippleEffectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Control stsw)
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
    public static bool GetEnableSystemDropShadow(DependencyObject d) => (bool)d.GetValue(EnableSystemDropShadowProperty);
    public static void SetEnableSystemDropShadow(DependencyObject d, bool value) => d.SetValue(EnableSystemDropShadowProperty, value);
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
    #endregion

    #region Border
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
    public static void SetIsBorderless(DependencyObject d, bool value) => d.SetValue(IsBorderlessProperty, value);
    private static void OnIsBorderlessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not IStswCornerControl stsw)
            return;

        if ((bool)e.NewValue)
        {
            stsw.BorderThickness = new(0);
            stsw.CornerClipping = false;
            stsw.CornerRadius = new(0);
        }
        else if (Application.Current.TryFindResource(d.GetType()) is Style defaultStyle)
        {
            var setters = defaultStyle.Setters.OfType<Setter>();
            stsw.BorderThickness = (Thickness?)setters.FirstOrDefault(x => x.Property.Name == nameof(stsw.BorderThickness))?.Value ?? new(0);
            stsw.CornerClipping = (bool?)setters.FirstOrDefault(x => x.Property.Name == nameof(stsw.CornerClipping))?.Value ?? false;
            stsw.CornerRadius = (CornerRadius?)setters.FirstOrDefault(x => x.Property.Name == nameof(stsw.CornerRadius))?.Value ?? new(0);
        }
    }

    /// <summary>
    /// Indicates whether the synchronization between the base <see cref="Control.BorderThicknessProperty"/>
    /// </summary>
    private static readonly DependencyProperty IsSyncingProperty
        = DependencyProperty.RegisterAttached(
            nameof(IsSyncingProperty)[..^8],
            typeof(bool),
            typeof(StswThickness)
        );
    private static bool GetIsSyncing(DependencyObject o) => (bool)o.GetValue(IsSyncingProperty);
    private static void SetIsSyncing(DependencyObject o, bool v) => o.SetValue(IsSyncingProperty, v);

    /// <summary>
    /// Overrides the metadata of the base <see cref="Control.BorderThicknessProperty"/> for your control type,
    /// to synchronize it with an extended <see cref="StswThickness"/> property.
    /// </summary>
    /// <typeparam name="TControl">The control type.</typeparam>
    /// <param name="getExt">Function to get the extended <see cref="StswThickness"/> value from the control.</param>
    /// <param name="setExt">Action to set the extended <see cref="StswThickness"/> value on the control.</param>
    internal static void OverrideBaseBorderThickness<TControl>(Func<TControl, StswThickness> getExt, Action<TControl, StswThickness> setExt) where TControl : Control
        => Control.BorderThicknessProperty.OverrideMetadata(
            typeof(TControl),
            new FrameworkPropertyMetadata(
                default(Thickness),
                (d, e) =>
                {
                    var ctrl = (TControl)d;
                    if (GetIsSyncing(ctrl))
                        return;

                    SetIsSyncing(ctrl, true);
                    try
                    {
                        var outer = (Thickness)e.NewValue;
                        var cur = getExt(ctrl);
                        var updated = new StswThickness(outer.Left, outer.Top, outer.Right, outer.Bottom, cur.Inside);
                        setExt(ctrl, updated);
                    }
                    finally
                    {
                        SetIsSyncing(ctrl, false);
                    }
                }));

    /// <summary>
    /// Creates a <see cref="PropertyChangedCallback"/> for the extended <see cref="StswThickness"/> property,
    /// to synchronize it with the base <see cref="Control.BorderThicknessProperty"/>.
    /// </summary>
    /// <typeparam name="TControl">The control type.</typeparam>
    /// <param name="setBase">Action to set the base <see cref="Thickness"/> value on the control.</param>
    /// <returns>The created <see cref="PropertyChangedCallback"/>.</returns>
    internal static PropertyChangedCallback CreateExtendedChangedCallback<TControl>(Action<TControl, Thickness> setBase) where TControl : Control
        => (d, e) =>
        {
            var ctrl = (TControl)d;
            if (GetIsSyncing(ctrl))
                return;

            SetIsSyncing(ctrl, true);
            try
            {
                var st = (StswThickness)e.NewValue;
                setBase(ctrl, st.Thickness);
            }
            finally
            {
                SetIsSyncing(ctrl, false);
            }
        };
    #endregion
}
