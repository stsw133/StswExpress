using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Represents a control that allows the user to select a single option from a group of mutually exclusive options.
/// </summary>
public class StswRadioButton : RadioButton, IStswCornerControl
{
    static StswRadioButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswRadioButton), new FrameworkPropertyMetadata(typeof(StswRadioButton)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswRadioButton), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    #region Events & methods
    /// <summary>
    /// Handles the event triggered when the radio button is checked. If animations are enabled in the settings,
    /// the method animates the control's main border to provide visual feedback for the checked state.
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnChecked(RoutedEventArgs e)
    {
        base.OnChecked(e);

        if (StswSettings.Default.EnableAnimations && StswControl.GetEnableAnimations(this))
        {
            if (GetTemplateChild("OPT_MainBorder") is Border border)
                StswSharedAnimations.AnimateClick(this, border, true);
        }
    }

    /// <summary>
    /// Handles the event triggered when the radio button is unchecked. If animations are enabled in the settings,
    /// the method animates the control's main border to provide visual feedback for the unchecked state.
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnUnchecked(RoutedEventArgs e)
    {
        base.OnUnchecked(e);

        if (StswSettings.Default.EnableAnimations && StswControl.GetEnableAnimations(this))
        {
            if (GetTemplateChild("OPT_MainBorder") is Border border)
                StswSharedAnimations.AnimateClick(this, border, false);
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
            typeof(StswRadioButton),
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
            typeof(StswRadioButton),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
