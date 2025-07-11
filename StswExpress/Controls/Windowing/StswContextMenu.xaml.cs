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
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;Button Content="Right Click Me"&gt;
///     &lt;Button.ContextMenu&gt;
///         &lt;se:StswContextMenu&gt;
///             &lt;MenuItem Header="Option 1"/&gt;
///             &lt;MenuItem Header="Option 2"/&gt;
///         &lt;/se:StswContextMenu&gt;
///     &lt;/Button.ContextMenu&gt;
/// &lt;/Button&gt;
/// </code>
/// </example>
[StswInfo("0.12.0")]
public class StswContextMenu : System.Windows.Controls.ContextMenu, IStswCornerControl
{
    static StswContextMenu()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswContextMenu), new FrameworkPropertyMetadata(typeof(StswContextMenu)));
    }

    #region Events & methods
    /// <inheritdoc/>
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
