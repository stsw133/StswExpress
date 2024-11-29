using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// Provides attached properties for customizing the appearance and behavior of controls.
/// </summary>
public static class StswControl
{
    /// <summary>
    /// When set to <see langword="true"/>, it enables animations within the control.
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
    /// When set to <see langword="true"/>, it enables a ripple effect that is triggered upon a mouse click within the control.
    /// The ripple effect is visualized as a circular animation starting from the point of click and expanding outward.
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
        if (obj is Control stsw)
        {
            if ((bool)e.NewValue)
                stsw.PreviewMouseDown += Button_PreviewMouseDown;
            else
                stsw.PreviewMouseDown -= Button_PreviewMouseDown;
        }
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
                        var rippleAdorner = new RippleAdorner(control, point, size, border);
                        adornerLayer.Add(rippleAdorner);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Attached property to control the borderless appearance of controls implementing the <see cref="IStswCornerControl"/> interface.
    /// When set to <see langword="true"/>, it hides the border by setting BorderThickness to <c>0</c>,
    /// CornerClipping to <see langword="false"/>, and CornerRadius to <c>0</c>.
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
        if (obj is IStswCornerControl stsw)
        {
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
    }

    /// <summary>
    /// Identifies the SubControlsDock attached property.
    /// Determines the docking position of sub-controls.
    /// </summary>
    public static readonly DependencyProperty SubControlsDockProperty
        = DependencyProperty.RegisterAttached(
            nameof(SubControlsDockProperty)[..^8],
            typeof(Dock),
            typeof(StswControl),
            new PropertyMetadata(Dock.Right)
        );
    public static Dock GetSubControlsDock(DependencyObject obj) => (Dock)obj.GetValue(SubControlsDockProperty);
    public static void SetSubControlsDock(DependencyObject obj, Dock value) => obj.SetValue(SubControlsDockProperty, value);
}
