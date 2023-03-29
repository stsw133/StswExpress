using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace StswExpress;

public class StswCalendar : UserControl
{
    public ICommand OnClickCommand { get; set; }

    public StswCalendar()
    {
        OnClickCommand = new StswRelayCommand<object>(OnClick);
    }

    static StswCalendar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswCalendar), new FrameworkPropertyMetadata(typeof(StswCalendar)));
    }

    #region Events
    /// OnApplyTemplate
    public override void OnApplyTemplate()
    {
        SelectedMonth = SelectedDate ?? DateTime.Now;

        /// Button: previous year
        if (GetTemplateChild("PART_ButtonPreviousYear") is StswRepeatButton btnPreviousYear)
            btnPreviousYear.Click += PART_ButtonPreviousYear_Click;
        /// Button: next year
        if (GetTemplateChild("PART_ButtonNextYear") is StswRepeatButton btnNextYear)
            btnNextYear.Click += PART_ButtonNextYear_Click;
        /// Button: previous month
        if (GetTemplateChild("PART_ButtonPreviousMonth") is StswRepeatButton btnPreviousMonth)
            btnPreviousMonth.Click += PART_ButtonPreviousMonth_Click;
        /// Button: next month
        if (GetTemplateChild("PART_ButtonNextMonth") is StswRepeatButton btnNextMonth)
            btnNextMonth.Click += PART_ButtonNextMonth_Click;
        /// Button: selection mode
        if (GetTemplateChild("PART_ButtonSelectionMode") is StswButton btnSelectionMode)
            btnSelectionMode.Click += PART_ButtonSelectionMode_Click;

        base.OnApplyTemplate();
    }

    /// PART_ButtonPreviousYear_Click
    public void PART_ButtonPreviousYear_Click(object sender, RoutedEventArgs e) => SelectedMonth = CheckNewDate(SelectionMode == SelectionModes.Year ? -120 : -12);

    /// PART_ButtonNextYear_Click
    private void PART_ButtonNextYear_Click(object sender, RoutedEventArgs e) => SelectedMonth = CheckNewDate(SelectionMode == SelectionModes.Year ? 120 : 12);

    /// PART_ButtonPreviousMonth_Click
    private void PART_ButtonPreviousMonth_Click(object sender, RoutedEventArgs e) => SelectedMonth = CheckNewDate(SelectionMode == SelectionModes.Year ? -12 : -1);

    /// PART_ButtonNextMonth_Click
    private void PART_ButtonNextMonth_Click(object sender, RoutedEventArgs e) => SelectedMonth = CheckNewDate(SelectionMode == SelectionModes.Year ? 12 : 1);

    /// PART_ButtonNextMonth_Click
    private void PART_ButtonSelectionMode_Click(object sender, RoutedEventArgs e) => SelectionMode = SelectionModes.Year;

    /// CheckNewDate
    private DateTime CheckNewDate(int months)
    {
        /// try add months to selected date
        var newDate = SelectedDate;
        try
        {
            newDate = SelectedMonth.AddMonths(months);
        }
        catch
        {
            return months > 0 ? DateTime.MaxValue : DateTime.MinValue;
        }

        /// check if new date is between range of Minimum and Maximum
        var min = new DateTime(newDate.Value.Year, newDate.Value.Month, 1);
        var max = new DateTime(newDate.Value.Year, newDate.Value.Month, DateTime.DaysInMonth(newDate.Value.Year, newDate.Value.Month));

        if (months > 0 && Maximum.HasValue && max > Maximum.Value)
            return Maximum.Value;
        else if (months < 0 && Minimum.HasValue && min < Minimum.Value)
            return Minimum.Value;

        return newDate.Value;
    }

    /// OnClick
    public void OnClick(object date)
    {
        if (SelectionMode == SelectionModes.Year)
        {
            SelectedMonth = new DateTime(SelectedMonth.Year, Convert.ToInt32(date), 1);
            SelectionMode = SelectionModes.Month;
        }
        else
        {
            SelectedDate = (DateTime)date;
            if (Parent is Popup popup)
                popup.IsOpen = false;
            else if (SelectedDate.Value.Month != SelectedMonth.Month)
                SelectedMonth = SelectedDate.Value;
        }
    }
    #endregion

    #region Properties
    /// Buttons
    public static readonly DependencyProperty ButtonsProperty
        = DependencyProperty.Register(
            nameof(Buttons),
            typeof(List<StswCalendarItem>),
            typeof(StswCalendar)
        );
    public List<StswCalendarItem> Buttons
    {
        get => (List<StswCalendarItem>)GetValue(ButtonsProperty);
        private set => SetValue(ButtonsProperty, value);
    }

    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswCalendar)
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// Maximum
    public static readonly DependencyProperty MaximumProperty
        = DependencyProperty.Register(
            nameof(Maximum),
            typeof(DateTime?),
            typeof(StswCalendar),
            new FrameworkPropertyMetadata(default(DateTime?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedMonthChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public DateTime? Maximum
    {
        get => (DateTime?)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }
    /// Minimum
    public static readonly DependencyProperty MinimumProperty
        = DependencyProperty.Register(
            nameof(Minimum),
            typeof(DateTime?),
            typeof(StswCalendar),
            new FrameworkPropertyMetadata(default(DateTime?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedMonthChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public DateTime? Minimum
    {
        get => (DateTime?)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    /// SelectedDate
    public static readonly DependencyProperty SelectedDateProperty
        = DependencyProperty.Register(
            nameof(SelectedDate),
            typeof(DateTime?),
            typeof(StswCalendar),
            new FrameworkPropertyMetadata(default(DateTime?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedDateChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public DateTime? SelectedDate
    {
        get => (DateTime?)GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }
    public static void OnSelectedDateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswCalendar stsw && stsw.Buttons?.Count > 0)
        {
            foreach (var item in stsw.Buttons)
                item.IsSelectedDay = stsw.SelectedDate.HasValue && item.Date == stsw.SelectedDate.Value.Date;
        }
    }
    /// SelectedMonth
    public static readonly DependencyProperty SelectedMonthProperty
        = DependencyProperty.Register(
            nameof(SelectedMonth),
            typeof(DateTime),
            typeof(StswCalendar),
            new FrameworkPropertyMetadata(DateTime.Now,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedMonthChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public DateTime SelectedMonth
    {
        get => (DateTime)GetValue(SelectedMonthProperty);
        private set => SetValue(SelectedMonthProperty, value);
    }
    public static void OnSelectedMonthChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswCalendar stsw)
        {
            if (stsw.SelectionMode == SelectionModes.Year)
            {
                /// display year
                if (stsw.GetTemplateChild("PART_ButtonSelectionMode") is StswButton buttonS && buttonS.Content is TextBlock block)
                    block.Text = stsw.SelectedMonth.Year.ToString();

                /// hide months when not in range of Minimum and Maximum
                if (stsw.GetTemplateChild("PART_SelectionModeYear") is UniformGrid grid)
                {
                    var min = stsw.Minimum ?? DateTime.MinValue; min = new DateTime(min.Year, min.Month, 1);
                    var max = stsw.Maximum ?? DateTime.MaxValue; max = new DateTime(max.Year, max.Month, DateTime.DaysInMonth(max.Year, max.Month));

                    for (int i = 0; i < 12; i++)
                        if (grid.Children[i] is StswButton button)
                            button.Visibility = new DateTime(stsw.SelectedMonth.Year, i + 1, 1).Between(min, max) ? Visibility.Visible : Visibility.Hidden;
                }
            }
            else if (stsw.SelectionMode == SelectionModes.Month)
            {
                /// display month and year
                if (stsw.GetTemplateChild("PART_ButtonSelectionMode") is StswButton buttonS && buttonS.Content is TextBlock block)
                    block.Text = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(stsw.SelectedMonth.Month).Capitalize() + " " + stsw.SelectedMonth.Year;

                /// calculate first button in grid
                var dateForButton = new DateTime(stsw.SelectedMonth.Year, stsw.SelectedMonth.Month, DateTime.DaysInMonth(stsw.SelectedMonth.Year, stsw.SelectedMonth.Month) / 2);
                dateForButton = dateForButton.AddDays(-21);
                while (dateForButton.DayOfWeek != DayOfWeek.Monday)
                    dateForButton = dateForButton.AddDays(1);

                /// put all 42 buttons to grid
                var newButtons = new List<StswCalendarItem>();
                while (newButtons.Count < 42)
                {
                    newButtons.Add(new StswCalendarItem()
                    {
                        Date = dateForButton,
                        IsCurrentDay = DateTime.Now.Date == dateForButton,
                        IsInMonth = dateForButton.Month == stsw.SelectedMonth.Month,
                        IsInRange = dateForButton >= (stsw.Minimum ?? DateTime.MinValue) && dateForButton <= (stsw.Maximum ?? DateTime.MaxValue),
                        IsSelectedDay = stsw.SelectedDate.HasValue ? dateForButton.Date == stsw.SelectedDate.Value.Date : false
                    });
                    dateForButton = dateForButton.AddDays(1);
                }
                stsw.Buttons = newButtons;
            }
        }
    }
    /// SelectionMode
    public enum SelectionModes
    {
        Year,
        Month
    }
    public static readonly DependencyProperty SelectionModeProperty
        = DependencyProperty.Register(
            nameof(SelectionMode),
            typeof(SelectionModes),
            typeof(StswCalendar),
            new FrameworkPropertyMetadata(SelectionModes.Month,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedMonthChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public SelectionModes SelectionMode
    {
        get => (SelectionModes)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    /// Names for days of week
    public string DayOfWeek1 => CultureInfo.CurrentCulture.DateTimeFormat.GetShortestDayName(DayOfWeek.Monday).Capitalize();
    public string DayOfWeek2 => CultureInfo.CurrentCulture.DateTimeFormat.GetShortestDayName(DayOfWeek.Tuesday).Capitalize();
    public string DayOfWeek3 => CultureInfo.CurrentCulture.DateTimeFormat.GetShortestDayName(DayOfWeek.Wednesday).Capitalize();
    public string DayOfWeek4 => CultureInfo.CurrentCulture.DateTimeFormat.GetShortestDayName(DayOfWeek.Thursday).Capitalize();
    public string DayOfWeek5 => CultureInfo.CurrentCulture.DateTimeFormat.GetShortestDayName(DayOfWeek.Friday).Capitalize();
    public string DayOfWeek6 => CultureInfo.CurrentCulture.DateTimeFormat.GetShortestDayName(DayOfWeek.Saturday).Capitalize();
    public string DayOfWeek7 => CultureInfo.CurrentCulture.DateTimeFormat.GetShortestDayName(DayOfWeek.Sunday).Capitalize();

    /// Names for months
    public string Month1 => CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(1);
    public string Month2 => CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(2);
    public string Month3 => CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(3);
    public string Month4 => CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(4);
    public string Month5 => CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(5);
    public string Month6 => CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(6);
    public string Month7 => CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(7);
    public string Month8 => CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(8);
    public string Month9 => CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(9);
    public string Month10 => CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(10);
    public string Month11 => CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(10);
    public string Month12 => CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(10);
    #endregion
}

public class StswCalendarItem : StswObservableObject
{
    public DateTime Date { get; internal set; }
    public bool IsCurrentDay { get; internal set; }
    public bool IsInMonth { get; internal set; }
    public bool IsInRange { get; internal set; }

    private bool isSelectedDay;
    public bool IsSelectedDay { get => isSelectedDay; internal set => SetProperty(ref isSelectedDay, value); }
}