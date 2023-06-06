using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;

public class StswSlider : Slider
{
    static StswSlider()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSlider), new FrameworkPropertyMetadata(typeof(StswSlider)));
    }

    #region Spatial properties
    /// > BorderThickness ...
    /// ThumbBorderThickness
    public static readonly DependencyProperty ThumbBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(ThumbBorderThickness),
            typeof(double),
            typeof(StswSlider)
        );
    public double ThumbBorderThickness
    {
        get => (double)GetValue(ThumbBorderThicknessProperty);
        set => SetValue(ThumbBorderThicknessProperty, value);
    }

    /// > CornerRadius ...
    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswSlider)
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// > Size ...
    /// TrackSize
    public static readonly DependencyProperty TrackSizeProperty
        = DependencyProperty.Register(
            nameof(TrackSize),
            typeof(double),
            typeof(StswSlider)
        );
    public double TrackSize
    {
        get => (double)GetValue(TrackSizeProperty);
        set => SetValue(TrackSizeProperty, value);
    }
    /// ThumbSize
    public static readonly DependencyProperty ThumbSizeProperty
        = DependencyProperty.Register(
            nameof(ThumbSize),
            typeof(double),
            typeof(StswSlider)
        );
    public double ThumbSize
    {
        get => (double)GetValue(ThumbSizeProperty);
        set => SetValue(ThumbSizeProperty, value);
    }
    #endregion

    #region Style properties
    /// > Background ...
    /// BackgroundDisabled
    public static readonly DependencyProperty BackgroundDisabledProperty
        = DependencyProperty.Register(
            nameof(BackgroundDisabled),
            typeof(Brush),
            typeof(StswSlider)
        );
    public Brush BackgroundDisabled
    {
        get => (Brush)GetValue(BackgroundDisabledProperty);
        set => SetValue(BackgroundDisabledProperty, value);
    }
    /// ThumbBackground
    public static readonly DependencyProperty ThumbBackgroundProperty
        = DependencyProperty.Register(
            nameof(ThumbBackground),
            typeof(Brush),
            typeof(StswSlider)
        );
    public Brush ThumbBackground
    {
        get => (Brush)GetValue(ThumbBackgroundProperty);
        set => SetValue(ThumbBackgroundProperty, value);
    }
    /// ThumbBackgroundMouseOver
    public static readonly DependencyProperty ThumbBackgroundMouseOverProperty
        = DependencyProperty.Register(
            nameof(ThumbBackgroundMouseOver),
            typeof(Brush),
            typeof(StswSlider)
        );
    public Brush ThumbBackgroundMouseOver
    {
        get => (Brush)GetValue(ThumbBackgroundMouseOverProperty);
        set => SetValue(ThumbBackgroundMouseOverProperty, value);
    }
    /// ThumbBackgroundDragging
    public static readonly DependencyProperty ThumbBackgroundDraggingProperty
        = DependencyProperty.Register(
            nameof(ThumbBackgroundDragging),
            typeof(Brush),
            typeof(StswSlider)
        );
    public Brush ThumbBackgroundDragging
    {
        get => (Brush)GetValue(ThumbBackgroundDraggingProperty);
        set => SetValue(ThumbBackgroundDraggingProperty, value);
    }
    /// ThumbBackgroundDisabled
    public static readonly DependencyProperty ThumbBackgroundDisabledProperty
        = DependencyProperty.Register(
            nameof(ThumbBackgroundDisabled),
            typeof(Brush),
            typeof(StswSlider)
        );
    public Brush ThumbBackgroundDisabled
    {
        get => (Brush)GetValue(ThumbBackgroundDisabledProperty);
        set => SetValue(ThumbBackgroundDisabledProperty, value);
    }
    /// RangeBackground
    public static readonly DependencyProperty RangeBackgroundProperty
        = DependencyProperty.Register(
            nameof(RangeBackground),
            typeof(Brush),
            typeof(StswSlider)
        );
    public Brush RangeBackground
    {
        get => (Brush)GetValue(RangeBackgroundProperty);
        set => SetValue(RangeBackgroundProperty, value);
    }

    /// > BorderBrush ...
    /// BorderBrushDisabled
    public static readonly DependencyProperty BorderBrushDisabledProperty
        = DependencyProperty.Register(
            nameof(BorderBrushDisabled),
            typeof(Brush),
            typeof(StswSlider)
        );
    public Brush BorderBrushDisabled
    {
        get => (Brush)GetValue(BorderBrushDisabledProperty);
        set => SetValue(BorderBrushDisabledProperty, value);
    }
    /// ThumbBorderBrush
    public static readonly DependencyProperty ThumbBorderBrushProperty
        = DependencyProperty.Register(
            nameof(ThumbBorderBrush),
            typeof(Brush),
            typeof(StswSlider)
        );
    public Brush ThumbBorderBrush
    {
        get => (Brush)GetValue(ThumbBorderBrushProperty);
        set => SetValue(ThumbBorderBrushProperty, value);
    }
    /// ThumbBorderBrushMouseOver
    public static readonly DependencyProperty ThumbBorderBrushMouseOverProperty
        = DependencyProperty.Register(
            nameof(ThumbBorderBrushMouseOver),
            typeof(Brush),
            typeof(StswSlider)
        );
    public Brush ThumbBorderBrushMouseOver
    {
        get => (Brush)GetValue(ThumbBorderBrushMouseOverProperty);
        set => SetValue(ThumbBorderBrushMouseOverProperty, value);
    }
    /// ThumbBorderBrushDragging
    public static readonly DependencyProperty ThumbBorderBrushDraggingProperty
        = DependencyProperty.Register(
            nameof(ThumbBorderBrushDragging),
            typeof(Brush),
            typeof(StswSlider)
        );
    public Brush ThumbBorderBrushDragging
    {
        get => (Brush)GetValue(ThumbBorderBrushDraggingProperty);
        set => SetValue(ThumbBorderBrushDraggingProperty, value);
    }
    /// ThumbBorderBrushDisabled
    public static readonly DependencyProperty ThumbBorderBrushDisabledProperty
        = DependencyProperty.Register(
            nameof(ThumbBorderBrushDisabled),
            typeof(Brush),
            typeof(StswSlider)
        );
    public Brush ThumbBorderBrushDisabled
    {
        get => (Brush)GetValue(ThumbBorderBrushDisabledProperty);
        set => SetValue(ThumbBorderBrushDisabledProperty, value);
    }

    /// > Icon ...
    /// ThumbIcon
    public static readonly DependencyProperty ThumbIconProperty
        = DependencyProperty.Register(
            nameof(ThumbIcon),
            typeof(Geometry),
            typeof(StswSlider)
        );
    public Geometry? ThumbIcon
    {
        get => (Geometry?)GetValue(ThumbIconProperty);
        set => SetValue(ThumbIconProperty, value);
    }
    #endregion
}
