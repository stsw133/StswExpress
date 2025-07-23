using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// A customizable toolbar control for grouping command buttons and tools.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswToolBar&gt;
///     &lt;Button Content="Save"/&gt;
///     &lt;Button Content="Open"/&gt;
/// &lt;/se:StswToolBar&gt;
/// </code>
/// </example>
[StswInfo("0.16.0")]
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

/// <summary>
/// A container control designed for managing multiple toolbars in a flexible and customizable layout.
/// Allows positioning and arranging toolbars dynamically.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswToolBarTray&gt;
///     &lt;se:StswToolBar&gt;
///         &lt;Button Content="Cut"/&gt;
///         &lt;Button Content="Copy"/&gt;
///         &lt;Button Content="Paste"/&gt;
///     &lt;/se:StswToolBar&gt;
/// &lt;/se:StswToolBarTray&gt;
/// </code>
/// </example>
[StswInfo("0.16.0")]
public class StswToolBarTray : ToolBarTray
{
    static StswToolBarTray()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswToolBarTray), new FrameworkPropertyMetadata(typeof(StswToolBarTray)));
    }
}
