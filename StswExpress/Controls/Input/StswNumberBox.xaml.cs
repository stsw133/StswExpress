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
public abstract class StswNumberBoxBase<T> : StswBoxBase where T : struct, INumber<T>
{
    static StswNumberBoxBase()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswNumberBoxBase<T>), new FrameworkPropertyMetadata(typeof(StswNumberBoxBase<T>)));
    }

    #region Events & methods
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
        var isComputed = false;
        var isInvalid = false;
        var isPlainNumber = false;

        T? result = Value;

        if (string.IsNullOrEmpty(Text))
        {
            result = null;
        }
        else if (TryParse(Text, out var parsed))
        {
            isPlainNumber = true;
            result = parsed;
        }
        else if (StswMath.TryCompute(Text, CultureInfo.CurrentCulture, out var computedValue))
        {
            try
            {
                result = T.CreateChecked(computedValue);
                isComputed = true;
            }
            catch (OverflowException)
            {
                isInvalid = true;
            }
        }
        else
        {
            isInvalid = true;
        }

        if (!EqualityComparer<T?>.Default.Equals(result, Value) || alwaysUpdate)
        {
            Value = result;

            var textBE = GetBindingExpression(TextProperty);
            var valueBE = GetBindingExpression(ValueProperty);

            if (!isInvalid && valueBE != null && valueBE.Status == BindingStatus.Active)
                valueBE.UpdateSource();

            if (textBE != null && textBE.Status is BindingStatus.Active or BindingStatus.UpdateSourceError)
            {
                if (string.IsNullOrEmpty(Text) || isPlainNumber)
                    textBE.UpdateSource();
                else if (isComputed)
                    textBE.UpdateTarget();
                else if (isInvalid && alwaysUpdate)
                    textBE.UpdateSource();
            }
        }
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the numeric format used for displaying values (e.g., "N2" for two decimal places, "C2" for currency).
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
    public static void OnFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswNumberBoxBase<T> stsw)
            return;

        stsw.FormatChanged(stsw.Format);
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
    public static void OnMinMaxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswNumberBoxBase<T> stsw)
            return;

        if (stsw.Value != null && !stsw.Value.Between(stsw.Minimum, stsw.Maximum))
            stsw.Value = stsw.MinMaxValidate(stsw.Value.GetValueOrDefault());
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
                null, OnValueChanging, false, UpdateSourceTrigger.PropertyChanged)
        );
    private static object? OnValueChanging(DependencyObject d, object? baseValue)
    {
        if (d is not StswNumberBoxBase<T> stsw)
            return baseValue;

        return stsw.MinMaxValidate((T?)baseValue);
    }
    #endregion
}

/// <summary>
/// Represents a control that allows users to provide value either by entering numeric value or using a "Up" and "Down" buttons.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswDecimalBox Value="{Binding Price}" Format="C2" Increment="0.01" Minimum="0"/&gt;
/// </code>
/// </example>
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
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswDoubleBox Value="{Binding Price}" Format="C2" Increment="0.01" Minimum="0"/&gt;
/// </code>
/// </example>
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
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswIntegerBox Value="{Binding Quantity}" Increment="1" Minimum="0"/&gt;
/// </code>
/// </example>
public class StswIntegerBox : StswNumberBoxBase<int>
{
    static StswIntegerBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswIntegerBox), new FrameworkPropertyMetadata(typeof(StswIntegerBox)));
    }
}
