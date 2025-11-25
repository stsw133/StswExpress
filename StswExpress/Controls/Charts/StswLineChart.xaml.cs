using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace StswExpress;
/// <summary>
/// Represents a chart control that visualizes the provided values as a line drawn on top of a canvas.
/// Each data point is represented by a <see cref="StswLineChartItem"/> container that stores calculated coordinates.
/// </summary>
/// <example>
/// <code>
/// &lt;se:StswLineChart ItemsSource="{Binding TrendData}" Height="250" PointSize="12" LineThickness="3"/&gt;
/// </code>
/// </example>
public class StswLineChart : ItemsControl
{
    static StswLineChart()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswLineChart), new FrameworkPropertyMetadata(typeof(StswLineChart)));
    }

    protected override DependencyObject GetContainerForItemOverride() => new StswLineChartItem();
    protected override bool IsItemItsOwnContainerOverride(object item) => item is StswLineChartItem;

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
        if (element is StswLineChartItem container)
            container.ValueChanged -= OnItemValueChanged;
        base.ClearContainerForItemOverride(element, item);
    }

    /// <inheritdoc/>
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        if (element is StswLineChartItem container)
            container.ValueChanged += OnItemValueChanged;
    }

    private void OnItemValueChanged(object? sender, EventArgs e) => RequestChartUpdate();

    private IEnumerable<StswLineChartItem> GetContainers()
    {
        for (var i = 0; i < Items.Count; i++)
            if (ItemContainerGenerator.ContainerFromIndex(i) is StswLineChartItem container)
                yield return container;
    }

    /// <summary>
    /// Calculates the coordinates of each chart item and updates drawing hints.
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

            var width = ActualWidth;
            var height = ActualHeight;
            if (width <= 0 || height <= 0)
            {
                foreach (var item in items)
                {
                    item.Percentage = 0;
                    item.Position = new Point();
                    item.PreviousPosition = new Point();
                    item.DotPosition = new Point();
                    item.ShowConnector = false;
                }
                return;
            }

            var sum = items.Sum(x => x.Value);
            var max = items.Max(x => x.Value);
            var min = items.Min(x => x.Value);
            var stepX = (items.Length <= 1) ? 0.0 : width / (items.Length - 1);
            var centerX = width / 2.0;

            StswLineChartItem? previous = null;
            for (var index = 0; index < items.Length; index++)
            {
                var item = items[index];
                item.Percentage = sum != 0 ? (double)(item.Value / sum) * 100.0 : 0.0;

                var relativeX = (items.Length <= 1) ? centerX : stepX * index;
                var denominator = max - min;
                var relativeYRatio = denominator == 0
                    ? 0.5
                    : (double)((item.Value - min) / denominator);
                var relativeY = height - (relativeYRatio * height);

                if (double.IsNaN(relativeY) || double.IsInfinity(relativeY))
                    relativeY = height / 2.0;

                item.Position = new Point(relativeX, relativeY);

                var dotSize = PointSize;
                var dotLeft = relativeX - dotSize / 2.0;
                var dotTop = relativeY - dotSize / 2.0;
                item.DotPosition = new Point(dotLeft, dotTop);

                if (previous is not null)
                {
                    item.PreviousPosition = previous.Position;
                    item.ShowConnector = true;
                }
                else
                {
                    item.PreviousPosition = item.Position;
                    item.ShowConnector = false;
                }

                previous = item;
            }
        }
        finally
        {
            _isRecalc = false;
        }
    }
    private bool _isRecalc;

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

    #region Style properties
    public double LineThickness
    {
        get => (double)GetValue(LineThicknessProperty);
        set => SetValue(LineThicknessProperty, value);
    }
    public static readonly DependencyProperty LineThicknessProperty
        = DependencyProperty.Register(
            nameof(LineThickness),
            typeof(double),
            typeof(StswLineChart),
            new FrameworkPropertyMetadata(2d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnLineThicknessChanged)
        );
    private static void OnLineThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StswLineChart chart)
            chart.RequestChartUpdate();
    }

    public double PointSize
    {
        get => (double)GetValue(PointSizeProperty);
        set => SetValue(PointSizeProperty, value);
    }
    public static readonly DependencyProperty PointSizeProperty
        = DependencyProperty.Register(
            nameof(PointSize),
            typeof(double),
            typeof(StswLineChart),
            new FrameworkPropertyMetadata(12d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPointSizeChanged)
        );
    private static void OnPointSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StswLineChart chart)
            chart.RequestChartUpdate();
    }
    #endregion
}