using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Represents a control designed for displaying chart's legend.
/// </summary>
public class StswChartLegend : HeaderedItemsControl
{
    static StswChartLegend()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswChartLegend), new FrameworkPropertyMetadata(typeof(StswChartLegend)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswChartLegend), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
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
        if (newValue != null)
            foreach (StswChartElementModel item in newValue)
                item.OnValueChangedCommand = new StswCommand(() => MakeChart(ItemsSource));

        base.OnItemsSourceChanged(oldValue, newValue);
    }

    /// <summary>
    /// Generates chart data based on the provided items source.
    /// </summary>
    /// <param name="itemsSource">The items source to generate the chart data from.</param>
    public virtual void MakeChart(IEnumerable itemsSource)
    {
        if (itemsSource == null)
            return;

        if (itemsSource?.GetType()?.IsListType(out var innerType) != true || innerType?.IsAssignableTo(typeof(StswChartElementModel)) != true)
            throw new Exception($"{nameof(ItemsSource)} of chart control has to derive from {nameof(StswChartElementModel)} class!");

        var items = itemsSource.OfType<StswChartElementModel>();

        /// calculate values
        foreach (var item in items)
            item.Percentage = item.Value != 0 ? Convert.ToDouble(item.Value / items.Sum(x => x.Value) * 100) : 0;

        //itemsSource = items.OrderByDescending(x => x.Value);
    }
    #endregion

    #region Logic properties
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
            typeof(StswChartLegend),
            new FrameworkPropertyMetadata(default(int), FrameworkPropertyMetadataOptions.AffectsArrange)
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
            typeof(StswChartLegend),
            new FrameworkPropertyMetadata(default(int), FrameworkPropertyMetadataOptions.AffectsArrange)
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
            typeof(StswChartLegend),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender)
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
            typeof(StswChartLegend),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
