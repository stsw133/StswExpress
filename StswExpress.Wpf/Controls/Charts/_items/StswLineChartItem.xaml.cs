using System.Windows;

namespace StswExpress.Wpf;

/// <summary>
/// A chart item representing a point in a line chart, including its position,
/// </summary>
public class StswLineChartItem : StswChartItem
{
    static StswLineChartItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswLineChartItem), new FrameworkPropertyMetadata(typeof(StswLineChartItem)));
    }

    #region Dependency properties
    /// <summary>
    /// Gets or sets the position of the dot representing the chart item.
    /// </summary>
    public Point DotPosition
    {
        get => (Point)GetValue(DotPositionProperty);
        set => SetValue(DotPositionProperty, value);
    }
    public static readonly DependencyProperty DotPositionProperty
        = DependencyProperty.Register(
            nameof(DotPosition),
            typeof(Point),
            typeof(StswLineChartItem)
        );

    /// <summary>
    /// Gets or sets the current position of the chart item within the line chart.
    /// </summary>
    public Point Position
    {
        get => (Point)GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }
    public static readonly DependencyProperty PositionProperty
        = DependencyProperty.Register(
            nameof(Position),
            typeof(Point),
            typeof(StswLineChartItem)
        );

    /// <summary>
    /// Gets or sets the previous position of the chart item within the line chart.
    /// </summary>
    public Point PreviousPosition
    {
        get => (Point)GetValue(PreviousPositionProperty);
        set => SetValue(PreviousPositionProperty, value);
    }
    public static readonly DependencyProperty PreviousPositionProperty
        = DependencyProperty.Register(
            nameof(PreviousPosition),
            typeof(Point),
            typeof(StswLineChartItem)
        );

    /// <summary>
    /// Gets or sets a value indicating whether to show the connector line
    /// </summary>
    public bool ShowConnector
    {
        get => (bool)GetValue(ShowConnectorProperty);
        set => SetValue(ShowConnectorProperty, value);
    }
    public static readonly DependencyProperty ShowConnectorProperty
        = DependencyProperty.Register(
            nameof(ShowConnector),
            typeof(bool),
            typeof(StswLineChartItem)
        );
    #endregion
}
