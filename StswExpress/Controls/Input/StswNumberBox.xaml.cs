using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// A numeric input control allowing users to enter a number manually or adjust the value using up/down buttons.
/// Supports custom formats, increment steps, and min/max value validation.
/// </summary>
[ContentProperty(nameof(Value))]
[Stsw("0.9.0", Changes = StswPlannedChanges.None)]
public abstract class StswNumberBoxBase<T> : StswBoxBase where T : struct, INumber<T>
{
    static StswNumberBoxBase()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswNumberBoxBase<T>), new FrameworkPropertyMetadata(typeof(StswNumberBoxBase<T>)));
    }

    #region Events & methods
    /// <summary>
    /// Occurs when the value of the control changes.
    /// </summary>
    public event EventHandler? ValueChanged;

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// Button: up
        if (GetTemplateChild("PART_ButtonUp") is ButtonBase btnUp)
            btnUp.Click += PART_ButtonUp_Click;
        /// Button: down
        if (GetTemplateChild("PART_ButtonDown") is ButtonBase btnDown)
            btnDown.Click += PART_ButtonDown_Click;

        OnFormatChanged(this, new DependencyPropertyChangedEventArgs());
    }

    private static bool TryParse(string? text, out T result) => T.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out result);
    private static T Add(T a, T b) => a + b;
    private static T Subtract(T a, T b) => a - b;
    private static bool IsZero(T value) => value == T.Zero;
    private static int Compare(T a, T b) => a.CompareTo(b);

    /// <summary>
    /// Handles the click event for the "Up" button, incrementing the numeric value.
    /// The increment is determined by the <see cref="Increment"/> property.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void PART_ButtonUp_Click(object sender, RoutedEventArgs e)
    {
        var result = Value ?? default;

        if (TryParse(Text, out var res))
            result = res;

        Value = IsZero(Increment) ? Add(result, T.One) : Add(result, Increment);

        Focus();
        CaretIndex = Text?.Length ?? 0;
    }

    /// <summary>
    /// Handles the click event for the "Down" button, decrementing the numeric value.
    /// The decrement is determined by the <see cref="Increment"/> property.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void PART_ButtonDown_Click(object sender, RoutedEventArgs e)
    {
        var result = Value ?? default;

        if (TryParse(Text, out var res))
            result = res;

        Value = IsZero(Increment) ? Subtract(result, T.One) : Subtract(result, Increment);

        Focus();
        CaretIndex = Text?.Length ?? 0;
    }

    /// <inheritdoc/>
    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        if (IsKeyboardFocused == true && !IsReadOnly && !IsZero(Increment))
        {
            if (TryParse(Text, out var res))
            {
                Value = e.Delta > 0 ? Add(res, Increment) : Subtract(res, Increment);

                e.Handled = true;
            }
        }
    }

    /// <summary>
    /// Ensures that the provided value does not exceed the <see cref="Maximum"/> or fall below the <see cref="Minimum"/>.
    /// Returns the validated value.
    /// </summary>
    private T? MinMaxValidate(T? newValue)
    {
        if (newValue == null)
            return newValue;

        if (Minimum.HasValue && Compare(newValue.GetValueOrDefault(), Minimum.Value) < 0)
            newValue = Minimum.Value;

        if (Maximum.HasValue && Compare(newValue.GetValueOrDefault(), Maximum.Value) > 0)
            newValue = Maximum.Value;

        return newValue;
    }

    /// <inheritdoc/>
    protected override void UpdateMainProperty(bool alwaysUpdate)
    {
        var result = Value;

        if (string.IsNullOrEmpty(Text))
            result = null;
        else if (TryParse(Text, out var res))
            result = res;
        else if (StswCalculator.TryCompute(Text, out var computedValue))
            result = T.CreateChecked(computedValue);

        if (!EqualityComparer<T?>.Default.Equals(result, Value) || alwaysUpdate)
        {
            Value = result;

            var bindingExpression = GetBindingExpression(TextProperty);
            if (bindingExpression != null && bindingExpression.Status.In(BindingStatus.Active, BindingStatus.UpdateSourceError))
                bindingExpression.UpdateSource();
        }
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the custom numeric format string used to display the value in the control.
    /// Example: "C2" for currency, "N0" for whole numbers.
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
            typeof(StswNumberBoxBase<T>),
            new FrameworkPropertyMetadata(default(string?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnFormatChanged)
        );
    public static void OnFormatChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswNumberBoxBase<T> stsw)
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
    /// Gets or sets the step value used when adjusting the number using the up/down buttons or mouse wheel.
    /// </summary>
    public T Increment
    {
        get => (T)GetValue(IncrementProperty);
        set => SetValue(IncrementProperty, value);
    }
    public static readonly DependencyProperty IncrementProperty
        = DependencyProperty.Register(
            nameof(Increment),
            typeof(T),
            typeof(StswNumberBoxBase<T>)
        );

    /// <summary>
    /// Gets or sets the maximum allowable value in the control. 
    /// The input value will be clamped to this maximum if exceeded.
    /// </summary>
    public T? Maximum
    {
        get => (T?)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }
    public static readonly DependencyProperty MaximumProperty
        = DependencyProperty.Register(
            nameof(Maximum),
            typeof(T?),
            typeof(StswNumberBoxBase<T>),
            new PropertyMetadata(default(T?), OnMinMaxChanged)
        );
    public static void OnMinMaxChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswNumberBoxBase<T> stsw)
        {
            if (stsw.Value != null && !stsw.Value.Between(stsw.Minimum, stsw.Maximum))
                stsw.Value = stsw.MinMaxValidate(stsw.Value.GetValueOrDefault());
        }
    }

    /// <summary>
    /// Gets or sets the minimum allowable value in the control. 
    /// The input value will be clamped to this minimum if lower.
    /// </summary>
    public T? Minimum
    {
        get => (T?)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }
    public static readonly DependencyProperty MinimumProperty
        = DependencyProperty.Register(
            nameof(Minimum),
            typeof(T?),
            typeof(StswNumberBoxBase<T>),
            new PropertyMetadata(default(T?), OnMinMaxChanged)
        );

    /// <summary>
    /// Gets or sets the numeric value of the control. 
    /// Supports data binding and updates when the user enters a new value or uses increment/decrement controls.
    /// </summary>
    public T? Value
    {
        get => (T?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    public static readonly DependencyProperty ValueProperty
        = DependencyProperty.Register(
            nameof(Value),
            typeof(T?),
            typeof(StswNumberBoxBase<T>),
            new FrameworkPropertyMetadata(default(T?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnValueChanged, OnValueChanging, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswNumberBoxBase<T> stsw)
        {
            /// event for non MVVM programming
            stsw.ValueChanged?.Invoke(stsw, new StswValueChangedEventArgs<T?>((T?)e.OldValue, (T?)e.NewValue));
        }
    }
    private static object? OnValueChanging(DependencyObject obj, object? baseValue)
    {
        if (obj is StswNumberBoxBase<T> stsw)
            return stsw.MinMaxValidate((T?)baseValue);
        
        return baseValue;
    }
    #endregion
}

/* usage:

<se:StswDecimalBox Value="{Binding Price}" Format="C2" Increment="0.01" Minimum="0"/>

*/

/// <summary>
/// Represents a control that allows users to provide value either by entering numeric value or using a "Up" and "Down" buttons.
/// </summary>
[Stsw("0.9.0", Changes = StswPlannedChanges.None)]
public class StswDecimalBox : StswNumberBoxBase<decimal>
{
    static StswDecimalBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswDecimalBox), new FrameworkPropertyMetadata(typeof(StswDecimalBox)));
    }
}

/// <summary>
/// Represents a control that allows users to provide value either by entering numeric value or using a "Up" and "Down" buttons.
/// </summary>
[Stsw("0.14.0", Changes = StswPlannedChanges.None)]
public class StswDoubleBox : StswNumberBoxBase<double>
{
    static StswDoubleBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswDoubleBox), new FrameworkPropertyMetadata(typeof(StswDoubleBox)));
    }
}

/// <summary>
/// Represents a control that allows users to provide value either by entering numeric value or using a "Up" and "Down" buttons.
/// </summary>
[Stsw("0.14.0", Changes = StswPlannedChanges.None)]
public class StswIntegerBox : StswNumberBoxBase<int>
{
    static StswIntegerBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswIntegerBox), new FrameworkPropertyMetadata(typeof(StswIntegerBox)));
    }
}
