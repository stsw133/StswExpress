using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// A slider control that allows users to select a numeric value within a range.
/// Supports custom thumb size, track size, and optional icon inside the thumb.
/// </summary>
[StswInfo(null)]
public class StswSlider : Slider
{
    static StswSlider()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSlider), new FrameworkPropertyMetadata(typeof(StswSlider)));
    }

    #region Style properties
    /// <summary>
    /// Gets or sets the degree to which the corners of the control's border are rounded by defining
    /// a radius value for each corner independently. This property allows users to control the roundness
    /// of corners, and large radius values are smoothly scaled to blend from corner to corner.
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
            typeof(StswSlider),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the thickness of the border around the slider's thumb.
    /// Controls the outline width of the draggable element.
    /// </summary>
    public double ThumbBorderThickness
    {
        get => (double)GetValue(ThumbBorderThicknessProperty);
        set => SetValue(ThumbBorderThicknessProperty, value);
    }
    public static readonly DependencyProperty ThumbBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(ThumbBorderThickness),
            typeof(double),
            typeof(StswSlider),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the icon displayed inside the slider thumb.
    /// Allows adding a visual representation, such as a symbol or indicator, within the draggable element.
    /// </summary>
    public Geometry? ThumbIcon
    {
        get => (Geometry?)GetValue(ThumbIconProperty);
        set => SetValue(ThumbIconProperty, value);
    }
    public static readonly DependencyProperty ThumbIconProperty
        = DependencyProperty.Register(
            nameof(ThumbIcon),
            typeof(Geometry),
            typeof(StswSlider),
            new FrameworkPropertyMetadata(default(Geometry?), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the size of the slider thumb.
    /// Defines the dimensions of the draggable element, affecting usability and visual prominence.
    /// </summary>
    public double ThumbSize
    {
        get => (double)GetValue(ThumbSizeProperty);
        set => SetValue(ThumbSizeProperty, value);
    }
    public static readonly DependencyProperty ThumbSizeProperty
        = DependencyProperty.Register(
            nameof(ThumbSize),
            typeof(double),
            typeof(StswSlider),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the size (height or width) of the slider track.
    /// Adjusts the thickness of the track where the thumb moves.
    /// </summary>
    public double TrackSize
    {
        get => (double)GetValue(TrackSizeProperty);
        set => SetValue(TrackSizeProperty, value);
    }
    public static readonly DependencyProperty TrackSizeProperty
        = DependencyProperty.Register(
            nameof(TrackSize),
            typeof(double),
            typeof(StswSlider),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}

/* usage:

<se:StswSlider Minimum="1" Maximum="10" Value="{Binding UserPreference}" ThumbIcon="{StaticResource CustomIcon}" ThumbSize="20" TrackSize="4"/>

*/
