using StswExpress.Commons;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace StswExpress;

/// <summary>
/// Represents a horizontal timeline control that visualizes chronological events as interactive points.
/// </summary>
[StswPlannedChanges(StswPlannedChanges.Finish)]
public class StswTimeline : ItemsControl
{
    private const string ItemsHostPartName = "PART_ItemsHost";

    private bool _isUpdateScheduled;
    private Canvas? _itemsHost;

    static StswTimeline()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswTimeline), new FrameworkPropertyMetadata(typeof(StswTimeline)));
    }

    public StswTimeline()
    {
        ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
        SizeChanged += (_, _) => InvalidateItemPositions();
        Loaded += (_, _) => InvalidateItemPositions();
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _itemsHost = GetTemplateChild(ItemsHostPartName) as Canvas;
        InvalidateItemPositions();
    }

    protected override DependencyObject GetContainerForItemOverride() => new StswTimelineItem();
    protected override bool IsItemItsOwnContainerOverride(object item) => item is StswTimelineItem;

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is not StswTimelineItem container)
            return;

        container.ParentTimeline = this;
        container.Content = item;

        if (item is StswTimelineItem)
            return;

        var dateSet = ApplyMemberValue(item, DateMemberPath, value => container.SetCurrentValue(StswTimelineItem.DateProperty, value.ConvertTo<DateTime?>()));
        if (!dateSet)
        {
            switch (item)
            {
                case DateTime dt:
                    container.SetCurrentValue(StswTimelineItem.DateProperty, dt);
                    break;
                case DateTimeOffset dto:
                    container.SetCurrentValue(StswTimelineItem.DateProperty, dto.DateTime);
                    break;
            }
        }

        var headerPath = !string.IsNullOrWhiteSpace(HeaderMemberPath)
            ? HeaderMemberPath
            : (!string.IsNullOrWhiteSpace(DisplayMemberPath) ? DisplayMemberPath : null);
        var headerSet = ApplyMemberValue(item, headerPath, value => container.SetCurrentValue(StswTimelineItem.HeaderProperty, value?.ToString()));
        if (!headerSet && container.Header == null && item != null && headerPath == null)
            container.SetCurrentValue(StswTimelineItem.HeaderProperty, item.ToString());

        ApplyMemberValue(item, DescriptionMemberPath, value => container.SetCurrentValue(StswTimelineItem.DescriptionProperty, value?.ToString()));

        if (!ApplyMemberValue(item, ToolTipMemberPath, value => container.SetCurrentValue(StswTimelineItem.ToolTipContentProperty, value))
         && container.ToolTipContent == null && container.Description != null)
        {
            container.SetCurrentValue(StswTimelineItem.ToolTipContentProperty, container.Description);
        }
    }

    protected override void ClearContainerForItemOverride(DependencyObject element, object item)
    {
        if (element is StswTimelineItem container)
        {
            container.ParentTimeline = null;
            container.RelativePosition = double.NaN;
        }

        base.ClearContainerForItemOverride(element, item);
    }

    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        InvalidateItemPositions();
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        InvalidateItemPositions();
    }

    private void ItemContainerGenerator_StatusChanged(object? sender, EventArgs e)
    {
        if (ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            InvalidateItemPositions();
    }

    internal void InvalidateItemPositions()
    {
        if (_isUpdateScheduled)
            return;

        _isUpdateScheduled = true;
        Dispatcher.BeginInvoke(new Action(() =>
        {
            _isUpdateScheduled = false;
            UpdateItemOffsets();
        }), DispatcherPriority.Render);
    }

    private void UpdateItemOffsets()
    {
        if (!IsLoaded || _itemsHost is null)
            return;

        var containers = GetContainers().ToList();
        if (containers.Count == 0)
            return;

        // ensure order matches item collection
        var orderedContainers = new List<StswTimelineItem>();
        for (var i = 0; i < Items.Count; i++)
        {
            if (ItemContainerGenerator.ContainerFromIndex(i) is StswTimelineItem container)
                orderedContainers.Add(container);
        }

        if (orderedContainers.Count == 0)
            orderedContainers = containers;

        var indexLookup = new Dictionary<StswTimelineItem, int>(orderedContainers.Count);
        for (var i = 0; i < orderedContainers.Count; i++)
            indexLookup[orderedContainers[i]] = i;

        var datedItems = orderedContainers.Where(c => c.Date.HasValue).ToList();

        var minDate = StartDate.HasValue ? StartDate.Value.ToUniversalTime() : datedItems.MinOrDefault(static x => x.Date?.ToUniversalTime());
        var maxDate = EndDate.HasValue ? EndDate.Value.ToUniversalTime() : datedItems.MaxOrDefault(static x => x.Date?.ToUniversalTime());

        if (minDate.HasValue && maxDate.HasValue && minDate > maxDate)
            (minDate, maxDate) = (maxDate, minDate);

        var availableWidth = Math.Max(0.0, ActualWidth - Padding.Left - Padding.Right);

        if (!minDate.HasValue || !maxDate.HasValue || minDate.Value == maxDate.Value)
        {
            for (var i = 0; i < orderedContainers.Count; i++)
            {
                var normalized = orderedContainers.Count <= 1 ? 0.5 : i / (double)(orderedContainers.Count - 1);
                orderedContainers[i].RelativePosition = normalized;
                ApplyContainerPosition(orderedContainers[i], availableWidth);
            }
            return;
        }

        var totalTicks = (maxDate.Value - minDate.Value).Ticks;
        if (totalTicks <= 0)
        {
            for (var i = 0; i < orderedContainers.Count; i++)
            {
                var normalized = orderedContainers.Count <= 1 ? 0.5 : i / (double)(orderedContainers.Count - 1);
                orderedContainers[i].RelativePosition = normalized;
                ApplyContainerPosition(orderedContainers[i], availableWidth);
            }
            return;
        }

        foreach (var container in orderedContainers)
        {
            var index = indexLookup[container];
            double normalized = container.Date.HasValue
                ? Math.Clamp((container.Date.Value.ToUniversalTime() - minDate.Value).Ticks / (double)totalTicks, 0.0, 1.0)
                : (orderedContainers.Count <= 1 ? 0.5 : index / (double)(orderedContainers.Count - 1));

            container.RelativePosition = normalized;
            ApplyContainerPosition(container, availableWidth);
        }
    }

    private void ApplyContainerPosition(StswTimelineItem container, double availableWidth)
    {
        var offset = Padding.Left + availableWidth * container.RelativePosition - container.IndicatorSize / 2;
        Canvas.SetLeft(container, double.IsNaN(offset) ? 0 : offset);
        Canvas.SetTop(container, Padding.Top);
    }

    private IEnumerable<StswTimelineItem> GetContainers()
    {
        for (var i = 0; i < Items.Count; i++)
        {
            if (ItemContainerGenerator.ContainerFromIndex(i) is StswTimelineItem container)
                yield return container;
        }
    }

    private bool ApplyMemberValue(object item, string? memberPath, Action<object?> setter)
    {
        if (string.IsNullOrWhiteSpace(memberPath))
            return false;

        if (!TryGetMemberValue(item, memberPath, out var value))
            return false;

        setter(value);
        return true;
    }

    private static bool TryGetMemberValue(object item, string memberPath, out object? value)
    {
        object? current = item;
        foreach (var part in memberPath.Split(['.'], StringSplitOptions.RemoveEmptyEntries))
        {
            if (current == null)
            {
                value = null;
                return true;
            }

            var property = current.GetType().GetProperty(part);
            if (property == null)
            {
                value = null;
                return false;
            }

            current = property.GetValue(current);
        }

        value = current;
        return true;
    }

    #region Logic properties
    public DateTime? StartDate
    {
        get => (DateTime?)GetValue(StartDateProperty);
        set => SetValue(StartDateProperty, value);
    }
    public static readonly DependencyProperty StartDateProperty = DependencyProperty.Register(
        nameof(StartDate), typeof(DateTime?), typeof(StswTimeline),
        new PropertyMetadata(null, OnRangeChanged));

    public DateTime? EndDate
    {
        get => (DateTime?)GetValue(EndDateProperty);
        set => SetValue(EndDateProperty, value);
    }
    public static readonly DependencyProperty EndDateProperty = DependencyProperty.Register(
        nameof(EndDate), typeof(DateTime?), typeof(StswTimeline),
        new PropertyMetadata(null, OnRangeChanged));

    public string? DateMemberPath
    {
        get => (string?)GetValue(DateMemberPathProperty);
        set => SetValue(DateMemberPathProperty, value);
    }
    public static readonly DependencyProperty DateMemberPathProperty = DependencyProperty.Register(
        nameof(DateMemberPath), typeof(string), typeof(StswTimeline));

    public string? HeaderMemberPath
    {
        get => (string?)GetValue(HeaderMemberPathProperty);
        set => SetValue(HeaderMemberPathProperty, value);
    }
    public static readonly DependencyProperty HeaderMemberPathProperty = DependencyProperty.Register(
        nameof(HeaderMemberPath), typeof(string), typeof(StswTimeline));

    public string? DescriptionMemberPath
    {
        get => (string?)GetValue(DescriptionMemberPathProperty);
        set => SetValue(DescriptionMemberPathProperty, value);
    }
    public static readonly DependencyProperty DescriptionMemberPathProperty = DependencyProperty.Register(
        nameof(DescriptionMemberPath), typeof(string), typeof(StswTimeline));

    public string? ToolTipMemberPath
    {
        get => (string?)GetValue(ToolTipMemberPathProperty);
        set => SetValue(ToolTipMemberPathProperty, value);
    }
    public static readonly DependencyProperty ToolTipMemberPathProperty = DependencyProperty.Register(
        nameof(ToolTipMemberPath), typeof(string), typeof(StswTimeline));
    #endregion

    #region Style properties
    public double ItemHeight
    {
        get => (double)GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }
    public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(
        nameof(ItemHeight), typeof(double), typeof(StswTimeline),
        new PropertyMetadata(72.0));

    public double IndicatorSize
    {
        get => (double)GetValue(IndicatorSizeProperty);
        set => SetValue(IndicatorSizeProperty, value);
    }
    public static readonly DependencyProperty IndicatorSizeProperty = DependencyProperty.Register(
        nameof(IndicatorSize), typeof(double), typeof(StswTimeline),
        new PropertyMetadata(12.0, OnIndicatorSizeChanged));

    public Brush LineBrush
    {
        get => (Brush)GetValue(LineBrushProperty);
        set => SetValue(LineBrushProperty, value);
    }
    public static readonly DependencyProperty LineBrushProperty = DependencyProperty.Register(
        nameof(LineBrush), typeof(Brush), typeof(StswTimeline));

    public double LineThickness
    {
        get => (double)GetValue(LineThicknessProperty);
        set => SetValue(LineThicknessProperty, value);
    }
    public static readonly DependencyProperty LineThicknessProperty = DependencyProperty.Register(
        nameof(LineThickness), typeof(double), typeof(StswTimeline),
        new PropertyMetadata(2.0));
    #endregion

    private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StswTimeline timeline)
            timeline.InvalidateItemPositions();
    }

    private static void OnIndicatorSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StswTimeline timeline)
            timeline.InvalidateItemPositions();
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == PaddingProperty)
            InvalidateItemPositions();
    }
}

