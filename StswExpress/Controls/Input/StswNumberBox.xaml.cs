using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// Represents a control that allows users to provide value either by entering numeric value or using a "Up" and "Down" buttons.
/// </summary>
[ContentProperty(nameof(Value))]
public abstract class StswNumberBoxBase<T> : StswBoxBase where T : struct, INumber<T>
{
    static StswNumberBoxBase()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswNumberBoxBase<T>), new FrameworkPropertyMetadata(typeof(StswNumberBoxBase<T>)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswNumberBoxBase<T>), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    #region Events & methods
    /// <summary>
    /// Occurs when the value of the control changes.
    /// </summary>
    public event EventHandler? ValueChanged;

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
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
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void PART_ButtonUp_Click(object sender, RoutedEventArgs e)
    {
        var result = Value ?? default;

        if (TryParse(Text, out var res))
            result = res;

        Value = Add(result, Increment);
    }

    /// <summary>
    /// Handles the click event for the "Down" button, decrementing the numeric value.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void PART_ButtonDown_Click(object sender, RoutedEventArgs e)
    {
        var result = Value ?? default;

        if (TryParse(Text, out var res))
            result = res;

        Value = Subtract(result, Increment);
    }

    /// <summary>
    /// Handles the MouseWheel event for the content, incrementing or decrementing the numeric value based on the wheel movement.
    /// </summary>
    /// <param name="e">The event arguments</param>
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
    /// 
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

    /// <summary>
    /// Updates the main property associated with the selected value in the control based on user input.
    /// </summary>
    /// <param name="alwaysUpdate">A value indicating whether to force a binding update regardless of changes.</param>
    protected override void UpdateMainProperty(bool alwaysUpdate)
    {
        var result = Value;

        if (string.IsNullOrEmpty(Text))
            result = null;
        else if (TryParse(Text, out var res))
            result = res;
        else if (StswFn.TryCompute(Text, out var computedValue))
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
    /// Gets or sets the custom number format string used to display the value in the control.
    /// When set, the value is formatted according to the provided format string.
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
            new PropertyMetadata(default(string?), OnFormatChanged)
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
    /// Gets or sets the increment value used when clicking the "Up" or "Down" button.
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
            stsw.ValueChanged?.Invoke(stsw, EventArgs.Empty);
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

/// <summary>
/// Represents a control that allows users to provide value either by entering numeric value or using a "Up" and "Down" buttons.
/// </summary>
public class StswDecimalBox : StswNumberBoxBase<decimal>
{
    static StswDecimalBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswDecimalBox), new FrameworkPropertyMetadata(typeof(StswDecimalBox)));
    }
}
/*
/// <summary>
/// Represents a control that allows users to provide value either by entering numeric value or using a "Up" and "Down" buttons.
/// </summary>
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
public class StswIntegerBox : StswNumberBoxBase<int>
{
    static StswIntegerBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswIntegerBox), new FrameworkPropertyMetadata(typeof(StswIntegerBox)));
    }
}
*/
