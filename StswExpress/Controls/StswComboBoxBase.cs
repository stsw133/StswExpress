using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;

public class StswComboBoxBase : ComboBox
{
    #region StyleColors
    /// StyleColorDisabledBackground
    public static readonly DependencyProperty StyleColorDisabledBackgroundProperty
        = DependencyProperty.Register(
            nameof(StyleColorDisabledBackground),
            typeof(Brush),
            typeof(StswComboBoxBase),
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
            typeof(StswComboBoxBase),
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
            typeof(StswComboBoxBase),
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
            typeof(StswComboBoxBase),
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
            typeof(StswComboBoxBase),
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
            typeof(StswComboBoxBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorPressedBorder
    {
        get => (Brush)GetValue(StyleColorPressedBorderProperty);
        set => SetValue(StyleColorPressedBorderProperty, value);
    }

    /// StyleColorReadOnlyBackground
    public static readonly DependencyProperty StyleColorReadOnlyBackgroundProperty
        = DependencyProperty.Register(
            nameof(StyleColorReadOnlyBackground),
            typeof(Brush),
            typeof(StswComboBoxBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorReadOnlyBackground
    {
        get => (Brush)GetValue(StyleColorReadOnlyBackgroundProperty);
        set => SetValue(StyleColorReadOnlyBackgroundProperty, value);
    }

    /// StyleColorReadOnlyBorder
    public static readonly DependencyProperty StyleColorReadOnlyBorderProperty
        = DependencyProperty.Register(
            nameof(StyleColorReadOnlyBorder),
            typeof(Brush),
            typeof(StswComboBoxBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorReadOnlyBorder
    {
        get => (Brush)GetValue(StyleColorReadOnlyBorderProperty);
        set => SetValue(StyleColorReadOnlyBorderProperty, value);
    }

    /// StyleThicknessSubBorder
    public static readonly DependencyProperty StyleThicknessSubBorderProperty
        = DependencyProperty.Register(
            nameof(StyleThicknessSubBorder),
            typeof(Thickness),
            typeof(StswComboBoxBase),
            new PropertyMetadata(default(Thickness))
        );
    public Thickness StyleThicknessSubBorder
    {
        get => (Thickness)GetValue(StyleThicknessSubBorderProperty);
        set => SetValue(StyleThicknessSubBorderProperty, value);
    }
    #endregion

    /// ButtonAlignment
    public static readonly DependencyProperty ButtonAlignmentProperty
        = DependencyProperty.Register(
            nameof(ButtonAlignment),
            typeof(Dock),
            typeof(StswComboBoxBase),
            new PropertyMetadata(Dock.Right)
        );
    public Dock ButtonAlignment
    {
        get => (Dock)GetValue(ButtonAlignmentProperty);
        set => SetValue(ButtonAlignmentProperty, value);
    }

    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswComboBoxBase),
            new PropertyMetadata(default(CornerRadius))
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// SelectionMode
    public static readonly DependencyProperty SelectionModeProperty
        = DependencyProperty.Register(
              nameof(SelectionMode),
              typeof(SelectionMode),
              typeof(StswComboBoxBase),
              new PropertyMetadata(default(SelectionMode))
          );
    public SelectionMode SelectionMode
    {
        get => (SelectionMode)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }
}
