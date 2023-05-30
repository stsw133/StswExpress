using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;

[ContentProperty(nameof(SelectedColor))]
public class StswColorBox : TextBox
{
    public ICommand ClickCommand { get; set; }

    public StswColorBox()
    {
        SetValue(ComponentsProperty, new ObservableCollection<UIElement>());

        ClickCommand = new StswRelayCommand<SolidColorBrush?>(Click);
    }
    static StswColorBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswColorBox), new FrameworkPropertyMetadata(typeof(StswColorBox)));
    }

    #region Events
    /// OnApplyTemplate
    public override void OnApplyTemplate()
    {
        /// Content
        if (GetTemplateChild("PART_ContentHost") is ScrollViewer content)
        {
            content.KeyDown += PART_ContentHost_KeyDown;
            content.LostFocus += PART_ContentHost_LostFocus;
        }

        base.OnApplyTemplate();
    }

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
            SelectedColor = default;
        else if (Text.Split(CultureInfo.CurrentCulture.TextInfo.ListSeparator) is string[] argb && argb.Length == 4
              && byte.TryParse(argb[0], out var a1) && byte.TryParse(argb[1], out var r1) && byte.TryParse(argb[2], out var g1) && byte.TryParse(argb[3], out var b1))
            SelectedColor = Color.FromArgb(a1, r1, g1, b1);
        else if (Text.Split(CultureInfo.CurrentCulture.TextInfo.ListSeparator) is string[] rgb && rgb.Length == 3
              && byte.TryParse(rgb[0], out var r2) && byte.TryParse(rgb[1], out var g2) && byte.TryParse(rgb[2], out var b2))
            SelectedColor = Color.FromRgb(r2, g2, b2);
        else if (new ColorConverter().IsValid(Text))
            SelectedColor = (Color)ColorConverter.ConvertFromString(Text);

        Text = SelectedColor.ToString();
        var bindingExpression = GetBindingExpression(TextProperty);
        if (bindingExpression != null && bindingExpression.Status.In(BindingStatus.Active/*, BindingStatus.UpdateSourceError*/))
            bindingExpression.UpdateSource();
    }

    /// Click
    public void Click(SolidColorBrush? brush)
    {
        if (brush != null)
            SelectedColor = brush.Color;
    }
    #endregion

    #region Properties
    /// Components
    public static readonly DependencyProperty ComponentsProperty
        = DependencyProperty.Register(
            nameof(Components),
            typeof(ObservableCollection<UIElement>),
            typeof(StswColorBox)
        );
    public ObservableCollection<UIElement> Components
    {
        get => (ObservableCollection<UIElement>)GetValue(ComponentsProperty);
        set => SetValue(ComponentsProperty, value);
    }
    /// ComponentsAlignment
    public static readonly DependencyProperty ComponentsAlignmentProperty
        = DependencyProperty.Register(
            nameof(ComponentsAlignment),
            typeof(Dock),
            typeof(StswColorBox)
        );
    public Dock ComponentsAlignment
    {
        get => (Dock)GetValue(ComponentsAlignmentProperty);
        set => SetValue(ComponentsAlignmentProperty, value);
    }

    /// Placeholder
    public static readonly DependencyProperty PlaceholderProperty
        = DependencyProperty.Register(
            nameof(Placeholder),
            typeof(string),
            typeof(StswColorBox)
        );
    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// SelectedColor
    public static readonly DependencyProperty SelectedColorProperty
        = DependencyProperty.Register(
            nameof(SelectedColor),
            typeof(Color),
            typeof(StswColorBox),
            new FrameworkPropertyMetadata(default(Color),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                null, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public Color SelectedColor
    {
        get => (Color)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    /// Text
    public new string? Text
    {
        get => base.Text;
        internal set => base.Text = value;
    }
    #endregion

    #region Style
    /// > Background ...
    /// BackgroundDisabled
    public static readonly DependencyProperty BackgroundDisabledProperty
        = DependencyProperty.Register(
            nameof(BackgroundDisabled),
            typeof(Brush),
            typeof(StswColorBox)
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
            typeof(StswColorBox)
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
            typeof(StswColorBox)
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
            typeof(StswColorBox)
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
            typeof(StswColorBox)
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
            typeof(StswColorBox)
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
            typeof(StswColorBox)
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
            typeof(StswColorBox)
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
            typeof(StswColorBox)
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
            typeof(StswColorBox)
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
            typeof(StswColorBox)
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
            typeof(StswColorBox)
        );
    public Thickness SubBorderThickness
    {
        get => (Thickness)GetValue(SubBorderThicknessProperty);
        set => SetValue(SubBorderThicknessProperty, value);
    }

    /// > CornerRadius ...
    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswColorBox)
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    #endregion
}
