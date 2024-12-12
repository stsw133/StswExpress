using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// Represents a custom calendar control with date selection functionality.
/// The control allows users to navigate between months and years, select individual days or months, and provides 
/// support for minimum and maximum date ranges. It also includes functionality for quick selection of the current date
/// and offers support for customization, such as corner radius and item appearance.
/// </summary>
[ContentProperty(nameof(SelectedDate))]
public class StswCalendar : Control, IStswCornerControl
{
    public StswCalendar()
    {
        SetValue(ItemsProperty, new ObservableCollection<StswCalendarItem>());
    }
    static StswCalendar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswCalendar), new FrameworkPropertyMetadata(typeof(StswCalendar)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswCalendar), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    #region Events & methods
    private ContentControl? _buttonToday;
    private ContentControl? _buttonClear;

    /// <summary>
    /// Occurs when the selected date in the control changes.
    /// </summary>
    public event EventHandler? SelectedDateChanged;

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// Button: previous year
        if (GetTemplateChild("PART_ButtonPreviousYear") is ButtonBase btnPreviousYear)
            btnPreviousYear.Click += (_, _) => SwitchMonth(ValidateSelectedMonth(CurrentUnit == StswCalendarUnit.Months ? -120 : -12));
        /// Button: previous month
        if (GetTemplateChild("PART_ButtonPreviousMonth") is ButtonBase btnPreviousMonth)
            btnPreviousMonth.Click += (_, _) => SwitchMonth(ValidateSelectedMonth(CurrentUnit == StswCalendarUnit.Months ? -12 : -1));
        /// Button: current unit
        if (GetTemplateChild("PART_ButtonCurrentUnit") is ButtonBase btnCurrentUnit)
            btnCurrentUnit.Click += (_, _) => CurrentUnit = CurrentUnit.GetNextValue();
        /// Button: next month
        if (GetTemplateChild("PART_ButtonNextMonth") is ButtonBase btnNextMonth)
            btnNextMonth.Click += (_, _) => SwitchMonth(ValidateSelectedMonth(CurrentUnit == StswCalendarUnit.Months ? 12 : 1));
        /// Button: next year
        if (GetTemplateChild("PART_ButtonNextYear") is ButtonBase btnNextYear)
            btnNextYear.Click += (_, _) => SwitchMonth(ValidateSelectedMonth(CurrentUnit == StswCalendarUnit.Months ? 120 : 12));
        /// Selector: list
        if (GetTemplateChild("PART_Selector") is Selector selector)
        {
            selector.PreviewMouseLeftButtonDown += Selector_PreviewMouseLeftButtonDown;
            selector.SelectionChanged += Selector_SelectionChanged;
        }
        /// Button: today
        if (GetTemplateChild("PART_ButtonToday") is ButtonBase btnToday)
        {
            btnToday.Click += (_, _) =>
            {
                SelectMonth(DateTime.Today);
                SelectDay(DateTime.Today);
            };
            _buttonToday = btnToday;
        }
        /// Button: clear
        if (GetTemplateChild("PART_ButtonClear") is ButtonBase btnClear)
        {
            btnClear.Click += (_, _) =>
            {
                SelectedDate = null;
                ClosePopupIfAny();
            };
            _buttonClear = btnClear;
        }

        /// set default month to create the initial view
        var defMonth = (SelectedDate ?? DateTime.Now).Date;
        SelectedMonth = new DateTime(defMonth.Year, defMonth.Month, 1);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void Selector_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (ItemsControl.ContainerFromElement(sender as ItemsControl, e.OriginalSource as DependencyObject) is ContentControl item)
        {
            var date = (item.Content as StswCalendarItem)?.Date;

            if (CurrentUnit == StswCalendarUnit.Days && date == SelectedDate?.Date)
                SelectDay(date);
            else if (CurrentUnit == StswCalendarUnit.Months && SelectionUnit != StswCalendarUnit.Months && date.HasValue)
                SelectMonth(date.Value);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void Selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        foreach (var item in e.AddedItems)
        {
            var date = (item as StswCalendarItem)?.Date;

            if (CurrentUnit == StswCalendarUnit.Days && date != SelectedDate?.Date)
                SelectDay(date);
            else if (CurrentUnit == StswCalendarUnit.Months && SelectionUnit == StswCalendarUnit.Months && date.HasValue)
                SelectMonth(date.Value);
        }
    }

    /// <summary>
    /// Closes the parent popup if the control is contained within one.
    /// </summary>
    /// <returns><see langword="true"/> if the popup was found and closed; otherwise, <see langword="false"/>.</returns>
    private bool ClosePopupIfAny()
    {
        if (StswFn.GetParentPopup(this) is Popup popup)
        {
            popup.IsOpen = false;
            if (StswFn.FindVisualAncestor<StswDatePicker>(popup) is StswDatePicker stsw)
            {
                stsw.Focus();
                stsw.CaretIndex = stsw.Text?.Length ?? 0;
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Updates the list of days displayed in the control for the currently selected month.
    /// </summary>
    private ObservableCollection<StswCalendarItem> GenerateDays()
    {
        var collection = new ObservableCollection<StswCalendarItem>();
        var middleDate = new DateTime(SelectedMonth.Year, SelectedMonth.Month, 15);
        var offset = ((int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek - (int)middleDate.DayOfWeek + 7) % 7;
        middleDate = middleDate.AddDays(offset);

        var today = DateTime.Today;
        for (var i = 0; i < 42; i++)
        {
            DateTime? newDate = null;
            var targetTicks = middleDate.Ticks + TimeSpan.TicksPerDay * (i - 21);
            if (targetTicks >= DateTime.MinValue.Ticks && targetTicks <= DateTime.MaxValue.Ticks)
                newDate = new DateTime(targetTicks);

            collection.Add(new StswCalendarItem
            {
                Content = newDate?.Day.ToString(),
                Date = newDate,
                InCurrentMonth = newDate?.Year == SelectedMonth.Year && newDate?.Month == SelectedMonth.Month,
                InMinMaxRange = newDate >= (Minimum ?? DateTime.MinValue) && newDate <= (Maximum ?? DateTime.MaxValue),
                IsCurrentDay = newDate == today,
                IsSpecialDay = newDate?.DayOfWeek == DayOfWeek.Sunday,
                IsSelected = newDate == SelectedDate?.Date
            });
        }

        return collection;
    }

    /// <summary>
    /// Updates the list of months displayed in the control for the currently selected year.
    /// </summary>
    private ObservableCollection<StswCalendarItem> GenerateMonths()
    {
        var collection = new ObservableCollection<StswCalendarItem>();
        var min = Minimum ?? DateTime.MinValue; min = new DateTime(min.Year, min.Month, 1);
        var max = Maximum ?? DateTime.MaxValue; max = new DateTime(max.Year, max.Month, DateTime.DaysInMonth(max.Year, max.Month));

        var today = DateTime.Today;
        for (var i = 1; i <= 12; i++)
        {
            var newDate = new DateTime(SelectedMonth.Year, i, 1);

            collection.Add(new StswCalendarItem
            {
                Content = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(i),
                Date = newDate,
                InMinMaxRange = newDate.Between(min, max),
                IsCurrentDay = newDate.Year == today.Year && newDate.Month == today.Month,
                IsSelected = SelectedDate?.Year == newDate.Year && SelectedDate?.Month == newDate.Month
            });
        }

        return collection;
    }

    /// <summary>
    /// Handles the selection of a day and sets the <see cref="SelectedDate"/> property.
    /// Closes the popup if applicable.
    /// </summary>
    /// <param name="date">The date to select.</param>
    private void SelectDay(DateTime? date)
    {
        if (date.HasValue)
        {
            SelectedDate = date.Value.Date;
            if (CurrentUnit == StswCalendarUnit.Months && Items.FirstOrDefault(x => x.Date == new DateTime(SelectedDate.Value.Year, SelectedDate.Value.Month, 1)) is StswCalendarItem newItem1)
                newItem1.IsSelected = true;
            else if (Items.FirstOrDefault(x => x.Date == SelectedDate) is StswCalendarItem newItem2)
                newItem2.IsSelected = true;
        }

        ClosePopupIfAny();
    }

    /// <summary>
    /// Handles the selection of a month and sets the <see cref="SelectedMonth"/> property.
    /// Updates the view or closes the popup depending on the current unit.
    /// </summary>
    /// <param name="date">The month to select.</param>
    private void SelectMonth(DateTime date)
    {
        SelectedMonth = new DateTime(date.Year, date.Month, 1);

        if (SelectionUnit == StswCalendarUnit.Months)
        {
            if (SelectedDate != SelectedMonth)
            {
                SelectedDate = SelectedMonth;
                if (!ClosePopupIfAny())
                    UpdateCalendarView();
            }
            else ClosePopupIfAny();

        }
        else if (SelectionUnit == StswCalendarUnit.Days && CurrentUnit != StswCalendarUnit.Days)
        {
            CurrentUnit = StswCalendarUnit.Days;
        }
    }

    /// <summary>
    /// Switches the currently displayed month in the control and updates the view.
    /// </summary>
    /// <param name="date">The new date to display.</param>
    private void SwitchMonth(DateTime date)
    {
        SelectedMonth = new DateTime(date.Year, date.Month, 1);
        UpdateCalendarView();
    }

    /// <summary>
    /// Updates the calendar view based on the current unit (days or months).
    /// Adjusts the header and regenerates the day or month buttons.
    /// </summary>
    private void UpdateCalendarView()
    {
        if (_isUpdatingView)
            return;

        _isUpdatingView = true;
        try
        {
            Items = CurrentUnit switch
            {
                StswCalendarUnit.Days => GenerateDays(),
                StswCalendarUnit.Months => GenerateMonths(),
                _ => []
            };
        }
        finally
        {
            _isUpdatingView = false;
        }
    }
    private bool _isUpdatingView;

    /// <summary>
    /// 
    /// </summary>
    private void UpdateCalendarViewName() => SelectedMonthName = CurrentUnit switch
    {
        StswCalendarUnit.Days => $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(SelectedMonth.Month).Capitalize()} {SelectedMonth.Year}",
        StswCalendarUnit.Months => SelectedMonth.Year.ToString(),
        _ => string.Empty
    };

    /// <summary>
    /// Validates and calculates a new date based on the current selected date and the number of months to add or subtract.
    /// Ensures the new date falls within the range defined by <see cref="Minimum"/> and <see cref="Maximum"/>.
    /// </summary>
    /// <param name="months">The number of months to add or subtract.</param>
    /// <returns>A validated <see cref="DateTime"/> within the allowable range.</returns>
    private DateTime ValidateSelectedMonth(int months)
    {
        var max = Maximum ?? DateTime.MaxValue;
        var min = Minimum ?? DateTime.MinValue;

        if (months > 0 && SelectedMonth > DateTime.MaxValue.AddMonths(-months))
            return max;
        if (months < 0 && SelectedMonth < DateTime.MinValue.AddMonths(-months))
            return min;

        var newDate = SelectedMonth.AddMonths(months);

        if (newDate > max)
            return max;
        if (newDate < min)
            return min;

        return newDate;
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the current unit of the control.
    /// </summary>
    public StswCalendarUnit CurrentUnit
    {
        get => (StswCalendarUnit)GetValue(CurrentUnitProperty);
        set => SetValue(CurrentUnitProperty, value);
    }
    public static readonly DependencyProperty CurrentUnitProperty
        = DependencyProperty.Register(
            nameof(CurrentUnit),
            typeof(StswCalendarUnit),
            typeof(StswCalendar),
            new PropertyMetadata(default(StswCalendarUnit), OnCurrentUnitChanged)
        );
    public static void OnCurrentUnitChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswCalendar stsw)
        {
            stsw.UpdateCalendarViewName();
            stsw.UpdateCalendarView();
        }
    }
    
    /// <summary>
    /// Gets or sets the collection of days or months displayed in the control.
    /// </summary>
    internal ObservableCollection<StswCalendarItem> Items
    {
        get => (ObservableCollection<StswCalendarItem>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }
    public static readonly DependencyProperty ItemsProperty
        = DependencyProperty.Register(
            nameof(Items),
            typeof(ObservableCollection<StswCalendarItem>),
            typeof(StswCalendar)
        );

    /// <summary>
    /// Gets or sets the maximum allowable date in the control.
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
            typeof(StswCalendar),
            new PropertyMetadata(default(DateTime?), OnMinMaxChanged)
        );

    /// <summary>
    /// Gets or sets the minimum allowable date in the control.
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
            typeof(StswCalendar),
            new PropertyMetadata(default(DateTime?), OnMinMaxChanged)
        );
    private static void OnMinMaxChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswCalendar stsw)
        {
            var min = stsw.Minimum ?? DateTime.MinValue;
            var max = stsw.Maximum ?? DateTime.MaxValue;

            /// check if selected date is allowed
            if (stsw.SelectedDate < min)
                stsw.SelectedDate = min;
            else if (stsw.SelectedDate > max)
                stsw.SelectedDate = max;

            /// today button visibility
            if (stsw._buttonToday != null)
                stsw._buttonToday.IsEnabled = DateTime.Today.Between(min, max);

            /// to update buttons (days or months based on current unit) visibilities
            if (stsw.CurrentUnit == StswCalendarUnit.Months)
            {
                min = new DateTime(min.Year, min.Month, 1);
                max = new DateTime(max.Year, max.Month, DateTime.DaysInMonth(max.Year, max.Month));
            }
            foreach (var item in stsw.Items)
                item.InMinMaxRange = item.Date.Between(min, max);
        }
    }
    
    /// <summary>
    /// Gets or sets the currently selected date in the control.
    /// </summary>
    public DateTime? SelectedDate
    {
        get => (DateTime?)GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }
    public static readonly DependencyProperty SelectedDateProperty
        = DependencyProperty.Register(
            nameof(SelectedDate),
            typeof(DateTime?),
            typeof(StswCalendar),
            new FrameworkPropertyMetadata(default(DateTime?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedDateChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnSelectedDateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswCalendar stsw)
        {
            /// without this, clicking on button with date from another month, will not change view to different (previous or next) month
            if (stsw.SelectedDate.HasValue)
                stsw.SelectedMonth = new DateTime(stsw.SelectedDate.Value.Year, stsw.SelectedDate.Value.Month, 1);

            /// marking new date in view (without this first date after loading calendar is not colored)
            if ((DateTime?)e.NewValue != (DateTime?)e.OldValue)
            {
                if (stsw.SelectedDate.HasValue && stsw.Items.FirstOrDefault(x => x.Date == ((DateTime?)e.NewValue)?.Date) is StswCalendarItem newItem)
                    newItem.IsSelected = true;
                else if (!stsw.SelectedDate.HasValue && stsw.Items.FirstOrDefault(x => x.Date == ((DateTime?)e.OldValue)?.Date) is StswCalendarItem oldItem)
                    oldItem.IsSelected = false;
            }

            /// event for non MVVM programming
            stsw.SelectedDateChanged?.Invoke(stsw, new StswValueChangedEventArgs<DateTime?>((DateTime?)e.OldValue, (DateTime?)e.NewValue));
        }
    }

    /// <summary>
    /// Gets or sets the currently displayed month in the control.
    /// </summary>
    public DateTime SelectedMonth
    {
        get => (DateTime)GetValue(SelectedMonthProperty);
        internal set => SetValue(SelectedMonthProperty, value);
    }
    public static readonly DependencyProperty SelectedMonthProperty
        = DependencyProperty.Register(
            nameof(SelectedMonth),
            typeof(DateTime),
            typeof(StswCalendar),
            new FrameworkPropertyMetadata(default(DateTime),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedMonthChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnSelectedMonthChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswCalendar stsw)
        {
            stsw.UpdateCalendarViewName();

            /// without this, clicking on button with date from another month, will not change view to different (previous or next) month
            if (stsw.CurrentUnit == StswCalendarUnit.Days && e.OldValue != e.NewValue)
                stsw.UpdateCalendarView();
        }
    }

    /// <summary>
    /// Gets or sets the currently displayed year and month name in the control.
    /// </summary>
    public string SelectedMonthName
    {
        get => (string)GetValue(SelectedMonthNameProperty);
        internal set => SetValue(SelectedMonthNameProperty, value);
    }
    public static readonly DependencyProperty SelectedMonthNameProperty
        = DependencyProperty.Register(
            nameof(SelectedMonthName),
            typeof(string),
            typeof(StswCalendar)
        );

    /// <summary>
    /// Gets or sets the selection unit of the control.
    /// </summary>
    public StswCalendarUnit SelectionUnit
    {
        get => (StswCalendarUnit)GetValue(SelectionUnitProperty);
        set => SetValue(SelectionUnitProperty, value);
    }
    public static readonly DependencyProperty SelectionUnitProperty
        = DependencyProperty.Register(
            nameof(SelectionUnit),
            typeof(StswCalendarUnit),
            typeof(StswCalendar),
            new PropertyMetadata(StswCalendarUnit.Days, OnSelectionUnitChanged)
        );
    private static void OnSelectionUnitChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswCalendar stsw)
        {
            /// because for selection unit by months, current unit cannot be switched between months and days unit, and can only stay at months unit
            if (e.NewValue is StswCalendarUnit stswCalendarUnit && stswCalendarUnit == StswCalendarUnit.Months)
                stsw.CurrentUnit = stswCalendarUnit;
        }
    }
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets a value indicating whether corner clipping is enabled for the control.
    /// When set to <see langword="true"/>, content within the control's border area is clipped to match
    /// the border's rounded corners, preventing elements from protruding beyond the border.
    /// </summary>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty
        = DependencyProperty.Register(
            nameof(CornerClipping),
            typeof(bool),
            typeof(StswCalendar),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the degree to which the corners of the control's border are rounded by defining
    /// a radius value for each corner independently. This property allows users to control the roundness
    /// of corners, and large radius values are smoothly scaled to blend from corner to corner.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswCalendar),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// Names for days of week
    public static string DayOfWeek1 => CultureInfo.CurrentCulture.DateTimeFormat.GetShortestDayName(CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek).Capitalize();
    public static string DayOfWeek2 => CultureInfo.CurrentCulture.DateTimeFormat.GetShortestDayName(CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek.GetNextValue(1)).Capitalize();
    public static string DayOfWeek3 => CultureInfo.CurrentCulture.DateTimeFormat.GetShortestDayName(CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek.GetNextValue(2)).Capitalize();
    public static string DayOfWeek4 => CultureInfo.CurrentCulture.DateTimeFormat.GetShortestDayName(CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek.GetNextValue(3)).Capitalize();
    public static string DayOfWeek5 => CultureInfo.CurrentCulture.DateTimeFormat.GetShortestDayName(CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek.GetNextValue(4)).Capitalize();
    public static string DayOfWeek6 => CultureInfo.CurrentCulture.DateTimeFormat.GetShortestDayName(CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek.GetNextValue(5)).Capitalize();
    public static string DayOfWeek7 => CultureInfo.CurrentCulture.DateTimeFormat.GetShortestDayName(CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek.GetNextValue(6)).Capitalize();
    #endregion
}

/// <summary>
/// Data model for <see cref="StswCalendar"/>'s items.
/// </summary>
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
