﻿using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// Represents a control that allows users to provide value either by entering numeric value or using a "Up" and "Down" buttons.
/// </summary>
[ContentProperty(nameof(Value))]
public class StswNumericBox : TextBox
{
    public StswNumericBox()
    {
        SetValue(ComponentsProperty, new ObservableCollection<IStswComponent>());
    }
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
        if (GetTemplateChild("PART_ButtonUp") is StswRepeatButton btnUp)
            btnUp.Click += PART_ButtonUp_Click;
        /// Button: down
        if (GetTemplateChild("PART_ButtonDown") is StswRepeatButton btnDown)
            btnDown.Click += PART_ButtonDown_Click;

        OnFormatChanged(this, new DependencyPropertyChangedEventArgs());
    }

    /// <summary>
    /// Handles the click event for the "Up" button, incrementing the numeric value.
    /// </summary>
    private void PART_ButtonUp_Click(object sender, RoutedEventArgs e)
    {
        if (double.TryParse(Text, out var result))
            Value = result;
        Value = Value == null ? 0 : Value + Increment;
    }

    /// <summary>
    /// Handles the click event for the "Down" button, decrementing the numeric value.
    /// </summary>
    private void PART_ButtonDown_Click(object sender, RoutedEventArgs e)
    {
        if (double.TryParse(Text, out var result))
            Value = result;
        Value = Value == null ? 0 : Value - Increment;
    }

    /// <summary>
    /// Handles the KeyDown event for the internal content host of the numeric box.
    /// If the Enter key is pressed, the LostFocus event is triggered for the content host.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Enter)
            UpdateMainProperty(true);
    }

    /// <summary>
    /// Handles the LostFocus event for the content, updating the value and applying any necessary formatting.
    /// </summary>
    protected override void OnLostFocus(RoutedEventArgs e)
    {
        UpdateMainProperty(false);
        base.OnLostFocus(e);
    }

    /// <summary>
    /// Handles the MouseWheel event for the content, incrementing or decrementing the numeric value based on the wheel movement.
    /// </summary>
    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        if (IsKeyboardFocused && !IsReadOnly && Increment != 0 && Value.HasValue)
        {
            if (double.TryParse(Text, out var result))
                Value = result;

            if (e.Delta > 0)
            {
                if (double.MaxValue - Increment >= Value)
                    Value += Increment;
                else
                    Value = double.MaxValue;
            }
            else if (e.Delta < 0)
            {
                if (double.MinValue + Increment <= Value)
                    Value -= Increment;
                else
                    Value = double.MinValue;
            }

            e.Handled = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private double? MinMaxValidate(double? newValue)
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
    /// 
    /// </summary>
    private void UpdateMainProperty(bool alwaysUpdate)
    {
        var result = Value;

        if (string.IsNullOrEmpty(Text))
            result = null;
        else if (StswFn.TryCalculateString(Text, out var res))
            result = res;
        else if (double.TryParse(Text, out res))
            result = res;

        if (result != Value || alwaysUpdate)
        {
            Value = result;

            Text = result?.ToString(Format);
            var bindingExpression = GetBindingExpression(TextProperty);
            if (bindingExpression != null && bindingExpression.Status.In(BindingStatus.Active/*, BindingStatus.UpdateSourceError*/))
                bindingExpression.UpdateSource();
        }
    }
    #endregion

    #region Main properties
    /// <summary>
    /// Gets or sets the collection of components to be displayed in the control.
    /// </summary>
    public ObservableCollection<IStswComponent> Components
    {
        get => (ObservableCollection<IStswComponent>)GetValue(ComponentsProperty);
        set => SetValue(ComponentsProperty, value);
    }
    public static readonly DependencyProperty ComponentsProperty
        = DependencyProperty.Register(
            nameof(Components),
            typeof(ObservableCollection<IStswComponent>),
            typeof(StswNumericBox)
        );

    /// <summary>
    /// Gets or sets the alignment of the components within the control.
    /// </summary>
    public Dock ComponentsAlignment
    {
        get => (Dock)GetValue(ComponentsAlignmentProperty);
        set => SetValue(ComponentsAlignmentProperty, value);
    }
    public static readonly DependencyProperty ComponentsAlignmentProperty
        = DependencyProperty.Register(
            nameof(ComponentsAlignment),
            typeof(Dock),
            typeof(StswNumericBox)
        );

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
            if (stsw.GetBindingExpression(TextProperty)?.ParentBinding is Binding binding and not null)
            {
                var newBinding = new Binding()
                {
                    ConverterCulture = binding.ConverterCulture,
                    Mode = binding.Mode,
                    Path = binding.Path,
                    RelativeSource = binding.RelativeSource,
                    StringFormat = stsw.Format,
                    UpdateSourceTrigger = binding.UpdateSourceTrigger
                };
                stsw.SetBinding(TextProperty, newBinding);
            }
        }
    }

    /// <summary>
    /// Gets or sets the increment value used when clicking the "Up" or "Down" button.
    /// </summary>
    public double Increment
    {
        get => (double)GetValue(IncrementProperty);
        set => SetValue(IncrementProperty, value);
    }
    public static readonly DependencyProperty IncrementProperty
        = DependencyProperty.Register(
            nameof(Increment),
            typeof(double),
            typeof(StswNumericBox)
        );

    /// <summary>
    /// Gets or sets the maximum allowable value in the control.
    /// </summary>
    public double? Maximum
    {
        get => (double?)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }
    public static readonly DependencyProperty MaximumProperty
        = DependencyProperty.Register(
            nameof(Maximum),
            typeof(double?),
            typeof(StswNumericBox),
            new PropertyMetadata(default(double?), OnMinMaxChanged)
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
    public double? Minimum
    {
        get => (double?)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }
    public static readonly DependencyProperty MinimumProperty
        = DependencyProperty.Register(
            nameof(Minimum),
            typeof(double?),
            typeof(StswNumericBox),
            new PropertyMetadata(default(double?), OnMinMaxChanged)
        );

    /// <summary>
    /// Gets or sets the placeholder text to display in the box when no value is provided.
    /// </summary>
    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }
    public static readonly DependencyProperty PlaceholderProperty
        = DependencyProperty.Register(
            nameof(Placeholder),
            typeof(string),
            typeof(StswNumericBox)
        );

    ///// <summary>
    ///// Gets or sets the text value of the control.
    ///// </summary>
    //[Browsable(false)]
    ////[Bindable(false)]
    //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    //[EditorBrowsable(EditorBrowsableState.Never)]
    //public new string? Text
    //{
    //    get => base.Text;
    //    internal set => base.Text = value;
    //}

    /// <summary>
    /// Gets or sets the numeric value of the control.
    /// </summary>
    public double? Value
    {
        get => (double?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, MinMaxValidate(value));
    }
    public static readonly DependencyProperty ValueProperty
        = DependencyProperty.Register(
            nameof(Value),
            typeof(double?),
            typeof(StswNumericBox),
            new FrameworkPropertyMetadata(default(double?),
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
    /// Gets or sets the degree to which the corners of the control are rounded.
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
            typeof(StswNumericBox)
        );

    /// <summary>
    /// Gets or sets the thickness of the border used as separator between box and drop-down button.
    /// </summary>
    public Thickness SubBorderThickness
    {
        get => (Thickness)GetValue(SubBorderThicknessProperty);
        set => SetValue(SubBorderThicknessProperty, value);
    }
    public static readonly DependencyProperty SubBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(SubBorderThickness),
            typeof(Thickness),
            typeof(StswNumericBox)
        );
    #endregion
}
