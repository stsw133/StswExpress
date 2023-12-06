using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
internal class StswChartPie : ItemsControl
{
    static StswChartPie()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswChartPie), new FrameworkPropertyMetadata(typeof(StswChartPie)));
    }
}
