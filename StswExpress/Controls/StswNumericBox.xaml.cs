using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Interaction logic for StswNumericBox.xaml
/// </summary>
public partial class StswNumericBox : UserControl
{
    public StswNumericBox()
    {
        InitializeComponent();

        SetValue(CustomControlsProperty, new ObservableCollection<UIElement>());
    }
    static StswNumericBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswNumericBox), new FrameworkPropertyMetadata(typeof(StswNumericBox)));
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

    /// CustomControls
    public static readonly DependencyProperty CustomControlsProperty
        = DependencyProperty.Register(
            nameof(CustomControls),
            typeof(ObservableCollection<UIElement>),
            typeof(StswNumericBox),
            new PropertyMetadata(default(ObservableCollection<UIElement>))
        );
    public ObservableCollection<UIElement> CustomControls
    {
        get => (ObservableCollection<UIElement>)GetValue(CustomControlsProperty);
        set => SetValue(CustomControlsProperty, value);
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

    /// IsReadOnly
    public static readonly DependencyProperty IsReadOnlyProperty
        = DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(StswNumericBox),
            new PropertyMetadata(default(bool))
        );
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
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
        if (obj is StswNumericBox stsw && stsw.Value != null)
        {
            if (stsw.Minimum != null && stsw.Value < stsw.Minimum)
                stsw.Value = (double)stsw.Minimum;
            if (stsw.Maximum != null && stsw.Value > stsw.Maximum)
                stsw.Value = (double)stsw.Maximum;
        }
    }
    #endregion

    #region Events
    /// Btn_Click
    private void Btn_Click(double increment)
    {
        Value += increment;
        if (Value == null)
        {
            if (((double?)0).Between(Minimum, Maximum))
                Value = 0;
            else
                Value = Math.Min(Math.Abs(Minimum ?? 0d), Math.Abs(Maximum ?? 0d));
        }
    }
    private void BtnDown_Click(object sender, RoutedEventArgs e) => Btn_Click(-Increment);
    private void BtnUp_Click(object sender, RoutedEventArgs e) => Btn_Click(Increment);

    /// TextBox_KeyDown
    private void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && double.TryParse(((TextBox)sender).Text, out var value))
            Value = value;
    }
    #endregion
}
