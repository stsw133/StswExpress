﻿using System.Windows;
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
    /// BorderBrushUnchecked
    public static readonly DependencyProperty BorderBrushUncheckedProperty
        = DependencyProperty.Register(
            nameof(BorderBrushUnchecked),
            typeof(Brush),
            typeof(StswCheckBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BorderBrushUnchecked
    {
        get => (Brush)GetValue(BorderBrushUncheckedProperty);
        set => SetValue(BorderBrushUncheckedProperty, value);
    }
    /// BorderBrushChecked
    public static readonly DependencyProperty BorderBrushCheckedProperty
        = DependencyProperty.Register(
            nameof(BorderBrushChecked),
            typeof(Brush),
            typeof(StswCheckBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BorderBrushChecked
    {
        get => (Brush)GetValue(BorderBrushCheckedProperty);
        set => SetValue(BorderBrushCheckedProperty, value);
    }
    /// BorderBrushIndeterminate
    public static readonly DependencyProperty BorderBrushIndeterminateProperty
        = DependencyProperty.Register(
            nameof(BorderBrushIndeterminate),
            typeof(Brush),
            typeof(StswCheckBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BorderBrushIndeterminate
    {
        get => (Brush)GetValue(BorderBrushIndeterminateProperty);
        set => SetValue(BorderBrushIndeterminateProperty, value);
    }
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
    /// ForegroundDisabled
    public static readonly DependencyProperty ForegroundDisabledProperty
        = DependencyProperty.Register(
            nameof(ForegroundDisabled),
            typeof(Brush),
            typeof(StswCheckBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush ForegroundDisabled
    {
        get => (Brush)GetValue(ForegroundDisabledProperty);
        set => SetValue(ForegroundDisabledProperty, value);
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

    /// BackgroundUncheckedMouseOver
    public static readonly DependencyProperty BackgroundUncheckedMouseOverProperty
        = DependencyProperty.Register(
            nameof(BackgroundUncheckedMouseOver),
            typeof(Brush),
            typeof(StswCheckBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BackgroundUncheckedMouseOver
    {
        get => (Brush)GetValue(BackgroundUncheckedMouseOverProperty);
        set => SetValue(BackgroundUncheckedMouseOverProperty, value);
    }
    /// BackgroundCheckedMouseOver
    public static readonly DependencyProperty BackgroundCheckedMouseOverProperty
        = DependencyProperty.Register(
            nameof(BackgroundCheckedMouseOver),
            typeof(Brush),
            typeof(StswCheckBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BackgroundCheckedMouseOver
    {
        get => (Brush)GetValue(BackgroundCheckedMouseOverProperty);
        set => SetValue(BackgroundCheckedMouseOverProperty, value);
    }
    /// BackgroundIndeterminateMouseOver
    public static readonly DependencyProperty BackgroundIndeterminateMouseOverProperty
        = DependencyProperty.Register(
            nameof(BackgroundIndeterminateMouseOver),
            typeof(Brush),
            typeof(StswCheckBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BackgroundIndeterminateMouseOver
    {
        get => (Brush)GetValue(BackgroundIndeterminateMouseOverProperty);
        set => SetValue(BackgroundIndeterminateMouseOverProperty, value);
    }
    /// BorderBrushUncheckedMouseOver
    public static readonly DependencyProperty BorderBrushUncheckedMouseOverProperty
        = DependencyProperty.Register(
            nameof(BorderBrushUncheckedMouseOver),
            typeof(Brush),
            typeof(StswCheckBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BorderBrushUncheckedMouseOver
    {
        get => (Brush)GetValue(BorderBrushUncheckedMouseOverProperty);
        set => SetValue(BorderBrushUncheckedMouseOverProperty, value);
    }
    /// BorderBrushCheckedMouseOver
    public static readonly DependencyProperty BorderBrushCheckedMouseOverProperty
        = DependencyProperty.Register(
            nameof(BorderBrushCheckedMouseOver),
            typeof(Brush),
            typeof(StswCheckBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BorderBrushCheckedMouseOver
    {
        get => (Brush)GetValue(BorderBrushCheckedMouseOverProperty);
        set => SetValue(BorderBrushCheckedMouseOverProperty, value);
    }
    /// BorderBrushIndeterminateMouseOver
    public static readonly DependencyProperty BorderBrushIndeterminateMouseOverProperty
        = DependencyProperty.Register(
            nameof(BorderBrushIndeterminateMouseOver),
            typeof(Brush),
            typeof(StswCheckBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BorderBrushIndeterminateMouseOver
    {
        get => (Brush)GetValue(BorderBrushIndeterminateMouseOverProperty);
        set => SetValue(BorderBrushIndeterminateMouseOverProperty, value);
    }
    /// ForegroundMouseOver
    public static readonly DependencyProperty ForegroundMouseOverProperty
        = DependencyProperty.Register(
            nameof(ForegroundMouseOver),
            typeof(Brush),
            typeof(StswCheckBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush ForegroundMouseOver
    {
        get => (Brush)GetValue(ForegroundMouseOverProperty);
        set => SetValue(ForegroundMouseOverProperty, value);
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

    /// BackgroundUncheckedPressed
    public static readonly DependencyProperty BackgroundUncheckedPressedProperty
        = DependencyProperty.Register(
            nameof(BackgroundUncheckedPressed),
            typeof(Brush),
            typeof(StswCheckBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BackgroundUncheckedPressed
    {
        get => (Brush)GetValue(BackgroundUncheckedPressedProperty);
        set => SetValue(BackgroundUncheckedPressedProperty, value);
    }
    /// BackgroundCheckedPressed
    public static readonly DependencyProperty BackgroundCheckedPressedProperty
        = DependencyProperty.Register(
            nameof(BackgroundCheckedPressed),
            typeof(Brush),
            typeof(StswCheckBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BackgroundCheckedPressed
    {
        get => (Brush)GetValue(BackgroundCheckedPressedProperty);
        set => SetValue(BackgroundCheckedPressedProperty, value);
    }
    /// BackgroundIndeterminatePressed
    public static readonly DependencyProperty BackgroundIndeterminatePressedProperty
        = DependencyProperty.Register(
            nameof(BackgroundIndeterminatePressed),
            typeof(Brush),
            typeof(StswCheckBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BackgroundIndeterminatePressed
    {
        get => (Brush)GetValue(BackgroundIndeterminatePressedProperty);
        set => SetValue(BackgroundIndeterminatePressedProperty, value);
    }
    /// BorderBrushUncheckedPressed
    public static readonly DependencyProperty BorderBrushUncheckedPressedProperty
        = DependencyProperty.Register(
            nameof(BorderBrushUncheckedPressed),
            typeof(Brush),
            typeof(StswCheckBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BorderBrushUncheckedPressed
    {
        get => (Brush)GetValue(BorderBrushUncheckedPressedProperty);
        set => SetValue(BorderBrushUncheckedPressedProperty, value);
    }
    /// BorderBrushCheckedPressed
    public static readonly DependencyProperty BorderBrushCheckedPressedProperty
        = DependencyProperty.Register(
            nameof(BorderBrushCheckedPressed),
            typeof(Brush),
            typeof(StswCheckBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BorderBrushCheckedPressed
    {
        get => (Brush)GetValue(BorderBrushCheckedPressedProperty);
        set => SetValue(BorderBrushCheckedPressedProperty, value);
    }
    /// BorderBrushIndeterminatePressed
    public static readonly DependencyProperty BorderBrushIndeterminatePressedProperty
        = DependencyProperty.Register(
            nameof(BorderBrushIndeterminatePressed),
            typeof(Brush),
            typeof(StswCheckBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush BorderBrushIndeterminatePressed
    {
        get => (Brush)GetValue(BorderBrushIndeterminatePressedProperty);
        set => SetValue(BorderBrushIndeterminatePressedProperty, value);
    }
    /// ForegroundPressed
    public static readonly DependencyProperty ForegroundPressedProperty
        = DependencyProperty.Register(
            nameof(ForegroundPressed),
            typeof(Brush),
            typeof(StswCheckBox),
            new PropertyMetadata(default(Brush))
        );
    public Brush ForegroundPressed
    {
        get => (Brush)GetValue(ForegroundPressedProperty);
        set => SetValue(ForegroundPressedProperty, value);
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

    /// IconUnchecked
    public static readonly DependencyProperty IconUncheckedProperty
        = DependencyProperty.Register(
            nameof(IconUnchecked),
            typeof(Geometry),
            typeof(StswCheckBox),
            new PropertyMetadata(default(Geometry?))
        );
    public Geometry? IconUnchecked
    {
        get => (Geometry?)GetValue(IconUncheckedProperty);
        set => SetValue(IconUncheckedProperty, value);
    }
    /// IconChecked
    public static readonly DependencyProperty IconCheckedProperty
        = DependencyProperty.Register(
            nameof(IconChecked),
            typeof(Geometry),
            typeof(StswCheckBox),
            new PropertyMetadata(default(Geometry?))
        );
    public Geometry? IconChecked
    {
        get => (Geometry?)GetValue(IconCheckedProperty);
        set => SetValue(IconCheckedProperty, value);
    }
    /// IconIndeterminate
    public static readonly DependencyProperty IconIndeterminateProperty
        = DependencyProperty.Register(
            nameof(IconIndeterminate),
            typeof(Geometry),
            typeof(StswCheckBox),
            new PropertyMetadata(default(Geometry?))
        );
    public Geometry? IconIndeterminate
    {
        get => (Geometry?)GetValue(IconIndeterminateProperty);
        set => SetValue(IconIndeterminateProperty, value);
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
    #endregion
}
