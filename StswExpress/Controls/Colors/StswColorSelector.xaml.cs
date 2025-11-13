using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// Represents a color selector control that provides a selection of standard and theme colors.
/// Supports automatic color selection, customizable palettes, and two-way binding for the selected color.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswColorSelector SelectedColor="{Binding UserPreferredColor}"/&gt;
/// </code>
/// </example>
[ContentProperty(nameof(ColorPaletteStandard))]
public class StswColorSelector : Control, IStswCornerControl
{
    public ICommand SelectColorCommand { get; }

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
    /// Executes the command to select a color in the color selector.
    /// Updates the <see cref="SelectedColor"/> property when a new color is chosen.
    /// </summary>
    /// <param name="brush">The selected color as a <see cref="SolidColorBrush"/>.</param>
    private void SelectColor(SolidColorBrush? brush)
    {
        if (brush != null)
            SelectedColor = brush.Color;
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the automatic color option used in the color selector.
    /// This color is typically used as a default or fallback selection.
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
    /// Gets or sets the array of standard colors available for selection in the color selector.
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
    /// Gets or sets the array of theme colors available for selection in the color selector.
    /// These colors typically follow a predefined theme for consistency across UI elements.
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
    /// Gets or sets the currently selected color in the control.
    /// Supports two-way binding to allow dynamic updates.
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
                null, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    #endregion

    #region Style properties
    /// <inheritdoc/>
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

    /// <inheritdoc/>
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
    /// Gets or sets the thickness of the separator between the color selection areas.
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
