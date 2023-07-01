using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

public class StswExpander : Expander
{
    static StswExpander()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswExpander), new FrameworkPropertyMetadata(typeof(StswExpander)));
    }

    #region Main properties
    /// ArrowVisibility
    public static readonly DependencyProperty ArrowVisibilityProperty
        = DependencyProperty.Register(
            nameof(ArrowVisibility),
            typeof(Visibility),
            typeof(StswExpander)
        );
    public Visibility ArrowVisibility
    {
        get => (Visibility)GetValue(ArrowVisibilityProperty);
        set => SetValue(ArrowVisibilityProperty, value);
    }
    #endregion

    #region Spatial properties
    /// > BorderThickness ...
    /// SubBorderThickness
    public static readonly DependencyProperty SubBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(SubBorderThickness),
            typeof(Thickness),
            typeof(StswExpander)
        );
    public Thickness SubBorderThickness
    {
        get => (Thickness)GetValue(SubBorderThicknessProperty);
        set => SetValue(SubBorderThicknessProperty, value);
    }

    /// > CornerRadius ...
    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswExpander)
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    #endregion
}
