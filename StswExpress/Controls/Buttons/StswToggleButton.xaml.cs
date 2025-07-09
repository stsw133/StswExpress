using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace StswExpress;

/// <summary>
/// Represents a toggle button control that allows users to switch between two states: <c>On</c> and <c>Off</c>.
/// This control extends <see cref="ToggleButton"/>, providing additional styling options such as corner rounding
/// and optional animations for state transitions.
/// </summary>
[Stsw(null)]
public class StswToggleButton : ToggleButton, IStswCornerControl
{
    private Border? _mainBorder;

    static StswToggleButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswToggleButton), new FrameworkPropertyMetadata(typeof(StswToggleButton)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _mainBorder = GetTemplateChild("OPT_MainBorder") as Border;
    }

    /// <inheritdoc/>
    [Stsw("0.12.0")]
    protected override void OnChecked(RoutedEventArgs e)
    {
        base.OnChecked(e);
        StswSharedAnimations.AnimateClick(this, _mainBorder, true);
    }

    /// <inheritdoc/>
    [Stsw("0.12.0")]
    protected override void OnUnchecked(RoutedEventArgs e)
    {
        base.OnUnchecked(e);
        StswSharedAnimations.AnimateClick(this, _mainBorder, false);
    }
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
            typeof(StswToggleButton),
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
            typeof(StswToggleButton),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}

/* usage:

<se:StswToggleButton Content="Enable Feature" IsChecked="True"/>

*/
