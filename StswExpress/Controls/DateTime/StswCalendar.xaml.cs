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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StswExpress;
/// <summary>
/// A control with date selection functionality.
/// </summary>
[ContentProperty(nameof(SelectedDate))]
public class StswCalendar : Control, IStswCornerControl
{
    public ICommand SelectDateCommand { get; set; }
    public ICommand SelectMonthCommand { get; set; }

    public StswCalendar()
    {
        SetValue(ListDaysProperty, new ObservableCollection<StswCalendarDay>());
        SetValue(ListMonthsProperty, new ObservableCollection<StswCalendarMonth>());

        SelectDateCommand = new StswCommand<DateTime?>(SelectDate);
        SelectMonthCommand = new StswCommand<int>(SelectMonth);
    }
    static StswCalendar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswCalendar), new FrameworkPropertyMetadata(typeof(StswCalendar)));
    }

    #region Events & methods
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

        CurrentMode = SelectionMode;

        /// Button: previous year
        if (GetTemplateChild("PART_ButtonPreviousYear") is ButtonBase btnPreviousYear)
            btnPreviousYear.Click += (_, _) => SelectedMonth = ValidateSelectedMonth(CurrentMode == StswCalendarMode.ByYear ? -120 : -12);
        /// Button: previous month
        if (GetTemplateChild("PART_ButtonPreviousMonth") is ButtonBase btnPreviousMonth)
            btnPreviousMonth.Click += (_, _) => SelectedMonth = ValidateSelectedMonth(CurrentMode == StswCalendarMode.ByYear ? -12 : -1);
        /// Button: current mode
        if (GetTemplateChild("PART_ButtonCurrentMode") is ButtonBase btnCurrentMode)
            btnCurrentMode.Click += (_, _) => CurrentMode = CurrentMode.GetNextValue();
        /// Button: next month
        if (GetTemplateChild("PART_ButtonNextMonth") is ButtonBase btnNextMonth)
            btnNextMonth.Click += (_, _) => SelectedMonth = ValidateSelectedMonth(CurrentMode == StswCalendarMode.ByYear ? 12 : 1);
        /// Button: next year
        if (GetTemplateChild("PART_ButtonNextYear") is ButtonBase btnNextYear)
            btnNextYear.Click += (_, _) => SelectedMonth = ValidateSelectedMonth(CurrentMode == StswCalendarMode.ByYear ? 120 : 12);
        /// Button: today
        if (GetTemplateChild("PART_ButtonToday") is ButtonBase btnToday)
            btnToday.Click += (_, _) => { SelectedMonth = DateTime.Now; SelectDate(DateTime.Now); };

        SelectedMonth = SelectedDate ?? DateTime.Now.Date;
        OnSelectedDateChanged(this, new DependencyPropertyChangedEventArgs());
    }

    /// <summary>
    /// 
    /// </summary>
    private void MakeListDays()
    {
        var newList = new ObservableCollection<StswCalendarDay>();
        
        for (int i = 1; i <= 42; i++)
            newList.Add(new StswCalendarDay());

        ListDays = newList;
    }

    /// <summary>
    /// 
    /// </summary>
    private void MakeListMonths()
    {
        var newList = new ObservableCollection<StswCalendarMonth>();
        
        for (int i = 1; i <= 12; i++)
            newList.Add(new StswCalendarMonth()
            {
                Month = i,
                Name = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(i)
            });

        ListMonths = newList;
    }

    /// <summary>
    /// Checks and calculates a new date based on the current selected date and the number of months to add or subtract.
    /// Ensures the new date falls within the range defined by <see cref="Minimum"/> and <see cref="Maximum"/>.
    /// </summary>
    private DateTime ValidateSelectedMonth(int months)
    {
        /// try add months to selected date
        if ((months > 0 && DateTime.MaxValue.AddMonths(-months) > SelectedMonth)
         || (months < 0 && DateTime.MinValue.AddMonths(-months) < SelectedMonth))
        {
            var newDate = SelectedMonth.AddMonths(months);

            /// check if new date is between range of Minimum and Maximum
            var max = new DateTime(newDate.Year, newDate.Month, DateTime.DaysInMonth(newDate.Year, newDate.Month));
            var min = new DateTime(newDate.Year, newDate.Month, 1);

            if (months > 0 && max > Maximum)
                return Maximum.Value;
            else if (months < 0 && min < Minimum)
                return Minimum.Value;

            return newDate;
        }
        else return months > 0 ? DateTime.MaxValue : DateTime.MinValue;
    }

    /// Command: select month
    /// <summary>
    /// Handles the execution of the select month command in the calendar control.
    /// Updates the selected month based on the provided date - it changes the selected month to the specified date's month.
    /// </summary>
    private void SelectMonth(int month)
    {
        SelectedMonth = new DateTime(SelectedMonth.Year, month, 1);

        if (SelectionMode == StswCalendarMode.ByMonth)
        {
            CurrentMode = StswCalendarMode.ByMonth;
        }
        else
        {
            SelectedDate = SelectedMonth;

            /// for DatePicker
            if (StswFn.GetParentPopup(this) is Popup popup)
                popup.IsOpen = false;
        }
    }

    /// Command: select date
    /// <summary>
    /// Handles the execution of the select date command in the calendar control.
    /// Updates the selected date based on the provided date - it sets the SelectedDate property and updates the SelectedMonth to match the selected date's month.
    /// Also, closes any open popups that contain the calendar control.
    /// </summary>
    private void SelectDate(DateTime? date)
    {
        if (date.Between(Minimum ?? DateTime.MinValue, Maximum ?? DateTime.MaxValue))
            SelectedDate = date;

        /// for DatePicker
        if (StswFn.GetParentPopup(this) is Popup popup)
            popup.IsOpen = false;
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
            new PropertyMetadata(default(StswCalendarMode), OnSelectedMonthChanged)
        );

    /// <summary>
    /// Gets or sets the collection of days displayed in the control.
    /// </summary>
    internal ObservableCollection<StswCalendarDay> ListDays
    {
        get => (ObservableCollection<StswCalendarDay>)GetValue(ListDaysProperty);
        set => SetValue(ListDaysProperty, value);
    }
    public static readonly DependencyProperty ListDaysProperty
        = DependencyProperty.Register(
            nameof(ListDays),
            typeof(ObservableCollection<StswCalendarDay>),
            typeof(StswCalendar)
        );

    /// <summary>
    /// Gets or sets the collection of months displayed in the control.
    /// </summary>
    internal ObservableCollection<StswCalendarMonth> ListMonths
    {
        get => (ObservableCollection<StswCalendarMonth>)GetValue(ListMonthsProperty);
        set => SetValue(ListMonthsProperty, value);
    }
    public static readonly DependencyProperty ListMonthsProperty
        = DependencyProperty.Register(
            nameof(ListMonths),
            typeof(ObservableCollection<StswCalendarMonth>),
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
            new PropertyMetadata(default(DateTime?), OnSelectedMonthChanged)
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
            new PropertyMetadata(default(DateTime?), OnSelectedMonthChanged)
        );

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
            if (stsw.SelectedDate.HasValue)
                stsw.SelectedMonth = new DateTime(stsw.SelectedDate.Value.Year, stsw.SelectedDate.Value.Month, 1);

            if (stsw.ListDays.FirstOrDefault(x => x.Date == ((DateTime?)e.OldValue)?.Date) is StswCalendarDay oldDay)
                oldDay.IsSelected = false;
            if (stsw.ListDays.FirstOrDefault(x => x.Date == stsw.SelectedDate?.Date) is StswCalendarDay newDay)
                newDay.IsSelected = true;

            /*
            foreach (var month in stsw.ListMonths)
                month.IsSelected = false;
            if (stsw.ListMonths.FirstOrDefault(x => x.Date == stsw.SelectedDate?.Date) is StswCalendarMonth newMonth && stsw.SelectionMode == StswCalendarMode.ByYear)
                newMonth.IsSelected = true;
            */

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
            if (e.Property.Name.In(nameof(Minimum), nameof(Maximum)))
            {
                if (stsw.GetTemplateChild("PART_ButtonToday") is ButtonBase buttonToday)
                    buttonToday.IsEnabled = DateTime.Today.Between(stsw.Minimum ?? DateTime.MinValue, stsw.Maximum ?? DateTime.MaxValue);
            }

            if (stsw.CurrentMode == StswCalendarMode.ByYear)
            {
                if (stsw.ListMonths.Count == 0)
                    stsw.MakeListMonths();

                /// display year
                stsw.SelectionName = stsw.SelectedMonth.Year.ToString();

                /// check range
                var max = stsw.Maximum ?? DateTime.MaxValue; max = new DateTime(max.Year, max.Month, DateTime.DaysInMonth(max.Year, max.Month));
                var min = stsw.Minimum ?? DateTime.MinValue; min = new DateTime(min.Year, min.Month, 1);

                foreach (var month in stsw.ListMonths)
                {
                    month.Date = new DateTime(stsw.SelectedMonth.Year, month.Month, 1);
                    month.InMinMaxRange = month.Date.Between(min, max);
                }
            }
            else if (stsw.CurrentMode == StswCalendarMode.ByMonth)
            {
                if (stsw.ListDays.Count == 0)
                    stsw.MakeListDays();

                /// display month and year
                stsw.SelectionName = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(stsw.SelectedMonth.Month).Capitalize()} {stsw.SelectedMonth.Year}";

                /// calculate date and change buttons of each day
                var middleDate = new DateTime(stsw.SelectedMonth.Year, stsw.SelectedMonth.Month, DateTime.DaysInMonth(stsw.SelectedMonth.Year, stsw.SelectedMonth.Month) / 2);
                while (middleDate.DayOfWeek != CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek)
                    middleDate = middleDate.AddDays(1);

                for (int i = 0; i < 42; i++)
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

                    stsw.ListDays[i] = new StswCalendarDay()
                    {
                        Date = newDate,
                        Month = stsw.SelectedMonth.Month,
                        Name = newDate?.Day.ToString(),
                        InMinMaxRange = newDate >= (stsw.Minimum ?? DateTime.MinValue) && newDate <= (stsw.Maximum ?? DateTime.MaxValue),
                        IsSelected = newDate == stsw.SelectedDate
                    };
                }
            }
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
            new PropertyMetadata(StswCalendarMode.ByMonth, OnSelectionModeChanged)
        );
    private static void OnSelectionModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswCalendar stsw)
        {
            if (e.NewValue is StswCalendarMode stswCalendarMode && stswCalendarMode == StswCalendarMode.ByYear)
                stsw.CurrentMode = stswCalendarMode;
        }
    }

    /// <summary>
    /// Gets the name of the current selection (year or month) for display purposes.
    /// </summary>
    internal string SelectionName
    {
        get => (string)GetValue(SelectionNameProperty);
        set => SetValue(SelectionNameProperty, value);
    }
    public static readonly DependencyProperty SelectionNameProperty
        = DependencyProperty.Register(
            nameof(SelectionName),
            typeof(string),
            typeof(StswCalendar)
        );
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
internal class StswCalendarMonth : StswObservableObject
{
    /// <summary>
    /// Gets the date associated with the calendar item.
    /// </summary>
    public DateTime? Date { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether the calendar item is within the allowable date range.
    /// </summary>
    public bool InMinMaxRange
    {
        get => inMinMaxRange;
        internal set => SetProperty(ref inMinMaxRange, value);
    }
    private bool inMinMaxRange;

    /// <summary>
    /// Gets or sets the number of month.
    /// </summary>
    public int Month { get; internal set; }

    /// <summary>
    /// Gets or sets the display name of the calendar item (e.g., day of the month).
    /// </summary>
    public string? Name { get; internal set; }

    /// <summary>
    /// Gets or sets a value indicating whether the calendar item is the selected date.
    /// </summary>
    public bool IsSelected
    {
        get => isSelected;
        internal set => SetProperty(ref isSelected, value);
    }
    private bool isSelected;
}

/// <summary>
/// Data model for StswCalendar's day items.
/// </summary>
internal class StswCalendarDay : StswCalendarMonth
{
    /// <summary>
    /// Gets a value indicating whether the calendar item is within the selected month.
    /// </summary>
    //public bool InCurrentMonth { get; internal set; }
    public bool InCurrentMonth => Date?.Month == Month;

    /// <summary>
    /// Gets a value indicating whether the calendar item represents the current day.
    /// </summary>
    //public bool IsCurrentDay { get; internal set; }
    public bool IsCurrentDay => Date?.Date == DateTime.Now.Date;
}
