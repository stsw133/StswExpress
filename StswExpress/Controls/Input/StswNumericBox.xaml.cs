using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// Represents a control that allows users to provide value either by entering numeric value or using a "Up" and "Down" buttons.
/// </summary>
[ContentProperty(nameof(Value))]
public class StswNumericBox : StswBoxBase
{
    static StswNumericBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswNumericBox), new FrameworkPropertyMetadata(typeof(StswNumericBox)));
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

    /// <summary>
    /// Handles the click event for the "Up" button, incrementing the numeric value.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void PART_ButtonUp_Click(object sender, RoutedEventArgs e)
    {
        if (decimal.TryParse(Text, out var result))
            Value = result;
        Value = Value + Increment ?? 0;
    }

    /// <summary>
    /// Handles the click event for the "Down" button, decrementing the numeric value.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void PART_ButtonDown_Click(object sender, RoutedEventArgs e)
    {
        if (decimal.TryParse(Text, out var result))
            Value = result;
        Value = Value - Increment ?? 0;
    }

    /// <summary>
    /// Handles the MouseWheel event for the content, incrementing or decrementing the numeric value based on the wheel movement.
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        if (IsKeyboardFocused && !IsReadOnly && Increment != 0 && decimal.TryParse(Text, out var result))
        {
            if (e.Delta > 0)
            {
                if (decimal.MaxValue - Increment >= result)
                    result += Increment;
                else
                    result = decimal.MaxValue;
            }
            else //if (e.Delta < 0)
            {
                if (decimal.MinValue + Increment <= result)
                    result -= Increment;
                else
                    result = decimal.MinValue;
            }
            Value = result;

            e.Handled = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private decimal? MinMaxValidate(decimal? newValue)
    {
        if (newValue.HasValue)
        {
            if (Minimum.HasValue && newValue < Minimum)
                newValue = Minimum;
            if (Maximum.HasValue && newValue > Maximum)
                newValue = Maximum;
        }
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
        else if (StswFn.TryCalculateString(Text, out var res))
            result = res;
        else if (decimal.TryParse(Text, out res))
            result = res;

        if (result != Value || alwaysUpdate)
        {
            Text = result?.ToString(Format);

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
            typeof(StswNumericBox),
            new PropertyMetadata(default(string?), OnFormatChanged)
        );
    public static void OnFormatChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswNumericBox stsw)
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
    public decimal Increment
    {
        get => (decimal)GetValue(IncrementProperty);
        set => SetValue(IncrementProperty, value);
    }
    public static readonly DependencyProperty IncrementProperty
        = DependencyProperty.Register(
            nameof(Increment),
            typeof(decimal),
            typeof(StswNumericBox)
        );

    /// <summary>
    /// Gets or sets the maximum allowable value in the control.
    /// </summary>
    public decimal? Maximum
    {
        get => (decimal?)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }
    public static readonly DependencyProperty MaximumProperty
        = DependencyProperty.Register(
            nameof(Maximum),
            typeof(decimal?),
            typeof(StswNumericBox),
            new PropertyMetadata(default(decimal?), OnMinMaxChanged)
        );
    public static void OnMinMaxChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswNumericBox stsw)
        {
            stsw.Value = stsw.MinMaxValidate(stsw.Value);
        }
    }

    /// <summary>
    /// Gets or sets the minimum allowable value in the control.
    /// </summary>
    public decimal? Minimum
    {
        get => (decimal?)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }
    public static readonly DependencyProperty MinimumProperty
        = DependencyProperty.Register(
            nameof(Minimum),
            typeof(decimal?),
            typeof(StswNumericBox),
            new PropertyMetadata(default(decimal?), OnMinMaxChanged)
        );

    /// <summary>
    /// Gets or sets the numeric value of the control.
    /// </summary>
    public decimal? Value
    {
        get => (decimal?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, MinMaxValidate(value));
    }
    public static readonly DependencyProperty ValueProperty
        = DependencyProperty.Register(
            nameof(Value),
            typeof(decimal?),
            typeof(StswNumericBox),
            new FrameworkPropertyMetadata(default(decimal?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnValueChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswNumericBox stsw)
        {
            stsw.ValueChanged?.Invoke(stsw, EventArgs.Empty);
        }
    }
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the thickness of the separator between box and drop-down button.
    /// </summary>
    public double SeparatorThickness
    {
        get => (double)GetValue(SeparatorThicknessProperty);
        set => SetValue(SeparatorThicknessProperty, value);
    }
    public static readonly DependencyProperty SeparatorThicknessProperty
        = DependencyProperty.Register(
            nameof(SeparatorThickness),
            typeof(double),
            typeof(StswNumericBox)
        );
    #endregion
}
