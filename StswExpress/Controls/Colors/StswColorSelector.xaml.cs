using System;
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
public class StswColorSelector : Control, IStswCornerControl
{
    public ICommand SelectColorCommand { get; set; }

    public StswColorSelector()
    {
        SelectColorCommand = new StswCommand<SolidColorBrush?>(SelectColor);
    }
    static StswColorSelector()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswColorSelector), new FrameworkPropertyMetadata(typeof(StswColorSelector)));
    }

    #region Events & methods
    /// <summary>
    /// Occurs when the selected color in the control changes.
    /// </summary>
    public event EventHandler? SelectedColorChanged;

    /// Command: select color
    /// <summary>
    /// Executes the command to select a color in the color selector.
    /// </summary>
    private void SelectColor(SolidColorBrush? brush)
    {
        if (brush != null)
            SelectedColor = brush.Color;
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the auto color used in the color selector.
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
    /// Gets or sets the standard color palette used in the color selector.
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
    /// Gets or sets the theme color palette used in the color selector.
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
    /// Gets or sets the selected color in the control.
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

    #region Style properties
    /// <summary>
    /// Gets or sets a value indicating whether corner clipping is enabled for the control.
    /// When set to <see langword="true"/>, content within the control's border area is clipped to match
    /// the border's rounded corners, preventing elements from protruding beyond the border.
    /// </summary>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty
        = DependencyProperty.Register(
            nameof(CornerClipping),
            typeof(bool),
            typeof(StswColorSelector),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the degree to which the corners of the control's border are rounded by defining
    /// a radius value for each corner independently. This property allows users to control the roundness
    /// of corners, and large radius values are smoothly scaled to blend from corner to corner.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswColorSelector),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the thickness of the separator between arrow icon and main button.
    /// </summary>
    public double SeparatorThickness
    {
        get => (double)GetValue(SeparatorThicknessProperty);
        set => SetValue(SeparatorThicknessProperty, value);
    }
    public static readonly DependencyProperty SeparatorThicknessProperty
        = DependencyProperty.Register(
            nameof(SeparatorThickness),
            typeof(double),
            typeof(StswColorSelector),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
