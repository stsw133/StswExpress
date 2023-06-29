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
            typeof(StswButton),
            new FrameworkPropertyMetadata(default(CornerRadius),
                FrameworkPropertyMetadataOptions.None,
                null, null, false, System.Windows.Data.UpdateSourceTrigger.PropertyChanged)
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    #endregion
}
