using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Interaction logic for StswCheckBox.xaml
/// </summary>
public partial class StswCheckBox : StswCheckBoxBase
{
    public StswCheckBox()
    {
        InitializeComponent();
    }
    static StswCheckBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswCheckBox), new FrameworkPropertyMetadata(typeof(StswCheckBox)));
    }
}

public class StswCheckBoxBase : CheckBox
{
    #region StyleColors
    /// StyleColorGlyph
    public static readonly DependencyProperty StyleColorGlyphProperty
        = DependencyProperty.Register(
            nameof(StyleColorGlyph),
            typeof(Brush),
            typeof(StswCheckBoxBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorGlyph
    {
        get => (Brush)GetValue(StyleColorGlyphProperty);
        set => SetValue(StyleColorGlyphProperty, value);
    }

    /// StyleColorDisabledBackground
    public static readonly DependencyProperty StyleColorDisabledBackgroundProperty
        = DependencyProperty.Register(
            nameof(StyleColorDisabledBackground),
            typeof(Brush),
            typeof(StswCheckBoxBase),
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
            typeof(StswCheckBoxBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorDisabledBorder
    {
        get => (Brush)GetValue(StyleColorDisabledBorderProperty);
        set => SetValue(StyleColorDisabledBorderProperty, value);
    }

    /// StyleColorDisabledGlyph
    public static readonly DependencyProperty StyleColorDisabledGlyphProperty
        = DependencyProperty.Register(
            nameof(StyleColorDisabledGlyph),
            typeof(Brush),
            typeof(StswCheckBoxBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorDisabledGlyph
    {
        get => (Brush)GetValue(StyleColorDisabledGlyphProperty);
        set => SetValue(StyleColorDisabledGlyphProperty, value);
    }

    /// StyleColorMouseOverBackground
    public static readonly DependencyProperty StyleColorMouseOverBackgroundProperty
        = DependencyProperty.Register(
            nameof(StyleColorMouseOverBackground),
            typeof(Brush),
            typeof(StswCheckBoxBase),
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
            typeof(StswCheckBoxBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorMouseOverBorder
    {
        get => (Brush)GetValue(StyleColorMouseOverBorderProperty);
        set => SetValue(StyleColorMouseOverBorderProperty, value);
    }

    /// StyleColorMouseOverGlyph
    public static readonly DependencyProperty StyleColorMouseOverGlyphProperty
        = DependencyProperty.Register(
            nameof(StyleColorMouseOverGlyph),
            typeof(Brush),
            typeof(StswCheckBoxBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorMouseOverGlyph
    {
        get => (Brush)GetValue(StyleColorMouseOverGlyphProperty);
        set => SetValue(StyleColorMouseOverGlyphProperty, value);
    }

    /// StyleColorPressedBackground
    public static readonly DependencyProperty StyleColorPressedBackgroundProperty
        = DependencyProperty.Register(
            nameof(StyleColorPressedBackground),
            typeof(Brush),
            typeof(StswCheckBoxBase),
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
            typeof(StswCheckBoxBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorPressedBorder
    {
        get => (Brush)GetValue(StyleColorPressedBorderProperty);
        set => SetValue(StyleColorPressedBorderProperty, value);
    }

    /// StyleColorPressedGlyph
    public static readonly DependencyProperty StyleColorPressedGlyphProperty
        = DependencyProperty.Register(
            nameof(StyleColorPressedGlyph),
            typeof(Brush),
            typeof(StswCheckBoxBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorPressedGlyph
    {
        get => (Brush)GetValue(StyleColorPressedGlyphProperty);
        set => SetValue(StyleColorPressedGlyphProperty, value);
    }

    /// StyleColorReadOnlyBackground
    public static readonly DependencyProperty StyleColorReadOnlyBackgroundProperty
        = DependencyProperty.Register(
            nameof(StyleColorReadOnlyBackground),
            typeof(Brush),
            typeof(StswCheckBoxBase),
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
            typeof(StswCheckBoxBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorReadOnlyBorder
    {
        get => (Brush)GetValue(StyleColorReadOnlyBorderProperty);
        set => SetValue(StyleColorReadOnlyBorderProperty, value);
    }

    /// StyleColorReadOnlyGlyph
    public static readonly DependencyProperty StyleColorReadOnlyGlyphProperty
        = DependencyProperty.Register(
            nameof(StyleColorReadOnlyGlyph),
            typeof(Brush),
            typeof(StswCheckBoxBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorReadOnlyGlyph
    {
        get => (Brush)GetValue(StyleColorReadOnlyGlyphProperty);
        set => SetValue(StyleColorReadOnlyGlyphProperty, value);
    }

    /// StyleCharUnchecked
    public static readonly DependencyProperty StyleCharUncheckedProperty
        = DependencyProperty.Register(
            nameof(StyleCharUnchecked),
            typeof(char),
            typeof(StswCheckBoxBase),
            new PropertyMetadata(default(char))
        );
    public char StyleCharUnchecked
    {
        get => (char)GetValue(StyleCharUncheckedProperty);
        set => SetValue(StyleCharUncheckedProperty, value);
    }

    /// StyleColorUncheckedBackground
    public static readonly DependencyProperty StyleColorUncheckedBackgroundProperty
        = DependencyProperty.Register(
            nameof(StyleColorUncheckedBackground),
            typeof(Brush),
            typeof(StswCheckBoxBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorUncheckedBackground
    {
        get => (Brush)GetValue(StyleColorUncheckedBackgroundProperty);
        set => SetValue(StyleColorUncheckedBackgroundProperty, value);
    }

    /// StyleCharChecked
    public static readonly DependencyProperty StyleCharCheckedProperty
        = DependencyProperty.Register(
            nameof(StyleCharChecked),
            typeof(char),
            typeof(StswCheckBoxBase),
            new PropertyMetadata(default(char))
        );
    public char StyleCharChecked
    {
        get => (char)GetValue(StyleCharCheckedProperty);
        set => SetValue(StyleCharCheckedProperty, value);
    }

    /// StyleColorCheckedBackground
    public static readonly DependencyProperty StyleColorCheckedBackgroundProperty
        = DependencyProperty.Register(
            nameof(StyleColorCheckedBackground),
            typeof(Brush),
            typeof(StswCheckBoxBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorCheckedBackground
    {
        get => (Brush)GetValue(StyleColorCheckedBackgroundProperty);
        set => SetValue(StyleColorCheckedBackgroundProperty, value);
    }

    /// StyleCharIndeterminate
    public static readonly DependencyProperty StyleCharIndeterminateProperty
        = DependencyProperty.Register(
            nameof(StyleCharIndeterminate),
            typeof(char),
            typeof(StswCheckBoxBase),
            new PropertyMetadata(default(char))
        );
    public char StyleCharIndeterminate
    {
        get => (char)GetValue(StyleCharIndeterminateProperty);
        set => SetValue(StyleCharIndeterminateProperty, value);
    }

    /// StyleColorIndeterminateBackground
    public static readonly DependencyProperty StyleColorIndeterminateBackgroundProperty
        = DependencyProperty.Register(
            nameof(StyleColorIndeterminateBackground),
            typeof(Brush),
            typeof(StswCheckBoxBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorIndeterminateBackground
    {
        get => (Brush)GetValue(StyleColorIndeterminateBackgroundProperty);
        set => SetValue(StyleColorIndeterminateBackgroundProperty, value);
    }
    #endregion

    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswCheckBoxBase),
            new PropertyMetadata(default(CornerRadius))
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    /*
    /// IsReadOnly
    public static readonly DependencyProperty IsReadOnlyProperty
        = DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(StswCheckBoxBase),
            new PropertyMetadata(default(bool))
        );
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }
    */
}
