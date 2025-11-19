using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace StswExpress;
/// <summary>
/// Represents a custom calendar control with date range selection functionality.
/// Supports navigation between months and years, selecting individual days or months, 
/// and setting minimum/maximum date ranges. Includes quick selection of the current date
/// and UI customization options.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswRangeCalendar SelectedDate="{Binding SelectedMonth}" SelectionUnit="Months" Maximum="2025-12-31"/&gt;
/// </code>
/// </example>
public class StswRangeCalendar : StswCalendar
{
    private bool _awaitingRangeEnd;
    private bool _suppressDateHandling;
    private bool _isUpdatingRangeFromSelection;
    private Selector? _selectorHost;
    private DateTime? _pendingRangeHoverDate;

    static StswRangeCalendar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswRangeCalendar), new FrameworkPropertyMetadata(typeof(StswRangeCalendar)));

        SelectedDateProperty.OverrideMetadata(typeof(StswRangeCalendar),
            new FrameworkPropertyMetadata(default(DateTime?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedDateChanged, null, false, UpdateSourceTrigger.PropertyChanged));

        ItemsProperty.OverrideMetadata(typeof(StswRangeCalendar), new PropertyMetadata(null, OnItemsPropertyChanged));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        DetachRangeSelector();
        SyncSelectedDateWithRange();
        base.OnApplyTemplate();
        AttachRangeSelector();
        UpdateRangeVisuals();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == CurrentUnitProperty || e.Property == SelectionUnitProperty)
            UpdateRangeVisuals();
    }

    /// <summary>
    /// Handles changes to the SelectedDate property.
    /// </summary>
    /// <param name="d">The dependency object where the property changed.</param>
    /// <param name="e">The event arguments containing old and new values.</param>
    private static new void OnSelectedDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        StswCalendar.OnSelectedDateChanged(d, e);

        if (d is StswRangeCalendar calendar)
            calendar.HandleDateSelection((DateTime?)e.NewValue);
    }

    /// <summary>
    /// Handles changes to the Items property.
    /// </summary>
    /// <param name="d">The dependency object where the property changed.</param>
    /// <param name="e">The event arguments containing old and new values.</param>
    private static void OnItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StswRangeCalendar calendar)
            calendar.AttachItems(e.OldValue as ObservableCollection<StswCalendarEntry>, e.NewValue as ObservableCollection<StswCalendarEntry>);
    }

    /// <summary>
    /// Attaches event handlers to the old and new Items collections.
    /// </summary>
    /// <param name="oldItems">The old items collection.</param>
    /// <param name="newItems">The new items collection.</param>
    private void AttachItems(ObservableCollection<StswCalendarEntry>? oldItems, ObservableCollection<StswCalendarEntry>? newItems)
    {
        if (oldItems is not null)
            oldItems.CollectionChanged -= Items_CollectionChanged;
        if (newItems is not null)
            newItems.CollectionChanged += Items_CollectionChanged;

        UpdateRangeVisuals();
    }

    /// <summary>
    /// Handles the CollectionChanged event of the Items collection.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => UpdateRangeVisuals();

    /// <summary>
    /// Handles date selection logic for the calendar.
    /// </summary>
    /// <param name="date">The selected date.</param>
    private void HandleDateSelection(DateTime? date)
    {
        if (_suppressDateHandling)
            return;

        if (!date.HasValue)
        {
            _awaitingRangeEnd = false;
            UpdateRangeFromSelection(() => SelectedRange = null);
            UpdatePreviewRange(null);
            return;
        }

        var normalizedSelection = NormalizeForSelectionUnit(date.Value);
        var adjustRangeWithModifier = !_awaitingRangeEnd
            && SelectedRange is not null
            && (Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) != ModifierKeys.None;

        if (SelectedRange is null)
        {
            UpdateRangeFromSelection(() => SelectedRange = new StswDateRange(normalizedSelection, normalizedSelection));
            _awaitingRangeEnd = true;
            UpdatePreviewRange(null);
            return;
        }

        if (adjustRangeWithModifier)
        {
            UpdateRangeFromSelection(() => UpdateRangeByClosestBoundary(normalizedSelection));
            UpdatePreviewRange(null);
            return;
        }

        if (!_awaitingRangeEnd)
        {
            UpdateRangeFromSelection(() =>
            {
                ApplyOrderedRange(normalizedSelection, normalizedSelection);
            });
            _awaitingRangeEnd = true;
            UpdatePreviewRange(null);
        }
        else
        {
            UpdateRangeFromSelection(() =>
            {
                if (SelectedRange is null)
                    return;

                ApplyOrderedRange(SelectedRange.Start, normalizedSelection);
            });
            _awaitingRangeEnd = false;
            UpdatePreviewRange(null);
        }
    }

    /// <summary>
    /// Updates the date range from the current selection while preventing recursive updates.
    /// </summary>
    /// <param name="update">The action to perform the update.</param>
    private void UpdateRangeFromSelection(Action update)
    {
        _isUpdatingRangeFromSelection = true;
        try
        {
            update();
        }
        finally
        {
            _isUpdatingRangeFromSelection = false;
        }
    }

    /// <summary>
    /// Handles changes to the SelectedRange property.
    /// </summary>
    /// <param name="oldRange">The old selected date range.</param>
    /// <param name="newRange">The new selected date range.</param>
    private void OnSelectedRangeChanged(StswDateRange? oldRange, StswDateRange? newRange)
    {
        if (oldRange is not null)
            oldRange.PropertyChanged -= SelectedRange_PropertyChanged;
        if (newRange is not null)
            newRange.PropertyChanged += SelectedRange_PropertyChanged;

        if (newRange is not null)
            EnsureRangeOrder(newRange);

        if (!_isUpdatingRangeFromSelection)
        {
            _awaitingRangeEnd = false;
            UpdatePreviewRange(null);
            SyncSelectedDateWithRange();
        }

        UpdateRangeVisuals();
    }

    /// <summary>
    /// Handles property changes within the SelectedRange object.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments containing the property name.</param>
    private void SelectedRange_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is StswDateRange range)
            EnsureRangeOrder(range);

        if (e.PropertyName is nameof(StswDateRange.Start) && !_isUpdatingRangeFromSelection)
            SyncSelectedDateWithRange();

        if (e.PropertyName is nameof(StswDateRange.Start) || e.PropertyName is nameof(StswDateRange.End))
            UpdateRangeVisuals();
    }

    /// <summary>
    /// Synchronizes the SelectedDate property with the start of the SelectedRange.
    /// </summary>
    private void SyncSelectedDateWithRange()
    {
        var target = SelectedRange is null
            ? (DateTime?)null
            : NormalizeForSelectionUnit(SelectedRange.Start);

        SetSelectedDateSilently(target);
    }

    /// <summary>
    /// Sets the SelectedDate property without triggering date handling logic.
    /// </summary>
    /// <param name="date">The date to set.</param>
    private void SetSelectedDateSilently(DateTime? date)
    {
        _suppressDateHandling = true;
        try
        {
            SelectedDate = date;
        }
        finally
        {
            _suppressDateHandling = false;
        }
    }

    /// <summary>
    /// Normalizes a DateTime value based on the SelectionUnit.
    /// </summary>
    /// <param name="value">The DateTime value to normalize.</param>
    /// <returns>The normalized DateTime value.</returns>
    private DateTime NormalizeForSelectionUnit(DateTime value) => SelectionUnit == StswCalendarUnit.Months ? value.ToFirstDayOfMonth() : value.Date;

    /// <summary>
    /// Normalizes a DateTime value based on the CurrentUnit.
    /// </summary>
    /// <param name="value">The DateTime value to normalize.</param>
    /// <returns>The normalized DateTime value.</returns>
    private DateTime NormalizeForCurrentUnit(DateTime value) => CurrentUnit == StswCalendarUnit.Months ? value.ToFirstDayOfMonth() : value.Date;

    /// <summary>
    /// Updates the visual representation of the selected date range in the calendar.
    /// </summary>
    private void UpdateRangeVisuals()
    {
        if (Items is null)
            return;

        DateTime? rangeStart = null;
        DateTime? rangeEnd = null;

        if (SelectedRange is not null)
        {
            rangeStart = NormalizeForCurrentUnit(SelectedRange.Start);
            rangeEnd = NormalizeForCurrentUnit(SelectedRange.End);
        }

        DateTime? coverageStart = rangeStart;
        DateTime? coverageEnd = rangeEnd;

        if (rangeStart.HasValue && rangeEnd.HasValue)
        {
            if (rangeStart > rangeEnd)
                (coverageStart, coverageEnd) = (rangeEnd, rangeStart);
        }
        else if (rangeStart.HasValue)
        {
            coverageEnd = rangeStart;
        }
        else if (rangeEnd.HasValue)
        {
            coverageStart = rangeEnd;
        }

        DateTime? previewStart = null;
        DateTime? previewEnd = null;

        if (_awaitingRangeEnd && rangeStart.HasValue && _pendingRangeHoverDate.HasValue)
        {
            previewStart = rangeStart;
            previewEnd = _pendingRangeHoverDate;

            if (previewStart > previewEnd)
                (previewStart, previewEnd) = (previewEnd, previewStart);
        }

        foreach (var item in Items)
        {
            if (item.Date is not DateTime itemDate)
            {
                item.IsInRange = false;
                item.IsRangeEdge = false;
                item.IsPreviewRange = false;
                continue;
            }

            var normalizedItemDate = NormalizeForCurrentUnit(itemDate);
            var isEdge = (rangeStart.HasValue && normalizedItemDate == rangeStart)
                || (rangeEnd.HasValue && normalizedItemDate == rangeEnd);

            var isInRange = coverageStart.HasValue && coverageEnd.HasValue
                ? normalizedItemDate.Between(coverageStart.Value, coverageEnd.Value)
                : false;

            var isPreview = previewStart.HasValue && previewEnd.HasValue
                ? normalizedItemDate.Between(previewStart.Value, previewEnd.Value)
                : false;

            item.IsRangeEdge = isEdge;
            item.IsInRange = isInRange;
            item.IsPreviewRange = isPreview;
        }
    }

    /// <summary>
    /// Adjusts the existing range by updating the closest boundary to the provided date.
    /// </summary>
    /// <param name="normalizedSelection">The normalized date to apply.</param>
    private void UpdateRangeByClosestBoundary(DateTime normalizedSelection)
    {
        if (SelectedRange is null)
            return;

        var start = SelectedRange.Start;
        var end = SelectedRange.End;

        if (start > end)
            (start, end) = (end, start);

        var distanceToStart = Math.Abs((normalizedSelection - start).Ticks);
        var distanceToEnd = Math.Abs((normalizedSelection - end).Ticks);

        if (distanceToStart <= distanceToEnd)
            start = normalizedSelection;
        else
            end = normalizedSelection;

        ApplyOrderedRange(start, end);
    }

    /// <summary>
    /// Applies the provided start and end ensuring chronological order.
    /// </summary>
    /// <param name="start">The first boundary.</param>
    /// <param name="end">The second boundary.</param>
    private void ApplyOrderedRange(DateTime start, DateTime end)
    {
        if (start > end)
            (start, end) = (end, start);

        if (SelectedRange is null)
        {
            SelectedRange = new StswDateRange(start, end);
            return;
        }

        SelectedRange.Start = start;
        SelectedRange.End = end;
    }

    /// <summary>
    /// Ensures the specified range stores its bounds in chronological order.
    /// </summary>
    /// <param name="range">The range to normalize.</param>
    private static void EnsureRangeOrder(StswDateRange range)
    {
        if (range.Start <= range.End)
            return;

        var start = range.Start;
        range.Start = range.End;
        range.End = start;
    }

    /// <summary>
    /// Updates the preview range highlight based on the hovered date.
    /// </summary>
    /// <param name="hoverDate">The hovered date.</param>
    private void UpdatePreviewRange(DateTime? hoverDate)
    {
        DateTime? normalizedHover = null;

        if (_awaitingRangeEnd && SelectedRange is not null && hoverDate.HasValue)
            normalizedHover = NormalizeForCurrentUnit(hoverDate.Value);

        if (_pendingRangeHoverDate == normalizedHover)
            return;

        _pendingRangeHoverDate = normalizedHover;
        UpdateRangeVisuals();
    }

    /// <summary>
    /// Hooks pointer tracking events to the selector hosting calendar entries.
    /// </summary>
    private void AttachRangeSelector()
    {
        if (GetTemplateChild("PART_Selector") is Selector selector)
        {
            _selectorHost = selector;
            selector.MouseMove += Selector_MouseMove;
            selector.MouseLeave += Selector_MouseLeave;
        }
    }

    /// <summary>
    /// Removes pointer tracking events to avoid leaks when the template changes.
    /// </summary>
    private void DetachRangeSelector()
    {
        if (_selectorHost is null)
            return;

        _selectorHost.MouseMove -= Selector_MouseMove;
        _selectorHost.MouseLeave -= Selector_MouseLeave;
        _selectorHost = null;
    }

    /// <summary>
    /// Handles mouse movement over calendar entries to update the preview range.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The mouse event arguments.</param>
    private void Selector_MouseMove(object sender, MouseEventArgs e)
    {
        if (sender is not ItemsControl itemsControl)
            return;

        if (ItemsControl.ContainerFromElement(itemsControl, e.OriginalSource as DependencyObject) is ContentControl container
         && container.Content is StswCalendarEntry entry)
            UpdatePreviewRange(entry.Date);
        else
            UpdatePreviewRange(null);
    }

    /// <summary>
    /// Handles mouse leaving the selector area to clear the preview range.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The mouse event arguments.</param>
    private void Selector_MouseLeave(object? sender, MouseEventArgs e) => UpdatePreviewRange(null);
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the currently selected date range.
    /// </summary>
    public StswDateRange? SelectedRange
    {
        get => (StswDateRange?)GetValue(SelectedRangeProperty);
        set => SetValue(SelectedRangeProperty, value);
    }
    public static readonly DependencyProperty SelectedRangeProperty
        = DependencyProperty.Register(
            nameof(SelectedRange),
            typeof(StswDateRange),
            typeof(StswRangeCalendar),
            new FrameworkPropertyMetadata(default(StswDateRange),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedRangeChanged)
        );
    private static void OnSelectedRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswRangeCalendar stsw)
            return;

        stsw.OnSelectedRangeChanged(e.OldValue as StswDateRange, e.NewValue as StswDateRange);
    }
    #endregion
}
