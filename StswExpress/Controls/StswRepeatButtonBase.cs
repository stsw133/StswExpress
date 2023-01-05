using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace StswExpress;

public class StswRepeatButtonBase : RepeatButton
{
    #region StyleColors
    /// StyleColorDisabledBackground
    public static readonly DependencyProperty StyleColorDisabledBackgroundProperty
        = DependencyProperty.Register(
            nameof(StyleColorDisabledBackground),
            typeof(Brush),
            typeof(StswRepeatButtonBase),
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
            typeof(StswRepeatButtonBase),
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
            typeof(StswRepeatButtonBase),
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
            typeof(StswRepeatButtonBase),
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
            typeof(StswRepeatButtonBase),
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
            typeof(StswRepeatButtonBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorPressedBorder
    {
        get => (Brush)GetValue(StyleColorPressedBorderProperty);
        set => SetValue(StyleColorPressedBorderProperty, value);
    }
    #endregion

    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswRepeatButtonBase),
            new PropertyMetadata(default(CornerRadius))
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
}
