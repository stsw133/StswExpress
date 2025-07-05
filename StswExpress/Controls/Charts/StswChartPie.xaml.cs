using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Represents a control for displaying pie charts.
/// Supports dynamic updates, percentage-based calculations, and customizable appearance,
/// including stroke thickness and visibility thresholds for percentage labels.
/// </summary>
[Stsw("0.4.0", Changes = StswPlannedChanges.None)]
public class StswChartPie : ItemsControl
{
    static StswChartPie()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswChartPie), new FrameworkPropertyMetadata(typeof(StswChartPie)));
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
    /// Generates and updates the pie chart based on the provided data source.
    /// Ensures that segment angles, percentage labels, and positioning are properly calculated.
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

/* usage:

<se:StswChartPie ItemsSource="{Binding SalesData}" StrokeThickness="10" MinPercentageRender="5"/>

*/
