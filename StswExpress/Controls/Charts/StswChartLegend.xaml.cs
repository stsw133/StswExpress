using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Represents a legend control for charts, displaying labels and corresponding colors.
/// Supports dynamic updates, customizable grid layout with adjustable rows and columns, 
/// and optional percentage visibility inside legend items.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswChartLegend ItemsSource="{Binding ChartData}" Columns="3" Rows="2" ShowDetails="True"/&gt;
/// </code>
/// </example>
public class StswChartLegend : HeaderedItemsControl
{
    static StswChartLegend()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswChartLegend), new FrameworkPropertyMetadata(typeof(StswChartLegend)));
    }

    protected override DependencyObject GetContainerForItemOverride() => new StswChartLegendItem();
    protected override bool IsItemItsOwnContainerOverride(object item) => item is StswChartLegendItem;

    #region Events & methods
    /// <inheritdoc/>
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        if (element is StswChartLegendItem c)
            c.ValueChanged += OnItemValueChanged;
    }

    /// <inheritdoc/>
    protected override void ClearContainerForItemOverride(DependencyObject element, object item)
    {
        if (element is StswChartLegendItem c)
            c.ValueChanged -= OnItemValueChanged;
        base.ClearContainerForItemOverride(element, item);
    }

    /// <inheritdoc/>
    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        MakeChart();
    }

    /// <summary>
    /// Handles the ValueChanged event of an item and triggers chart regeneration.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void OnItemValueChanged(object? sender, EventArgs e) => MakeChart();

    /// <summary>
    /// Generates and updates the chart legend based on the current items.
    /// </summary>
    public virtual void MakeChart()
    {
        var items = GetContainers();
        if (items.Count == 0)
            return;

        var total = items.Sum(i => i.Value);
        var hasTotal = total != 0;

        foreach (var item in items)
            item.Percentage = hasTotal ? Convert.ToDouble(item.Value / total * 100m) : 0d;
    }

    /// <summary>
    /// Retrieves the list of legend item containers.
    /// </summary>
    /// <returns>A list of <see cref="StswChartLegendItem"/> containers.</returns>
    private List<StswChartLegendItem> GetContainers()
    {
        var list = new List<StswChartLegendItem>(Items.Count);
        foreach (var it in Items)
        {
            var c = ItemContainerGenerator.ContainerFromItem(it) as StswChartLegendItem
                    ?? it as StswChartLegendItem;
            if (c != null)
                list.Add(c);
        }
        return list;
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the number of columns in the chart legend.
    /// Determines how legend items are arranged in a grid format.
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
    /// Controls how legend items are distributed vertically within the available space.
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
    /// Gets or sets a value indicating whether percentage values should be displayed 
    /// inside the color rectangles of the legend items.
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
    /// <inheritdoc/>
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

    /// <inheritdoc/>
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