/// <summary>
/// Represents an individual timeline point rendered within <see cref="StswTimeline"/>.
/// </summary>
[StswPlannedChanges(StswPlannedChanges.Finish)]
public class StswTimelineItem : ContentControl
{
    static StswTimelineItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswTimelineItem), new FrameworkPropertyMetadata(typeof(StswTimelineItem)));
    }

    internal double RelativePosition { get; set; } = double.NaN;
    internal StswTimeline? ParentTimeline { get; set; }

    #region Logic properties
    public DateTime? Date
    {
        get => (DateTime?)GetValue(DateProperty);
        set => SetValue(DateProperty, value);
    }
    public static readonly DependencyProperty DateProperty = DependencyProperty.Register(
        nameof(Date), typeof(DateTime?), typeof(StswTimelineItem),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnLayoutAffectingPropertyChanged));

    public string? Header
    {
        get => (string?)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        nameof(Header), typeof(string), typeof(StswTimelineItem));

    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }
    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description), typeof(string), typeof(StswTimelineItem));

    public object? ToolTipContent
    {
        get => GetValue(ToolTipContentProperty);
        set => SetValue(ToolTipContentProperty, value);
    }
    public static readonly DependencyProperty ToolTipContentProperty = DependencyProperty.Register(
        nameof(ToolTipContent), typeof(object), typeof(StswTimelineItem));
    #endregion

    #region Style properties
    public double IndicatorSize
    {
        get => (double)GetValue(IndicatorSizeProperty);
        set => SetValue(IndicatorSizeProperty, value);
    }
    public static readonly DependencyProperty IndicatorSizeProperty = DependencyProperty.Register(
        nameof(IndicatorSize), typeof(double), typeof(StswTimelineItem),
        new FrameworkPropertyMetadata(12.0, FrameworkPropertyMetadataOptions.AffectsMeasure, OnLayoutAffectingPropertyChanged));
    #endregion

    private static void OnLayoutAffectingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StswTimelineItem item)
            item.ParentTimeline?.InvalidateItemPositions();
    }
}

internal static class StswTimelineEnumerableExtensions
{
    public static DateTime? MinOrDefault(this IEnumerable<StswTimelineItem> items, Func<StswTimelineItem, DateTime?> selector)
    {
        DateTime? result = null;
        foreach (var item in items)
        {
            var value = selector(item);
            if (value.HasValue)
                result = result.HasValue && result.Value <= value.Value ? result : value;
        }
        return result;
    }

    public static DateTime? MaxOrDefault(this IEnumerable<StswTimelineItem> items, Func<StswTimelineItem, DateTime?> selector)
    {
        DateTime? result = null;
        foreach (var item in items)
        {
            var value = selector(item);
            if (value.HasValue)
                result = result.HasValue && result.Value >= value.Value ? result : value;
        }
        return result;
    }
}