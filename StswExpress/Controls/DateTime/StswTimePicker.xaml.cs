using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// A control that allows users to select and display time.
/// </summary>
[ContentProperty(nameof(SelectedTime))]
public class StswTimePicker : StswBoxBase
{
    static StswTimePicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswTimePicker), new FrameworkPropertyMetadata(typeof(StswTimePicker)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswTimePicker), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    #region Events & methods
    /// <summary>
    /// Occurs when the selected time in the control changes.
    /// </summary>
    public event EventHandler? SelectedTimeChanged;

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        OnFormatChanged(this, new DependencyPropertyChangedEventArgs());
    }
    /*
    /// <summary>
    /// Handles the MouseDown event for the internal content host of the time picker.
    /// If the Middle mouse button is pressed, the IncrementType value is changed.
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Pressed)
            IncrementType = IncrementType.GetNextValue();
    }
    */
    /// <summary>
    /// Handles the MouseWheel event for the internal content host of the time picker.
    /// Adjusts the selected time based on the mouse wheel's scrolling direction and the IncrementType property.
    /// </summary>
    /// <param name="e">The event arguments</param>
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
    /// 
    /// </summary>
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

    /// <summary>
    /// Updates the main property associated with the selected time in the control based on user input.
    /// </summary>
    /// <param name="alwaysUpdate">A value indicating whether to force a binding update regardless of changes.</param>
    protected override void UpdateMainProperty(bool alwaysUpdate)
    {
        var result = SelectedTime;

        if (string.IsNullOrEmpty(Text))
            result = null;
        else if (Format != null && TimeSpan.TryParseExact(Text, Format, CultureInfo.CurrentCulture, TimeSpanStyles.None, out var res))
            result = res;
        else if (TimeSpan.TryParse(Text, out res))
            result = res;

        if (result != SelectedTime || alwaysUpdate)
        {
            SelectedTime = result;

            var bindingExpression = GetBindingExpression(TextProperty);
            if (bindingExpression != null && bindingExpression.Status.In(BindingStatus.Active, BindingStatus.UpdateSourceError))
                bindingExpression.UpdateSource();
        }
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the custom time and time format string used to display the selected time in the control.
    /// When set, the time is formatted according to the provided format string.
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
            new PropertyMetadata(default(string?), OnFormatChanged)
        );
    public static void OnFormatChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswTimePicker stsw)
        {
            if (stsw.GetBindingExpression(TextProperty)?.ParentBinding is Binding binding)
            {
                var newBinding = binding.Clone();
                newBinding.StringFormat = stsw.Format;
                stsw.SetBinding(TextProperty, newBinding);
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether or not the drop-down portion of the control is currently open.
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
    /// Gets or sets the type of increment to be applied when scrolling the mouse wheel over the time picker.
    /// This property defines how the time changes when the mouse wheel is scrolled up or down.
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
        if (obj is StswTimePicker stsw)
        {
            if (stsw.SelectedTime != null && !stsw.SelectedTime.Between(stsw.Minimum, stsw.Maximum))
                stsw.SelectedTime = stsw.MinMaxValidate(stsw.SelectedTime);
        }
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
        if (obj is StswTimePicker stsw)
        {
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

                /// event for non MVVM programming
                stsw.SelectedTimeChanged?.Invoke(stsw, new StswValueChangedEventArgs<TimeSpan?>((TimeSpan?)e.OldValue, (TimeSpan?)e.NewValue));
            }
        }
    }
    private static object? OnSelectedTimeChanging(DependencyObject obj, object? baseValue)
    {
        if (obj is StswTimePicker stsw)
            return stsw.MinMaxValidate((TimeSpan?)baseValue);

        return baseValue;
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
        if (obj is StswTimePicker stsw)
        {
            if (stsw.SelectedTime.HasValue)
            {
                var t = stsw.SelectedTime.Value;
                stsw.SelectedTime = new TimeSpan(t.Days, stsw.SelectedTimeH, t.Minutes, t.Seconds, t.Milliseconds);
            }
            else stsw.SelectedTime = new TimeSpan(stsw.SelectedTimeH, 0, 0);
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
        if (obj is StswTimePicker stsw)
        {
            if (stsw.SelectedTime.HasValue)
            {
                var t = stsw.SelectedTime.Value;
                stsw.SelectedTime = new TimeSpan(t.Days, t.Hours, stsw.SelectedTimeM, t.Seconds, t.Milliseconds);
            }
            else stsw.SelectedTime = new TimeSpan(0, stsw.SelectedTimeM, 0);
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
        if (obj is StswTimePicker stsw)
        {
            if (stsw.SelectedTime.HasValue)
            {
                var t = stsw.SelectedTime.Value;
                stsw.SelectedTime = new TimeSpan(t.Days, t.Hours, t.Minutes, stsw.SelectedTimeS, t.Milliseconds);
            }
            else stsw.SelectedTime = new TimeSpan(0, 0, stsw.SelectedTimeS);
        }
    }
    #endregion
}
