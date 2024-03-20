using System;
using System.Collections;
using System.Linq;
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
    public void MakeChart(IEnumerable itemsSource)
    {
        if (itemsSource == null)
            return;

        if (itemsSource?.GetType()?.IsListType(out var innerType) != true || innerType?.IsAssignableTo(typeof(StswChartElementModel)) != true)
            throw new Exception($"{nameof(ItemsSource)} of chart control has to derive from {nameof(StswChartElementModel)} class!");
        
        var items = itemsSource.OfType<StswChartElementModel>();

        /// calculate values
        foreach (var item in items!)
        {
            item.Percentage = Convert.ToDouble(item.Value / items.Sum(x => x.Value) * 100);

            item.Internal ??= new StswChartElementColumnModel();
            if (item.Internal is StswChartElementColumnModel elem)
            {
                if (elem.Elements?.Count > 0)
                    foreach (var element in elem.Elements)
                        item.Percentage = Convert.ToDouble(element.Value / elem.Elements.Sum(x => x.Value) * 100);
                else
                    elem.Elements ??= new()
                    {
                        new()
                        {
                            Brush = item.Brush,
                            Name = item.Name,
                            Percentage = item.Percentage,
                            Value = item.Value
                        }
                    };
            }
        }
    }
    #endregion
}
