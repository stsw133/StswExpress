using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace StswExpress;

/// <summary>
/// Represents a control for displaying pie charts.
/// Supports dynamic updates, percentage-based calculations, and customizable appearance,
/// including stroke thickness and visibility thresholds for percentage labels.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswChartPie ItemsSource="{Binding SalesData}" StrokeThickness="10" MinPercentageRender="5"/&gt;
/// </code>
/// </example>
public class StswChartPie : ItemsControl
{
    static StswChartPie()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswChartPie), new FrameworkPropertyMetadata(typeof(StswChartPie)));
    }

    protected override DependencyObject GetContainerForItemOverride() => new StswChartPieItem();
    protected override bool IsItemItsOwnContainerOverride(object item) => item is StswChartPieItem;

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        RequestChartUpdate();
    }

    /// <inheritdoc/>
    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        RequestChartUpdate();
    }

    /// <summary>
    /// Handles the ValueChanged event of an item and triggers chart regeneration.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void OnItemValueChanged(object? sender, EventArgs e) => RequestChartUpdate();

    /// <inheritdoc/>
    protected override void ClearContainerForItemOverride(DependencyObject element, object item)
    {
        if (element is StswChartPieItem c)
            c.ValueChanged -= OnItemValueChanged;
        base.ClearContainerForItemOverride(element, item);
    }

    /// <summary>
    /// Retrieves all the pie chart item containers.
    /// </summary>
    /// <returns>An enumerable of <see cref="StswChartPieItem"/> containers.</returns>
    private IEnumerable<StswChartPieItem> GetContainers()
    {
        for (var i = 0; i < Items.Count; i++)
            if (ItemContainerGenerator.ContainerFromIndex(i) is StswChartPieItem c)
                yield return c;
    }

    /// <inheritdoc/>
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        if (element is StswChartPieItem c)
            c.ValueChanged += OnItemValueChanged;
    }

    /// <summary>
    /// Generates and updates the pie chart based on the provided data source.
    /// Ensures that segment angles, percentage labels, and positioning are properly calculated.
    /// </summary>
    /// <param name="itemsSource">The collection of data items used to generate the chart.</param>
    /// <exception cref="Exception">Thrown if the provided <paramref name="itemsSource"/> does not derive from <see cref="StswChartElementModel"/>.</exception>
    public virtual void MakeChart()
    {
        var items = GetContainers().ToArray();
        if (items.Length == 0)
            return;

        /// calculate values
        var totalValue = items.Sum(x => x.Value);
        var total = totalValue == 0 ? 1m : totalValue;

        var totalPercent = 0.0;
        var count = items.Length;

        foreach (var item in items)
        {
            item.Percentage = (double)(item.Value / total) * 100.0;
            item.Angle = -90 + totalPercent * 3.6;
            item.TextSize = FontSize + item.Percentage * 2.0;

            /// calculate center for percentage text
            var angleMid = (item.Angle + (item.Percentage * 3.6) / 2.0) * Math.PI / 180.0;
            var radius = (count == 1) ? 0.0 : (1000 - StrokeThickness) / 2.0 * (1.5 - item.Percentage / 125.0);
            var cx = Math.Cos(angleMid) * radius;
            var cy = Math.Sin(angleMid) * radius;
            item.Center = new Point(cx, cy);

            totalPercent += item.Percentage;
        }
        ApplyMinPercentageVisibility(items);
        ApplyStrokeDashes(items);
    }

    /// <summary>
    /// Requests a chart update and throttles recalculations to a single dispatcher pass.
    /// </summary>
    private void RequestChartUpdate()
    {
        if (_chartUpdateOperation is { Status: DispatcherOperationStatus.Pending })
            return;

        var priority = IsLoaded ? DispatcherPriority.Render : DispatcherPriority.Loaded;
        _chartUpdateOperation = Dispatcher.BeginInvoke(priority, new Action(() =>
        {
            _chartUpdateOperation = null;
            MakeChart();
        }));
    }
    private DispatcherOperation? _chartUpdateOperation;

    /// <summary>
    /// Applies visibility settings for percentage labels based on the minimum percentage threshold.
    /// </summary>
    /// <param name="items">The collection of pie chart items.</param>
    private void ApplyMinPercentageVisibility(IEnumerable<StswChartPieItem> items)
    {
        foreach (var item in items)
            item.IsPercentageVisible = item.Percentage >= MinPercentageRender;
    }

    /// <summary>
    /// Applies stroke dash patterns to pie chart segments based on their percentage values and the configured stroke thickness.
    /// </summary>
    /// <param name="items">The collection of pie chart items.</param>
    private void ApplyStrokeDashes(IEnumerable<StswChartPieItem> items)
    {
        var t = Math.Max(1.0, StrokeThickness);

        foreach (var item in items)
        {
            var diameter = 1000 - t;
            var r = Math.Max(0.0, diameter / 2.0);
            var circumference = 2.0 * Math.PI * r;

            var dash = (item.Percentage <= 0) ? 0.0001
                      : (item.Percentage >= 100) ? (circumference / t)
                      : (item.Percentage / 100.0) * (circumference / t);

            item.StrokeDashArray = new DoubleCollection([dash, 100.0]);
        }
    }
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the minimum percentage threshold below which percentage labels are hidden.
    /// Segments with a percentage value lower than this threshold will not display their labels.
    /// </summary>
    public double MinPercentageRender
    {
        get => (double)GetValue(MinPercentageRenderProperty);
        set => SetValue(MinPercentageRenderProperty, value);
    }
    public static readonly DependencyProperty MinPercentageRenderProperty
        = DependencyProperty.Register(
            nameof(MinPercentageRender),
            typeof(double),
            typeof(StswChartPie),
            new FrameworkPropertyMetadata(default(double),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnMinPercentageRenderChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnMinPercentageRenderChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswChartPie stsw)
            return;

        stsw.RequestChartUpdate();
    }

    /// <summary>
    /// Gets or sets the stroke thickness of the pie chart's segments.
    /// Valid values range between 1 and 500, affecting the width of the pie slices.
    /// </summary>
    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }
    public static readonly DependencyProperty StrokeThicknessProperty
        = DependencyProperty.Register(
            nameof(StrokeThickness),
            typeof(double),
            typeof(StswChartPie),
            new FrameworkPropertyMetadata(default(double),
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnStrokeThicknessChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnStrokeThicknessChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswChartPie stsw)
            return;

        stsw.RequestChartUpdate();
    }
    #endregion
}
