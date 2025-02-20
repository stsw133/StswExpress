using System.Windows;
using System.Windows.Controls;

namespace StswExpress;
/// <summary>
/// A customizable menu control with extended functionality, 
/// including support for corner customization and styling.
/// </summary>
public class StswMenu : Menu, IStswCornerControl
{
    static StswMenu()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswMenu), new FrameworkPropertyMetadata(typeof(StswMenu)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswMenu), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
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
            typeof(StswMenu),
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
            typeof(StswMenu),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}

/* usage:

<se:StswMenu>
    <se:StswMenuItem Header="File">
        <se:StswMenuItem Header="Open"/>
        <se:StswMenuItem Header="Save"/>
    </se:StswMenuItem>
    <se:StswMenuItem Header="Edit">
        <se:StswMenuItem Header="Undo"/>
        <se:StswMenuItem Header="Redo"/>
    </se:StswMenuItem>
</se:StswMenu>

*/
