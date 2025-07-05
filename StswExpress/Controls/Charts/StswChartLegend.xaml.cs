using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Represents a legend control for charts, displaying labels and corresponding colors.
/// Supports dynamic updates, customizable grid layout with adjustable rows and columns, 
/// and optional percentage visibility inside legend items.
/// </summary>
[Stsw("0.4.0", Changes = StswPlannedChanges.None)]
public class StswChartLegend : HeaderedItemsControl
{
    static StswChartLegend()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswChartLegend), new FrameworkPropertyMetadata(typeof(StswChartLegend)));
    }

    #region Events & methods
    /// <inheritdoc/>
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        MakeChart(newValue);
        if (newValue != null)
            foreach (StswChartElementModel item in newValue)
                item.OnValueChangedCommand = new StswCommand(() => MakeChart(ItemsSource));

        base.OnItemsSourceChanged(oldValue, newValue);
    }

    /// <summary>
    /// Generates and updates the legend based on the provided data source.
    /// Ensures that percentage values are calculated correctly for each legend item.
    /// </summary>
    /// <param name="itemsSource">The collection of data items used to generate the legend.</param>
    /// <exception cref="Exception">Thrown if the provided <paramref name="itemsSource"/> does not derive from <see cref="StswChartElementModel"/>.</exception>
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

/* usage:

<se:StswChartLegend ItemsSource="{Binding ChartData}" Columns="3" Rows="2" ShowDetails="True"/>

*/
