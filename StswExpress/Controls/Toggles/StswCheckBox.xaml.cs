using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// A customizable checkbox control that supports three states: checked, unchecked, and indeterminate.
/// Includes animations, icon customization, and read-only mode.
/// </summary>
/// <remarks>
/// The control provides enhanced visual customization, including the ability to change icons 
/// for different states and prevent state changes when read-only mode is enabled.
/// </remarks>
public class StswCheckBox : CheckBox, IStswCornerControl
{
    static StswCheckBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswCheckBox), new FrameworkPropertyMetadata(typeof(StswCheckBox)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswCheckBox), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    #region Events & methods
    /// <summary>
    /// Prevents state changes when the <see cref="IsReadOnly"/> property is set to <see langword="true"/>.
    /// </summary>
    protected override void OnToggle()
    {
        if (!IsReadOnly)
            base.OnToggle();
    }

    /// <summary>
    /// Handles the checked event and triggers an animation if animations are enabled.
    /// </summary>
    /// <param name="e">The event arguments.</param>
    protected override void OnChecked(RoutedEventArgs e)
    {
        base.OnChecked(e);

        if (StswSettings.Default.EnableAnimations && StswControl.GetEnableAnimations(this))
        {
            if (GetTemplateChild("OPT_MainBorder") is Border border)
                StswSharedAnimations.AnimateClick(this, border, true);
        }
    }

    /// <summary>
    /// Handles the unchecked event and triggers an animation if animations are enabled.
    /// </summary>
    /// <param name="e">The event arguments.</param>
    protected override void OnUnchecked(RoutedEventArgs e)
    {
        base.OnUnchecked(e);

        if (StswSettings.Default.EnableAnimations && StswControl.GetEnableAnimations(this))
        {
            if (GetTemplateChild("OPT_MainBorder") is Border border)
                StswSharedAnimations.AnimateClick(this, border, false);
        }
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the scale of the icon inside the checkbox.
    /// </summary>
    public GridLength IconScale
    {
        get => (GridLength)GetValue(IconScaleProperty);
        set => SetValue(IconScaleProperty, value);
    }
    public static readonly DependencyProperty IconScaleProperty
        = DependencyProperty.Register(
            nameof(IconScale),
            typeof(GridLength),
            typeof(StswCheckBox)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the checkbox is in read-only mode.
    /// When set to <see langword="true"/>, the checkbox cannot be toggled.
    /// </summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }
    public static readonly DependencyProperty IsReadOnlyProperty
        = DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(StswCheckBox)
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
            typeof(StswCheckBox),
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
            typeof(StswCheckBox),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the brush used to render the checkbox glyph (icon).
    /// </summary>
    public Brush? GlyphBrush
    {
        get => (Brush?)GetValue(GlyphBrushProperty);
        set => SetValue(GlyphBrushProperty, value);
    }
    public static readonly DependencyProperty GlyphBrushProperty
        = DependencyProperty.Register(
            nameof(GlyphBrush),
            typeof(Brush),
            typeof(StswCheckBox),
            new FrameworkPropertyMetadata(default(Brush?), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the geometry used for the icon when the checkbox is in the checked state.
    /// </summary>
    public Geometry? IconChecked
    {
        get => (Geometry?)GetValue(IconCheckedProperty);
        set => SetValue(IconCheckedProperty, value);
    }
    public static readonly DependencyProperty IconCheckedProperty
        = DependencyProperty.Register(
            nameof(IconChecked),
            typeof(Geometry),
            typeof(StswCheckBox),
            new FrameworkPropertyMetadata(default(Geometry?), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the geometry used for the icon when the checkbox is in the indeterminate state.
    /// </summary>
    public Geometry? IconIndeterminate
    {
        get => (Geometry?)GetValue(IconIndeterminateProperty);
        set => SetValue(IconIndeterminateProperty, value);
    }
    public static readonly DependencyProperty IconIndeterminateProperty
        = DependencyProperty.Register(
            nameof(IconIndeterminate),
            typeof(Geometry),
            typeof(StswCheckBox),
            new FrameworkPropertyMetadata(default(Geometry?), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the geometry used for the icon when the checkbox is in the unchecked state.
    /// </summary>
    public Geometry? IconUnchecked
    {
        get => (Geometry?)GetValue(IconUncheckedProperty);
        set => SetValue(IconUncheckedProperty, value);
    }
    public static readonly DependencyProperty IconUncheckedProperty
        = DependencyProperty.Register(
            nameof(IconUnchecked),
            typeof(Geometry),
            typeof(StswCheckBox),
            new FrameworkPropertyMetadata(default(Geometry?), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}

/* usage:

<se:StswCheckBox Content="Advanced settings" IsIndeterminate="True" IsReadOnly="True"/>

*/
