using System.Windows;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Represents a context menu with extended functionality, including support for corner customization.
/// </summary>
public class StswContextMenu : System.Windows.Controls.ContextMenu, IStswCornerControl
{
    static StswContextMenu()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswContextMenu), new FrameworkPropertyMetadata(typeof(StswContextMenu)));
    }

    /// <summary>
    /// Called when the context menu is opened. Applies custom styling and layout updates to menu items.
    /// </summary>
    /// <param name="e">The event arguments</param>
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
    
    #region Style properties
    /// <summary>
    /// Gets or sets a value indicating whether corner clipping is enabled for the control.
    /// When set to <see langword="true"/>, content within the control's border area is clipped to match
    /// the border's rounded corners, preventing elements from protruding beyond the border.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the degree to which the corners of the control's border are rounded by defining
    /// a radius value for each corner independently. This property allows users to control the roundness
    /// of corners, and large radius values are smoothly scaled to blend from corner to corner.
    /// </summary>
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
