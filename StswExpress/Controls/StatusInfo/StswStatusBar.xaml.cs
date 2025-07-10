using System.Windows;
using System.Windows.Controls.Primitives;

namespace StswExpress;

/// <summary>
/// A customizable status bar for displaying application state and notifications.
/// Supports corner radius customization and custom status items.
/// </summary>
/// <remarks>
/// This control provides a structured way to present status information in an application.
/// It allows for dynamic updates and styling of individual status bar items.
/// </remarks>
[StswInfo("0.16.0")]
public class StswStatusBar : StatusBar
{
    static StswStatusBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswStatusBar), new FrameworkPropertyMetadata(typeof(StswStatusBar)));
    }

    protected override DependencyObject GetContainerForItemOverride() => new StswStatusBarItem();
    protected override bool IsItemItsOwnContainerOverride(object item) => item is StswStatusBarItem;

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
            typeof(StswStatusBar),
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
            typeof(StswStatusBar),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}

/* usage:

<se:StswStatusBar>
    <se:StswStatusBarItem Content="Ready"/>
    <se:StswStatusBarItem Content="Connection: Stable"/>
</se:StswStatusBar>

*/
