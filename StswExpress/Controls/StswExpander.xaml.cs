using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// Interaction logic for StswExpander.xaml
/// </summary>
public partial class StswExpander : Expander
{
    public StswExpander()
    {
        InitializeComponent();
    }
    static StswExpander()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswExpander), new FrameworkPropertyMetadata(typeof(StswExpander)));
    }

    #region Properties
    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswExpander)
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    #endregion

    #region Style
    /// SubBackground
    public static readonly DependencyProperty SubBackgroundProperty
        = DependencyProperty.Register(
            nameof(SubBackground),
            typeof(Brush),
            typeof(StswExpander)
        );
    public Brush SubBackground
    {
        get => (Brush)GetValue(SubBackgroundProperty);
        set => SetValue(SubBackgroundProperty, value);
    }

    /// BorderBrushDisabled
    public static readonly DependencyProperty BorderBrushDisabledProperty
        = DependencyProperty.Register(
            nameof(BorderBrushDisabled),
            typeof(Brush),
            typeof(StswExpander)
        );
    public Brush BorderBrushDisabled
    {
        get => (Brush)GetValue(BorderBrushDisabledProperty);
        set => SetValue(BorderBrushDisabledProperty, value);
    }
    /// SubBackgroundDisabled
    public static readonly DependencyProperty SubBackgroundDisabledProperty
        = DependencyProperty.Register(
            nameof(SubBackgroundDisabled),
            typeof(Brush),
            typeof(StswExpander)
        );
    public Brush SubBackgroundDisabled
    {
        get => (Brush)GetValue(SubBackgroundDisabledProperty);
        set => SetValue(SubBackgroundDisabledProperty, value);
    }
    /// SubForegroundDisabled
    public static readonly DependencyProperty SubForegroundDisabledProperty
        = DependencyProperty.Register(
            nameof(SubForegroundDisabled),
            typeof(Brush),
            typeof(StswExpander)
        );
    public Brush SubForegroundDisabled
    {
        get => (Brush)GetValue(SubForegroundDisabledProperty);
        set => SetValue(SubForegroundDisabledProperty, value);
    }

    /// BorderBrushMouseOver
    public static readonly DependencyProperty BorderBrushMouseOverProperty
        = DependencyProperty.Register(
            nameof(BorderBrushMouseOver),
            typeof(Brush),
            typeof(StswExpander)
        );
    public Brush BorderBrushMouseOver
    {
        get => (Brush)GetValue(BorderBrushMouseOverProperty);
        set => SetValue(BorderBrushMouseOverProperty, value);
    }
    /// SubBackgroundMouseOver
    public static readonly DependencyProperty SubBackgroundMouseOverProperty
        = DependencyProperty.Register(
            nameof(SubBackgroundMouseOver),
            typeof(Brush),
            typeof(StswExpander)
        );
    public Brush SubBackgroundMouseOver
    {
        get => (Brush)GetValue(SubBackgroundMouseOverProperty);
        set => SetValue(SubBackgroundMouseOverProperty, value);
    }
    /// SubForegroundMouseOver
    public static readonly DependencyProperty SubForegroundMouseOverProperty
        = DependencyProperty.Register(
            nameof(SubForegroundMouseOver),
            typeof(Brush),
            typeof(StswExpander)
        );
    public Brush SubForegroundMouseOver
    {
        get => (Brush)GetValue(SubForegroundMouseOverProperty);
        set => SetValue(SubForegroundMouseOverProperty, value);
    }

    /// BorderBrushPressed
    public static readonly DependencyProperty BorderBrushPressedProperty
        = DependencyProperty.Register(
            nameof(BorderBrushPressed),
            typeof(Brush),
            typeof(StswExpander)
        );
    public Brush BorderBrushPressed
    {
        get => (Brush)GetValue(BorderBrushPressedProperty);
        set => SetValue(BorderBrushPressedProperty, value);
    }
    /// SubBackgroundPressed
    public static readonly DependencyProperty SubBackgroundPressedProperty
        = DependencyProperty.Register(
            nameof(SubBackgroundPressed),
            typeof(Brush),
            typeof(StswExpander)
        );
    public Brush SubBackgroundPressed
    {
        get => (Brush)GetValue(SubBackgroundPressedProperty);
        set => SetValue(SubBackgroundPressedProperty, value);
    }
    /// SubForegroundPressed
    public static readonly DependencyProperty SubForegroundPressedProperty
        = DependencyProperty.Register(
            nameof(SubForegroundPressed),
            typeof(Brush),
            typeof(StswExpander)
        );
    public Brush SubForegroundPressed
    {
        get => (Brush)GetValue(SubForegroundPressedProperty);
        set => SetValue(SubForegroundPressedProperty, value);
    }

    /// BorderBrushChecked
    public static readonly DependencyProperty BorderBrushCheckedProperty
        = DependencyProperty.Register(
            nameof(BorderBrushChecked),
            typeof(Brush),
            typeof(StswExpander)
        );
    public Brush BorderBrushChecked
    {
        get => (Brush)GetValue(BorderBrushCheckedProperty);
        set => SetValue(BorderBrushCheckedProperty, value);
    }
    /// SubBackgroundChecked
    public static readonly DependencyProperty SubBackgroundCheckedProperty
        = DependencyProperty.Register(
            nameof(SubBackgroundChecked),
            typeof(Brush),
            typeof(StswExpander)
        );
    public Brush SubBackgroundChecked
    {
        get => (Brush)GetValue(SubBackgroundCheckedProperty);
        set => SetValue(SubBackgroundCheckedProperty, value);
    }
    /// SubForegroundChecked
    public static readonly DependencyProperty SubForegroundCheckedProperty
        = DependencyProperty.Register(
            nameof(SubForegroundChecked),
            typeof(Brush),
            typeof(StswExpander)
        );
    public Brush SubForegroundChecked
    {
        get => (Brush)GetValue(SubForegroundCheckedProperty);
        set => SetValue(SubForegroundCheckedProperty, value);
    }

    /// SubBorderThickness
    public static readonly DependencyProperty SubBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(SubBorderThickness),
            typeof(Thickness),
            typeof(StswExpander)
        );
    public Thickness SubBorderThickness
    {
        get => (Thickness)GetValue(SubBorderThicknessProperty);
        set => SetValue(SubBorderThicknessProperty, value);
    }
    /// SubPadding
    public static readonly DependencyProperty SubPaddingProperty
        = DependencyProperty.Register(
            nameof(SubPadding),
            typeof(Thickness),
            typeof(StswExpander)
        );
    public Thickness SubPadding
    {
        get => (Thickness)GetValue(SubPaddingProperty);
        set => SetValue(SubPaddingProperty, value);
    }
    #endregion
}
