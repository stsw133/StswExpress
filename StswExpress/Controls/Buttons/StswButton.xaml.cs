using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

public class StswButton : Button
{
    static StswButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswButton), new FrameworkPropertyMetadata(typeof(StswButton)));
    }

    #region Spatial properties
    /// > CornerRadius ...
    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswButton)
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    #endregion
}
