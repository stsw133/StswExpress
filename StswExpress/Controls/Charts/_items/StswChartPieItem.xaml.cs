using System.Windows;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Represents a pie chart item control used within a pie chart to display individual segments.
/// </summary>
public class StswChartPieItem : StswChartItem
{
    static StswChartPieItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswChartPieItem), new FrameworkPropertyMetadata(typeof(StswChartPieItem)));
    }

    #region Logic properties
    /// <summary>
    /// Gets or sets the angle of the chart item.
    /// </summary>
    public double Angle
    {
        get => (double)GetValue(AngleProperty);
        set => SetValue(AngleProperty, value);
    }
    public static readonly DependencyProperty AngleProperty
        = DependencyProperty.Register(
            nameof(Angle),
            typeof(double),
            typeof(StswChartPieItem)
        );

    /// <summary>
    /// Gets or sets the angle of the chart item.
    /// </summary>
    public Point Center
    {
        get => (Point)GetValue(CenterProperty);
        set => SetValue(CenterProperty, value);
    }
    public static readonly DependencyProperty CenterProperty
        = DependencyProperty.Register(
            nameof(Center),
            typeof(Point),
            typeof(StswChartPieItem)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the visibility of the chart item's percentage.
    /// </summary>
    public bool IsPercentageVisible
    {
        get => (bool)GetValue(IsPercentageVisibleProperty);
        set => SetValue(IsPercentageVisibleProperty, value);
    }
    public static readonly DependencyProperty IsPercentageVisibleProperty
        = DependencyProperty.Register(
            nameof(IsPercentageVisible),
            typeof(bool),
            typeof(StswChartPieItem)
        );

    /// <summary>
    /// Gets or sets the stroke dash array used to represent the chart item.
    /// </summary>
    public DoubleCollection? StrokeDashArray
    {
        get => (DoubleCollection?)GetValue(StrokeDashArrayProperty);
        set => SetValue(StrokeDashArrayProperty, value);
    }
    public static readonly DependencyProperty StrokeDashArrayProperty
        = DependencyProperty.Register(
            nameof(StrokeDashArray),
            typeof(DoubleCollection),
            typeof(StswChartPieItem)
        );

    /// <summary>
    /// Gets or sets the size of the chart item's text.
    /// </summary>
    public double TextSize
    {
        get => (double)GetValue(TextSizeProperty);
        set => SetValue(TextSizeProperty, value);
    }
    public static readonly DependencyProperty TextSizeProperty
        = DependencyProperty.Register(
            nameof(TextSize),
            typeof(double),
            typeof(StswChartPieItem)
        );
    #endregion
}
