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
        var totalPercent = 0d;
        foreach (var item in items)
        {
            item.Percentage = Convert.ToDouble(item.Value / items.Sum(x => x.Value) * 100);

            item.Internal ??= new StswChartElementPieModel();
            if (item.Internal is StswChartElementPieModel elem)
            {
                elem.Angle = -90 + totalPercent * 3.6;
                elem.TextSize = FontSize + item.Percentage * 2;

                /// calculate center for percentage text
                var angleRadians = (elem.Angle + item.Percentage * 3.6 / 2) * Math.PI / 180;
                var radius = items.Count() == 1 ? 0 : (1000 - StrokeThickness) / 2 * (1.5 - item.Percentage/125);
                var centerX = Math.Cos(angleRadians) * radius;
                var centerY = Math.Sin(angleRadians) * radius;
                elem.Center = new Point(centerX, centerY);
            }

            totalPercent += item.Percentage;
        }
        OnMinPercentageRenderChanged(this, new DependencyPropertyChangedEventArgs());
        OnStrokeThicknessChanged(this, new DependencyPropertyChangedEventArgs());

        //itemsSource = items.OrderByDescending(x => x.Value);
    }
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the percentage threshold below which percentages should not be visible.
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
        if (obj is StswChartPie stsw)
        {
            var items = stsw.ItemsSource?.OfType<StswChartElementModel>();
            if (items != null)
                foreach (var item in items)
                    if (item.Internal is StswChartElementPieModel elem)
                        elem.IsPercentageVisible = item.Percentage >= stsw.MinPercentageRender;
        }
    }

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
            var items = stsw.ItemsSource?.OfType<StswChartElementModel>();
            if (items != null)
                foreach (var item in items)
                    if (item.Internal is StswChartElementPieModel elem)
                        elem.StrokeDashArray = new DoubleCollection(new[] { Math.PI * item.Percentage * (1000 - stsw.StrokeThickness) / stsw.StrokeThickness / 100, 100 });
        }
    }
    #endregion
}
