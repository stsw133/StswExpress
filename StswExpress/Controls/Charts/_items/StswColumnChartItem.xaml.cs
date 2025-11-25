using System.Windows;

namespace StswExpress;

/// <summary>
/// Represents an individual column item within a <see cref="StswColumnChart"/> control.
/// </summary>
public class StswColumnChartItem : StswChartItem
{
    static StswColumnChartItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswColumnChartItem), new FrameworkPropertyMetadata(typeof(StswColumnChartItem)));
    }

    #region Logic properties
    /// <summary>
    /// Gets or sets the height of the column in the chart.
    /// </summary>
    public double ColumnHeight
    {
        get => (double)GetValue(ColumnHeightProperty);
        set => SetValue(ColumnHeightProperty, value);
    }
    public static readonly DependencyProperty ColumnHeightProperty
        = DependencyProperty.Register(
            nameof(ColumnHeight),
            typeof(double),
            typeof(StswColumnChartItem),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the height of the column representing the data value.
    /// </summary>
    public double ColumnWidth
    {
        get => (double)GetValue(ColumnWidthProperty);
        set => SetValue(ColumnWidthProperty, value);
    }
    public static readonly DependencyProperty ColumnWidthProperty
        = DependencyProperty.Register(
            nameof(ColumnWidth),
            typeof(double),
            typeof(StswColumnChartItem),
            new PropertyMetadata(double.NaN)
        );
    #endregion
}
