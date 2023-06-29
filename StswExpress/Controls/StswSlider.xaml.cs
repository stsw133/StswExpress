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
