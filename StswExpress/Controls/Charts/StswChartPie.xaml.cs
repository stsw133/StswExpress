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
public class StswChartPie : ItemsControl
{
    static StswChartPie()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswChartPie), new FrameworkPropertyMetadata(typeof(StswChartPie)));
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
    public void MakeChart(IEnumerable itemsSource)
    {
        if (itemsSource == null)
            return;

        var items = itemsSource.OfType<StswChartModel>();
        if (itemsSource.OfType<StswChartModel>() is ICollection { Count: > 0 } && items?.Count() == 0)
            throw new ArgumentException($"ItemsSource value must be of '{nameof(StswChartModel)}' type.");

        var totalPercent = 0d;
        foreach (var item in items!)
        {
            item.Angle = -90 + totalPercent * 3.6;
            item.Percentage = Convert.ToDouble(item.Value / items.Sum(x => x.Value) * 100);
            item.StrokeDashArray = new DoubleCollection(new[] { Math.PI * item.Percentage * (1000 - StrokeThickness) / StrokeThickness / 100, 100 });
            item.Description ??= $"{item.Value}  ({item.Percentage:N2}%)";
            totalPercent += item.Percentage;

            double angleRadians = (item.Angle + item.Percentage * 3.6 / 2) * Math.PI / 180;
            double radius = (1000 - StrokeThickness) / 2 * 1.25; //* 2 + 25 + item.Percentage * 1.5;
            double centerX = Math.Cos(angleRadians) * radius;
            double centerY = Math.Sin(angleRadians) * radius;
            item.Center = new Point(centerX, centerY);
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
                    item.StrokeDashArray = new DoubleCollection(new[] { Math.PI * item.Percentage * (1000 - stsw.StrokeThickness) / stsw.StrokeThickness / 100, 100 });
        }
    }
    #endregion
}
