using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// A customizable radio button that allows the user to select a single option from a group.
/// Supports icon customization, animations, and read-only mode.
/// </summary>
/// <remarks>
/// The control provides enhanced visual customization, including the ability to change icons 
/// for different states and prevent state changes when read-only mode is enabled.
/// </remarks>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswRadioBox Content="Option A" GroupName="Settings"/&gt;
/// &lt;se:StswRadioBox Content="Option B" GroupName="Settings" IsChecked="True"/&gt;
/// </code>
/// </example>
[StswInfo("0.1.0")]
public class StswRadioBox : RadioButton, IStswCornerControl
{
    private Border? _mainBorder;

    static StswRadioBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswRadioBox), new FrameworkPropertyMetadata(typeof(StswRadioBox)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _mainBorder = GetTemplateChild("OPT_MainBorder") as Border;
    }

    /// <inheritdoc/>
    [StswInfo("0.6.0")]
    protected override void OnToggle()
    {
        if (!IsReadOnly)
            base.OnToggle();
    }

    /// <inheritdoc/>
    [StswInfo("0.12.0")]
    protected override void OnChecked(RoutedEventArgs e)
    {
        base.OnChecked(e);
        StswSharedAnimations.AnimateClick(this, _mainBorder, true);
    }

    /// <inheritdoc/>
    [StswInfo("0.12.0")]
    protected override void OnUnchecked(RoutedEventArgs e)
    {
        base.OnUnchecked(e);
        StswSharedAnimations.AnimateClick(this, _mainBorder, false);
    }

    /// <inheritdoc/>
    [StswInfo("0.20.1")]
    protected override void OnClick()
    {
        if (AllowUncheck && IsChecked == true)
        {
            SetCurrentValue(IsCheckedProperty, false);
            RaiseEvent(new RoutedEventArgs(ClickEvent, this));
            return;
        }

        base.OnClick();
    }

    /// <inheritdoc/>
    [StswInfo("0.20.1")]
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (AllowUncheck && IsChecked == true && (e.Key == Key.Space || e.Key == Key.Enter))
        {
            SetCurrentValue(IsCheckedProperty, false);
            RaiseEvent(new RoutedEventArgs(ClickEvent, this));
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets a value indicating whether the radio button can be unchecked by clicking it again when it is already checked.
    /// </summary>
    [StswInfo("0.20.1")]
    public bool AllowUncheck
    {
        get => (bool)GetValue(AllowUncheckProperty);
        set => SetValue(AllowUncheckProperty, value);
    }
    public static readonly DependencyProperty AllowUncheckProperty
        = DependencyProperty.Register(
            nameof(AllowUncheck),
            typeof(bool),
            typeof(StswRadioBox)
        );

    /// <summary>
    /// Gets or sets the scale of the icon inside the radio button.
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
            typeof(StswRadioBox)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the radio button is in read-only mode.
    /// When set to <see langword="true"/>, the button cannot be toggled.
    /// </summary>
    [StswInfo("0.6.0")]
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }
    public static readonly DependencyProperty IsReadOnlyProperty
        = DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(StswRadioBox)
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
            typeof(StswRadioBox),
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
            typeof(StswRadioBox),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the brush used to render the radio button's glyph (icon).
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
            typeof(StswRadioBox),
            new FrameworkPropertyMetadata(default(Brush?), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the geometry used for the icon when the radio button is in the checked state.
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
            typeof(StswRadioBox),
            new FrameworkPropertyMetadata(default(Geometry?), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the geometry used for the icon when the radio button is in the indeterminate state.
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
            typeof(StswRadioBox),
            new FrameworkPropertyMetadata(default(Geometry?), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the geometry used for the icon when the radio button is in the unchecked state.
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
            typeof(StswRadioBox),
            new FrameworkPropertyMetadata(default(Geometry?), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
