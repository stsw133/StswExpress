using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// A customizable toolbar control for grouping command buttons and tools.
/// </summary>
[Stsw("0.16.0", Changes = StswPlannedChanges.None)]
public class StswToolBar : ToolBar, IStswCornerControl
{
    static StswToolBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswToolBar), new FrameworkPropertyMetadata(typeof(StswToolBar)));
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
            typeof(StswToolBar),
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
            typeof(StswToolBar),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}

/* usage:

<se:StswToolBar>
    <Button Content="Save"/>
    <Button Content="Open"/>
</se:StswToolBar>

*/

/// <summary>
/// A container control designed for managing multiple toolbars in a flexible and customizable layout.
/// Allows positioning and arranging toolbars dynamically.
/// </summary>
[Stsw("0.16.0", Changes = StswPlannedChanges.None)]
public class StswToolBarTray : ToolBarTray
{
    static StswToolBarTray()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswToolBarTray), new FrameworkPropertyMetadata(typeof(StswToolBarTray)));
    }
}

/* usage:

<se:StswToolBarTray>
    <se:StswToolBar>
        <Button Content="Cut"/>
        <Button Content="Copy"/>
        <Button Content="Paste"/>
    </se:StswToolBar>
</se:StswToolBarTray>

*/
