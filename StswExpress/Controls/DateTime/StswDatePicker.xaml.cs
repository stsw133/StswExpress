using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// A date picker control that allows users to select a date using a text box and a drop-down calendar.
/// Supports custom date formats, min/max date validation, and incremental adjustments via mouse scroll.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswDatePicker SelectedDate="{Binding BirthDate}" Format="dd/MM/yyyy" IncrementType="Day"/&gt;
/// </code>
/// </example>
[ContentProperty(nameof(SelectedDate))]
[StswInfo(null)]
public class StswDatePicker : StswBoxBase
{
    static StswDatePicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswDatePicker), new FrameworkPropertyMetadata(typeof(StswDatePicker)));
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

        if (IsKeyboardFocused && !IsReadOnly && IncrementType != StswDateTimeIncrementType.None)
        {
            if (DateTime.TryParse(Text, out var result))
            {
                if (e.Delta > 0)
                {
                    result = IncrementType switch
                    {
                        StswDateTimeIncrementType.Year => DateTime.MaxValue.AddYears(-1) >= result ? result.AddYears(1) : DateTime.MaxValue,
                        StswDateTimeIncrementType.Month => DateTime.MaxValue.AddMonths(-1) >= result ? result.AddMonths(1) : DateTime.MaxValue,
                        StswDateTimeIncrementType.Day => DateTime.MaxValue.AddDays(-1) >= result ? result.AddDays(1) : DateTime.MaxValue,
                        StswDateTimeIncrementType.Hour => DateTime.MaxValue.AddHours(-1) >= result ? result.AddHours(1) : DateTime.MaxValue,
                        StswDateTimeIncrementType.Minute => DateTime.MaxValue.AddMinutes(-1) >= result ? result.AddMinutes(1) : DateTime.MaxValue,
                        StswDateTimeIncrementType.Second => DateTime.MaxValue.AddSeconds(-1) >= result ? result.AddSeconds(1) : DateTime.MaxValue,
                        _ => result
                    };
                }
                else
                {
                    result = IncrementType switch
                    {
                        StswDateTimeIncrementType.Year => DateTime.MinValue.AddYears(1) <= result ? result.AddYears(-1) : DateTime.MinValue,
                        StswDateTimeIncrementType.Month => DateTime.MinValue.AddMonths(1) <= result ? result.AddMonths(-1) : DateTime.MinValue,
                        StswDateTimeIncrementType.Day => DateTime.MinValue.AddDays(1) <= result ? result.AddDays(-1) : DateTime.MinValue,
                        StswDateTimeIncrementType.Hour => DateTime.MinValue.AddHours(1) <= result ? result.AddHours(-1) : DateTime.MinValue,
                        StswDateTimeIncrementType.Minute => DateTime.MinValue.AddMinutes(1) <= result ? result.AddMinutes(-1) : DateTime.MinValue,
                        StswDateTimeIncrementType.Second => DateTime.MinValue.AddSeconds(1) <= result ? result.AddSeconds(-1) : DateTime.MinValue,
                        _ => result
                    };
                }
                SelectedDate = result;

                e.Handled = true;
            }
        }
    }

    /// <summary>
    /// Ensures the provided date value is within the defined minimum and maximum limits.
    /// If the value is outside the allowed range, it is adjusted accordingly.
    /// </summary>
    /// <param name="newValue">The new date value to validate.</param>
    /// <returns>The validated <see cref="DateTime"/> value within the allowable range.</returns>
    private DateTime? MinMaxValidate(DateTime? newValue)
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
        var result = SelectedDate;

        if (string.IsNullOrEmpty(Text))
            result = null;
        else if (Format != null && DateTime.TryParseExact(Text, Format, CultureInfo.CurrentCulture, DateTimeStyles.None, out var res))
            result = res;
        else if (DateTime.TryParse(Text, out res))
            result = res;

        if (result != SelectedDate || alwaysUpdate)
        {
            SelectedDate = result;

            var bindingExpression = GetBindingExpression(TextProperty);
            if (bindingExpression != null && bindingExpression.Status.In(BindingStatus.Active, BindingStatus.UpdateSourceError))
                bindingExpression.UpdateSource();
        }
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the custom date and time format string used to display the selected date in the control.
    /// When set, the date is formatted according to the provided format string.
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
            typeof(StswDatePicker),
            new FrameworkPropertyMetadata(default(string?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnFormatChanged)
        );
    public static void OnFormatChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswDatePicker stsw)
            return;

        stsw.Format ??= "d";
        if (stsw.GetBindingExpression(TextProperty)?.ParentBinding is Binding binding)
        {
            var newBinding = binding.Clone();
            newBinding.StringFormat = stsw.Format;
            stsw.SetBinding(TextProperty, newBinding);
        }
    }

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
            typeof(StswDatePicker)
        );

    /// <summary>
    /// Gets or sets the increment type that determines how the date changes when scrolling with the mouse wheel.
    /// </summary>
    public StswDateTimeIncrementType IncrementType
    {
        get => (StswDateTimeIncrementType)GetValue(IncrementTypeProperty);
        set => SetValue(IncrementTypeProperty, value);
    }
    public static readonly DependencyProperty IncrementTypeProperty
        = DependencyProperty.Register(
            nameof(IncrementType),
            typeof(StswDateTimeIncrementType),
            typeof(StswDatePicker)
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
            typeof(StswDatePicker),
            new PropertyMetadata(default(DateTime?), OnMinMaxChanged)
        );
    public static void OnMinMaxChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswDatePicker stsw)
            return;

        if (stsw.SelectedDate != null && !stsw.SelectedDate.Between(stsw.Minimum, stsw.Maximum))
            stsw.SelectedDate = stsw.MinMaxValidate(stsw.SelectedDate);
    }

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
            typeof(StswDatePicker),
            new PropertyMetadata(default(DateTime?), OnMinMaxChanged)
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
            typeof(StswDatePicker),
            new FrameworkPropertyMetadata(default(DateTime?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                null, OnSelectedDateChanging, false, UpdateSourceTrigger.PropertyChanged)
        );
    private static object? OnSelectedDateChanging(DependencyObject obj, object? baseValue)
    {
        if (obj is not StswDatePicker stsw)
            return baseValue;

        return stsw.MinMaxValidate((DateTime?)baseValue);
    }

    /// <summary>
    /// Gets or sets the selection unit of the control.
    /// Determines whether the user selects an individual day or an entire month.
    /// </summary>
    [StswInfo("0.12.0")]
    public StswCalendarUnit SelectionUnit
    {
        get => (StswCalendarUnit)GetValue(SelectionUnitProperty);
        set => SetValue(SelectionUnitProperty, value);
    }
    public static readonly DependencyProperty SelectionUnitProperty
        = DependencyProperty.Register(
            nameof(SelectionUnit),
            typeof(StswCalendarUnit),
            typeof(StswDatePicker)
        );
    #endregion
}
