using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Interaction logic for StswCheckButton.xaml
/// </summary>
public partial class StswCheckButton : StswCheckButtonBase
{
    public StswCheckButton()
    {
        InitializeComponent();
    }
    static StswCheckButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswCheckButton), new FrameworkPropertyMetadata(typeof(StswCheckButton)));
    }
}

public class StswCheckButtonBase : ToggleButton
{
    #region StyleColors
    /// StyleColorDisabledBackground
    public static readonly DependencyProperty StyleColorDisabledBackgroundProperty
        = DependencyProperty.Register(
            nameof(StyleColorDisabledBackground),
            typeof(Brush),
            typeof(StswCheckButtonBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorDisabledBackground
    {
        get => (Brush)GetValue(StyleColorDisabledBackgroundProperty);
        set => SetValue(StyleColorDisabledBackgroundProperty, value);
    }

    /// StyleColorDisabledBorder
    public static readonly DependencyProperty StyleColorDisabledBorderProperty
        = DependencyProperty.Register(
            nameof(StyleColorDisabledBorder),
            typeof(Brush),
            typeof(StswCheckButtonBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorDisabledBorder
    {
        get => (Brush)GetValue(StyleColorDisabledBorderProperty);
        set => SetValue(StyleColorDisabledBorderProperty, value);
    }

    /// StyleColorMouseOverBackground
    public static readonly DependencyProperty StyleColorMouseOverBackgroundProperty
        = DependencyProperty.Register(
            nameof(StyleColorMouseOverBackground),
            typeof(Brush),
            typeof(StswCheckButtonBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorMouseOverBackground
    {
        get => (Brush)GetValue(StyleColorMouseOverBackgroundProperty);
        set => SetValue(StyleColorMouseOverBackgroundProperty, value);
    }

    /// StyleColorMouseOverBorder
    public static readonly DependencyProperty StyleColorMouseOverBorderProperty
        = DependencyProperty.Register(
            nameof(StyleColorMouseOverBorder),
            typeof(Brush),
            typeof(StswCheckButtonBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorMouseOverBorder
    {
        get => (Brush)GetValue(StyleColorMouseOverBorderProperty);
        set => SetValue(StyleColorMouseOverBorderProperty, value);
    }

    /// StyleColorPressedBackground
    public static readonly DependencyProperty StyleColorPressedBackgroundProperty
        = DependencyProperty.Register(
            nameof(StyleColorPressedBackground),
            typeof(Brush),
            typeof(StswCheckButtonBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorPressedBackground
    {
        get => (Brush)GetValue(StyleColorPressedBackgroundProperty);
        set => SetValue(StyleColorPressedBackgroundProperty, value);
    }

    /// StyleColorPressedBorder
    public static readonly DependencyProperty StyleColorPressedBorderProperty
        = DependencyProperty.Register(
            nameof(StyleColorPressedBorder),
            typeof(Brush),
            typeof(StswCheckButtonBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorPressedBorder
    {
        get => (Brush)GetValue(StyleColorPressedBorderProperty);
        set => SetValue(StyleColorPressedBorderProperty, value);
    }

    /// StyleColorCheckedBackground
    public static readonly DependencyProperty StyleColorCheckedBackgroundProperty
        = DependencyProperty.Register(
            nameof(StyleColorCheckedBackground),
            typeof(Brush),
            typeof(StswCheckButtonBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorCheckedBackground
    {
        get => (Brush)GetValue(StyleColorCheckedBackgroundProperty);
        set => SetValue(StyleColorCheckedBackgroundProperty, value);
    }

    /// StyleColorCheckedBorder
    public static readonly DependencyProperty StyleColorCheckedBorderProperty
        = DependencyProperty.Register(
            nameof(StyleColorCheckedBorder),
            typeof(Brush),
            typeof(StswCheckButtonBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorCheckedBorder
    {
        get => (Brush)GetValue(StyleColorCheckedBorderProperty);
        set => SetValue(StyleColorCheckedBorderProperty, value);
    }
    #endregion

    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswCheckButtonBase),
            new PropertyMetadata(default(CornerRadius))
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
}
