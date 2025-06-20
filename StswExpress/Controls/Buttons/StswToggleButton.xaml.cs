using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace StswExpress;

/// <summary>
/// Represents a toggle button control that allows users to switch between two states: <c>On</c> and <c>Off</c>.
/// This control extends <see cref="ToggleButton"/>, providing additional styling options such as corner rounding
/// and optional animations for state transitions.
/// </summary>
public class StswToggleButton : ToggleButton, IStswCornerControl
{
    static StswToggleButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswToggleButton), new FrameworkPropertyMetadata(typeof(StswToggleButton)));
    }

    #region Events & methods
    /// <summary>
    /// Invoked when the button is checked.
    /// If animations are enabled in the settings, the method triggers an animation 
    /// on the control's main border to visually indicate the checked state.
    /// </summary>
    /// <param name="e">The event arguments associated with the checked event.</param>
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
    /// Invoked when the button is unchecked.
    /// If animations are enabled in the settings, the method triggers an animation 
    /// on the control's main border to visually indicate the unchecked state.
    /// </summary>
    /// <param name="e">The event arguments associated with the unchecked event.</param>
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
            typeof(StswToggleButton),
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
            typeof(StswToggleButton),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}

/* usage:

<se:StswToggleButton Content="Enable Feature" IsChecked="True"/>

*/
