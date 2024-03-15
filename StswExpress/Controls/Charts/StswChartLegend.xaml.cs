using System;
using System.Collections;
using System.Linq;
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

    #region Events & methods
    /// <summary>
    /// Occurs when the ItemsSource property value changes.
    /// </summary>
    /// <param name="oldValue">The old value of the ItemsSource property.</param>
    /// <param name="newValue">The new value of the ItemsSource property.</param>
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        MakeChart(newValue);
        base.OnItemsSourceChanged(oldValue, newValue);
    }

    /// <summary>
    /// Generates chart data based on the provided items source.
    /// </summary>
    /// <param name="itemsSource">The items source to generate the chart data from.</param>
    public static void MakeChart(IEnumerable itemsSource)
    {
        if (itemsSource == null)
            return;

        var items = itemsSource.OfType<StswChartLegendModel>();
        if (itemsSource.OfType<StswChartLegendModel>() is ICollection { Count: > 0 } && items?.Count() == 0)
            throw new ArgumentException($"ItemsSource value must be of '{nameof(StswChartLegendModel)}' type.");

        foreach (var item in items!)
            item.Percentage = Convert.ToDouble(item.Value / items.Sum(x => x.Value) * 100);
    }
    #endregion

    #region Main properties
    /// <summary>
    /// Gets or sets the number of columns in the chart legend.
    /// </summary>
    public int Columns
    {
        get => (int)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }
    public static readonly DependencyProperty ColumnsProperty
        = DependencyProperty.Register(
            nameof(Columns),
            typeof(int),
            typeof(StswChartLegend)
        );

    /// <summary>
    /// Gets or sets the number of rows in the chart legend.
    /// </summary>
    public int Rows
    {
        get => (int)GetValue(RowsProperty);
        set => SetValue(RowsProperty, value);
    }
    public static readonly DependencyProperty RowsProperty
        = DependencyProperty.Register(
            nameof(Rows),
            typeof(int),
            typeof(StswChartLegend)
        );

    /// <summary>
    /// Gets or sets whether options show percentage inside their color rectangles.
    /// </summary>
    public bool ShowDetails
    {
        get => (bool)GetValue(ShowDetailsProperty);
        set => SetValue(ShowDetailsProperty, value);
    }
    public static readonly DependencyProperty ShowDetailsProperty
        = DependencyProperty.Register(
            nameof(ShowDetails),
            typeof(bool),
            typeof(StswChartLegend)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets a value indicating whether corner clipping is enabled for the control.
    /// When set to <see langword="true"/>, content within the control's border area is clipped to match
    /// the border's rounded corners, preventing elements from protruding beyond the border.
    /// </summary>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty
        = DependencyProperty.Register(
            nameof(CornerClipping),
            typeof(bool),
            typeof(StswChartLegend)
        );

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
            typeof(StswChartLegend)
        );

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
/// Data model for chart items.
/// </summary>
public class StswChartLegendModel : StswObservableObject
{
    /// <summary>
    /// Gets or sets the brush used to represent the chart item.
    /// </summary>
    public Brush? Brush { get; set; }

    /// <summary>
    /// Gets or sets the name of the chart item.
    /// </summary>
    public string? Name
    {
        get => name;
        set
        {
            SetProperty(ref name, value);
            Brush ??= new SolidColorBrush(StswFn.GenerateColor(Name ?? string.Empty, 130));
        }
    }
    private string? name;

    /// <summary>
    /// Gets or sets the percentage value of the chart item.
    /// </summary>
    public double Percentage { get; internal set; }

    /// <summary>
    /// Gets or sets the value of the chart item.
    /// </summary>
    public decimal Value { get; set; }
}
