using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Represents a radio button control that allows the user to select a single option from a group of mutually exclusive options.
/// This control extends <see cref="RadioButton"/> with additional styling options, including corner rounding and animations.
/// </summary>
public class StswRadioButton : RadioButton, IStswCornerControl
{
    static StswRadioButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswRadioButton), new FrameworkPropertyMetadata(typeof(StswRadioButton)));
    }

    #region Events & methods
    /// <summary>
    /// Invoked when the button is checked. 
    /// If animations are enabled in the settings, the method triggers an animation 
    /// on the control's main border to provide visual feedback.
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
            typeof(StswRadioButton),
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
            typeof(StswRadioButton),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}

/* usage:

<StackPanel>
    <se:StswRadioButton Content="Option 1" GroupName="Group1"/>
    <se:StswRadioButton Content="Option 2" GroupName="Group1"/>
</StackPanel>

*/
