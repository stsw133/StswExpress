using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Interaction logic for StswNumericBox.xaml
/// </summary>
public partial class StswNumericBox : TextBox
{
    public StswNumericBox()
    {
        InitializeComponent();

        SetValue(ButtonsProperty, new ObservableCollection<ButtonBase>());
    }
    static StswNumericBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswNumericBox), new FrameworkPropertyMetadata(typeof(StswNumericBox)));
        TextProperty.OverrideMetadata(typeof(StswNumericBox), new FrameworkPropertyMetadata(null, OnTextChanged));
    }

    #region Style
    /// BackgroundDisabled
    public static readonly DependencyProperty BackgroundDisabledProperty
        = DependencyProperty.Register(
            nameof(BackgroundDisabled),
            typeof(Brush),
            typeof(StswNumericBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BackgroundDisabled
    {
        get => (Brush)GetValue(BackgroundDisabledProperty);
        set => SetValue(BackgroundDisabledProperty, value);
    }
    /// BorderBrushDisabled
    public static readonly DependencyProperty BorderBrushDisabledProperty
        = DependencyProperty.Register(
            nameof(BorderBrushDisabled),
            typeof(Brush),
            typeof(StswNumericBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BorderBrushDisabled
    {
        get => (Brush)GetValue(BorderBrushDisabledProperty);
        set => SetValue(BorderBrushDisabledProperty, value);
    }
    /// ForegroundDisabled
    public static readonly DependencyProperty ForegroundDisabledProperty
        = DependencyProperty.Register(
            nameof(ForegroundDisabled),
            typeof(Brush),
            typeof(StswNumericBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush ForegroundDisabled
    {
        get => (Brush)GetValue(ForegroundDisabledProperty);
        set => SetValue(ForegroundDisabledProperty, value);
    }

    /// BackgroundMouseOver
    public static readonly DependencyProperty BackgroundMouseOverProperty
        = DependencyProperty.Register(
            nameof(BackgroundMouseOver),
            typeof(Brush),
            typeof(StswNumericBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BackgroundMouseOver
    {
        get => (Brush)GetValue(BackgroundMouseOverProperty);
        set => SetValue(BackgroundMouseOverProperty, value);
    }
    /// BorderBrushMouseOver
    public static readonly DependencyProperty BorderBrushMouseOverProperty
        = DependencyProperty.Register(
            nameof(BorderBrushMouseOver),
            typeof(Brush),
            typeof(StswNumericBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BorderBrushMouseOver
    {
        get => (Brush)GetValue(BorderBrushMouseOverProperty);
        set => SetValue(BorderBrushMouseOverProperty, value);
    }
    /// ForegroundMouseOver
    public static readonly DependencyProperty ForegroundMouseOverProperty
        = DependencyProperty.Register(
            nameof(ForegroundMouseOver),
            typeof(Brush),
            typeof(StswNumericBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush ForegroundMouseOver
    {
        get => (Brush)GetValue(ForegroundMouseOverProperty);
        set => SetValue(ForegroundMouseOverProperty, value);
    }

    /// BackgroundFocused
    public static readonly DependencyProperty BackgroundFocusedProperty
        = DependencyProperty.Register(
            nameof(BackgroundFocused),
            typeof(Brush),
            typeof(StswNumericBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BackgroundFocused
    {
        get => (Brush)GetValue(BackgroundFocusedProperty);
        set => SetValue(BackgroundFocusedProperty, value);
    }
    /// BorderBrushFocused
    public static readonly DependencyProperty BorderBrushFocusedProperty
        = DependencyProperty.Register(
            nameof(BorderBrushFocused),
            typeof(Brush),
            typeof(StswNumericBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BorderBrushFocused
    {
        get => (Brush)GetValue(BorderBrushFocusedProperty);
        set => SetValue(BorderBrushFocusedProperty, value);
    }
    /// ForegroundFocused
    public static readonly DependencyProperty ForegroundFocusedProperty
        = DependencyProperty.Register(
            nameof(ForegroundFocused),
            typeof(Brush),
            typeof(StswNumericBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush ForegroundFocused
    {
        get => (Brush)GetValue(ForegroundFocusedProperty);
        set => SetValue(ForegroundFocusedProperty, value);
    }

    /// BackgroundReadOnly
    public static readonly DependencyProperty BackgroundReadOnlyProperty
        = DependencyProperty.Register(
            nameof(BackgroundReadOnly),
            typeof(Brush),
            typeof(StswNumericBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BackgroundReadOnly
    {
        get => (Brush)GetValue(BackgroundReadOnlyProperty);
        set => SetValue(BackgroundReadOnlyProperty, value);
    }

    /// ForegroundPlaceholder
    public static readonly DependencyProperty ForegroundPlaceholderProperty
        = DependencyProperty.Register(
            nameof(ForegroundPlaceholder),
            typeof(Brush),
            typeof(StswNumericBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush ForegroundPlaceholder
    {
        get => (Brush)GetValue(ForegroundPlaceholderProperty);
        set => SetValue(ForegroundPlaceholderProperty, value);
    }

    /// SubBorderThickness
    public static readonly DependencyProperty SubBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(SubBorderThickness),
            typeof(Thickness),
            typeof(StswNumericBox),
            new PropertyMetadata(default(Thickness))
        );
    public Thickness SubBorderThickness
    {
        get => (Thickness)GetValue(SubBorderThicknessProperty);
        set => SetValue(SubBorderThicknessProperty, value);
    }
    #endregion

    #region Properties
    /// ButtonClearVisibility
    public static readonly DependencyProperty ButtonClearVisibilityProperty
        = DependencyProperty.Register(
            nameof(ButtonClearVisibility),
            typeof(Visibility),
            typeof(StswNumericBox),
            new PropertyMetadata(default(Visibility))
        );
    public Visibility ButtonClearVisibility
    {
        get => (Visibility)GetValue(ButtonClearVisibilityProperty);
        set => SetValue(ButtonClearVisibilityProperty, value);
    }
    /// Buttons
    public static readonly DependencyProperty ButtonsProperty
        = DependencyProperty.Register(
            nameof(Buttons),
            typeof(ObservableCollection<ButtonBase>),
            typeof(StswNumericBox),
            new PropertyMetadata(default(ObservableCollection<ButtonBase>))
        );
    public ObservableCollection<ButtonBase> Buttons
    {
        get => (ObservableCollection<ButtonBase>)GetValue(ButtonsProperty);
        set => SetValue(ButtonsProperty, value);
    }
    /// ButtonsAlignment
    public static readonly DependencyProperty ButtonsAlignmentProperty
        = DependencyProperty.Register(
              nameof(ButtonsAlignment),
              typeof(Dock),
              typeof(StswNumericBox),
              new PropertyMetadata(default(Dock))
          );
    public Dock ButtonsAlignment
    {
        get => (Dock)GetValue(ButtonsAlignmentProperty);
        set => SetValue(ButtonsAlignmentProperty, value);
    }

    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswNumericBox),
            new PropertyMetadata(default(CornerRadius))
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// Increment
    public static readonly DependencyProperty IncrementProperty
        = DependencyProperty.Register(
              nameof(Increment),
              typeof(double),
              typeof(StswNumericBox),
              new PropertyMetadata(default(double))
          );
    public double Increment
    {
        get => (double)GetValue(IncrementProperty);
        set => SetValue(IncrementProperty, value);
    }
    /// Maximum
    public static readonly DependencyProperty MaximumProperty
        = DependencyProperty.Register(
            nameof(Maximum),
            typeof(double?),
            typeof(StswNumericBox),
            new PropertyMetadata(default(double?))
        );
    public double? Maximum
    {
        get => (double?)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }
    /// Minimum
    public static readonly DependencyProperty MinimumProperty
        = DependencyProperty.Register(
            nameof(Minimum),
            typeof(double?),
            typeof(StswNumericBox),
            new PropertyMetadata(default(double?))
        );
    public double? Minimum
    {
        get => (double?)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }
    
    /// Placeholder
    public static readonly DependencyProperty PlaceholderProperty
        = DependencyProperty.Register(
            nameof(Placeholder),
            typeof(string),
            typeof(StswNumericBox),
            new PropertyMetadata(default(string?))
        );
    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// Value
    public static readonly DependencyProperty ValueProperty
        = DependencyProperty.Register(
            nameof(Value),
            typeof(double?),
            typeof(StswNumericBox),
            new FrameworkPropertyMetadata(default(double?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                ValueChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public double? Value
    {
        get => (double?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    public static void ValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswNumericBox stsw)
        {
            if (stsw.Value != null)
            {
                if (stsw.Minimum != null && stsw.Value < stsw.Minimum)
                    stsw.Value = stsw.Minimum;
                if (stsw.Maximum != null && stsw.Value > stsw.Maximum)
                    stsw.Value = stsw.Maximum;
            }
            stsw.Text = stsw.Value?.ToString();
        }
    }
    public static void OnTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswNumericBox stsw)
        {
            if (double.TryParse(stsw.Text, out double value))
                stsw.Value = value;
            else
            {
                stsw.Value = null;
                if (!string.IsNullOrEmpty(stsw.Text) && stsw.GetBindingExpression(ValueProperty) is not null and BindingExpression binding)
                    Validation.MarkInvalid(binding, new ValidationError(new ExceptionValidationRule(), ValueProperty, "Incorrect value!", null));
            }
        }
    }
    #endregion

    #region Events
    /// BtnUp_Click
    private void BtnUp_Click(object sender, RoutedEventArgs e) => Value = Value == null ? 0 : Value + Increment;

    /// BtnDown_Click
    private void BtnDown_Click(object sender, RoutedEventArgs e) => Value = Value == null ? 0 : Value - Increment;

    /// PART_ButtonClear_Click
    private void PART_ButtonClear_Click(object sender, RoutedEventArgs e)
    {
        var bindingExpression = BindingOperations.GetBindingExpression(this, ValueProperty);
        var boundType = bindingExpression?.ResolvedSource?.GetType().GetProperty(bindingExpression.ResolvedSourcePropertyName)?.PropertyType;
        if (boundType != null && Nullable.GetUnderlyingType(boundType) != null)
            Value = null;
        else
            Value = 0;

        Text = Value?.ToString();
    }

    /// PART_ContentHost_MouseWheel
    private void PART_ContentHost_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (IsKeyboardFocused && !IsReadOnly)
        {
            if (e.Delta > 0)
                Value += Increment;
            else
                Value -= Increment;

            e.Handled = true;
        }
    }
    #endregion
}
