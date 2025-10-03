using System.Windows;
using System.Windows.Controls.Primitives;

namespace StswExpress;

/// <summary>
/// Represents a button control that continuously triggers an action while it is pressed and held.
/// This control extends <see cref="RepeatButton"/>, providing additional styling options such as corner rounding.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswRepeatButton Command="{Binding MyCommand}" Content="Hold Me"/&gt;
/// </code>
/// </example>
public class StswRepeatButton : RepeatButton, IStswCornerControl
{
    static StswRepeatButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswRepeatButton), new FrameworkPropertyMetadata(typeof(StswRepeatButton)));
    }

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
            typeof(StswRepeatButton),
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
            typeof(StswRepeatButton),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
