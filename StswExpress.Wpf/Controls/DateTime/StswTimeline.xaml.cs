using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace StswExpress.Wpf;

/// <summary>
/// Displays a timeline of points laid out horizontally or vertically based on their dates.
/// Supports optional <see cref="Minimum"/> and <see cref="Maximum"/> bounds and opens a popup on hover for each item.
/// </summary>
[StswPlannedChanges(StswPlannedChanges.Finish)]
public class StswTimeline : ItemsControl
{
    static StswTimeline()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswTimeline), new FrameworkPropertyMetadata(typeof(StswTimeline)));
    }

    #region Logic properties
    /// <summary>
    /// Gets or sets the path to the property that provides the date for each item.
    /// </summary>
    public string? DateMemberPath
    {
        get => (string?)GetValue(DateMemberPathProperty);
        set => SetValue(DateMemberPathProperty, value);
    }
    public static readonly DependencyProperty DateMemberPathProperty
        = DependencyProperty.Register(
            nameof(DateMemberPath),
            typeof(string),
            typeof(StswTimeline),
            new FrameworkPropertyMetadata(default(string),
                FrameworkPropertyMetadataOptions.AffectsArrange,
                OnDateMemberPathChanged)
        );
    private static void OnDateMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var stsw = (StswTimeline)d;
        foreach (var item in stsw.Items)
            stsw.ApplyContainerBindings(item);
        stsw.UpdateAutomaticRange();
        stsw.InvalidateArrange();
    }

    /// <summary>
    /// Gets or sets the template used to display the popup content for each item.
    /// </summary>
    public DataTemplate? ItemPopupTemplate
    {
        get => (DataTemplate?)GetValue(ItemPopupTemplateProperty);
        set => SetValue(ItemPopupTemplateProperty, value);
    }
    public static readonly DependencyProperty ItemPopupTemplateProperty
        = DependencyProperty.Register(
            nameof(ItemPopupTemplate),
            typeof(DataTemplate),
            typeof(StswTimeline)
        );

    /// <summary>
    /// Gets or sets the template selector used to choose the popup template for each item.
    /// </summary>
    public DataTemplateSelector? ItemPopupTemplateSelector
    {
        get => (DataTemplateSelector?)GetValue(ItemPopupTemplateSelectorProperty);
        set => SetValue(ItemPopupTemplateSelectorProperty, value);
    }
    public static readonly DependencyProperty ItemPopupTemplateSelectorProperty
        = DependencyProperty.Register(
            nameof(ItemPopupTemplateSelector),
            typeof(DataTemplateSelector),
            typeof(StswTimeline)
        );

    /// <summary>
    /// Gets or sets the maximum date for the timeline.
    /// </summary>
    public DateTime? Maximum
    {
        get => (DateTime?)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }
    public static readonly DependencyProperty MaximumProperty
        = DependencyProperty.Register(
            nameof(Maximum),
            typeof(DateTime?),
            typeof(StswTimeline),
            new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.AffectsArrange,
                OnRangeChanged)
        );
    private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var stsw = (StswTimeline)d;
        stsw.UpdateAutomaticRange();
        stsw.InvalidateArrange();
    }

    /// <summary>
    /// Gets or sets the minimum date for the timeline.
    /// </summary>
    public DateTime? Minimum
    {
        get => (DateTime?)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }
    public static readonly DependencyProperty MinimumProperty
        = DependencyProperty.Register(
            nameof(Minimum),
            typeof(DateTime?),
            typeof(StswTimeline),
            new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.AffectsArrange,
                OnRangeChanged)
        );

    /// <summary>
    /// Gets or sets the orientation of the timeline.
    /// </summary>
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
    public static readonly DependencyProperty OrientationProperty
        = DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(StswTimeline),
            new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsArrange)
        );

    /// <summary>
    /// Gets or sets the initial delay, in milliseconds, before a tooltip is shown for an item.
    /// </summary>
    public int ToolTipInitialShowDelay
    {
        get => (int)GetValue(ToolTipInitialShowDelayProperty);
        set => SetValue(ToolTipInitialShowDelayProperty, value);
    }
    public static readonly DependencyProperty ToolTipInitialShowDelayProperty
        = DependencyProperty.Register(
            nameof(ToolTipInitialShowDelay),
            typeof(int),
            typeof(StswTimeline),
            new FrameworkPropertyMetadata(200)
        );

    /// <summary>
    /// Gets or sets the duration, in milliseconds, that a tooltip stays visible for an item.
    /// </summary>
    public int ToolTipShowDuration
    {
        get => (int)GetValue(ToolTipShowDurationProperty);
        set => SetValue(ToolTipShowDurationProperty, value);
    }
    public static readonly DependencyProperty ToolTipShowDurationProperty
        = DependencyProperty.Register(
            nameof(ToolTipShowDuration),
            typeof(int),
            typeof(StswTimeline),
            new FrameworkPropertyMetadata(10000)
        );
    #endregion

    #region Overrides
    /// <inheritdoc/>
    protected override DependencyObject GetContainerForItemOverride() => new StswTimelineItem();
    /// <inheritdoc/>
    protected override bool IsItemItsOwnContainerOverride(object item) => item is StswTimelineItem;
    /// <inheritdoc/>
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        if (element is StswTimelineItem timelineItem)
            ConfigureContainer(timelineItem, item);
    }

    /// <inheritdoc/>
    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);

        if (e?.NewItems != null)
            foreach (var item in e.NewItems)
                ApplyContainerBindings(item);

        UpdateAutomaticRange();
        InvalidateArrange();
    }

    /// <inheritdoc/>
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        base.OnItemsSourceChanged(oldValue, newValue);
        UpdateAutomaticRange();
        InvalidateArrange();
    }
    #endregion

    #region Logic
    private DateTime? _itemsMinimum, _itemsMaximum;

    /// <summary>
    /// Applies necessary bindings to the container for the given item.
    /// </summary>
    /// <param name="item">The item to apply bindings for.</param>
    private void ApplyContainerBindings(object item)
    {
        if (ItemContainerGenerator.ContainerFromItem(item) is StswTimelineItem container)
            ConfigureContainer(container, item);
    }

    /// <summary>
    /// Configures the timeline item container with necessary bindings.
    /// </summary>
    /// <param name="container">The timeline item container.</param>
    /// <param name="item">The item associated with the container.</param>
    private void ConfigureContainer(StswTimelineItem container, object item)
    {
        container.SetBinding(StswTimelineItem.OrientationProperty, new Binding(nameof(Orientation))
        {
            Source = this,
            Mode = BindingMode.OneWay
        });
        container.SetBinding(StswTimelineItem.PopupTemplateProperty, new Binding(nameof(ItemPopupTemplate))
        {
            Source = this,
            Mode = BindingMode.OneWay
        });
        container.SetBinding(StswTimelineItem.PopupTemplateSelectorProperty, new Binding(nameof(ItemPopupTemplateSelector))
        {
            Source = this,
            Mode = BindingMode.OneWay
        });
        container.SetBinding(StswTimelineItem.ToolTipInitialShowDelayProperty, new Binding(nameof(ToolTipInitialShowDelay))
        {
            Source = this,
            Mode = BindingMode.OneWay
        });
        container.SetBinding(StswTimelineItem.ToolTipShowDurationProperty, new Binding(nameof(ToolTipShowDuration))
        {
            Source = this,
            Mode = BindingMode.OneWay
        });

        SetDateBinding(container, item);
    }

    /// <summary>
    /// Sets the date binding for the timeline item container based on the provided item.
    /// </summary>
    /// <param name="container">The timeline item container.</param>
    /// <param name="item">The item associated with the container.</param>
    private void SetDateBinding(StswTimelineItem container, object item)
    {
        BindingOperations.ClearBinding(container, StswTimelineItem.DateProperty);

        if (item is StswTimelineItem timelineItem && ReferenceEquals(container, timelineItem))
            return;

        if (item is DateTime dateTime)
        {
            container.Date = dateTime;
            return;
        }

        if (item is DateTime?)
        {
            var nullableDate = (DateTime?)item;
            if (nullableDate.HasValue)
            {
                container.Date = nullableDate.Value;
                return;
            }
        }

        var path = string.IsNullOrWhiteSpace(DateMemberPath) ? "Date" : DateMemberPath;
        var descriptor = TypeDescriptor.GetProperties(item)[path];

        if (descriptor != null)
            BindingOperations.SetBinding(container, StswTimelineItem.DateProperty, new Binding(path)
            {
                Source = item,
                Mode = BindingMode.OneWay
            });
    }

    /// <summary>
    /// Updates the automatic minimum and maximum dates based on the items in the timeline.
    /// </summary>
    private void UpdateAutomaticRange()
    {
        if (Minimum.HasValue && Maximum.HasValue)
        {
            _itemsMinimum = null;
            _itemsMaximum = null;
            return;
        }

        DateTime? min = null;
        DateTime? max = null;

        foreach (var item in Items)
        {
            var date = GetItemDate(item);
            if (!date.HasValue)
                continue;

            if (!min.HasValue || date.Value < min.Value)
                min = date.Value;
            if (!max.HasValue || date.Value > max.Value)
                max = date.Value;
        }

        _itemsMinimum = min;
        _itemsMaximum = max;
    }

    /// <summary>
    /// Gets the date associated with the given item.
    /// </summary>
    /// <param name="item">The item to retrieve the date from.</param>
    /// <returns>The date associated with the item, or <see langword="null"/> if not found.</returns>
    private DateTime? GetItemDate(object item)
    {
        if (item is StswTimelineItem timelineItem)
            return timelineItem.Date;

        if (item is DateTime dateTime)
            return dateTime;

        if (item is DateTime?)
        {
            var nullableDate = (DateTime?)item;
            if (nullableDate.HasValue)
                return nullableDate.Value;
        }

        var path = string.IsNullOrWhiteSpace(DateMemberPath) ? "Date" : DateMemberPath;
        var descriptor = TypeDescriptor.GetProperties(item)[path];
        var value = descriptor?.GetValue(item);

        return value switch
        {
            DateTime dt => dt,
            _ => null
        };
    }

    /// <summary>
    /// Gets the effective minimum and maximum dates for the timeline.
    /// </summary>
    /// <returns>A tuple containing the effective minimum and maximum dates.</returns>
    internal (DateTime Minimum, DateTime Maximum) GetEffectiveRange()
    {
        var minimum = Minimum ?? _itemsMinimum ?? _itemsMaximum ?? DateTime.Now;
        var maximum = Maximum ?? _itemsMaximum ?? _itemsMinimum ?? minimum;

        return (minimum, maximum);
    }
    #endregion
}

