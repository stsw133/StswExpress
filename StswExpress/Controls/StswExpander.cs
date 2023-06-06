﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;

public class StswExpander : Expander
{
    static StswExpander()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswExpander), new FrameworkPropertyMetadata(typeof(StswExpander)));
    }

    #region Main properties
    /// ArrowVisibility
    public static readonly DependencyProperty ArrowVisibilityProperty
        = DependencyProperty.Register(
            nameof(ArrowVisibility),
            typeof(Visibility),
            typeof(StswExpander)
        );
    public Visibility ArrowVisibility
    {
        get => (Visibility)GetValue(ArrowVisibilityProperty);
        set => SetValue(ArrowVisibilityProperty, value);
    }
    #endregion

    #region Spatial properties
    /// > BorderThickness ...
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

    /// > CornerRadius ...
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

    /// > Padding ...
    /// HeaderPadding
    public static readonly DependencyProperty HeaderPaddingProperty
        = DependencyProperty.Register(
            nameof(HeaderPadding),
            typeof(Thickness),
            typeof(StswExpander)
        );
    public Thickness HeaderPadding
    {
        get => (Thickness)GetValue(HeaderPaddingProperty);
        set => SetValue(HeaderPaddingProperty, value);
    }
    #endregion

    #region Style properties
    /// > Background ...
    /// HeaderBackground
    public static readonly DependencyProperty HeaderBackgroundProperty
        = DependencyProperty.Register(
            nameof(HeaderBackground),
            typeof(Brush),
            typeof(StswExpander)
        );
    public Brush HeaderBackground
    {
        get => (Brush)GetValue(HeaderBackgroundProperty);
        set => SetValue(HeaderBackgroundProperty, value);
    }
    /// HeaderBackgroundMouseOver
    public static readonly DependencyProperty HeaderBackgroundMouseOverProperty
        = DependencyProperty.Register(
            nameof(HeaderBackgroundMouseOver),
            typeof(Brush),
            typeof(StswExpander)
        );
    public Brush HeaderBackgroundMouseOver
    {
        get => (Brush)GetValue(HeaderBackgroundMouseOverProperty);
        set => SetValue(HeaderBackgroundMouseOverProperty, value);
    }
    /// HeaderBackgroundPressed
    public static readonly DependencyProperty HeaderBackgroundPressedProperty
        = DependencyProperty.Register(
            nameof(HeaderBackgroundPressed),
            typeof(Brush),
            typeof(StswExpander)
        );
    public Brush HeaderBackgroundPressed
    {
        get => (Brush)GetValue(HeaderBackgroundPressedProperty);
        set => SetValue(HeaderBackgroundPressedProperty, value);
    }
    /// HeaderBackgroundChecked
    public static readonly DependencyProperty HeaderBackgroundCheckedProperty
        = DependencyProperty.Register(
            nameof(HeaderBackgroundChecked),
            typeof(Brush),
            typeof(StswExpander)
        );
    public Brush HeaderBackgroundChecked
    {
        get => (Brush)GetValue(HeaderBackgroundCheckedProperty);
        set => SetValue(HeaderBackgroundCheckedProperty, value);
    }
    /// HeaderBackgroundDisabled
    public static readonly DependencyProperty HeaderBackgroundDisabledProperty
        = DependencyProperty.Register(
            nameof(HeaderBackgroundDisabled),
            typeof(Brush),
            typeof(StswExpander)
        );
    public Brush HeaderBackgroundDisabled
    {
        get => (Brush)GetValue(HeaderBackgroundDisabledProperty);
        set => SetValue(HeaderBackgroundDisabledProperty, value);
    }

    /// > BorderBrush ...
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

    /// > Foreground ...
    /// HeaderForegroundDisabled
    public static readonly DependencyProperty HeaderForegroundDisabledProperty
        = DependencyProperty.Register(
            nameof(HeaderForegroundDisabled),
            typeof(Brush),
            typeof(StswExpander)
        );
    public Brush HeaderForegroundDisabled
    {
        get => (Brush)GetValue(HeaderForegroundDisabledProperty);
        set => SetValue(HeaderForegroundDisabledProperty, value);
    }
    /// HeaderForegroundMouseOver
    public static readonly DependencyProperty HeaderForegroundMouseOverProperty
        = DependencyProperty.Register(
            nameof(HeaderForegroundMouseOver),
            typeof(Brush),
            typeof(StswExpander)
        );
    public Brush HeaderForegroundMouseOver
    {
        get => (Brush)GetValue(HeaderForegroundMouseOverProperty);
        set => SetValue(HeaderForegroundMouseOverProperty, value);
    }
    /// HeaderForegroundPressed
    public static readonly DependencyProperty HeaderForegroundPressedProperty
        = DependencyProperty.Register(
            nameof(HeaderForegroundPressed),
            typeof(Brush),
            typeof(StswExpander)
        );
    public Brush HeaderForegroundPressed
    {
        get => (Brush)GetValue(HeaderForegroundPressedProperty);
        set => SetValue(HeaderForegroundPressedProperty, value);
    }
    /// HeaderForegroundChecked
    public static readonly DependencyProperty HeaderForegroundCheckedProperty
        = DependencyProperty.Register(
            nameof(HeaderForegroundChecked),
            typeof(Brush),
            typeof(StswExpander)
        );
    public Brush HeaderForegroundChecked
    {
        get => (Brush)GetValue(HeaderForegroundCheckedProperty);
        set => SetValue(HeaderForegroundCheckedProperty, value);
    }
    #endregion
}
