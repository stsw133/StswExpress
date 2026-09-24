using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace StswExpress.Wpf;

/// <summary>
/// Non-generic visual and editing base shared by all Stsw numeric input controls.
/// </summary>
public abstract class StswNumberBoxBase : StswInputBoxBase, IStswBoxControl, IStswCornerControl
{
    protected StswNumberBoxBase()
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
        DependencyProperty.Register(nameof(CornerClipping), typeof(bool), typeof(StswNumberBoxBase));

    /// <inheritdoc/>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(StswNumberBoxBase));

    /// <inheritdoc/>
    public ReadOnlyObservableCollection<ValidationError> Errors
    {
        get => (ReadOnlyObservableCollection<ValidationError>)GetValue(ErrorsProperty);
        set => SetValue(ErrorsProperty, value);
    }
    public static readonly DependencyProperty ErrorsProperty =
        DependencyProperty.Register(nameof(Errors), typeof(ReadOnlyObservableCollection<ValidationError>), typeof(StswNumberBoxBase));

    /// <summary>
    /// Gets or sets the numeric format used for displaying values (e.g. "N2" or "C2").
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
            typeof(StswNumberBoxBase),
            new FrameworkPropertyMetadata(
                default(string?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnFormatChanged));

    private static void OnFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StswNumberBoxBase input)
            input.FormatChanged((string?)e.NewValue);
    }

    /// <inheritdoc/>
    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }
    public static readonly DependencyProperty HasErrorProperty =
        DependencyProperty.Register(nameof(HasError), typeof(bool), typeof(StswNumberBoxBase));

    /// <inheritdoc/>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(object), typeof(StswNumberBoxBase));

    /// <summary>
    /// Gets or sets the thickness of the separator between the editable area and the increment/decrement buttons.
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
            typeof(StswNumberBoxBase),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender));

    /// <inheritdoc/>
    public ObservableCollection<IStswSubControl> SubControls
    {
        get => (ObservableCollection<IStswSubControl>)GetValue(SubControlsProperty);
        set => SetValue(SubControlsProperty, value);
    }
    public static readonly DependencyProperty SubControlsProperty =
        DependencyProperty.Register(nameof(SubControls), typeof(ObservableCollection<IStswSubControl>), typeof(StswNumberBoxBase));

    /// <summary>
    /// Internal editing buffer. Numeric controls expose <c>Value</c> as their public value property.
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

    #region Overrides

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        FormatChanged(Format);
    }

    /// <summary>
    /// Enter is the explicit numeric commit path supplied by <see cref="StswInputBoxBase.CommitOnEnter"/>.
    /// </summary>
    protected override void OnCommit()
    {
        UpdateMainProperty(alwaysUpdate: true);
    }

    /// <summary>
    /// Losing focus commits a valid changed value.
    /// </summary>
    protected override void OnLostFocus(RoutedEventArgs e)
    {
        UpdateMainProperty(alwaysUpdate: false);
        base.OnLostFocus(e);
    }

    #endregion

    #region Logic

    /// <summary>
    /// Applies the requested numeric format to the internal Text-to-Value binding without replacing the user's binding on Value.
    /// </summary>
    protected virtual void FormatChanged(string? newFormat)
    {
        if (GetBindingExpression(TextProperty)?.ParentBinding is Binding binding)
        {
            var newBinding = (Binding)binding.Clone();
            newBinding.StringFormat = newFormat;
            SetBinding(TextProperty, newBinding);
        }
    }

    /// <summary>
    /// Parses/validates the editing buffer and commits the control's main numeric property.
    /// </summary>
    protected abstract void UpdateMainProperty(bool alwaysUpdate);

    #endregion
}

/// <summary>
/// A numeric input control allowing users to enter a number or expression manually or adjust the value using up/down buttons.
/// Supports custom formats, increment steps, and min/max value validation.
/// </summary>
[ContentProperty(nameof(Value))]
public abstract class StswNumberBoxBase<T> : StswNumberBoxBase where T : struct, INumber<T>
{
    #region Dependency properties

    /// <summary>
    /// Gets or sets the step value used when adjusting the number using the up/down buttons or mouse wheel.
    /// </summary>
    public T Increment
    {
        get => (T)GetValue(IncrementProperty);
        set => SetValue(IncrementProperty, value);
    }
    public static readonly DependencyProperty IncrementProperty =
        DependencyProperty.Register(nameof(Increment), typeof(T), typeof(StswNumberBoxBase<T>));

    /// <summary>
    /// Gets or sets the maximum allowable value. Values assigned to <see cref="Value"/> are clamped to this limit.
    /// </summary>
    public T? Maximum
    {
        get => (T?)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }
    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(
            nameof(Maximum),
            typeof(T?),
            typeof(StswNumberBoxBase<T>),
            new PropertyMetadata(default(T?), OnMinMaxChanged));