/// <summary>
/// A panel that positions timeline items proportionally between the effective minimum and maximum dates.
/// </summary>
public class StswTimelinePanel : Panel
{
    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        if (InternalChildren.Count == 0)
            return finalSize;

        if (ItemsControl.GetItemsOwner(this) is not StswTimeline owner)
            return finalSize;

        var (minimum, maximum) = owner.GetEffectiveRange();
        var span = maximum - minimum;
        var orientation = owner.Orientation;
        var length = orientation == Orientation.Horizontal ? finalSize.Width : finalSize.Height;
        var center = orientation == Orientation.Horizontal ? finalSize.Height / 2 : finalSize.Width / 2;
        var totalSeconds = Math.Max(span.TotalSeconds, 0.001);

        var maxHalfExtent = 0d;
        foreach (UIElement child in InternalChildren)
        {
            var extent = orientation == Orientation.Horizontal ? child.DesiredSize.Width : child.DesiredSize.Height;
            maxHalfExtent = Math.Max(maxHalfExtent, extent / 2);
        }

        var usableLength = Math.Max(0, length - maxHalfExtent * 2);
        var startOffset = maxHalfExtent;

        foreach (UIElement child in InternalChildren)
        {
            if (child is not StswTimelineItem item)
            {
                child.Arrange(new Rect(finalSize));
                continue;
            }

            var offsetSeconds = (item.Date - minimum).TotalSeconds;
            var ratio = Math.Min(1, Math.Max(0, offsetSeconds / totalSeconds));
            var position = startOffset + usableLength * ratio;

            if (orientation == Orientation.Horizontal)
            {
                var x = position - child.DesiredSize.Width / 2;
                var y = center - child.DesiredSize.Height / 2;
                child.Arrange(new Rect(new Point(x, y), child.DesiredSize));
            }
            else
            {
                var x = center - child.DesiredSize.Width / 2;
                var y = position - child.DesiredSize.Height / 2;
                child.Arrange(new Rect(new Point(x, y), child.DesiredSize));
            }
        }

        return finalSize;
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        foreach (UIElement child in InternalChildren)
            child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        return availableSize;
    }
}
