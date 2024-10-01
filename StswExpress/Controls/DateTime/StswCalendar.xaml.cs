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
/// A control with date selection functionality.
/// </summary>
[ContentProperty(nameof(SelectedDate))]
public class StswCalendar : Control, IStswCornerControl
{
    public ICommand SelectDayCommand { get; set; }
    public ICommand SelectMonthCommand { get; set; }

    public StswCalendar()
    {
        SetValue(ListOfDaysProperty, new ObservableCollection<StswCalendarItem>());
        SetValue(ListOfMonthsProperty, new ObservableCollection<StswCalendarItem>());

        SelectDayCommand = new StswCommand<DateTime?>(SelectDay);
        SelectMonthCommand = new StswCommand<DateTime>(SelectMonth);
    }
    static StswCalendar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswCalendar), new FrameworkPropertyMetadata(typeof(StswCalendar)));
    }

    #region Events & methods
    private ContentControl? _buttonCurrentMode;
    private ContentControl? _buttonToday;

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
            btnPreviousYear.Click += (_, _) => SwitchMonth(ValidateSelectedMonth(CurrentMode == StswCalendarMode.Months ? -120 : -12));
        /// Button: previous month
        if (GetTemplateChild("PART_ButtonPreviousMonth") is ButtonBase btnPreviousMonth)
            btnPreviousMonth.Click += (_, _) => SwitchMonth(ValidateSelectedMonth(CurrentMode == StswCalendarMode.Months ? -12 : -1));
        /// Button: current mode
        if (GetTemplateChild("PART_ButtonCurrentMode") is ButtonBase btnCurrentMode)
        {
            btnCurrentMode.Click += (_, _) => CurrentMode = CurrentMode.GetNextValue();
            _buttonCurrentMode = btnCurrentMode;
        }
        /// Button: next month
        if (GetTemplateChild("PART_ButtonNextMonth") is ButtonBase btnNextMonth)
            btnNextMonth.Click += (_, _) => SwitchMonth(ValidateSelectedMonth(CurrentMode == StswCalendarMode.Months ? 12 : 1));
        /// Button: next year
        if (GetTemplateChild("PART_ButtonNextYear") is ButtonBase btnNextYear)
            btnNextYear.Click += (_, _) => SwitchMonth(ValidateSelectedMonth(CurrentMode == StswCalendarMode.Months ? 120 : 12));
        /// Button: today
        if (GetTemplateChild("PART_ButtonToday") is ButtonBase btnToday)
        {
            btnToday.Click += (_, _) => { SelectMonth(DateTime.Now); SelectDay(DateTime.Now); };
            _buttonToday = btnToday;
        }

        /// set default month to create first view
        var defMonth = (SelectedDate ?? DateTime.Now).Date;
        SelectedMonth = new DateTime(defMonth.Year, defMonth.Month, 1);

        if (CurrentMode != SelectionMode)
            CurrentMode = SelectionMode;
        else
            OnCurrentModeChanged(this, new DependencyPropertyChangedEventArgs());
    }

    /// <summary>
    /// Generates the list of days to display in the control based on the selected month.
    /// </summary>
    private void MakeListOfDays()
    {
        if (ListOfDays.Count == 0)
        {
            var newList = new ObservableCollection<StswCalendarItem>();

            for (var i = 1; i <= 42; i++)
                newList.Add(new StswCalendarItem());

            ListOfDays = newList;
        }

        var middleDate = new DateTime(SelectedMonth.Year, SelectedMonth.Month, DateTime.DaysInMonth(SelectedMonth.Year, SelectedMonth.Month) / 2);
        while (middleDate.DayOfWeek != CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek)
            middleDate = middleDate.AddDays(1);

        for (var i = 0; i < 42; i++)
        {
            DateTime? newDate;
            try
            {
                newDate = middleDate.AddDays(i - 21);
            }
            catch
            {
                newDate = null;
            }

            ListOfDays[i] = new StswCalendarItem()
            {
                Date = newDate,
                Name = newDate?.Day.ToString(),
                InCurrentMonth = newDate.HasValue && (newDate.Value.Year == SelectedMonth.Year && newDate.Value.Month == SelectedMonth.Month),
                InMinMaxRange = newDate >= (Minimum ?? DateTime.MinValue) && newDate <= (Maximum ?? DateTime.MaxValue),
                IsCurrentDay = newDate == DateTime.Now.Date,
                IsSpecialDay = newDate?.DayOfWeek == DayOfWeek.Sunday,
                IsSelected = newDate == SelectedDate?.Date
            };
        }
    }

    /// <summary>
    /// Generates the list of months to display in the control based on the selected year.
    /// </summary>
    private void MakeListOfMonths()
    {
        if (ListOfMonths.Count == 0)
        {
            var newList = new ObservableCollection<StswCalendarItem>();

            for (var i = 1; i <= 12; i++)
                newList.Add(new StswCalendarItem());

            ListOfMonths = newList;
        }

        var max = Maximum ?? DateTime.MaxValue; max = new DateTime(max.Year, max.Month, DateTime.DaysInMonth(max.Year, max.Month));
        var min = Minimum ?? DateTime.MinValue; min = new DateTime(min.Year, min.Month, 1);

        for (var i = 0; i < 12; i++)
        {
            var newDate = new DateTime(SelectedMonth.Year, i + 1, 1);
            ListOfMonths[i] = new StswCalendarItem()
            {
                Date = newDate,
                Name = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(i + 1),
                InMinMaxRange = newDate.Between(min, max),
                IsCurrentDay = newDate.Year == DateTime.Now.Year && newDate.Month == DateTime.Now.Month,
                IsSelected = SelectedDate.HasValue && newDate == new DateTime(SelectedDate.Value.Year, SelectedDate.Value.Month, 1)
            };
        }
    }

    /// <summary>
    /// Validates and calculates a new date based on the current selected date and the number of months to add or subtract.
    /// Ensures the new date falls within the range defined by <see cref="Minimum"/> and <see cref="Maximum"/>.
    /// </summary>
    /// <param name="months">The number of months to add or subtract.</param>
    /// <returns>A validated <see cref="DateTime"/> within the allowable range.</returns>
    private DateTime ValidateSelectedMonth(int months)
    {
        /// try to add months to the selected date
        if ((months > 0 && DateTime.MaxValue.AddMonths(-months) > SelectedMonth)
         || (months < 0 && DateTime.MinValue.AddMonths(-months) < SelectedMonth))
        {
            var newDate = SelectedMonth.AddMonths(months);

            /// check if new date is within the Minimum and Maximum range
            var max = new DateTime(newDate.Year, newDate.Month, DateTime.DaysInMonth(newDate.Year, newDate.Month));
            var min = new DateTime(newDate.Year, newDate.Month, 1);

            if (months > 0 && max > Maximum)
                return Maximum.Value;
            else if (months < 0 && min < Minimum)
                return Minimum.Value;

            return newDate;
        }
        else
        {
            return months > 0 ? DateTime.MaxValue : DateTime.MinValue;
        }
    }

    /// <summary>
    /// Handles the selection of a day.
    /// </summary>
    /// <param name="date">The date to select.</param>
    private void SelectDay(DateTime? date)
    {
        SelectedDate = date?.Date;

        /// close the parent popup if applicable (e.g., when used in a DatePicker)
        if (StswFn.GetParentPopup(this) is Popup popup)
            popup.IsOpen = false;
    }

    /// <summary>
    /// Handles the selection of a month.
    /// </summary>
    /// <param name="date">The month to select.</param>
    private void SelectMonth(DateTime date)
    {
        SelectedMonth = new DateTime(date.Year, date.Month, 1);

        if (SelectionMode == StswCalendarMode.Months)
        {
            SelectedDate = SelectedMonth;

            /// close the parent popup if applicable (e.g., when used in a DatePicker)
            if (StswFn.GetParentPopup(this) is Popup popup)
                popup.IsOpen = false;
        }
        else if (SelectionMode == StswCalendarMode.Days && CurrentMode != StswCalendarMode.Days)
            CurrentMode = StswCalendarMode.Days;
        else
            OnCurrentModeChanged(this, new DependencyPropertyChangedEventArgs());
    }

    /// <summary>
    /// Switches the currently displayed month in the control.
    /// </summary>
    /// <param name="date">The new date to display.</param>
    private void SwitchMonth(DateTime date)
    {
        SelectedMonth = new DateTime(date.Year, date.Month, 1);
        OnCurrentModeChanged(this, new DependencyPropertyChangedEventArgs());
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the current mode of the control.
    /// </summary>
    public StswCalendarMode CurrentMode
    {
        get => (StswCalendarMode)GetValue(CurrentModeProperty);
        set => SetValue(CurrentModeProperty, value);
    }
    public static readonly DependencyProperty CurrentModeProperty
        = DependencyProperty.Register(
            nameof(CurrentMode),
            typeof(StswCalendarMode),
            typeof(StswCalendar),
            new PropertyMetadata(default(StswCalendarMode), OnCurrentModeChanged)
        );
    public static void OnCurrentModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswCalendar stsw)
        {
            if (stsw.CurrentMode == StswCalendarMode.Days)
            {
                if (stsw._buttonCurrentMode != null)
                    stsw._buttonCurrentMode.Content = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(stsw.SelectedMonth.Month).Capitalize()} {stsw.SelectedMonth.Year}";

                stsw.MakeListOfDays();
            }
            else if (stsw.CurrentMode == StswCalendarMode.Months)
            {
                if (stsw._buttonCurrentMode != null)
                    stsw._buttonCurrentMode.Content = stsw.SelectedMonth.Year.ToString();

                stsw.MakeListOfMonths();
            }
        }
    }

    /// <summary>
    /// Gets or sets the collection of days displayed in the control.
    /// </summary>
    internal ObservableCollection<StswCalendarItem> ListOfDays
    {
        get => (ObservableCollection<StswCalendarItem>)GetValue(ListOfDaysProperty);
        set => SetValue(ListOfDaysProperty, value);
    }
    public static readonly DependencyProperty ListOfDaysProperty
        = DependencyProperty.Register(
            nameof(ListOfDays),
            typeof(ObservableCollection<StswCalendarItem>),
            typeof(StswCalendar)
        );

    /// <summary>
    /// Gets or sets the collection of months displayed in the control.
    /// </summary>
    internal ObservableCollection<StswCalendarItem> ListOfMonths
    {
        get => (ObservableCollection<StswCalendarItem>)GetValue(ListOfMonthsProperty);
        set => SetValue(ListOfMonthsProperty, value);
    }
    public static readonly DependencyProperty ListOfMonthsProperty
        = DependencyProperty.Register(
            nameof(ListOfMonths),
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
            if (stsw._buttonToday != null)
                stsw._buttonToday.IsEnabled = DateTime.Today.Between(stsw.Minimum ?? DateTime.MinValue, stsw.Maximum ?? DateTime.MaxValue);

            OnCurrentModeChanged(obj, e);
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
            foreach (var day in stsw.ListOfDays)
                day.IsSelected = day.Date == stsw.SelectedDate;

            if (stsw.SelectedDate.HasValue)
                stsw.SelectedMonth = new DateTime(stsw.SelectedDate.Value.Year, stsw.SelectedDate.Value.Month, 1);

            stsw.SelectedDateChanged?.Invoke(stsw, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Gets or sets the currently displayed month in the control.
    /// </summary>
    internal DateTime SelectedMonth
    {
        get => (DateTime)GetValue(SelectedMonthProperty);
        set => SetValue(SelectedMonthProperty, value);
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
            foreach (var month in stsw.ListOfMonths)
                month.IsSelected = month.Date == stsw.SelectedMonth;

            if (stsw.CurrentMode == StswCalendarMode.Days && e.OldValue != e.NewValue)
                OnCurrentModeChanged(stsw, new DependencyPropertyChangedEventArgs());
        }
    }

    /// <summary>
    /// Gets or sets the selection mode of the control.
    /// </summary>
    public StswCalendarMode SelectionMode
    {
        get => (StswCalendarMode)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }
    public static readonly DependencyProperty SelectionModeProperty
        = DependencyProperty.Register(
            nameof(SelectionMode),
            typeof(StswCalendarMode),
            typeof(StswCalendar),
            new PropertyMetadata(StswCalendarMode.Days, OnSelectionModeChanged)
        );
    private static void OnSelectionModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswCalendar stsw)
        {
            if (e.NewValue is StswCalendarMode stswCalendarMode && stswCalendarMode == StswCalendarMode.Months)
                stsw.CurrentMode = stswCalendarMode;
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

    /// <summary>
    /// Gets or sets the thickness of the buttons in the control.
    /// </summary>
    public Thickness ItemBorderThickness
    {
        get => (Thickness)GetValue(ItemBorderThicknessProperty);
        set => SetValue(ItemBorderThicknessProperty, value);
    }
    public static readonly DependencyProperty ItemBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(ItemBorderThickness),
            typeof(Thickness),
            typeof(StswCalendar),
            new FrameworkPropertyMetadata(default(Thickness), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the degree to which the corners of the buttons in the control are rounded.
    /// </summary>
    public CornerRadius ItemCornerRadius
    {
        get => (CornerRadius)GetValue(ItemCornerRadiusProperty);
        set => SetValue(ItemCornerRadiusProperty, value);
    }
    public static readonly DependencyProperty ItemCornerRadiusProperty
        = DependencyProperty.Register(
            nameof(ItemCornerRadius),
            typeof(CornerRadius),
            typeof(StswCalendar),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the margin of the days/months group in the control.
    /// </summary>
    public Thickness ItemPadding
    {
        get => (Thickness)GetValue(ItemPaddingProperty);
        set => SetValue(ItemPaddingProperty, value);
    }
    public static readonly DependencyProperty ItemPaddingProperty
        = DependencyProperty.Register(
            nameof(ItemPadding),
            typeof(Thickness),
            typeof(StswCalendar),
            new FrameworkPropertyMetadata(default(Thickness), FrameworkPropertyMetadataOptions.AffectsMeasure)
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
/// Data model for StswCalendar's month items.
/// </summary>
internal class StswCalendarItem : StswObservableObject
{
    /// <summary>
    /// Gets the date associated with the calendar item.
    /// </summary>
    public DateTime? Date { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether the calendar item is within the selected month.
    /// </summary>
    public bool? InCurrentMonth { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether the calendar item is within the allowable date range.
    /// </summary>
    public bool InMinMaxRange
    {
        get => _inMinMaxRange;
        internal set => SetProperty(ref _inMinMaxRange, value);
    }
    private bool _inMinMaxRange;

    /// <summary>
    /// Gets a value indicating whether the calendar item represents the current day.
    /// </summary>
    public bool IsCurrentDay { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether the calendar item represents the special day (e.g., sunday).
    /// </summary>
    public bool IsSpecialDay { get; internal set; }

    /// <summary>
    /// Gets or sets the display name of the calendar item (e.g., day of the month).
    /// </summary>
    public string? Name { get; internal set; }

    /// <summary>
    /// Gets or sets a value indicating whether the calendar item is the selected date.
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        internal set => SetProperty(ref _isSelected, value);
    }
    private bool _isSelected;
}
