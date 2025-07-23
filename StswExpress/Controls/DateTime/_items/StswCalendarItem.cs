using System;

namespace StswExpress;

/// <summary>
/// Data model for <see cref="StswCalendar"/>'s items.
/// </summary>
[StswInfo("0.14.0")]
internal class StswCalendarItem : StswObservableObject, IStswSelectionItem
{
    /// <summary>
    /// Gets or sets the content associated with the calendar item.
    /// </summary>
    public object? Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }
    private object? _content;

    /// <summary>
    /// Gets or sets the date associated with the calendar item.
    /// </summary>
    public DateTime? Date
    {
        get => _date;
        set => SetProperty(ref _date, value);
    }
    private DateTime? _date;

    /// <summary>
    /// Gets or sets a value indicating whether the calendar item is within the selected month.
    /// </summary>
    public bool? InCurrentMonth
    {
        get => _inCurrentMonth;
        set => SetProperty(ref _inCurrentMonth, value);
    }
    private bool? _inCurrentMonth;

    /// <summary>
    /// Gets a value indicating whether the calendar item is within the allowable date range.
    /// </summary>
    public bool? InMinMaxRange
    {
        get => _inMinMaxRange;
        set => SetProperty(ref _inMinMaxRange, value);
    }
    private bool? _inMinMaxRange;

    /// <summary>
    /// Gets a value indicating whether the calendar item represents the current day.
    /// </summary>
    public bool? IsCurrentDay
    {
        get => _isCurrentDay;
        set => SetProperty(ref _isCurrentDay, value);
    }
    private bool? _isCurrentDay;

    /// <summary>
    /// Gets or sets the selection associated with the item.
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
    private bool _isSelected;

    /// <summary>
    /// Gets a value indicating whether the calendar item represents the special day (e.g., sunday).
    /// </summary>
    public bool? IsSpecialDay
    {
        get => _isSpecialDay;
        set => SetProperty(ref _isSpecialDay, value);
    }
    private bool? _isSpecialDay;
}
