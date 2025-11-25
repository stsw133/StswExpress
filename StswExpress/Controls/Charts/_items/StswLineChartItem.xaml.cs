using System.Windows;

namespace StswExpress;

public class StswLineChartItem : StswChartItem
{
    static StswLineChartItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswLineChartItem), new FrameworkPropertyMetadata(typeof(StswLineChartItem)));
    }

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
}
