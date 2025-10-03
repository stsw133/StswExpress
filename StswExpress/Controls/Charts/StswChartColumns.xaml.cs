using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Represents a chart control that visualizes data as vertical columns. 
/// Automatically adjusts column heights based on data values and resizes dynamically with the control.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswChartColumns ItemsSource="{Binding RevenueData}" Width="400" Height="300"/&gt;
/// </code>
/// </example>
[StswPlannedChanges(StswPlannedChanges.Refactor)]
public class StswChartColumns : ItemsControl
{
    static StswChartColumns()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswChartColumns), new FrameworkPropertyMetadata(typeof(StswChartColumns)));
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

    /// <inheritdoc/>
    protected override async void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        await Task.Run(() => ResizeChart());
    }

    /// <summary>
    /// Generates and updates the chart based on the provided data source.
    /// Ensures column heights are proportional to data values.
    /// </summary>
    /// <param name="itemsSource">The collection of data items used to generate the chart.</param>
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
        {
            item.Percentage = item.Value != 0 ? Convert.ToDouble(item.Value / items.Sum(x => x.Value) * 100) : 0;

            item.Internal ??= new StswChartElementColumnModel();
            if (item.Internal is StswChartElementColumnModel elem)
            {
                elem.Height = item.Percentage != 0 ? item.Percentage * ActualHeight / items.Max(x => x.Percentage) : 0;
                elem.Width = ActualWidth / items.Count();
            }
        }

        //itemsSource = items.OrderByDescending(x => x.Value);
    }

    /// <summary>
    /// Adjusts the size of the chart columns based on updated percentage values and available space.
    /// Ensures the chart remains properly scaled after control resizing.
    /// </summary>
    /// <exception cref="Exception">Thrown if the <see cref="ItemsControl.ItemsSource"/> does not derive from <see cref="StswChartElementModel"/>.</exception>
    private void ResizeChart()
    {
        if (ItemsSource == null)
            return;

        if (ItemsSource?.GetType()?.IsListType(out var innerType) != true || innerType?.IsAssignableTo(typeof(StswChartElementModel)) != true)
            throw new Exception($"{nameof(ItemsSource)} of chart control has to derive from {nameof(StswChartElementModel)} class!");

        var items = ItemsSource.OfType<StswChartElementModel>();

        /// calculate values
        foreach (var item in items)
        {
            item.Internal ??= new StswChartElementColumnModel();
            if (item.Internal is StswChartElementColumnModel elem)
            {
                elem.Height = item.Percentage != 0 ? item.Percentage * ActualHeight / items.Max(x => x.Percentage) : 0;
                elem.Width = ActualWidth / items.Count();
            }
        }
    }
    #endregion
}
