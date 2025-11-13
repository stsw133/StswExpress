using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

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
        SyncSelectedDateWithRange();
        base.OnApplyTemplate();
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
            return;
        }

        var normalizedSelection = NormalizeForSelectionUnit(date.Value);

        if (SelectedRange is null)
        {
            UpdateRangeFromSelection(() => SelectedRange = new StswDateRange(normalizedSelection, normalizedSelection));
            _awaitingRangeEnd = true;
            return;
        }

        if (!_awaitingRangeEnd)
        {
            UpdateRangeFromSelection(() =>
            {
                SelectedRange.Start = normalizedSelection;
                SelectedRange.End = normalizedSelection;
            });
            _awaitingRangeEnd = true;
        }
        else
        {
            UpdateRangeFromSelection(() => SelectedRange.End = normalizedSelection);
            _awaitingRangeEnd = false;
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

        if (!_isUpdatingRangeFromSelection)
        {
            _awaitingRangeEnd = false;
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

        foreach (var item in Items)
        {
            if (item.Date is not DateTime itemDate)
            {
                item.IsInRange = false;
                item.IsRangeEdge = false;
                continue;
            }

            var normalizedItemDate = NormalizeForCurrentUnit(itemDate);
            var isEdge = (rangeStart.HasValue && normalizedItemDate == rangeStart)
                || (rangeEnd.HasValue && normalizedItemDate == rangeEnd);

            var isInRange = coverageStart.HasValue && coverageEnd.HasValue
                ? normalizedItemDate.Between(coverageStart.Value, coverageEnd.Value)
                : false;

            item.IsRangeEdge = isEdge;
            item.IsInRange = isInRange;
        }
    }
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
        if (d is StswRangeCalendar calendar)
            calendar.OnSelectedRangeChanged(e.OldValue as StswDateRange, e.NewValue as StswDateRange);
    }
    #endregion
}