    /// <summary>
    /// Gets or sets the minimum allowable value. Values assigned to <see cref="Value"/> are clamped to this limit.
    /// </summary>
    public T? Minimum
    {
        get => (T?)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }
    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(
            nameof(Minimum),
            typeof(T?),
            typeof(StswNumberBoxBase<T>),
            new PropertyMetadata(default(T?), OnMinMaxChanged));

    private static void OnMinMaxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var input = (StswNumberBoxBase<T>)d;
        if (input.Value != null && !input.Value.Between(input.Minimum, input.Maximum))
            input.Value = input.MinMaxValidate(input.Value.GetValueOrDefault());
    }

    /// <summary>
    /// Gets or sets the numeric value of the control.
    /// </summary>
    public T? Value
    {
        get => (T?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(T?),
            typeof(StswNumberBoxBase<T>),
            new FrameworkPropertyMetadata(
                default(T?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                null,
                OnValueChanging,
                false,
                UpdateSourceTrigger.PropertyChanged));

    private static object? OnValueChanging(DependencyObject d, object? baseValue)
    {
        var input = (StswNumberBoxBase<T>)d;
        return input.MinMaxValidate((T?)baseValue);
    }

    #endregion

    #region Template

    private ButtonBase? _btnDown;
    private ButtonBase? _btnUp;

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        if (_btnUp != null)
            _btnUp.Click -= PART_ButtonUp_Click;
        if (_btnDown != null)
            _btnDown.Click -= PART_ButtonDown_Click;

        base.OnApplyTemplate();

        _btnUp = GetTemplateChild("PART_ButtonUp") as ButtonBase;
        if (_btnUp != null)
            _btnUp.Click += PART_ButtonUp_Click;

        _btnDown = GetTemplateChild("PART_ButtonDown") as ButtonBase;
        if (_btnDown != null)
            _btnDown.Click += PART_ButtonDown_Click;
    }

    #endregion

    #region Logic

    private void PART_ButtonUp_Click(object sender, RoutedEventArgs e)
    {
        var result = Value ?? default;

        if (TryParse(Text, out var parsed))
            result = parsed;

        Value = IsZero(Increment) ? Add(result, T.One) : Add(result, Increment);

        Focus();
        CaretIndex = Text?.Length ?? 0;
    }

    private void PART_ButtonDown_Click(object sender, RoutedEventArgs e)
    {
        var result = Value ?? default;

        if (TryParse(Text, out var parsed))
            result = parsed;

        Value = IsZero(Increment) ? Subtract(result, T.One) : Subtract(result, Increment);

        Focus();
        CaretIndex = Text?.Length ?? 0;
    }

    /// <inheritdoc/>
    protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
    {
        // Handle numeric stepping before PART_ContentHost/StswScrollView gets the wheel event.
        // This preserves the historical behaviour: the box must own keyboard focus and
        // Increment == 0 disables wheel stepping.
        if (IsKeyboardFocused && !IsReadOnly && !IsZero(Increment) && TryParse(Text, out var parsed))
        {
            Value = e.Delta > 0 ? Add(parsed, Increment) : Subtract(parsed, Increment);
            CaretIndex = Text?.Length ?? 0;
            e.Handled = true;
            return;
        }

        base.OnPreviewMouseWheel(e);
    }

    private T? MinMaxValidate(T? newValue)
    {
        if (newValue == null)
            return null;

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

    #region Helpers

    private static bool TryParse(string? text, out T result)
        => T.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out result);

    private static T Add(T a, T b) => a + b;
    private static T Subtract(T a, T b) => a - b;
    private static bool IsZero(T value) => value == T.Zero;
    private static int Compare(T a, T b) => a.CompareTo(b);

    #endregion
}

/// <summary>
/// Represents a numeric input control backed by <see cref="decimal"/>.
/// </summary>
public class StswDecimalBox : StswNumberBoxBase<decimal>
{
    static StswDecimalBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswDecimalBox), new FrameworkPropertyMetadata(typeof(StswDecimalBox)));
    }
}

/// <summary>
/// Represents a numeric input control backed by <see cref="double"/>.
/// </summary>
public class StswDoubleBox : StswNumberBoxBase<double>
{
    static StswDoubleBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswDoubleBox), new FrameworkPropertyMetadata(typeof(StswDoubleBox)));
    }
}

/// <summary>
/// Represents a numeric input control backed by <see cref="int"/>.
/// </summary>
public class StswIntegerBox : StswNumberBoxBase<int>
{
    static StswIntegerBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswIntegerBox), new FrameworkPropertyMetadata(typeof(StswIntegerBox)));
    }
}
