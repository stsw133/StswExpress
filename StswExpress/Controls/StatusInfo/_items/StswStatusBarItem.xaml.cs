using System.Windows;
using System.Windows.Controls.Primitives;

namespace StswExpress;

/// <summary>
/// Represents an individual item inside the <see cref="StswStatusBar"/>.
/// Supports corner customization and tooltip integration.
/// </summary>
/// <remarks>
/// This control extends <see cref="StatusBarItem"/> to provide additional styling capabilities,
/// including corner radius control for rounded edges.
/// </remarks>
[Stsw("0.16.0", Changes = StswPlannedChanges.None)]
public class StswStatusBarItem : StatusBarItem, IStswCornerControl
{
    static StswStatusBarItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswStatusBarItem), new FrameworkPropertyMetadata(typeof(StswStatusBarItem)));
    }

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
            typeof(StswStatusBarItem),
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
            typeof(StswStatusBarItem),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}

/* usage:

<se:StswStatusBar>
    <se:StswStatusBarItem Content="Loading..." CornerRadius="5"/>
</se:StswStatusBar>

*/
