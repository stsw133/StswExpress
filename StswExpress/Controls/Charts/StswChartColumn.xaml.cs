using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace StswExpress;

/// <summary>
/// Represents a chart control that visualizes data as vertical columns. 
/// Automatically adjusts column heights based on data values and resizes dynamically with the control.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswChartColumn ItemsSource="{Binding RevenueData}" Width="400" Height="300"/&gt;
/// </code>
/// </example>
public class StswChartColumn : ItemsControl
{
    static StswChartColumn()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswChartColumn), new FrameworkPropertyMetadata(typeof(StswChartColumn)));
    }

    protected override DependencyObject GetContainerForItemOverride() => new StswChartColumnItem();
    protected override bool IsItemItsOwnContainerOverride(object item) => item is StswChartColumnItem;

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        RequestChartUpdate();
    }

    /// <inheritdoc/>
    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        RequestChartUpdate();
    }

    /// <inheritdoc/>
    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        RequestChartUpdate();
    }

    /// <inheritdoc/>
    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        RequestChartUpdate();
    }

    /// <inheritdoc/>
    protected override void ClearContainerForItemOverride(DependencyObject element, object item)
    {
        if (element is StswChartColumnItem c)
            c.ValueChanged -= OnItemValueChanged;
        base.ClearContainerForItemOverride(element, item);
    }

    /// <summary>
    /// Gets the containers for all items in the chart.
    /// </summary>
    /// <returns>An enumerable of <see cref="StswChartColumnItem"/> containers.</returns>
    private IEnumerable<StswChartColumnItem> GetContainers()
    {
        for (var i = 0; i < Items.Count; i++)
            if (ItemContainerGenerator.ContainerFromIndex(i) is StswChartColumnItem c)
                yield return c;
    }

    /// <inheritdoc/>
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        if (element is StswChartColumnItem c)
        {
            c.ValueChanged += OnItemValueChanged;
            if (!double.IsNaN(c.ColumnWidth))
                c.ColumnWidth = double.NaN;
        }
    }

    /// <summary>
    /// Handles the ValueChanged event of an item and triggers the recalculation of the chart.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void OnItemValueChanged(object? sender, EventArgs e) => RequestChartUpdate();

    /// <summary>
    /// Recalculates the heights and percentages of the chart columns based on their values and the available height.
    /// </summary>
    public void MakeChart()
    {
        if (_isRecalc)
            return;

        _isRecalc = true;
        try
        {
            var items = GetContainers().ToArray();
            if (items.Length == 0)
                return;

            var availH = ActualHeight;
            var availW = ActualWidth;
            if (availH <= 0 || availW <= 0)
            {
                foreach (var it in items)
                {
                    it.ColumnHeight = 0;
                    it.ColumnWidth = 0;
                }
                return;
            }

            var max = items.Max(x => x.Value);
            var sum = items.Sum(x => x.Value);
            var w = availW / items.Length;

            if (max <= 0)
            {
                foreach (var item in items)
                {
                    item.Percentage = 0;
                    item.ColumnHeight = 0;
                    item.ColumnWidth = w;
                }
                return;
            }

            foreach (var item in items)
            {
                item.Percentage = sum != 0 ? (double)(item.Value / sum) * 100.0 : 0.0;
                var ratio = (double)(item.Value / max);
                item.ColumnHeight = ratio * availH;
                item.ColumnWidth = w;
            }
        }
        finally
        {
            _isRecalc = false;
        }
    }
    private bool _isRecalc;

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
    #endregion
}
