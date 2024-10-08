using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace StswExpress;

/// <summary>
/// Represents a control that allows users to switch between two states: <c>on</c> and <c>off</c>.
/// </summary>
public class StswToggleButton : ToggleButton, IStswCornerControl
{
    static StswToggleButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswToggleButton), new FrameworkPropertyMetadata(typeof(StswToggleButton)));
    }

    #region Events & methods
    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnChecked(RoutedEventArgs e)
    {
        base.OnChecked(e);

        if (StswSettings.Default.EnableAnimations)
        {
            if (GetTemplateChild("OPT_MainBorder") is Border border)
                StswAnimations.AnimateClick(this, border, true);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnUnchecked(RoutedEventArgs e)
    {
        base.OnUnchecked(e);

        if (StswSettings.Default.EnableAnimations)
        {
            if (GetTemplateChild("OPT_MainBorder") is Border border)
                StswAnimations.AnimateClick(this, border, false);
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
            typeof(StswToggleButton),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender)
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
            typeof(StswToggleButton),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
