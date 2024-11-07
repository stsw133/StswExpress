using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Represents a control designed for displaying column charts.
/// </summary>
public class StswChartColumns : ItemsControl
{
    static StswChartColumns()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswChartColumns), new FrameworkPropertyMetadata(typeof(StswChartColumns)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswChartColumns), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
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
    /// Occurs when the render size changes.
    /// </summary>
    /// <param name="sizeInfo">The size info.</param>
    protected override async void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        await Task.Run(() => ResizeChart());
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
    /// Resizes chart based on the percentage values and number of columns.
    /// </summary>
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
