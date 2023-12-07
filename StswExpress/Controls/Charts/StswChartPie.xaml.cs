using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Represents a control designed for displaying pie charts.
/// </summary>
internal class StswChartPie : HeaderedItemsControl
{
    static StswChartPie()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswChartPie), new FrameworkPropertyMetadata(typeof(StswChartPie)));
    }
}
