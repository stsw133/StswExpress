using System;
using System.Collections.ObjectModel;
using System.Globalization;
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
        SetValue(ButtonsProperty, new ObservableCollection<StswCalendarItem>());

        SelectDateCommand = new StswRelayCommand<object?>(SelectDate_Executed);
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

        //Loaded += (s, e) =>
        //{
        //    Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        //    Arrange(new Rect(DesiredSize));
        //};
    }

    /// PART_ButtonPreviousYear_Click
    public void PART_ButtonPreviousYear_Click(object sender, RoutedEventArgs e) => SelectedMonth = CheckNewDate(SelectionMode == SelectionModes.ByYear ? -120 : -12);

    /// PART_ButtonNextYear_Click
    private void PART_ButtonNextYear_Click(object sender, RoutedEventArgs e) => SelectedMonth = CheckNewDate(SelectionMode == SelectionModes.ByYear ? 120 : 12);

    /// PART_ButtonPreviousMonth_Click
    private void PART_ButtonPreviousMonth_Click(object sender, RoutedEventArgs e) => SelectedMonth = CheckNewDate(SelectionMode == SelectionModes.ByYear ? -12 : -1);

    /// PART_ButtonNextMonth_Click
    private void PART_ButtonNextMonth_Click(object sender, RoutedEventArgs e) => SelectedMonth = CheckNewDate(SelectionMode == SelectionModes.ByYear ? 12 : 1);

    /// PART_ButtonNextMonth_Click
    private void PART_ButtonSelectionMode_Click(object sender, RoutedEventArgs e) => SelectionMode = StswFn.GetNextEnumValue(SelectionMode);

    /// CheckNewDate
    private DateTime CheckNewDate(int months)
    {
        /// try add months to selected date
        var newDate = SelectedDate;
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
    public void SelectDate_Executed(object? date)
    {
        if (SelectionMode == SelectionModes.ByYear)
        {
            SelectedMonth = new DateTime(SelectedMonth.Year, Convert.ToInt32(date), 1);
            SelectionMode = SelectionModes.ByMonth;
        }
        else
        {
            SelectedDate = (DateTime?)date;

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
    /// Buttons
    public static readonly DependencyProperty ButtonsProperty
        = DependencyProperty.Register(
            nameof(Buttons),
            typeof(ObservableCollection<StswCalendarItem>),
            typeof(StswCalendar)
        );
    public ObservableCollection<StswCalendarItem> Buttons
    {
        get => (ObservableCollection<StswCalendarItem>)GetValue(ButtonsProperty);
        private set => SetValue(ButtonsProperty, value);
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
        if (obj is StswCalendar stsw)
        {
            if (stsw.Buttons?.Count > 0)
                foreach (var item in stsw.Buttons)
                    item.IsSelectedDay = stsw.SelectedDate.HasValue && item.Date == stsw.SelectedDate.Value.Date;

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
            if (stsw.SelectionMode == SelectionModes.ByYear)
            {
                /// display year
                if (stsw.GetTemplateChild("PART_ButtonSelectionMode") is StswButton btnMode)
                    btnMode.Content = stsw.SelectedMonth.Year.ToString();

                /// hide months when not in range of Minimum and Maximum
                if (stsw.GetTemplateChild("PART_SelectionModeYear") is UniformGrid grid)
                {
                    var max = stsw.Maximum ?? DateTime.MaxValue; max = new DateTime(max.Year, max.Month, DateTime.DaysInMonth(max.Year, max.Month));
                    var min = stsw.Minimum ?? DateTime.MinValue; min = new DateTime(min.Year, min.Month, 1);

                    for (int i = 0; i < 12; i++)
                        if (grid.Children[i] is StswButton button)
                            button.Visibility = new DateTime(stsw.SelectedMonth.Year, i + 1, 1).Between(min, max) ? Visibility.Visible : Visibility.Hidden;
                }
            }
            else if (stsw.SelectionMode == SelectionModes.ByMonth)
            {
                /// display month and year
                if (stsw.GetTemplateChild("PART_ButtonSelectionMode") is StswButton btnMode)
                    btnMode.Content = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(stsw.SelectedMonth.Month).Capitalize() + " " + stsw.SelectedMonth.Year;

                /// clear previous 42 buttons in grid
                var newButtons = new ObservableCollection<StswCalendarItem>();

                /// calculate first button in grid
                DateTime dateForButton;

                if (stsw.SelectedMonth.Year == DateTime.MinValue.Year && stsw.SelectedMonth.Month == DateTime.MinValue.Month)
                {
                    dateForButton = new DateTime(stsw.SelectedMonth.Year, stsw.SelectedMonth.Month, 1);
                    while ((newButtons.Count + (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek) % 7 != (int)dateForButton.DayOfWeek)
                        newButtons.Add(new StswCalendarItem() { Date = DateTime.MinValue });
                }
                else
                {
                    dateForButton = new DateTime(stsw.SelectedMonth.Year, stsw.SelectedMonth.Month, DateTime.DaysInMonth(stsw.SelectedMonth.Year, stsw.SelectedMonth.Month) / 2);
                    dateForButton = dateForButton.AddDays(-21);
                    while (dateForButton.DayOfWeek != CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek)
                        dateForButton = dateForButton.AddDays(1);
                }

                /// put all 42 buttons to grid
                var daysToRender = (stsw.SelectedMonth.Year == DateTime.MaxValue.Year && stsw.SelectedMonth.Month == DateTime.MaxValue.Month ? (DateTime.MaxValue - dateForButton).TotalDays : 42);
                while (newButtons.Count < daysToRender)
                {
                    newButtons.Add(new StswCalendarItem()
                    {
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
                    newButtons.Add(new StswCalendarItem() { Date = DateTime.MaxValue });

                stsw.Buttons = newButtons;
            }
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
            new FrameworkPropertyMetadata(SelectionModes.ByMonth,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedMonthChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public SelectionModes SelectionMode
    {
        get => (SelectionModes)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    /// Names for days of week
    public string DayOfWeek1 => CultureInfo.CurrentCulture.DateTimeFormat.GetShortestDayName(CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek).Capitalize();
    public string DayOfWeek2 => CultureInfo.CurrentCulture.DateTimeFormat.GetShortestDayName(StswFn.GetNextEnumValue(CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek, 1)).Capitalize();
    public string DayOfWeek3 => CultureInfo.CurrentCulture.DateTimeFormat.GetShortestDayName(StswFn.GetNextEnumValue(CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek, 2)).Capitalize();
    public string DayOfWeek4 => CultureInfo.CurrentCulture.DateTimeFormat.GetShortestDayName(StswFn.GetNextEnumValue(CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek, 3)).Capitalize();
    public string DayOfWeek5 => CultureInfo.CurrentCulture.DateTimeFormat.GetShortestDayName(StswFn.GetNextEnumValue(CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek, 4)).Capitalize();
    public string DayOfWeek6 => CultureInfo.CurrentCulture.DateTimeFormat.GetShortestDayName(StswFn.GetNextEnumValue(CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek, 5)).Capitalize();
    public string DayOfWeek7 => CultureInfo.CurrentCulture.DateTimeFormat.GetShortestDayName(StswFn.GetNextEnumValue(CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek, 6)).Capitalize();

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
    public string Month11 => CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(11);
    public string Month12 => CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(12);
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
}

public class StswCalendarItem : StswObservableObject
{
    public DateTime Date { get; internal set; }
    public bool IsCurrentDay { get; internal set; }
    public bool InCurrentMonth { get; internal set; }
    public bool InMinMaxRange { get; internal set; }

    private bool isSelectedDay;
    public bool IsSelectedDay { get => isSelectedDay; internal set => SetProperty(ref isSelectedDay, value); }
}