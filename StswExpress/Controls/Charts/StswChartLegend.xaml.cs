using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Represents a control designed for displaying chart's legend.
/// </summary>
public class StswChartLegend : HeaderedItemsControl
{
    static StswChartLegend()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswChartLegend), new FrameworkPropertyMetadata(typeof(StswChartLegend)));
    }
}

/// <summary>
/// 
/// </summary>
public class StswChartModel : StswObservableObject
{
    /// <summary>
    /// 
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public double Percent { get; internal set; }

    /// <summary>
    /// 
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public Brush? Brush { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public DoubleCollection? StrokeDashArray
    {
        get => strokeDashArray;
        internal set => SetProperty(ref strokeDashArray, value);
    }
    private DoubleCollection? strokeDashArray;

    /// <summary>
    /// 
    /// </summary>
    public double Angle { get; internal set; }
}