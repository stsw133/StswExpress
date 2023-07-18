﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Represents a color selector control that provides a selection of standard and theme colors.
/// </summary>
[ContentProperty(nameof(ColorPaletteStandard))]
public class StswColorSelector : UserControl
{
    public ICommand SelectColorCommand { get; set; }

    public StswColorSelector()
    {
        SelectColorCommand = new StswRelayCommand<SolidColorBrush?>(SelectColor_Executed);
    }
    static StswColorSelector()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswColorSelector), new FrameworkPropertyMetadata(typeof(StswColorSelector)));
    }

    #region Events
    public event EventHandler? SelectedColorChanged;

    /// Command: select color
    public void SelectColor_Executed(SolidColorBrush? brush)
    {
        if (brush != null)
            SelectedColor = brush.Color;
    }
    #endregion

    #region Main properties
    /// <summary>
    /// 
    /// </summary>
    public SolidColorBrush ColorAuto
    {
        get => (SolidColorBrush)GetValue(ColorAutoProperty);
        set => SetValue(ColorAutoProperty, value);
    }
    public static readonly DependencyProperty ColorAutoProperty
        = DependencyProperty.Register(
            nameof(ColorAuto),
            typeof(SolidColorBrush),
            typeof(StswColorSelector)
        );

    /// <summary>
    /// 
    /// </summary>
    public SolidColorBrush[] ColorPaletteStandard
    {
        get => (SolidColorBrush[])GetValue(ColorPaletteStandardProperty);
        set => SetValue(ColorPaletteStandardProperty, value);
    }
    public static readonly DependencyProperty ColorPaletteStandardProperty
        = DependencyProperty.Register(
            nameof(ColorPaletteStandard),
            typeof(SolidColorBrush[]),
            typeof(StswColorSelector)
        );

    /// <summary>
    /// 
    /// </summary>
    public SolidColorBrush[] ColorPaletteTheme
    {
        get => (SolidColorBrush[])GetValue(ColorPaletteThemeProperty);
        set => SetValue(ColorPaletteThemeProperty, value);
    }
    public static readonly DependencyProperty ColorPaletteThemeProperty
        = DependencyProperty.Register(
            nameof(ColorPaletteTheme),
            typeof(SolidColorBrush[]),
            typeof(StswColorSelector)
        );

    /// <summary>
    /// 
    /// </summary>
    public Color SelectedColor
    {
        get => (Color)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }
    public static readonly DependencyProperty SelectedColorProperty
        = DependencyProperty.Register(
            nameof(SelectedColor),
            typeof(Color),
            typeof(StswColorSelector),
            new FrameworkPropertyMetadata(default(Color),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedColorChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnSelectedColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswColorSelector stsw)
            stsw.SelectedColorChanged?.Invoke(stsw, EventArgs.Empty);
    }
    #endregion

    #region Spacial properties
    /// <summary>
    /// 
    /// </summary>
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

    /// <summary>
    /// 
    /// </summary>
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

    /// <summary>
    /// 
    /// </summary>
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
}
