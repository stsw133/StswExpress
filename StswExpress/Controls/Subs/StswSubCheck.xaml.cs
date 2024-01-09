using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Represents a control that functions as a sub control and displays an icon.
/// </summary>
public class StswSubCheck : CheckBox, IStswSubControl//, IStswIconControl
{
    static StswSubCheck()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSubCheck), new FrameworkPropertyMetadata(typeof(StswSubCheck)));
    }

    #region Main properties
    /// <summary>
    /// Gets or sets the visibility of the content within the control.
    /// </summary>
    public Visibility? ContentVisibility
    {
        get => (Visibility)GetValue(ContentVisibilityProperty);
        set => SetValue(ContentVisibilityProperty, value);
    }
    public static readonly DependencyProperty ContentVisibilityProperty
        = DependencyProperty.Register(
            nameof(ContentVisibility),
            typeof(Visibility),
            typeof(StswSubCheck)
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
            typeof(StswSubCheck)
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
            typeof(StswSubCheck)
        );

    /// <summary>
    /// Gets or sets the scale of the icon.
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
            typeof(StswSubCheck)
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
            typeof(StswSubCheck)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the control is in a busy/loading state.
    /// </summary>
    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }
    public static readonly DependencyProperty IsBusyProperty
        = DependencyProperty.Register(
            nameof(IsBusy),
            typeof(bool),
            typeof(StswSubCheck)
        );

    /// <summary>
    /// Gets or sets the orientation of the control.
    /// </summary>
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
    public static readonly DependencyProperty OrientationProperty
        = DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(StswSubCheck)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the fill brush of the icon.
    /// </summary>
    public Brush IconFill
    {
        get => (Brush)GetValue(IconFillProperty);
        set => SetValue(IconFillProperty, value);
    }
    public static readonly DependencyProperty IconFillProperty
        = DependencyProperty.Register(
            nameof(IconFill),
            typeof(Brush),
            typeof(StswSubCheck)
        );

    /// <summary>
    /// Gets or sets the stroke brush of the icon.
    /// </summary>
    public Brush IconStroke
    {
        get => (Brush)GetValue(IconStrokeProperty);
        set => SetValue(IconStrokeProperty, value);
    }
    public static readonly DependencyProperty IconStrokeProperty
        = DependencyProperty.Register(
            nameof(IconStroke),
            typeof(Brush),
            typeof(StswSubCheck)
        );

    /// <summary>
    /// Gets or sets the stroke thickness of the icon.
    /// </summary>
    public double IconStrokeThickness
    {
        get => (double)GetValue(IconStrokeThicknessProperty);
        set => SetValue(IconStrokeThicknessProperty, value);
    }
    public static readonly DependencyProperty IconStrokeThicknessProperty
        = DependencyProperty.Register(
            nameof(IconStrokeThickness),
            typeof(double),
            typeof(StswSubCheck)
        );
    #endregion
}
