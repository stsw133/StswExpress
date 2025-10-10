using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// Represents a custom calendar control with date selection functionality.
/// Supports navigation between months and years, selecting individual days or months, 
/// and setting minimum/maximum date ranges. Includes quick selection of the current date 
/// and UI customization options.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswCalendar SelectedDate="{Binding SelectedMonth}" SelectionUnit="Months" Maximum="2025-12-31"/&gt;
/// </code>
/// </example>
[ContentProperty(nameof(SelectedDate))]
public class StswCalendar : Control, IStswCornerControl
{
    private ContentControl? _buttonClear, _buttonToday;
    private Selector? _selector;

    public StswCalendar()
    {
        SetValue(ItemsProperty, new ObservableCollection<StswCalendarItem>());
    }
    static StswCalendar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswCalendar), new FrameworkPropertyMetadata(typeof(StswCalendar)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        AttachNavigationButton("PART_ButtonPreviousYear", -12, -120);
        AttachNavigationButton("PART_ButtonPreviousMonth", -1, -12);
        AttachNavigationButton("PART_ButtonNextMonth", 1, 12);
        AttachNavigationButton("PART_ButtonNextYear", 12, 120);
        AttachCurrentUnitButton();
        AttachSelector();
        AttachTodayButton();
        AttachClearButton();

        SelectedMonth = (SelectedDate ?? DateTime.Now).Date.ToFirstDayOfMonth();

        StswBindingWatcher.WatchBindingAssignment(this, SelectedDateProperty, SetClearButtonVisibility);
        SetClearButtonVisibility();
        UpdateTodayButtonState();
    }

    /// <summary>
    /// Updates the enabled state of the "Today" button based on the current date and the defined minimum and maximum date range.
    /// </summary>
    /// <param name="partName">The name of the button part in the control template.</param>
    /// <param name="daysUnitMonths">Number of months to navigate when in days view.</param>
    /// <param name="monthsUnitMonths">Number of months to navigate when in months view.</param>
    private void AttachNavigationButton(string partName, int daysUnitMonths, int monthsUnitMonths)
    {
        if (GetTemplateChild(partName) is ButtonBase button)
            button.Click += (_, _) => NavigateCalendar(daysUnitMonths, monthsUnitMonths);
    }

    /// <summary>
    /// Attaches the event handler to the button that switches between day and month views.
    /// </summary>
    private void AttachCurrentUnitButton()
    {
        if (GetTemplateChild("PART_ButtonCurrentUnit") is ButtonBase button)
            button.Click += (_, _) => CurrentUnit = CurrentUnit.GetNextValue();
    }

    /// <summary>
    /// Attaches event handlers to the calendar selector for handling date selection and navigation.
    /// </summary>
    private void AttachSelector()
    {
        if (_selector != null)
        {
            _selector.PreviewMouseLeftButtonDown -= Selector_PreviewMouseLeftButtonDown;
            _selector.SelectionChanged -= Selector_SelectionChanged;
        }

        if (GetTemplateChild("PART_Selector") is Selector selector)
        {
            _selector = selector;
            selector.PreviewMouseLeftButtonDown += Selector_PreviewMouseLeftButtonDown;
            selector.SelectionChanged += Selector_SelectionChanged;
        }
    }

    /// <summary>
    /// Attaches the event handler to the "Today" button, allowing users to quickly select the current date.
    /// </summary>
    private void AttachTodayButton()
    {
        if (GetTemplateChild("PART_ButtonToday") is ButtonBase button)
        {
            button.Click += (_, _) => SelectDay(DateTime.Today);
            _buttonToday = button;
        }
    }

    /// <summary>
    /// Attaches the event handler to the "Clear" button, allowing users to clear the selected date.
    /// </summary>
    private void AttachClearButton()
    {
        if (GetTemplateChild("PART_ButtonClear") is ButtonBase button)
        {
            button.Click += (_, _) => SelectDay(null);
            _buttonClear = button;
        }
    }

    /// <summary>
    /// Updates the enabled state of the "Today" button based on the current date and the defined minimum and maximum date range.
    /// </summary>
    /// <param name="daysUnitMonths">Number of months to navigate when in days view.</param>
    /// <param name="monthsUnitMonths">Number of months to navigate when in months view.</param>
    private void NavigateCalendar(int daysUnitMonths, int monthsUnitMonths)
    {
        var months = CurrentUnit == StswCalendarUnit.Months ? monthsUnitMonths : daysUnitMonths;
        SwitchMonth(ValidateSelectedMonth(months));
    }

    /*
    /// <inheritdoc/>
    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        if (IsKeyboardFocusWithin)
        SelectMonth(SelectedMonth.AddMonths(e.Delta > 0 ? 1 : -1));
    }
    */

    /// <summary>
    /// Handles the mouse left button down event on the calendar selector.
    /// Determines whether the clicked date should be selected or whether the month should be switched.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The mouse button event arguments.</param>
    private void Selector_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is ItemsControl itemsControl
         && ItemsControl.ContainerFromElement(itemsControl, e.OriginalSource as DependencyObject) is ContentControl container
         && container.Content is StswCalendarItem calendarItem)
        {
            var date = calendarItem.Date;
            if (date == SelectedDate?.Date && CurrentUnit == SelectionUnit)
            {
                SelectDay(date);
                return;
            }

            if (CurrentUnit == StswCalendarUnit.Days && date.HasValue
             && date.Value.ToFirstDayOfMonth() != SelectedMonth)
            {
                SwitchMonth(date.Value.ToFirstDayOfMonth());
                e.Handled = true;
                return;
            }

            if (CurrentUnit == StswCalendarUnit.Months && SelectionUnit != StswCalendarUnit.Months && date.HasValue)
            {
                SelectMonth(date.Value);
                e.Handled = true;
                return;
            }
        }
    }

    /// <summary>
    /// Handles the selection change event in the calendar selector.
    /// Updates the selected date if a new date is chosen.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The selection changed event arguments.</param>
    private void Selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is Control control && !control.IsLoaded)
            return;

        foreach (var item in e.AddedItems)
        {
            if (item is StswCalendarItem calendarItem)
            {
                if (CurrentUnit == StswCalendarUnit.Days && calendarItem.InCurrentMonth != true)
                    continue;

                var date = calendarItem.Date;
                if (date != SelectedDate?.Date && CurrentUnit == SelectionUnit)
                    SelectDay(date);
            }
        }
    }

    /// <summary>
    /// Closes the parent popup if the control is contained within one.
    /// </summary>
    /// <returns><see langword="true"/> if the popup was found and closed; otherwise, <see langword="false"/>.</returns>
    private bool ClosePopupIfAny()
    {
        if (StswFnUI.GetParentPopup(this) is Popup popup)
        {
            popup.IsOpen = false;
            if (StswFnUI.FindVisualAncestor<StswDatePicker>(popup) is StswDatePicker stsw)
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
        var culture = CultureInfo.CurrentCulture.DateTimeFormat;
        var middleDate = new DateTime(SelectedMonth.Year, SelectedMonth.Month, 15);
        var offset = ((int)culture.FirstDayOfWeek - (int)middleDate.DayOfWeek + 7) % 7;
        var startTicks = middleDate.AddDays(offset - 21).Ticks;

        var min = Minimum ?? DateTime.MinValue;
        var max = Maximum ?? DateTime.MaxValue;
        var today = DateTime.Today;

        for (var i = 0; i < 42; i++)
        {
            var ticks = startTicks + TimeSpan.TicksPerDay * i;
            DateTime? date = (ticks >= DateTime.MinValue.Ticks && ticks <= DateTime.MaxValue.Ticks) ? new DateTime(ticks) : null;

            collection.Add(new StswCalendarItem
            {
                Content = date?.Day.ToString(),
                Date = date,
                InCurrentMonth = date?.Year == SelectedMonth.Year && date?.Month == SelectedMonth.Month,
                InMinMaxRange = date >= min && date <= max,
                IsCurrentDay = date == today,
                IsSpecialDay = date?.DayOfWeek == DayOfWeek.Sunday,
                IsSelected = date == SelectedDate?.Date
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
        var min = (Minimum ?? DateTime.MinValue).ToFirstDayOfMonth();
        var max = (Maximum ?? DateTime.MaxValue).ToLastDayOfMonth();
        var today = DateTime.Today;
        var format = CultureInfo.CurrentCulture.DateTimeFormat;

        for (var i = 1; i <= 12; i++)
        {
            var date = new DateTime(SelectedMonth.Year, i, 1);

            collection.Add(new StswCalendarItem
            {
                Content = format.GetAbbreviatedMonthName(i),
                Date = date,
                InMinMaxRange = date.Between(min, max),
                IsCurrentDay = date.Year == today.Year && date.Month == today.Month,
                IsSelected = SelectedDate?.Year == date.Year && SelectedDate?.Month == date.Month
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
        if (!date.HasValue)
        {
            SelectedDate = null;
            ClosePopupIfAny();
            return;
        }

        if (SelectionUnit == StswCalendarUnit.Months)
        {
            SelectMonth(date.Value);
            return;
        }

        var targetMonth = date.Value.ToFirstDayOfMonth();
        if (SelectedMonth != targetMonth)
            SelectMonth(date.Value);

        var targetDate = date.Value.Date;
        if (SelectedDate != targetDate)
            SelectedDate = targetDate;

        ClosePopupIfAny();
    }

    /// <summary>
    /// Handles the selection of a month and sets the <see cref="SelectedMonth"/> property.
    /// Updates the view or closes the popup depending on the current unit.
    /// </summary>
    /// <param name="date">The month to select.</param>
    private void SelectMonth(DateTime date)
    {
        var targetMonth = date.ToFirstDayOfMonth();
        if (SelectedMonth != targetMonth)
            SelectedMonth = targetMonth;

        if (SelectionUnit == StswCalendarUnit.Months)
        {
            if (SelectedDate != targetMonth)
            {
                SelectedDate = targetMonth;
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
    /// Updates the visibility of the clear button based on the binding type of <see cref="SelectedDate"/>.
    /// The button is only visible if <see cref="SelectedDate"/> is bound to a nullable <see cref="DateTime"/> property.
    /// </summary>
    private void SetClearButtonVisibility()
    {
        if (_buttonClear != null && GetBindingExpression(SelectedDateProperty) is BindingExpression binding)
        {
            var dataItem = binding.ResolvedSource;
            var propertyName = binding.ParentBinding.Path.Path;

            if (dataItem != null && !string.IsNullOrEmpty(propertyName))
            {
                var propertyInfo = dataItem.GetType().GetProperty(propertyName);
                _buttonClear.Visibility = propertyInfo?.PropertyType == typeof(DateTime?) ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }

    /// <summary>
    /// Updates the enabled state of the "Today" button based on the current date and the defined minimum and maximum date range.
    /// </summary>
    private void UpdateTodayButtonState()
    {
        if (_buttonToday == null)
            return;

        var min = Minimum ?? DateTime.MinValue;
        var max = Maximum ?? DateTime.MaxValue;
        _buttonToday.IsEnabled = DateTime.Today.Between(min, max);
    }

    /// <summary>
    /// Switches the currently displayed month in the control and updates the view.
    /// </summary>
    /// <param name="date">The new date to display.</param>
    private void SwitchMonth(DateTime date) => SelectedMonth = date.ToFirstDayOfMonth();

    /// <summary>
    /// Updates the calendar view based on the current unit (Days or Months).
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

            UpdateItemSelection(SelectedDate);
        }
        finally
        {
            _isUpdatingView = false;
        }
    }
    private bool _isUpdatingView;

    /// <summary>
    /// Updates the selection state of calendar items based on the provided date.
    /// </summary>
    /// <param name="date">The date to match for selection.</param>
    private void UpdateItemSelection(DateTime? date)
    {
        if (Items is null)
            return;

        var targetDate = date.HasValue
            ? (SelectionUnit == StswCalendarUnit.Months ? date.Value.ToFirstDayOfMonth() : date.Value.Date)
            : (DateTime?)null;

        foreach (var item in Items)
            item.IsSelected = item.Date == targetDate;
    }

    /// <summary>
    /// Synchronizes the <see cref="SelectedMonth"/> property with the month of the provided date.
    /// </summary>
    /// <param name="date">The date to synchronize with.</param>
    private void SyncSelectedMonthToDate(DateTime? date)
    {
        if (!date.HasValue)
            return;

        var targetMonth = date.Value.ToFirstDayOfMonth();
        if (SelectedMonth != targetMonth)
            SelectedMonth = targetMonth;
    }

    /// <summary>
    /// Updates the display name of the currently selected month or year based on the current view mode.
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
        var min = Minimum ?? DateTime.MinValue;
        var max = Maximum ?? DateTime.MaxValue;
        var candidate = AddMonthsSafely(SelectedMonth, months);
        return Clamp(candidate, min, max);
    }

    /// <summary>
    /// Adds a specified number of months to a given date, ensuring the result does not exceed the bounds of <see cref="DateTime.MinValue"/> and <see cref="DateTime.MaxValue"/>.
    /// </summary>
    /// <param name="date">The original date to which months will be added.</param>
    /// <param name="months">The number of months to add (can be negative to subtract).</param>
    /// <returnsA <see cref="DateTime"/> that is the result of adding the specified number of months, clamped within valid date range.</returns>
    private static DateTime AddMonthsSafely(DateTime date, int months)
    {
        if (months == 0)
            return date;

        if (months > 0 && date > DateTime.MaxValue.AddMonths(-months))
            return DateTime.MaxValue;
        if (months < 0 && date < DateTime.MinValue.AddMonths(-months))
            return DateTime.MinValue;

        return date.AddMonths(months);
    }

    /// <summary>
    /// Clamps a given date to ensure it falls within the specified minimum and maximum bounds.
    /// </summary>
    /// <param name="value">The date to be clamped.</param>
    /// <param name="min">The minimum allowable date.</param>
    /// <param name="max">The maximum allowable date.</param>
    /// <returnsA <see cref="DateTime"/> that is within the specified range.</returns>
    private static DateTime Clamp(DateTime value, DateTime min, DateTime max)
    {
        if (value < min)
            return min;
        if (value > max)
            return max;
        return value;
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the current view mode of the calendar (days or months).
    /// Controls how the calendar displays and interacts with date selection.
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
        if (obj is not StswCalendar stsw)
            return;

        stsw.UpdateCalendarViewName();
        stsw.UpdateCalendarView();
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
        if (obj is not StswCalendar stsw)
            return;

        var min = stsw.Minimum ?? DateTime.MinValue;
        var max = stsw.Maximum ?? DateTime.MaxValue;
        var selectionUnit = stsw.SelectionUnit;

        /// check if selected date is allowed
        if (stsw.SelectedDate.HasValue)
        {
            var adjustedMin = selectionUnit == StswCalendarUnit.Months ? min.ToFirstDayOfMonth() : min;
            var adjustedMax = selectionUnit == StswCalendarUnit.Months ? max.ToFirstDayOfMonth() : max;
            var clamped = Clamp(stsw.SelectedDate.Value, adjustedMin, adjustedMax);
            if (clamped != stsw.SelectedDate.Value)
                stsw.SelectedDate = clamped;
        }

        stsw.UpdateTodayButtonState();

        /// to update buttons (days or months based on current unit) visibilities
        if (stsw.Items is { } items)
        {
            var rangeMin = stsw.CurrentUnit == StswCalendarUnit.Months ? min.ToFirstDayOfMonth() : min;
            var rangeMax = stsw.CurrentUnit == StswCalendarUnit.Months ? max.ToLastDayOfMonth() : max;

            foreach (var item in items)
                item.InMinMaxRange = item.Date.Between(rangeMin, rangeMax);
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
        if (obj is not StswCalendar stsw)
            return;

        stsw.SyncSelectedMonthToDate(stsw.SelectedDate);
        stsw.UpdateItemSelection(stsw.SelectedDate);
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
        if (obj is not StswCalendar stsw)
            return;

        var oldValue = (DateTime)e.OldValue;
        var newValue = (DateTime)e.NewValue;

        if ((stsw.CurrentUnit == StswCalendarUnit.Days && (oldValue.Year != newValue.Year || oldValue.Month != newValue.Month))
         || (stsw.CurrentUnit == StswCalendarUnit.Months && oldValue.Year != newValue.Year))
        {
            stsw.UpdateCalendarViewName();
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
    /// Gets or sets the unit used for date selection.
    /// Determines whether the calendar selects individual days or whole months.
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
        if (obj is not StswCalendar stsw)
            return;

        /// for Months selection unit, only Months view is available
        if (e.NewValue is StswCalendarUnit stswCalendarUnit && stswCalendarUnit == StswCalendarUnit.Months)
            stsw.CurrentUnit = stswCalendarUnit;
    }
    #endregion

    #region Style properties
    /// <inheritdoc/>
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

    /// <inheritdoc/>
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
