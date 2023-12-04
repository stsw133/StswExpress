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
    }

    #region Style properties
    /// <summary>
    /// 
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
            typeof(StswRadioButton)
        );

    /// <summary>
    /// Gets or sets the degree to which the corners of the control are rounded.
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
            typeof(StswRadioButton)
        );
    #endregion
}
