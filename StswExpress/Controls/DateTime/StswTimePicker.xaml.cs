using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// A time picker control that allows users to select a time using a text box and a drop-down time selector.
/// Supports different time formats, min/max validation, and incremental adjustments via mouse scroll.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswTimePicker SelectedTime="{Binding StartTime}" Format="HH:mm" IncrementType="Minute"/&gt;
/// </code>
/// </example>
[ContentProperty(nameof(SelectedTime))]
public class StswTimePicker : StswBoxBase
{
    static StswTimePicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswTimePicker), new FrameworkPropertyMetadata(typeof(StswTimePicker)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        OnFormatChanged(this, new DependencyPropertyChangedEventArgs());
    }
    /*
    /// <inheritdoc/>
    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Pressed)
            IncrementType = IncrementType.GetNextValue();
    }
    */
    /// <inheritdoc/>
    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        if (IsKeyboardFocused && !IsReadOnly && IncrementType != StswTimeSpanIncrementType.None && TimeSpan.TryParse(Text, out var result))
        {
            if (e.Delta > 0)
            {
                result = IncrementType switch
                {
                    StswTimeSpanIncrementType.Day => TimeSpan.MaxValue.Add(new(-1, 0, 0, 0)) >= result ? result.Add(new(1, 0, 0, 0)) : TimeSpan.MaxValue,
                    StswTimeSpanIncrementType.Hour => TimeSpan.MaxValue.Add(new(0, -1, 0, 0)) >= result ? result.Add(new(0, 1, 0, 0)) : TimeSpan.MaxValue,
                    StswTimeSpanIncrementType.Minute => TimeSpan.MaxValue.Add(new(0, 0, -1, 0)) >= result ? result.Add(new(0, 0, 1, 0)) : TimeSpan.MaxValue,
                    StswTimeSpanIncrementType.Second => TimeSpan.MaxValue.Add(new(0, 0, 0, -1)) >= result ? result.Add(new(0, 0, 0, 1)) : TimeSpan.MaxValue,
                    _ => result
                };
            }
            else
            {
                result = IncrementType switch
                {
                    StswTimeSpanIncrementType.Day => TimeSpan.MinValue.Add(new(1, 0, 0, 0)) <= result ? result.Add(new(-1, 0, 0, 0)) : TimeSpan.MinValue,
                    StswTimeSpanIncrementType.Hour => TimeSpan.MinValue.Add(new(0, 1, 0, 0)) <= result ? result.Add(new(0, -1, 0, 0)) : TimeSpan.MinValue,
                    StswTimeSpanIncrementType.Minute => TimeSpan.MinValue.Add(new(0, 0, 1, 0)) <= result ? result.Add(new(0, 0, -1, 0)) : TimeSpan.MinValue,
                    StswTimeSpanIncrementType.Second => TimeSpan.MinValue.Add(new(0, 0, 0, 1)) <= result ? result.Add(new(0, 0, 0, -1)) : TimeSpan.MinValue,
                    _ => result
                };
            }
            SelectedTime = result;

            e.Handled = true;
        }
    }

    /// <summary>
    /// Ensures that the provided time value is within the defined minimum and maximum limits.
    /// If the value is outside the allowed range, it is adjusted accordingly.
    /// </summary>
    /// <param name="newValue">The new time value to validate.</param>
    /// <returns>The validated <see cref="TimeSpan"/> value within the allowable range.</returns>
    private TimeSpan? MinMaxValidate(TimeSpan? newValue)
    {
        if (newValue == null)
            return newValue;

        if (Minimum.HasValue && newValue < Minimum)
            newValue = Minimum;

        if (Maximum.HasValue && newValue > Maximum)
            newValue = Maximum;

        return newValue;
    }

    /// <inheritdoc/>
    protected override void UpdateMainProperty(bool alwaysUpdate)
    {
        var isPlain = false;
        var isInvalid = false;

        var result = SelectedTime;

        if (string.IsNullOrWhiteSpace(Text))
        {
            result = null;
        }
        else if (Format != null && TimeSpan.TryParseExact(Text, Format, CultureInfo.CurrentCulture, TimeSpanStyles.None, out var tsExact))
        {
            isPlain = true;
            result = tsExact;
        }
        else if (TimeSpan.TryParse(Text, CultureInfo.CurrentCulture, out var ts))
        {
            isPlain = true;
            result = ts;
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
    /// Adjusts the visibility of the hour, minute, and second input fields based on the current <see cref="Format"/>.
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
            case "c":  // "c" = "[-][d.]hh:mm:ss[.fffffff]"
            case "G":  // "G" = "d:hh:mm:ss"
            case "t":  // "t" = "hh:mm:ss"
            case "T":  // "T" = "hh:mm:ss.fffffff"
                IsHoursVisible = true;
                IsMinutesVisible = true;
                IsSecondsVisible = true;
                return;
            case "f":  // "f" = "hh:mm"
            case "F":  // "F" = "hh:mm.fffffff"
            case "g":  // "g" = "d:hh:mm"
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

    #region Logic properties
    /// <summary>
    /// Gets or sets the format used for displaying the time value.
    /// The format follows standard time formatting conventions, such as "HH:mm".
    /// </summary>
    public string? Format
    {
        get => (string?)GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }
    public static readonly DependencyProperty FormatProperty
        = DependencyProperty.Register(
            nameof(Format),
            typeof(string),
            typeof(StswTimePicker),
            new FrameworkPropertyMetadata(default(string?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnFormatChanged)
        );
    public static void OnFormatChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswTimePicker stsw)
            return;

        stsw.UpdateVisibilityBasedOnFormat();
        stsw.FormatChanged(stsw.Format);
    }

    /// <summary>
    /// Gets or sets the increment type that determines how the time changes when scrolling with the mouse wheel.
    /// </summary>
    public StswTimeSpanIncrementType IncrementType
    {
        get => (StswTimeSpanIncrementType)GetValue(IncrementTypeProperty);
        set => SetValue(IncrementTypeProperty, value);
    }
    public static readonly DependencyProperty IncrementTypeProperty
        = DependencyProperty.Register(
            nameof(IncrementType),
            typeof(StswTimeSpanIncrementType),
            typeof(StswTimePicker)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the drop-down menu is currently open.
    /// </summary>
    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }
    public static readonly DependencyProperty IsDropDownOpenProperty
        = DependencyProperty.Register(
            nameof(IsDropDownOpen),
            typeof(bool),
            typeof(StswTimePicker)
        );

    /// <summary>
    /// Gets a value indicating whether the hours input field is visible based on the selected <see cref="Format"/>.
    /// </summary>
    public bool IsHoursVisible
    {
        get => (bool)GetValue(IsHoursVisibleProperty);
        private set => SetValue(IsHoursVisibleProperty, value);
    }
    public static readonly DependencyProperty IsHoursVisibleProperty
        = DependencyProperty.Register(
            nameof(IsHoursVisible),
            typeof(bool),
            typeof(StswTimePicker),
            new PropertyMetadata(true)
        );

    /// <summary>
    /// Gets a value indicating whether the minutes input field is visible based on the selected <see cref="Format"/>.
    /// </summary>
    public bool IsMinutesVisible
    {
        get => (bool)GetValue(IsMinutesVisibleProperty);
        private set => SetValue(IsMinutesVisibleProperty, value);
    }
    public static readonly DependencyProperty IsMinutesVisibleProperty
        = DependencyProperty.Register(
            nameof(IsMinutesVisible),
            typeof(bool),
            typeof(StswTimePicker),
            new PropertyMetadata(true)
        );

    /// <summary>
    /// Gets a value indicating whether the seconds input field is visible based on the selected <see cref="Format"/>.
    /// </summary>
    public bool IsSecondsVisible
    {
        get => (bool)GetValue(IsSecondsVisibleProperty);
        private set => SetValue(IsSecondsVisibleProperty, value);
    }
    public static readonly DependencyProperty IsSecondsVisibleProperty
        = DependencyProperty.Register(
            nameof(IsSecondsVisible),
            typeof(bool),
            typeof(StswTimePicker),
            new PropertyMetadata(true)
        );

    /// <summary>
    /// Gets or sets the maximum allowable time in the control.
    /// </summary>
    public TimeSpan? Maximum
    {
        get => (TimeSpan?)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }
    public static readonly DependencyProperty MaximumProperty
        = DependencyProperty.Register(
            nameof(Maximum),
            typeof(TimeSpan?),
            typeof(StswTimePicker),
            new PropertyMetadata(default(TimeSpan?), OnMinMaxChanged)
        );
    public static void OnMinMaxChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswTimePicker stsw)
            return;

        if (stsw.SelectedTime != null && !stsw.SelectedTime.Between(stsw.Minimum, stsw.Maximum))
            stsw.SelectedTime = stsw.MinMaxValidate(stsw.SelectedTime);
    }

    /// <summary>
    /// Gets or sets the minimum allowable time in the control.
    /// </summary>
    public TimeSpan? Minimum
    {
        get => (TimeSpan?)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }
    public static readonly DependencyProperty MinimumProperty
        = DependencyProperty.Register(
            nameof(Minimum),
            typeof(TimeSpan?),
            typeof(StswTimePicker),
            new PropertyMetadata(default(TimeSpan?), OnMinMaxChanged)
        );

    /// <summary>
    /// Gets or sets the currently selected time in the control.
    /// </summary>
    public TimeSpan? SelectedTime
    {
        get => (TimeSpan?)GetValue(SelectedTimeProperty);
        set => SetValue(SelectedTimeProperty, value);
    }
    public static readonly DependencyProperty SelectedTimeProperty
        = DependencyProperty.Register(
            nameof(SelectedTime),
            typeof(TimeSpan?),
            typeof(StswTimePicker),
            new FrameworkPropertyMetadata(default(TimeSpan?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedTimeChanged, OnSelectedTimeChanging, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnSelectedTimeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswTimePicker stsw)
            return;

        if (!stsw._isTimeChanging)
        {
            stsw._isTimeChanging = true;
            if (stsw.SelectedTime.HasValue)
            {
                stsw.SelectedTimeH = stsw.SelectedTime.Value.Hours;
                stsw.SelectedTimeM = stsw.SelectedTime.Value.Minutes;
                stsw.SelectedTimeS = stsw.SelectedTime.Value.Seconds;
            }
            stsw._isTimeChanging = false;
        }
    }
    private static object? OnSelectedTimeChanging(DependencyObject obj, object? baseValue)
    {
        if (obj is not StswTimePicker stsw)
            return baseValue;

        return stsw.MinMaxValidate((TimeSpan?)baseValue);
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
    internal static readonly DependencyProperty SelectedTimeHProperty
        = DependencyProperty.Register(
            nameof(SelectedTimeH),
            typeof(int),
            typeof(StswTimePicker),
            new FrameworkPropertyMetadata(default(int),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedTimeHChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnSelectedTimeHChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswTimePicker stsw)
            return;

        if (stsw.SelectedTime.HasValue)
        {
            var t = stsw.SelectedTime.Value;
            stsw.SelectedTime = new TimeSpan(t.Days, stsw.SelectedTimeH, t.Minutes, t.Seconds, t.Milliseconds);
        }
        else stsw.SelectedTime = new TimeSpan(stsw.SelectedTimeH, 0, 0);
    }

    /// <summary>
    /// Gets or sets the currently selected minute for the selected time.
    /// </summary>
    internal int SelectedTimeM
    {
        get => (int)GetValue(SelectedTimeMProperty);
        set => SetValue(SelectedTimeMProperty, value);
    }
    internal static readonly DependencyProperty SelectedTimeMProperty
        = DependencyProperty.Register(
            nameof(SelectedTimeM),
            typeof(int),
            typeof(StswTimePicker),
            new FrameworkPropertyMetadata(default(int),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedTimeMChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnSelectedTimeMChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswTimePicker stsw)
            return;

        if (stsw.SelectedTime.HasValue)
        {
            var t = stsw.SelectedTime.Value;
            stsw.SelectedTime = new TimeSpan(t.Days, t.Hours, stsw.SelectedTimeM, t.Seconds, t.Milliseconds);
        }
        else stsw.SelectedTime = new TimeSpan(0, stsw.SelectedTimeM, 0);
    }

    /// <summary>
    /// Gets or sets the currently selected second for the selected time.
    /// </summary>
    internal int SelectedTimeS
    {
        get => (int)GetValue(SelectedTimeSProperty);
        set => SetValue(SelectedTimeSProperty, value);
    }
    internal static readonly DependencyProperty SelectedTimeSProperty
        = DependencyProperty.Register(
            nameof(SelectedTimeS),
            typeof(int),
            typeof(StswTimePicker),
            new FrameworkPropertyMetadata(default(int),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedTimeSChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnSelectedTimeSChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswTimePicker stsw)
            return;

        if (stsw.SelectedTime.HasValue)
        {
            var t = stsw.SelectedTime.Value;
            stsw.SelectedTime = new TimeSpan(t.Days, t.Hours, t.Minutes, stsw.SelectedTimeS, t.Milliseconds);
        }
        else stsw.SelectedTime = new TimeSpan(0, 0, stsw.SelectedTimeS);
    }
    #endregion
}
