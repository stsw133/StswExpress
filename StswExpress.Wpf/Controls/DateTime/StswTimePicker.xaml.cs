using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace StswExpress.Wpf;

/// <summary>
/// A time picker control that allows users to enter a time manually or select it from a drop-down time selector.
/// Supports custom time formats, min/max validation, and incremental adjustments via mouse wheel.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswTimePicker SelectedTime="{Binding StartTime}" Format="hh\\:mm" IncrementType="Minute"/&gt;
/// </code>
/// </example>
[ContentProperty(nameof(SelectedTime))]
public class StswTimePicker : StswInputBoxBase, IStswBoxControl, IStswCornerControl
{
    static StswTimePicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswTimePicker), new FrameworkPropertyMetadata(typeof(StswTimePicker)));
    }

    public StswTimePicker()
    {
        SetValue(SubControlsProperty, new ObservableCollection<IStswSubControl>());
    }

    #region Dependency properties

    /// <inheritdoc/>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty =
        DependencyProperty.Register(nameof(CornerClipping), typeof(bool), typeof(StswTimePicker));

    /// <inheritdoc/>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(StswTimePicker));

    /// <inheritdoc/>
    public ReadOnlyObservableCollection<ValidationError> Errors
    {
        get => (ReadOnlyObservableCollection<ValidationError>)GetValue(ErrorsProperty);
        set => SetValue(ErrorsProperty, value);
    }
    public static readonly DependencyProperty ErrorsProperty =
        DependencyProperty.Register(nameof(Errors), typeof(ReadOnlyObservableCollection<ValidationError>), typeof(StswTimePicker));

    /// <summary>
    /// Gets or sets the format used for displaying the time value.
    /// </summary>
    public string? Format
    {
        get => (string?)GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }
    public static readonly DependencyProperty FormatProperty =
        DependencyProperty.Register(
            nameof(Format),
            typeof(string),
            typeof(StswTimePicker),
            new FrameworkPropertyMetadata(
                default(string?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnFormatChanged));

    private static void OnFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var input = (StswTimePicker)d;
        input.UpdateVisibilityBasedOnFormat();
        input.FormatChanged(input.Format);
    }

    /// <inheritdoc/>
    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }
    public static readonly DependencyProperty HasErrorProperty =
        DependencyProperty.Register(nameof(HasError), typeof(bool), typeof(StswTimePicker));

    /// <inheritdoc/>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(object), typeof(StswTimePicker));

    /// <summary>
    /// Gets or sets the increment type that determines how the time changes when scrolling with the mouse wheel.
    /// </summary>
    public StswTimeSpanIncrementType IncrementType
    {
        get => (StswTimeSpanIncrementType)GetValue(IncrementTypeProperty);
        set => SetValue(IncrementTypeProperty, value);
    }
    public static readonly DependencyProperty IncrementTypeProperty =
        DependencyProperty.Register(nameof(IncrementType), typeof(StswTimeSpanIncrementType), typeof(StswTimePicker));

    /// <summary>
    /// Gets or sets a value indicating whether the drop-down menu is currently open.
    /// </summary>
    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }
    public static readonly DependencyProperty IsDropDownOpenProperty =
        DependencyProperty.Register(nameof(IsDropDownOpen), typeof(bool), typeof(StswTimePicker));

    /// <summary>
    /// Gets a value indicating whether the hours input field is visible based on the selected <see cref="Format"/>.
    /// </summary>
    public bool IsHoursVisible
    {
        get => (bool)GetValue(IsHoursVisibleProperty);
        private set => SetValue(IsHoursVisibleProperty, value);
    }
    public static readonly DependencyProperty IsHoursVisibleProperty =
        DependencyProperty.Register(nameof(IsHoursVisible), typeof(bool), typeof(StswTimePicker), new PropertyMetadata(true));

    /// <summary>
    /// Gets a value indicating whether the minutes input field is visible based on the selected <see cref="Format"/>.
    /// </summary>
    public bool IsMinutesVisible
    {
        get => (bool)GetValue(IsMinutesVisibleProperty);
        private set => SetValue(IsMinutesVisibleProperty, value);
    }
    public static readonly DependencyProperty IsMinutesVisibleProperty =
        DependencyProperty.Register(nameof(IsMinutesVisible), typeof(bool), typeof(StswTimePicker), new PropertyMetadata(true));

    /// <summary>
    /// Gets a value indicating whether the seconds input field is visible based on the selected <see cref="Format"/>.
    /// </summary>
    public bool IsSecondsVisible
    {
        get => (bool)GetValue(IsSecondsVisibleProperty);
        private set => SetValue(IsSecondsVisibleProperty, value);
    }
    public static readonly DependencyProperty IsSecondsVisibleProperty =
        DependencyProperty.Register(nameof(IsSecondsVisible), typeof(bool), typeof(StswTimePicker), new PropertyMetadata(true));

    /// <summary>
    /// Gets or sets the maximum allowable time in the control.
    /// </summary>
    public TimeSpan? Maximum
    {
        get => (TimeSpan?)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }
    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(
            nameof(Maximum),
            typeof(TimeSpan?),
            typeof(StswTimePicker),
            new PropertyMetadata(default(TimeSpan?), OnMinMaxChanged));

    /// <summary>
    /// Gets or sets the minimum allowable time in the control.
    /// </summary>
    public TimeSpan? Minimum
    {
        get => (TimeSpan?)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }
    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(
            nameof(Minimum),
            typeof(TimeSpan?),
            typeof(StswTimePicker),
            new PropertyMetadata(default(TimeSpan?), OnMinMaxChanged));

    private static void OnMinMaxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var input = (StswTimePicker)d;
        if (input.SelectedTime != null && !input.SelectedTime.Between(input.Minimum, input.Maximum))
            input.SelectedTime = input.MinMaxValidate(input.SelectedTime);
    }

    /// <summary>
    /// Gets or sets the currently selected time in the control.
    /// </summary>
    public TimeSpan? SelectedTime
    {
        get => (TimeSpan?)GetValue(SelectedTimeProperty);
        set => SetValue(SelectedTimeProperty, value);
    }
    public static readonly DependencyProperty SelectedTimeProperty =
        DependencyProperty.Register(
            nameof(SelectedTime),
            typeof(TimeSpan?),
            typeof(StswTimePicker),
            new FrameworkPropertyMetadata(
                default(TimeSpan?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedTimeChanged,
                OnSelectedTimeChanging,
                false,
                UpdateSourceTrigger.PropertyChanged));

    private static void OnSelectedTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var input = (StswTimePicker)d;
        if (input._isTimeChanging)
            return;

        input._isTimeChanging = true;
        try
        {
            if (input.SelectedTime.HasValue)
            {
                input.SelectedTimeH = input.SelectedTime.Value.Hours;
                input.SelectedTimeM = input.SelectedTime.Value.Minutes;
                input.SelectedTimeS = input.SelectedTime.Value.Seconds;
            }
        }
        finally
        {
            input._isTimeChanging = false;
        }
    }

    private static object? OnSelectedTimeChanging(DependencyObject d, object? baseValue)
    {
        var input = (StswTimePicker)d;
        return input.MinMaxValidate((TimeSpan?)baseValue);
    }

    private bool _isTimeChanging;

    /// <summary>
    /// Gets or sets the currently selected hour for the selected time.
    /// </summary>
    internal int SelectedTimeH
    {
        get => (int)GetValue(SelectedTimeHProperty);
        set => SetValue(SelectedTimeHProperty, value);
    }
    internal static readonly DependencyProperty SelectedTimeHProperty =
        DependencyProperty.Register(
            nameof(SelectedTimeH),
            typeof(int),
            typeof(StswTimePicker),
            new FrameworkPropertyMetadata(
                default(int),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedTimeHChanged,
                null,
                false,
                UpdateSourceTrigger.PropertyChanged));

    private static void OnSelectedTimeHChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var input = (StswTimePicker)d;
        if (input._isTimeChanging)
            return;

        if (input.SelectedTime.HasValue)
        {
            var time = input.SelectedTime.Value;
            input.SelectedTime = new TimeSpan(time.Days, input.SelectedTimeH, time.Minutes, time.Seconds, time.Milliseconds);
        }
        else
        {
            input.SelectedTime = new TimeSpan(input.SelectedTimeH, 0, 0);
        }
    }

    /// <summary>
    /// Gets or sets the currently selected minute for the selected time.
    /// </summary>
    internal int SelectedTimeM
    {
        get => (int)GetValue(SelectedTimeMProperty);
        set => SetValue(SelectedTimeMProperty, value);
    }
    internal static readonly DependencyProperty SelectedTimeMProperty =
        DependencyProperty.Register(
            nameof(SelectedTimeM),
            typeof(int),
            typeof(StswTimePicker),
            new FrameworkPropertyMetadata(
                default(int),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedTimeMChanged,
                null,
                false,
                UpdateSourceTrigger.PropertyChanged));

    private static void OnSelectedTimeMChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var input = (StswTimePicker)d;
        if (input._isTimeChanging)
            return;

        if (input.SelectedTime.HasValue)
        {
            var time = input.SelectedTime.Value;
            input.SelectedTime = new TimeSpan(time.Days, time.Hours, input.SelectedTimeM, time.Seconds, time.Milliseconds);
        }
        else
        {
            input.SelectedTime = new TimeSpan(0, input.SelectedTimeM, 0);
        }
    }

    /// <summary>
    /// Gets or sets the currently selected second for the selected time.
    /// </summary>
    internal int SelectedTimeS
    {
        get => (int)GetValue(SelectedTimeSProperty);
        set => SetValue(SelectedTimeSProperty, value);
    }
    internal static readonly DependencyProperty SelectedTimeSProperty =
        DependencyProperty.Register(
            nameof(SelectedTimeS),
            typeof(int),
            typeof(StswTimePicker),
            new FrameworkPropertyMetadata(
                default(int),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedTimeSChanged,
                null,
                false,
                UpdateSourceTrigger.PropertyChanged));

    private static void OnSelectedTimeSChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var input = (StswTimePicker)d;
        if (input._isTimeChanging)
            return;

        if (input.SelectedTime.HasValue)
        {
            var time = input.SelectedTime.Value;
            input.SelectedTime = new TimeSpan(time.Days, time.Hours, time.Minutes, input.SelectedTimeS, time.Milliseconds);
        }
        else
        {
            input.SelectedTime = new TimeSpan(0, 0, input.SelectedTimeS);
        }
    }

    /// <summary>
    /// Gets or sets the thickness of the separator between the editable area and the drop-down button.
    /// </summary>
    public double SeparatorThickness
    {
        get => (double)GetValue(SeparatorThicknessProperty);
        set => SetValue(SeparatorThicknessProperty, value);
    }
    public static readonly DependencyProperty SeparatorThicknessProperty =
        DependencyProperty.Register(
            nameof(SeparatorThickness),
            typeof(double),
            typeof(StswTimePicker),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender));

    /// <inheritdoc/>
    public ObservableCollection<IStswSubControl> SubControls
    {
        get => (ObservableCollection<IStswSubControl>)GetValue(SubControlsProperty);
        set => SetValue(SubControlsProperty, value);
    }
    public static readonly DependencyProperty SubControlsProperty =
        DependencyProperty.Register(nameof(SubControls), typeof(ObservableCollection<IStswSubControl>), typeof(StswTimePicker));

    /// <summary>
    /// Internal editing buffer. <see cref="SelectedTime"/> is the public semantic value of the control.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new string? Text
    {
        get => base.Text;
        internal set => base.Text = value ?? string.Empty;
    }

    #endregion

    #region Template and overrides

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        UpdateVisibilityBasedOnFormat();
        FormatChanged(Format);
    }

    /// <summary>
    /// Enter is the explicit time commit path supplied by <see cref="StswInputBoxBase.CommitOnEnter"/>.
    /// </summary>
    protected override void OnCommit()
    {
        UpdateMainProperty(alwaysUpdate: true);
    }

    /// <summary>
    /// Losing focus commits a valid changed time.
    /// </summary>
    protected override void OnLostFocus(RoutedEventArgs e)
    {
        UpdateMainProperty(alwaysUpdate: false);
        base.OnLostFocus(e);
    }

    /// <inheritdoc/>
    protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
    {
        // Handle time stepping before PART_ContentHost/StswScrollView gets the wheel event.
        if (IsKeyboardFocused && !IsReadOnly && IncrementType != StswTimeSpanIncrementType.None && TryParseTime(Text, out var result))
        {
            result = IncrementTime(result, IncrementType, increase: e.Delta > 0);
            SelectedTime = result;
            CaretIndex = Text?.Length ?? 0;
            e.Handled = true;
            return;
        }

        base.OnPreviewMouseWheel(e);
    }

    #endregion

    #region Logic

    /// <summary>
    /// Applies the requested time format to the internal Text-to-SelectedTime binding without replacing the user's binding on SelectedTime.
    /// </summary>
    private void FormatChanged(string? newFormat)
    {
        if (GetBindingExpression(TextProperty)?.ParentBinding is Binding binding)
        {
            var newBinding = (Binding)binding.Clone();
            newBinding.StringFormat = newFormat;
            SetBinding(TextProperty, newBinding);
        }
    }

    /// <summary>
    /// Parses the editing buffer and commits <see cref="SelectedTime"/>.
    /// </summary>
    private void UpdateMainProperty(bool alwaysUpdate)
    {
        var isPlain = false;
        var isInvalid = false;

        var result = SelectedTime;

        if (string.IsNullOrWhiteSpace(Text))
        {
            result = null;
        }
        else if (!string.IsNullOrEmpty(Format)
            && TimeSpan.TryParseExact(Text, Format, CultureInfo.CurrentCulture, TimeSpanStyles.None, out var exact))
        {
            isPlain = true;
            result = exact;
        }
        else if (TimeSpan.TryParse(Text, CultureInfo.CurrentCulture, out var parsed))
        {
            isPlain = true;
            result = parsed;
        }
        else
        {
            isInvalid = true;
        }

        if (result != SelectedTime || alwaysUpdate)
        {
            SelectedTime = result;

            var textBE = GetBindingExpression(TextProperty);
            var valueBE = GetBindingExpression(SelectedTimeProperty);

            if (!isInvalid && valueBE?.Status == BindingStatus.Active)
                valueBE.UpdateSource();

            if (textBE != null && textBE.Status is BindingStatus.Active or BindingStatus.UpdateSourceError)
            {
                if (string.IsNullOrWhiteSpace(Text) || isPlain)
                    textBE.UpdateSource();
                else if (isInvalid && alwaysUpdate)
                    textBE.UpdateSource();
            }
        }
    }

    /// <summary>
    /// Ensures that the provided time value is within the defined minimum and maximum limits.
    /// </summary>
    private TimeSpan? MinMaxValidate(TimeSpan? value)
    {
        if (value == null)
            return null;

        if (Minimum.HasValue && value < Minimum)
            value = Minimum;

        if (Maximum.HasValue && value > Maximum)
            value = Maximum;

        return value;
    }

    private bool TryParseTime(string? text, out TimeSpan result)
    {
        if (!string.IsNullOrEmpty(Format)
            && TimeSpan.TryParseExact(text, Format, CultureInfo.CurrentCulture, TimeSpanStyles.None, out result))
            return true;

        return TimeSpan.TryParse(text, CultureInfo.CurrentCulture, out result);
    }

    private static TimeSpan IncrementTime(TimeSpan value, StswTimeSpanIncrementType incrementType, bool increase)
    {
        return (incrementType, increase) switch
        {
            (StswTimeSpanIncrementType.Day, true) => TimeSpan.MaxValue.Add(new TimeSpan(-1, 0, 0, 0)) >= value ? value.Add(new TimeSpan(1, 0, 0, 0)) : TimeSpan.MaxValue,
            (StswTimeSpanIncrementType.Hour, true) => TimeSpan.MaxValue.Add(new TimeSpan(0, -1, 0, 0)) >= value ? value.Add(new TimeSpan(0, 1, 0, 0)) : TimeSpan.MaxValue,
            (StswTimeSpanIncrementType.Minute, true) => TimeSpan.MaxValue.Add(new TimeSpan(0, 0, -1, 0)) >= value ? value.Add(new TimeSpan(0, 0, 1, 0)) : TimeSpan.MaxValue,
            (StswTimeSpanIncrementType.Second, true) => TimeSpan.MaxValue.Add(new TimeSpan(0, 0, 0, -1)) >= value ? value.Add(new TimeSpan(0, 0, 0, 1)) : TimeSpan.MaxValue,

            (StswTimeSpanIncrementType.Day, false) => TimeSpan.MinValue.Add(new TimeSpan(1, 0, 0, 0)) <= value ? value.Add(new TimeSpan(-1, 0, 0, 0)) : TimeSpan.MinValue,
            (StswTimeSpanIncrementType.Hour, false) => TimeSpan.MinValue.Add(new TimeSpan(0, 1, 0, 0)) <= value ? value.Add(new TimeSpan(0, -1, 0, 0)) : TimeSpan.MinValue,
            (StswTimeSpanIncrementType.Minute, false) => TimeSpan.MinValue.Add(new TimeSpan(0, 0, 1, 0)) <= value ? value.Add(new TimeSpan(0, 0, -1, 0)) : TimeSpan.MinValue,
            (StswTimeSpanIncrementType.Second, false) => TimeSpan.MinValue.Add(new TimeSpan(0, 0, 0, 1)) <= value ? value.Add(new TimeSpan(0, 0, 0, -1)) : TimeSpan.MinValue,
            _ => value
        };
    }

    /// <summary>
    /// Adjusts the visibility of the hour, minute, and second selectors based on the current <see cref="Format"/>.
    /// </summary>
    private void UpdateVisibilityBasedOnFormat()
    {
        if (string.IsNullOrEmpty(Format))
        {
            IsHoursVisible = true;
            IsMinutesVisible = true;
            IsSecondsVisible = true;
            return;
        }

        switch (Format)
        {
            case "c":
            case "G":
            case "t":
            case "T":
                IsHoursVisible = true;
                IsMinutesVisible = true;
                IsSecondsVisible = true;
                return;
            case "f":
            case "F":
            case "g":
                IsHoursVisible = true;
                IsMinutesVisible = true;
                IsSecondsVisible = false;
                return;
        }

        IsHoursVisible = Format.Contains('h');
        IsMinutesVisible = Format.Contains('m');
        IsSecondsVisible = Format.Contains('s');
    }

    #endregion
}
