using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Represents a control designed for displaying chart's legend.
/// </summary>
public class StswChartLegend : HeaderedItemsControl, IStswScrollableControl
{
    static StswChartLegend()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswChartLegend), new FrameworkPropertyMetadata(typeof(StswChartLegend)));
    }

    #region Style properties
    /// <summary>
    /// Gets or sets the data model for properties of the scroll viewer associated with the control.
    /// The <see cref="StswScrollViewerModel"/> class provides customization options for the appearance and behavior of the scroll viewer.
    /// </summary>
    public StswScrollViewerModel ScrollViewer
    {
        get => (StswScrollViewerModel)GetValue(ScrollViewerProperty);
        set => SetValue(ScrollViewerProperty, value);
    }
    public static readonly DependencyProperty ScrollViewerProperty
        = DependencyProperty.Register(
            nameof(ScrollViewer),
            typeof(StswScrollViewerModel),
            typeof(StswChartLegend)
        );
    #endregion
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