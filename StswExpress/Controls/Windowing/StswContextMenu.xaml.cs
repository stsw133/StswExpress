using System.Windows;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// A context menu with extended functionality, including support for corner radius customization.
/// Automatically applies styles to menu items when opened, ensuring a consistent visual appearance.
/// </summary>
/// <remarks>
/// This control enhances the standard WPF context menu by providing automatic styling updates for menu items,
/// including text, background, and border properties.
/// </remarks>
public class StswContextMenu : System.Windows.Controls.ContextMenu, IStswCornerControl
{
    static StswContextMenu()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswContextMenu), new FrameworkPropertyMetadata(typeof(StswContextMenu)));
    }

    #region Events & methods
    /// <summary>
    /// Called when the context menu is opened. 
    /// Applies custom styling and layout updates to ensure consistency across menu items.
    /// </summary>
    /// <param name="e">The event arguments.</param>
    protected override void OnOpened(RoutedEventArgs e)
    {
        base.OnOpened(e);

        foreach (var item in Items)
        {
            if (item is FrameworkElement frameworkElement)
            {
                frameworkElement.ClearValue(BackgroundProperty);
                frameworkElement.ClearValue(BorderBrushProperty);
                frameworkElement.ClearValue(ForegroundProperty);
                if (frameworkElement.ReadLocalValue(ForegroundProperty) == DependencyProperty.UnsetValue)
                    frameworkElement.SetCurrentValue(ForegroundProperty, FindResource("StswText.Static.Foreground") as Brush);
                frameworkElement.UpdateLayout();
            }
        }

        ClearValue(BackgroundProperty);
        ClearValue(BorderBrushProperty);
        ClearValue(ForegroundProperty);
        if (ReadLocalValue(ForegroundProperty) == DependencyProperty.UnsetValue)
            SetCurrentValue(ForegroundProperty, FindResource("StswText.Static.Foreground") as Brush);
        UpdateLayout();
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
            typeof(StswContextMenu),
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
            typeof(StswContextMenu),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}

/* usage:

<Button Content="Right Click Me">
    <Button.ContextMenu>
        <se:StswContextMenu>
            <MenuItem Header="Option 1"/>
            <MenuItem Header="Option 2"/>
        </se:StswContextMenu>
    </Button.ContextMenu>
</Button>

*/
