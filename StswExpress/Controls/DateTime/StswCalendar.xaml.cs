﻿using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;

[ContentProperty(nameof(SelectedDate))]
public class StswCalendar : UserControl
{
    public ICommand SelectDateCommand { get; set; }

    public StswCalendar()
    {
        SetValue(ItemsProperty, new ObservableCollection<StswCalendarItem>());

        SelectDateCommand = new StswRelayCommand<DateTime?>(SelectDate_Executed);
    }
    static StswCalendar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswCalendar), new FrameworkPropertyMetadata(typeof(StswCalendar)));
    }

    #region Events
    public event EventHandler? SelectedDateChanged;

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
    public void PART_ButtonPreviousYear_Click(object sender, RoutedEventArgs e) => SelectedMonth = CheckNewDate(SelectionMode == SelectionModes.ByYear ? -120 : -12);

    /// PART_ButtonNextYear_Click
    private void PART_ButtonNextYear_Click(object sender, RoutedEventArgs e) => SelectedMonth = CheckNewDate(SelectionMode == SelectionModes.ByYear ? 120 : 12);

    /// PART_ButtonPreviousMonth_Click
    private void PART_ButtonPreviousMonth_Click(object sender, RoutedEventArgs e) => SelectedMonth = CheckNewDate(SelectionMode == SelectionModes.ByYear ? -12 : -1);

    /// PART_ButtonNextMonth_Click
    private void PART_ButtonNextMonth_Click(object sender, RoutedEventArgs e) => SelectedMonth = CheckNewDate(SelectionMode == SelectionModes.ByYear ? 12 : 1);

    /// PART_ButtonSelectionMode_Click
    private void PART_ButtonSelectionMode_Click(object sender, RoutedEventArgs e) => SelectionMode = StswFn.GetNextEnumValue(SelectionMode);

    /// CheckNewDate
    private DateTime CheckNewDate(int months)
    {
        /// try add months to selected date
        DateTime? newDate;
        if ((months > 0 && DateTime.MaxValue.AddMonths(-months) > SelectedMonth) || (months < 0 && DateTime.MinValue.AddMonths(-months) < SelectedMonth))
            newDate = SelectedMonth.AddMonths(months);
        else
            return months > 0 ? DateTime.MaxValue : DateTime.MinValue;

        /// check if new date is between range of Minimum and Maximum
        var max = new DateTime(newDate.Value.Year, newDate.Value.Month, DateTime.DaysInMonth(newDate.Value.Year, newDate.Value.Month));
        var min = new DateTime(newDate.Value.Year, newDate.Value.Month, 1);

        if (months > 0 && max > Maximum)
            return Maximum.Value;
        else if (months < 0 && min < Minimum)
            return Minimum.Value;

        return newDate.Value;
    }

    /// Command: select date
    public void SelectDate_Executed(DateTime? date)
    {
        if (SelectionMode == SelectionModes.ByYear)
        {
            SelectedMonth = date ?? DateTime.Now;
            SelectionMode = SelectionModes.ByMonth;
        }
        else
        {
            SelectedDate = date;

            if (SelectedDate.HasValue && SelectedDate.Value.Month != SelectedMonth.Month)
                SelectedMonth = SelectedDate.Value;

            var popupRootFinder = VisualTreeHelper.GetParent(this);
            while (popupRootFinder != null)
            {
                var logicalRoot = LogicalTreeHelper.GetParent(popupRootFinder);
                if (logicalRoot is Popup popup)
                {
                    popup.IsOpen = false;
                    break;
                }
                popupRootFinder = VisualTreeHelper.GetParent(popupRootFinder);
            }
        }
    }
    #endregion

    #region Main properties
    /// Items
    public static readonly DependencyProperty ItemsProperty
        = DependencyProperty.Register(
            nameof(Items),
            typeof(ObservableCollection<StswCalendarItem>),
            typeof(StswCalendar)
        );
    public ObservableCollection<StswCalendarItem> Items
    {
        get => (ObservableCollection<StswCalendarItem>)GetValue(ItemsProperty);
        internal set => SetValue(ItemsProperty, value);
    }

    /// Maximum
    public static readonly DependencyProperty MaximumProperty
        = DependencyProperty.Register(
            nameof(Maximum),
            typeof(DateTime?),
            typeof(StswCalendar),
            new PropertyMetadata(default(DateTime?), OnSelectedMonthChanged)
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
            new PropertyMetadata(default(DateTime?), OnSelectedMonthChanged)
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
        if (obj is StswCalendar stsw)
        {
            stsw.Items.ToList().ForEach(x => x.IsSelectedDay = stsw.SelectedDate.HasValue && x.Date == stsw.SelectedDate.Value.Date);

            stsw.SelectedDateChanged?.Invoke(stsw, EventArgs.Empty);
        }
    }
    /// SelectedMonth
    public static readonly DependencyProperty SelectedMonthProperty
        = DependencyProperty.Register(
            nameof(SelectedMonth),
            typeof(DateTime),
            typeof(StswCalendar),
            new FrameworkPropertyMetadata(default(DateTime),
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
            var newButtons = new ObservableCollection<StswCalendarItem>();

            if (stsw.SelectionMode == SelectionModes.ByYear)
            {
                /// display year
                stsw.SelectionName = stsw.SelectedMonth.Year.ToString();

                var max = stsw.Maximum ?? DateTime.MaxValue; max = new DateTime(max.Year, max.Month, DateTime.DaysInMonth(max.Year, max.Month));
                var min = stsw.Minimum ?? DateTime.MinValue; min = new DateTime(min.Year, min.Month, 1);

                /// generate items
                for (int i = 1; i <= 12; i++)
                {
                    newButtons.Add(new StswCalendarItem()
                    {
                        Name = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(i),
                        Date = new DateTime(stsw.SelectedMonth.Year, i, 1),
                        InCurrentMonth = true,
                        InMinMaxRange = new DateTime(stsw.SelectedMonth.Year, i, 1).Between(min, max)
                    });
                }
            }
            else if (stsw.SelectionMode == SelectionModes.ByMonth)
            {
                /// display month and year
                stsw.SelectionName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(stsw.SelectedMonth.Month).Capitalize() + " " + stsw.SelectedMonth.Year;

                /// calculate first button in grid
                DateTime dateForButton;
                if (stsw.SelectedMonth.Year == DateTime.MinValue.Year && stsw.SelectedMonth.Month == DateTime.MinValue.Month) /// only for 0001-01-01
                {
                    dateForButton = new DateTime(stsw.SelectedMonth.Year, stsw.SelectedMonth.Month, 1);
                    while ((newButtons.Count + (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek) % 7 != (int)dateForButton.DayOfWeek)
                        newButtons.Add(new StswCalendarItem());
                }
                else
                {
                    dateForButton = new DateTime(stsw.SelectedMonth.Year, stsw.SelectedMonth.Month, DateTime.DaysInMonth(stsw.SelectedMonth.Year, stsw.SelectedMonth.Month) / 2);
                    dateForButton = dateForButton.AddDays(-21);
                    while (dateForButton.DayOfWeek != CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek)
                        dateForButton = dateForButton.AddDays(1);
                }

                /// generate all 42 items
                var daysToRender = (stsw.SelectedMonth.Year == DateTime.MaxValue.Year && stsw.SelectedMonth.Month == DateTime.MaxValue.Month ? (DateTime.MaxValue - dateForButton).TotalDays : 42);
                while (newButtons.Count < daysToRender)
                {
                    newButtons.Add(new StswCalendarItem()
                    {
                        Name = dateForButton.Day.ToString(),
                        Date = dateForButton,
                        IsCurrentDay = DateTime.Now.Date == dateForButton,
                        InCurrentMonth = dateForButton.Month == stsw.SelectedMonth.Month,
                        InMinMaxRange = dateForButton >= (stsw.Minimum ?? DateTime.MinValue) && dateForButton <= (stsw.Maximum ?? DateTime.MaxValue),
                        IsSelectedDay = stsw.SelectedDate.HasValue && dateForButton.Date == stsw.SelectedDate.Value.Date
                    });
                    if (newButtons.Count < daysToRender)
                        dateForButton = dateForButton.AddDays(1);
                }
                while (newButtons.Count < 42)
                    newButtons.Add(new StswCalendarItem());
            }

            stsw.Items = newButtons;
        }
    }
    /// SelectionMode
    public enum SelectionModes
    {
        ByYear,
        ByMonth
    }
    public static readonly DependencyProperty SelectionModeProperty
        = DependencyProperty.Register(
            nameof(SelectionMode),
            typeof(SelectionModes),
            typeof(StswCalendar),
            new PropertyMetadata(SelectionModes.ByMonth, OnSelectedMonthChanged)
        );
    public SelectionModes SelectionMode
    {
        get => (SelectionModes)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }
    /// SelectionName
    public static readonly DependencyProperty SelectionNameProperty
        = DependencyProperty.Register(
            nameof(SelectionName),
            typeof(string),
            typeof(StswCalendar)
        );
    public string SelectionName
    {
        get => (string)GetValue(SelectionNameProperty);
        internal set => SetValue(SelectionNameProperty, value);
    }
    #endregion

    #region Spatial properties
    /// > BorderThickness ...
    /// SubBorderThickness
    public static readonly DependencyProperty SubBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(SubBorderThickness),
            typeof(Thickness),
            typeof(StswCalendar)
        );
    public Thickness SubBorderThickness
    {
        get => (Thickness)GetValue(SubBorderThicknessProperty);
        set => SetValue(SubBorderThicknessProperty, value);
    }

    /// > CornerRadius ...
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
    
    /// > Padding ...
    /// SubPadding
    public static readonly DependencyProperty SubPaddingProperty
        = DependencyProperty.Register(
            nameof(SubPadding),
            typeof(Thickness),
            typeof(StswCalendar)
        );
    public Thickness SubPadding
    {
        get => (Thickness)GetValue(SubPaddingProperty);
        set => SetValue(SubPaddingProperty, value);
    }
    #endregion

    #region Style properties
    /// Names for days of week
    public static string DayOfWeek1 => CultureInfo.CurrentCulture.DateTimeFormat.GetShortestDayName(CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek).Capitalize();
    public static string DayOfWeek2 => CultureInfo.CurrentCulture.DateTimeFormat.GetShortestDayName(StswFn.GetNextEnumValue(CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek, 1)).Capitalize();
    public static string DayOfWeek3 => CultureInfo.CurrentCulture.DateTimeFormat.GetShortestDayName(StswFn.GetNextEnumValue(CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek, 2)).Capitalize();
    public static string DayOfWeek4 => CultureInfo.CurrentCulture.DateTimeFormat.GetShortestDayName(StswFn.GetNextEnumValue(CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek, 3)).Capitalize();
    public static string DayOfWeek5 => CultureInfo.CurrentCulture.DateTimeFormat.GetShortestDayName(StswFn.GetNextEnumValue(CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek, 4)).Capitalize();
    public static string DayOfWeek6 => CultureInfo.CurrentCulture.DateTimeFormat.GetShortestDayName(StswFn.GetNextEnumValue(CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek, 5)).Capitalize();
    public static string DayOfWeek7 => CultureInfo.CurrentCulture.DateTimeFormat.GetShortestDayName(StswFn.GetNextEnumValue(CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek, 6)).Capitalize();
    #endregion
}

public class StswCalendarItem : StswObservableObject
{
    public string? Name { get; internal set; }
    public DateTime Date { get; internal set; }
    public bool IsCurrentDay { get; internal set; }
    public bool InCurrentMonth { get; internal set; }
    public bool InMinMaxRange { get; internal set; }

    private bool isSelectedDay;
    public bool IsSelectedDay { get => isSelectedDay; internal set => SetProperty(ref isSelectedDay, value); }
}