using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;

[ContentProperty(nameof(ColorPaletteStandard))]
public class StswColorSelector : UserControl
{
    public ICommand ClickCommand { get; set; }

    public StswColorSelector()
    {
        ClickCommand = new StswRelayCommand<SolidColorBrush?>(Click);
    }
    static StswColorSelector()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswColorSelector), new FrameworkPropertyMetadata(typeof(StswColorSelector)));
    }

    #region Events
    /// Click
    public void Click(SolidColorBrush? brush)
    {
        if (brush != null)
            SelectedColor = brush.Color;
    }
    #endregion

    #region Main properties
    /// ColorPaletteStandard
    public static readonly DependencyProperty ColorPaletteStandardProperty
        = DependencyProperty.Register(
            nameof(ColorPaletteStandard),
            typeof(SolidColorBrush[]),
            typeof(StswColorSelector)
        );
    public SolidColorBrush[] ColorPaletteStandard
    {
        get => (SolidColorBrush[])GetValue(ColorPaletteStandardProperty);
        set => SetValue(ColorPaletteStandardProperty, value);
    }
    /// ColorPaletteTheme
    public static readonly DependencyProperty ColorPaletteThemeProperty
        = DependencyProperty.Register(
            nameof(ColorPaletteTheme),
            typeof(SolidColorBrush[]),
            typeof(StswColorSelector)
        );
    public SolidColorBrush[] ColorPaletteTheme
    {
        get => (SolidColorBrush[])GetValue(ColorPaletteThemeProperty);
        set => SetValue(ColorPaletteThemeProperty, value);
    }
    /// SelectedColor
    public static readonly DependencyProperty SelectedColorProperty
        = DependencyProperty.Register(
            nameof(SelectedColor),
            typeof(Color),
            typeof(StswColorSelector)
        );
    public Color SelectedColor
    {
        get => (Color)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }
    #endregion

    #region Spacial properties
    /// > BorderThickness ...
    /// SubBorderThickness
    public static readonly DependencyProperty SubBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(SubBorderThickness),
            typeof(Thickness),
            typeof(StswColorSelector)
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
            typeof(StswColorSelector)
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// > Padding ...
    /// SubPadding
    public static readonly DependencyProperty SubPaddingProperty
        = DependencyProperty.Register(
            nameof(SubPadding),
            typeof(Thickness),
            typeof(StswColorSelector)
        );
    public Thickness SubPadding
    {
        get => (Thickness)GetValue(SubPaddingProperty);
        set => SetValue(SubPaddingProperty, value);
    }
    #endregion

    #region Style properties
    /// > Opacity ...
    /// OpacityDisabled
    public static readonly DependencyProperty OpacityDisabledProperty
        = DependencyProperty.Register(
            nameof(OpacityDisabled),
            typeof(double),
            typeof(StswColorSelector)
        );
    public double OpacityDisabled
    {
        get => (double)GetValue(OpacityDisabledProperty);
        set => SetValue(OpacityDisabledProperty, value);
    }
    #endregion
}
