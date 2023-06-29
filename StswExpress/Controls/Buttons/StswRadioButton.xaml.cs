using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

public class StswRadioButton : RadioButton
{
    static StswRadioButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswRadioButton), new FrameworkPropertyMetadata(typeof(StswRadioButton)));
    }

    #region Spatial properties
    /// > CornerRadius ...
    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswRadioButton)
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    #endregion
}
