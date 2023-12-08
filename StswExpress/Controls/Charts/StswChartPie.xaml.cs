using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Represents a control designed for displaying pie charts.
/// </summary>
public class StswChartPie : HeaderedItemsControl
{
    static StswChartPie()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswChartPie), new FrameworkPropertyMetadata(typeof(StswChartPie)));
    }

    #region Events & methods
    /// <summary>
    /// 
    /// </summary>
    /// <param name=""></param>
    /// <exception cref="ArgumentException"></exception>
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        MakeChart(newValue);
        base.OnItemsSourceChanged(oldValue, newValue);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newValue"></param>
    public void MakeChart(IEnumerable itemsSource)
    {
        var items = itemsSource?.OfType<StswChartModel>();

        if (items == null || (itemsSource is ICollection { Count: > 0 } && items?.Count() == 0))
            throw new ArgumentException($"ItemsSource value must be of '{nameof(StswChartModel)}' type.");

        var totalPercent = 0d;
        foreach (var item in items!)
        {
            item.Angle = -90 + totalPercent * 3.6;
            item.Percent = Convert.ToDouble(item.Value / items.Sum(x => x.Value) * 100);
            item.StrokeDashArray = new DoubleCollection(new[] { Math.PI * item.Percent * (1000 - StrokeThickness) / StrokeThickness / 100, 100 });
            item.Description ??= $"{item.Value} ({item.Percent:N2}%)";
            totalPercent += item.Percent;
        }
    }
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the stroke thickness of the ellipse (between 1 and 500).
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
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnStrokeThicknessChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnStrokeThicknessChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswChartPie stsw)
        {
            var items = stsw.ItemsSource?.OfType<StswChartModel>();
            if (items != null)
                foreach (var item in items!)
                    item.StrokeDashArray = new DoubleCollection(new[] { Math.PI * item.Percent * (1000 - stsw.StrokeThickness) / stsw.StrokeThickness / 100, 100 });
        }
    }
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