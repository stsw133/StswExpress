using System.Windows;

namespace StswExpress;

/// <summary>
/// Represents a legend item in a chart, providing visual representation and interaction for chart data.
/// </summary>
public class StswChartLegendItem : StswChartItem
{
    static StswChartLegendItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswChartLegendItem), new FrameworkPropertyMetadata(typeof(StswChartLegendItem)));
    }
}
