using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Represents a control that displays a header and allows the user to collapse or expand content.
/// </summary>
public class StswExpander : Expander
{
    static StswExpander()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswExpander), new FrameworkPropertyMetadata(typeof(StswExpander)));
    }

    #region Main properties
    /// <summary>
    /// Gets or sets the visibility of the arrow icon in the drop button.
    /// </summary>
    public Visibility ArrowVisibility
    {
        get => (Visibility)GetValue(ArrowVisibilityProperty);
        set => SetValue(ArrowVisibilityProperty, value);
    }
    public static readonly DependencyProperty ArrowVisibilityProperty
        = DependencyProperty.Register(
            nameof(ArrowVisibility),
            typeof(Visibility),
            typeof(StswExpander)
        );
    #endregion

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
            typeof(StswExpander)
        );

    /// <summary>
    /// Gets or sets the thickness of the border used as separator between header and content.
    /// </summary>
    public Thickness SubBorderThickness
    {
        get => (Thickness)GetValue(SubBorderThicknessProperty);
        set => SetValue(SubBorderThicknessProperty, value);
    }
    public static readonly DependencyProperty SubBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(SubBorderThickness),
            typeof(Thickness),
            typeof(StswExpander)
        );
    #endregion
}
