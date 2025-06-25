using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace StswExpress;
/// <summary>
/// A navigation control that manages multiple contexts and navigation elements.
/// Supports pinned items, compact/full modes, and dynamic content switching.
/// </summary>
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public class StswNavigationTree : TreeView, IStswCornerControl
{
    static StswNavigationTree()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswNavigationTree), new FrameworkPropertyMetadata(typeof(StswNavigationTree)));
    }

    protected override DependencyObject GetContainerForItemOverride() => new StswNavigationViewItem();
    protected override bool IsItemItsOwnContainerOverride(object item) => item is StswNavigationViewItem;

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
            typeof(StswNavigationTree),
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
            typeof(StswNavigationTree),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}

/* usage:

<se:StswNavigationView TabStripMode="Full">
    <se:StswNavigationViewItem Header="Dashboard"/>
    <se:StswNavigationViewItem Header="Settings"/>
</se:StswNavigationView>

*/
