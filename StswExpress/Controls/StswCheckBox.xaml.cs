using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Interaction logic for StswCheckBox.xaml
/// </summary>
public partial class StswCheckBox : CheckBox
{
    public StswCheckBox()
    {
        InitializeComponent();
    }
    static StswCheckBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswCheckBox), new FrameworkPropertyMetadata(typeof(StswCheckBox)));
    }

    #region Style
    /// GlyphBrush
    public static readonly DependencyProperty GlyphBrushProperty
        = DependencyProperty.Register(
            nameof(GlyphBrush),
            typeof(Brush),
            typeof(StswCheckBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush GlyphBrush
    {
        get => (Brush)GetValue(GlyphBrushProperty);
        set => SetValue(GlyphBrushProperty, value);
    }

    /// BackgroundDisabled
    public static readonly DependencyProperty BackgroundDisabledProperty
        = DependencyProperty.Register(
            nameof(BackgroundDisabled),
            typeof(Brush),
            typeof(StswCheckBox),
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
            typeof(StswCheckBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BorderBrushDisabled
    {
        get => (Brush)GetValue(BorderBrushDisabledProperty);
        set => SetValue(BorderBrushDisabledProperty, value);
    }
    /// GlyphBrushDisabled
    public static readonly DependencyProperty GlyphBrushDisabledProperty
        = DependencyProperty.Register(
            nameof(GlyphBrushDisabled),
            typeof(Brush),
            typeof(StswCheckBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush GlyphBrushDisabled
    {
        get => (Brush)GetValue(GlyphBrushDisabledProperty);
        set => SetValue(GlyphBrushDisabledProperty, value);
    }

    /// BackgroundMouseOver
    public static readonly DependencyProperty BackgroundMouseOverProperty
        = DependencyProperty.Register(
            nameof(BackgroundMouseOver),
            typeof(Brush),
            typeof(StswCheckBox),
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
            typeof(StswCheckBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BorderBrushMouseOver
    {
        get => (Brush)GetValue(BorderBrushMouseOverProperty);
        set => SetValue(BorderBrushMouseOverProperty, value);
    }
    /// GlyphBrushMouseOver
    public static readonly DependencyProperty GlyphBrushMouseOverProperty
        = DependencyProperty.Register(
            nameof(GlyphBrushMouseOver),
            typeof(Brush),
            typeof(StswCheckBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush GlyphBrushMouseOver
    {
        get => (Brush)GetValue(GlyphBrushMouseOverProperty);
        set => SetValue(GlyphBrushMouseOverProperty, value);
    }

    /// BackgroundPressed
    public static readonly DependencyProperty BackgroundPressedProperty
        = DependencyProperty.Register(
            nameof(BackgroundPressed),
            typeof(Brush),
            typeof(StswCheckBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BackgroundPressed
    {
        get => (Brush)GetValue(BackgroundPressedProperty);
        set => SetValue(BackgroundPressedProperty, value);
    }
    /// BorderBrushPressed
    public static readonly DependencyProperty BorderBrushPressedProperty
        = DependencyProperty.Register(
            nameof(BorderBrushPressed),
            typeof(Brush),
            typeof(StswCheckBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BorderBrushPressed
    {
        get => (Brush)GetValue(BorderBrushPressedProperty);
        set => SetValue(BorderBrushPressedProperty, value);
    }
    /// GlyphBrushPressed
    public static readonly DependencyProperty GlyphBrushPressedProperty
        = DependencyProperty.Register(
            nameof(GlyphBrushPressed),
            typeof(Brush),
            typeof(StswCheckBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush GlyphBrushPressed
    {
        get => (Brush)GetValue(GlyphBrushPressedProperty);
        set => SetValue(GlyphBrushPressedProperty, value);
    }
    /*
    /// StyleColorReadOnlyBackground
    public static readonly DependencyProperty StyleColorReadOnlyBackgroundProperty
        = DependencyProperty.Register(
            nameof(StyleColorReadOnlyBackground),
            typeof(Brush),
            typeof(StswCheckBox),
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
            typeof(StswCheckBox),
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
            typeof(StswCheckBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush StyleColorReadOnlyGlyph
    {
        get => (Brush)GetValue(StyleColorReadOnlyGlyphProperty);
        set => SetValue(StyleColorReadOnlyGlyphProperty, value);
    }
    */
    /// SymbolUnchecked
    public static readonly DependencyProperty SymbolUncheckedProperty
        = DependencyProperty.Register(
            nameof(SymbolUnchecked),
            typeof(char),
            typeof(StswCheckBox),
            new PropertyMetadata(default(char))
        );
    public char SymbolUnchecked
    {
        get => (char)GetValue(SymbolUncheckedProperty);
        set => SetValue(SymbolUncheckedProperty, value);
    }
    /// BackgroundUnchecked
    public static readonly DependencyProperty BackgroundUncheckedProperty
        = DependencyProperty.Register(
            nameof(BackgroundUnchecked),
            typeof(Brush),
            typeof(StswCheckBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BackgroundUnchecked
    {
        get => (Brush)GetValue(BackgroundUncheckedProperty);
        set => SetValue(BackgroundUncheckedProperty, value);
    }

    /// SymbolChecked
    public static readonly DependencyProperty SymbolCheckedProperty
        = DependencyProperty.Register(
            nameof(SymbolChecked),
            typeof(char),
            typeof(StswCheckBox),
            new PropertyMetadata(default(char))
        );
    public char SymbolChecked
    {
        get => (char)GetValue(SymbolCheckedProperty);
        set => SetValue(SymbolCheckedProperty, value);
    }
    /// BackgroundChecked
    public static readonly DependencyProperty BackgroundCheckedProperty
        = DependencyProperty.Register(
            nameof(BackgroundChecked),
            typeof(Brush),
            typeof(StswCheckBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BackgroundChecked
    {
        get => (Brush)GetValue(BackgroundCheckedProperty);
        set => SetValue(BackgroundCheckedProperty, value);
    }

    /// SymbolIndeterminate
    public static readonly DependencyProperty SymbolIndeterminateProperty
        = DependencyProperty.Register(
            nameof(SymbolIndeterminate),
            typeof(char),
            typeof(StswCheckBox),
            new PropertyMetadata(default(char))
        );
    public char SymbolIndeterminate
    {
        get => (char)GetValue(SymbolIndeterminateProperty);
        set => SetValue(SymbolIndeterminateProperty, value);
    }
    /// BackgroundIndeterminate
    public static readonly DependencyProperty BackgroundIndeterminateProperty
        = DependencyProperty.Register(
            nameof(BackgroundIndeterminate),
            typeof(Brush),
            typeof(StswCheckBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BackgroundIndeterminate
    {
        get => (Brush)GetValue(BackgroundIndeterminateProperty);
        set => SetValue(BackgroundIndeterminateProperty, value);
    }
    #endregion

    #region Properties
    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswCheckBox),
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
            typeof(StswCheckBox),
            new PropertyMetadata(default(bool))
        );
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }
    */
    #endregion
}
