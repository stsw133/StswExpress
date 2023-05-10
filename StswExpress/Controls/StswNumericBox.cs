using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;

[ContentProperty(nameof(Value))]
public class StswNumericBox : TextBox
{
    public StswNumericBox()
    {
        SetValue(ButtonsProperty, new ObservableCollection<UIElement>());
    }
    static StswNumericBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswNumericBox), new FrameworkPropertyMetadata(typeof(StswNumericBox)));
    }

    #region Events
    /// OnApplyTemplate
    public override void OnApplyTemplate()
    {
        /// Button: up
        if (GetTemplateChild("PART_ButtonUp") is StswRepeatButton btnUp)
            btnUp.Click += PART_ButtonUp_Click;
        /// Button: down
        if (GetTemplateChild("PART_ButtonDown") is StswRepeatButton btnDown)
            btnDown.Click += PART_ButtonDown_Click;

        /// Content
        if (GetTemplateChild("PART_ContentHost") is ScrollViewer content)
        {
            content.KeyDown += PART_ContentHost_KeyDown;
            content.LostFocus += PART_ContentHost_LostFocus;
            content.MouseWheel += PART_ContentHost_MouseWheel;
        }

        base.OnApplyTemplate();
    }

    /// PART_ButtonUp_Click
    private void PART_ButtonUp_Click(object sender, RoutedEventArgs e) => Value = Value == null ? 0 : Value + Increment;

    /// PART_ButtonDown_Click
    private void PART_ButtonDown_Click(object sender, RoutedEventArgs e) => Value = Value == null ? 0 : Value - Increment;

    /// PART_ContentHost_KeyDown
    protected void PART_ContentHost_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            PART_ContentHost_LostFocus(sender, new RoutedEventArgs());
    }

    /// PART_ContentHost_LostFocus
    private void PART_ContentHost_LostFocus(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(Text))
            Value = null;
        else if (StswFn.TryCalculateString(Text, out var result))
            Value = result;
        else if (double.TryParse(Text, out result))
            Value = result;
        
        Text = Value?.ToString();
        var bindingExpression = GetBindingExpression(TextProperty);
        if (bindingExpression != null && bindingExpression.Status == BindingStatus.Active)
            bindingExpression.UpdateSource();
    }

    /// PART_ContentHost_MouseWheel
    private void PART_ContentHost_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (IsKeyboardFocused && !IsReadOnly && Value != null && Increment != 0)
        {
            var step = e.Delta > 0 ? Increment : -Increment;

            try
            {
                if ((Value + step).Between(Minimum ?? double.MinValue, Maximum ?? double.MaxValue))
                    Value += step;
            }
            catch { }

            e.Handled = true;
        }
    }
    #endregion

    #region Properties
    /// Buttons
    public static readonly DependencyProperty ButtonsProperty
        = DependencyProperty.Register(
            nameof(Buttons),
            typeof(ObservableCollection<UIElement>),
            typeof(StswNumericBox)
        );
    public ObservableCollection<UIElement> Buttons
    {
        get => (ObservableCollection<UIElement>)GetValue(ButtonsProperty);
        set => SetValue(ButtonsProperty, value);
    }
    /// ButtonsAlignment
    public static readonly DependencyProperty ButtonsAlignmentProperty
        = DependencyProperty.Register(
            nameof(ButtonsAlignment),
            typeof(Dock),
            typeof(StswNumericBox)
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
            typeof(StswNumericBox)
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// Format
    public static readonly DependencyProperty FormatProperty
        = DependencyProperty.Register(
            nameof(Format),
            typeof(string),
            typeof(StswNumericBox),
            new FrameworkPropertyMetadata(default(string?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnFormatChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public string? Format
    {
        get => (string?)GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }
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

    /// Increment
    public static readonly DependencyProperty IncrementProperty
        = DependencyProperty.Register(
            nameof(Increment),
            typeof(double),
            typeof(StswNumericBox)
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
            typeof(StswNumericBox)
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
            typeof(StswNumericBox)
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
            typeof(StswNumericBox)
        );
    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// Text
    public new string? Text
    {
        get => base.Text;
        internal set => base.Text = value;
    }

    /// Value
    public static readonly DependencyProperty ValueProperty
        = DependencyProperty.Register(
            nameof(Value),
            typeof(double?),
            typeof(StswNumericBox),
            new FrameworkPropertyMetadata(default(double?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnValueChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public double? Value
    {
        get => (double?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    public static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
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
        }
    }
    #endregion

    #region Style
    /// > Background ...
    /// BackgroundDisabled
    public static readonly DependencyProperty BackgroundDisabledProperty
        = DependencyProperty.Register(
            nameof(BackgroundDisabled),
            typeof(Brush),
            typeof(StswNumericBox)
        );
    public Brush BackgroundDisabled
    {
        get => (Brush)GetValue(BackgroundDisabledProperty);
        set => SetValue(BackgroundDisabledProperty, value);
    }
    /// BackgroundMouseOver
    public static readonly DependencyProperty BackgroundMouseOverProperty
        = DependencyProperty.Register(
            nameof(BackgroundMouseOver),
            typeof(Brush),
            typeof(StswNumericBox)
        );
    public Brush BackgroundMouseOver
    {
        get => (Brush)GetValue(BackgroundMouseOverProperty);
        set => SetValue(BackgroundMouseOverProperty, value);
    }
    /// BackgroundFocused
    public static readonly DependencyProperty BackgroundFocusedProperty
        = DependencyProperty.Register(
            nameof(BackgroundFocused),
            typeof(Brush),
            typeof(StswNumericBox)
        );
    public Brush BackgroundFocused
    {
        get => (Brush)GetValue(BackgroundFocusedProperty);
        set => SetValue(BackgroundFocusedProperty, value);
    }
    /// BackgroundReadOnly
    public static readonly DependencyProperty BackgroundReadOnlyProperty
        = DependencyProperty.Register(
            nameof(BackgroundReadOnly),
            typeof(Brush),
            typeof(StswNumericBox)
        );
    public Brush BackgroundReadOnly
    {
        get => (Brush)GetValue(BackgroundReadOnlyProperty);
        set => SetValue(BackgroundReadOnlyProperty, value);
    }

    /// > BorderBrush ...
    /// BorderBrushDisabled
    public static readonly DependencyProperty BorderBrushDisabledProperty
        = DependencyProperty.Register(
            nameof(BorderBrushDisabled),
            typeof(Brush),
            typeof(StswNumericBox)
        );
    public Brush BorderBrushDisabled
    {
        get => (Brush)GetValue(BorderBrushDisabledProperty);
        set => SetValue(BorderBrushDisabledProperty, value);
    }
    /// BorderBrushMouseOver
    public static readonly DependencyProperty BorderBrushMouseOverProperty
        = DependencyProperty.Register(
            nameof(BorderBrushMouseOver),
            typeof(Brush),
            typeof(StswNumericBox)
        );
    public Brush BorderBrushMouseOver
    {
        get => (Brush)GetValue(BorderBrushMouseOverProperty);
        set => SetValue(BorderBrushMouseOverProperty, value);
    }
    /// BorderBrushFocused
    public static readonly DependencyProperty BorderBrushFocusedProperty
        = DependencyProperty.Register(
            nameof(BorderBrushFocused),
            typeof(Brush),
            typeof(StswNumericBox)
        );
    public Brush BorderBrushFocused
    {
        get => (Brush)GetValue(BorderBrushFocusedProperty);
        set => SetValue(BorderBrushFocusedProperty, value);
    }

    /// > Foreground ...
    /// ForegroundDisabled
    public static readonly DependencyProperty ForegroundDisabledProperty
        = DependencyProperty.Register(
            nameof(ForegroundDisabled),
            typeof(Brush),
            typeof(StswNumericBox)
        );
    public Brush ForegroundDisabled
    {
        get => (Brush)GetValue(ForegroundDisabledProperty);
        set => SetValue(ForegroundDisabledProperty, value);
    }
    /// ForegroundMouseOver
    public static readonly DependencyProperty ForegroundMouseOverProperty
        = DependencyProperty.Register(
            nameof(ForegroundMouseOver),
            typeof(Brush),
            typeof(StswNumericBox)
        );
    public Brush ForegroundMouseOver
    {
        get => (Brush)GetValue(ForegroundMouseOverProperty);
        set => SetValue(ForegroundMouseOverProperty, value);
    }
    /// ForegroundFocused
    public static readonly DependencyProperty ForegroundFocusedProperty
        = DependencyProperty.Register(
            nameof(ForegroundFocused),
            typeof(Brush),
            typeof(StswNumericBox)
        );
    public Brush ForegroundFocused
    {
        get => (Brush)GetValue(ForegroundFocusedProperty);
        set => SetValue(ForegroundFocusedProperty, value);
    }
    /// ForegroundPlaceholder
    public static readonly DependencyProperty ForegroundPlaceholderProperty
        = DependencyProperty.Register(
            nameof(ForegroundPlaceholder),
            typeof(Brush),
            typeof(StswNumericBox)
        );
    public Brush ForegroundPlaceholder
    {
        get => (Brush)GetValue(ForegroundPlaceholderProperty);
        set => SetValue(ForegroundPlaceholderProperty, value);
    }

    /// > BorderThickness ...
    /// SubBorderThickness
    public static readonly DependencyProperty SubBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(SubBorderThickness),
            typeof(Thickness),
            typeof(StswNumericBox)
        );
    public Thickness SubBorderThickness
    {
        get => (Thickness)GetValue(SubBorderThicknessProperty);
        set => SetValue(SubBorderThicknessProperty, value);
    }
    #endregion
}
