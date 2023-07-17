using System.Windows;
using System.Windows.Controls.Primitives;

namespace StswExpress;

/// <summary>
/// Represents a control that provides a user interface element used to repeatedly trigger actions when pressed and held.
/// </summary>
public class StswRepeatButton : RepeatButton
{
    static StswRepeatButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswRepeatButton), new FrameworkPropertyMetadata(typeof(StswRepeatButton)));
    }

    #region Style properties
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
            typeof(StswRepeatButton)
        );
    #endregion
}
