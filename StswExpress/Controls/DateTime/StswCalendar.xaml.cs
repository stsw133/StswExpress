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
    public ICommand SelectDayCommand { get; set; }
    public ICommand SelectMonthCommand { get; set; }

    public StswCalendar()
    {
        SetValue(ListOfDaysProperty, new ObservableCollection<StswCalendarItem>());
        SetValue(ListOfMonthsProperty, new ObservableCollection<StswCalendarItem>());

        for (int i = 0; i < 42; i++)
            ListOfDays.Add(new());

        for (int i = 0; i < 12; i++)
            ListOfMonths.Add(new());

        SelectDayCommand = new StswCommand<DateTime?>(SelectDay);
        SelectMonthCommand = new StswCommand<DateTime>(SelectMonth);
    }
    static StswCalendar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswCalendar), new FrameworkPropertyMetadata(typeof(StswCalendar)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswCalendar), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
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

        /// set default month to create the initial view
        var defMonth = (SelectedDate ?? DateTime.Now).Date;
        SelectedMonth = new DateTime(defMonth.Year, defMonth.Month, 1);

        /// set header name and update buttons in the view
        if (CurrentMode != SelectionMode)
            CurrentMode = SelectionMode;
        else
            UpdateCalendarView();
    }

    /// <summary>
    /// Updates the list of days displayed in the control for the currently selected month.
    /// </summary>
    private void UpdateListOfDays()
    {
        var middleDate = new DateTime(SelectedMonth.Year, SelectedMonth.Month, 15);
        var offset = ((int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek - (int)middleDate.DayOfWeek + 7) % 7;
        middleDate = middleDate.AddDays(offset);

        var now = DateTime.Now.Date;
        for (var i = 0; i < ListOfDays.Count; i++)
        {
            DateTime? newDate = null;
            var targetTicks = middleDate.Ticks + TimeSpan.TicksPerDay * (i - 21);
            if (targetTicks >= DateTime.MinValue.Ticks && targetTicks <= DateTime.MaxValue.Ticks)
                newDate = new DateTime(targetTicks);

            var item = ListOfDays[i];
            item.Date = newDate;
            item.Name = newDate?.Day.ToString();
            item.InCurrentMonth = newDate?.Year == SelectedMonth.Year && newDate?.Month == SelectedMonth.Month;
            item.InMinMaxRange = newDate >= (Minimum ?? DateTime.MinValue) && newDate <= (Maximum ?? DateTime.MaxValue);
            item.IsCurrentDay = newDate == now;
            item.IsSpecialDay = newDate?.DayOfWeek == DayOfWeek.Sunday;
            item.IsSelected = newDate == SelectedDate?.Date;
        }
    }

    /// <summary>
    /// Updates the list of months displayed in the control for the currently selected year.
    /// </summary>
    private void UpdateListOfMonths()
    {
        var now = DateTime.Now.Date;
        var max = Maximum ?? DateTime.MaxValue; max = new DateTime(max.Year, max.Month, DateTime.DaysInMonth(max.Year, max.Month));
        var min = Minimum ?? DateTime.MinValue; min = new DateTime(min.Year, min.Month, 1);

        for (var i = 0; i < ListOfMonths.Count; i++)
        {
            var newDate = new DateTime(SelectedMonth.Year, i + 1, 1);

            var item = ListOfMonths[i];
            item.Date = newDate;
            item.Name = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(i + 1);
            item.InMinMaxRange = newDate.Between(min, max);
            item.IsCurrentDay = newDate.Year == now.Year && newDate.Month == now.Month;
            item.IsSelected = SelectedDate?.Year == newDate.Year && SelectedDate?.Month == newDate.Month;
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

    /// <summary>
    /// Closes the parent popup if the control is contained within one.
    /// </summary>
    /// <returns><see langword="true"/> if the popup was found and closed; otherwise, <see langword="false"/>.</returns>
    private bool ClosePopupIfAny()
    {
        if (StswFn.GetParentPopup(this) is Popup popup)
        {
            popup.IsOpen = false;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Handles the selection of a day and sets the <see cref="SelectedDate"/> property.
    /// Closes the popup if applicable.
    /// </summary>
    /// <param name="date">The date to select.</param>
    private void SelectDay(DateTime? date)
    {
        SelectedDate = date?.Date;
        ClosePopupIfAny();
    }

    /// <summary>
    /// Handles the selection of a month and sets the <see cref="SelectedMonth"/> property.
    /// Updates the view or closes the popup depending on the current mode.
    /// </summary>
    /// <param name="date">The month to select.</param>
    private void SelectMonth(DateTime date)
    {
        var selectedMonth = new DateTime(date.Year, date.Month, 1);
        if (SelectedMonth != selectedMonth)
            SelectedMonth = selectedMonth;
        else /// to refresh button selections
            OnSelectedMonthChanged(this, new DependencyPropertyChangedEventArgs());

        if (SelectionMode == StswCalendarMode.Months)
        {
            SelectedDate = SelectedMonth;
            if (!ClosePopupIfAny())
                UpdateCalendarView();
        }
        else if (SelectionMode == StswCalendarMode.Days && CurrentMode != StswCalendarMode.Days)
        {
            CurrentMode = StswCalendarMode.Days;
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
    /// Updates the calendar view based on the current mode (days or months).
    /// Adjusts the header and regenerates the day or month buttons.
    /// </summary>
    private void UpdateCalendarView()
    {
        if (CurrentMode == StswCalendarMode.Days)
        {
            if (_buttonCurrentMode != null)
                _buttonCurrentMode.Content = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(SelectedMonth.Month).Capitalize()} {SelectedMonth.Year}";

            UpdateListOfDays();
        }
        else if (CurrentMode == StswCalendarMode.Months)
        {
            if (_buttonCurrentMode != null)
                _buttonCurrentMode.Content = SelectedMonth.Year.ToString();

            UpdateListOfMonths();
        }
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
            stsw.UpdateCalendarView();
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

            stsw.UpdateCalendarView(); /// to update buttons (days or months based on current mode) visibilities
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
            /// change visual selections on buttons (so only one is selected and old ones are deselected)
            stsw.ListOfDays.ModifyEach(x => x.IsSelected = false);
            if (stsw.ListOfDays.FirstOrDefault(x => x.Date == stsw.SelectedDate?.Date) is StswCalendarItem item)
                item.IsSelected = true;

            /// without this, clicking on button with date from another month, will not change view to different (previous or next) month
            if (stsw.SelectedDate.HasValue)
                stsw.SelectedMonth = new DateTime(stsw.SelectedDate.Value.Year, stsw.SelectedDate.Value.Month, 1);

            /// event to non MVVM programming
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
            /// change visual selections on buttons (so only one is selected and old ones are deselected)
            stsw.ListOfMonths.ModifyEach(x => x.IsSelected = false);
            if (stsw.ListOfMonths.FirstOrDefault(x => x.Date == stsw.SelectedMonth.Date) is StswCalendarItem item)
                item.IsSelected = true;

            /// without this, clicking on button with date from another month, will not change view to different (previous or next) month
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
            /// because for selection mode by months, current mode cannot be switched between months and days mode, and can only stay at months mode
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
    public DateTime? Date
    {
        get => _date;
        internal set => SetProperty(ref _date, value);
    }
    private DateTime? _date;

    /// <summary>
    /// Gets a value indicating whether the calendar item is within the selected month.
    /// </summary>
    public bool? InCurrentMonth
    {
        get => _inCurrentMonth;
        internal set => SetProperty(ref _inCurrentMonth, value);
    }
    private bool? _inCurrentMonth;

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
    public bool IsCurrentDay
    {
        get => _isCurrentDay;
        internal set => SetProperty(ref _isCurrentDay, value);
    }
    private bool _isCurrentDay;

    /// <summary>
    /// Gets a value indicating whether the calendar item represents the special day (e.g., sunday).
    /// </summary>
    public bool IsSpecialDay
    {
        get => _isSpecialDay;
        internal set => SetProperty(ref _isSpecialDay, value);
    }
    private bool _isSpecialDay;

    /// <summary>
    /// Gets or sets the display name of the calendar item (e.g., day of the month).
    /// </summary>
    public string? Name
    {
        get => _name;
        internal set => SetProperty(ref _name, value);
    }
    private string? _name;

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
