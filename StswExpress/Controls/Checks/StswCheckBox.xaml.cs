using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Represents a control that allows the user to toggle between three states: checked, unchecked and indeterminate.
/// </summary>
public class StswCheckBox : CheckBox
{
    static StswCheckBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswCheckBox), new FrameworkPropertyMetadata(typeof(StswCheckBox)));
    }

    #region Main properties
    /// <summary>
    /// Gets or sets the scale of the icon in the box.
    /// </summary>
    public GridLength IconScale
    {
        get => (GridLength)GetValue(IconScaleProperty);
        set => SetValue(IconScaleProperty, value);
    }
    public static readonly DependencyProperty IconScaleProperty
        = DependencyProperty.Register(
            nameof(IconScale),
            typeof(GridLength),
            typeof(StswCheckBox)
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
            typeof(StswCheckBox)
        );

    /// <summary>
    /// Gets or sets the brush used to render the glyph (icon).
    /// </summary>
    public Brush? GlyphBrush
    {
        get => (Brush?)GetValue(GlyphBrushProperty);
        set => SetValue(GlyphBrushProperty, value);
    }
    public static readonly DependencyProperty GlyphBrushProperty
        = DependencyProperty.Register(
            nameof(GlyphBrush),
            typeof(Brush),
            typeof(StswCheckBox)
        );

    /// <summary>
    /// Gets or sets the geometry used for the icon in the checked state.
    /// </summary>
    public Geometry? IconChecked
    {
        get => (Geometry?)GetValue(IconCheckedProperty);
        set => SetValue(IconCheckedProperty, value);
    }
    public static readonly DependencyProperty IconCheckedProperty
        = DependencyProperty.Register(
            nameof(IconChecked),
            typeof(Geometry),
            typeof(StswCheckBox)
        );

    /// <summary>
    /// Gets or sets the geometry used for the icon in the indeterminate state.
    /// </summary>
    public Geometry? IconIndeterminate
    {
        get => (Geometry?)GetValue(IconIndeterminateProperty);
        set => SetValue(IconIndeterminateProperty, value);
    }
    public static readonly DependencyProperty IconIndeterminateProperty
        = DependencyProperty.Register(
            nameof(IconIndeterminate),
            typeof(Geometry),
            typeof(StswCheckBox)
        );

    /// <summary>
    /// Gets or sets the geometry used for the icon in the unchecked state.
    /// </summary>
    public Geometry? IconUnchecked
    {
        get => (Geometry?)GetValue(IconUncheckedProperty);
        set => SetValue(IconUncheckedProperty, value);
    }
    public static readonly DependencyProperty IconUncheckedProperty
        = DependencyProperty.Register(
            nameof(IconUnchecked),
            typeof(Geometry),
            typeof(StswCheckBox)
        );
    #endregion
}
