using System.Windows;
using System.Windows.Controls;

namespace StswExpress;
/// <summary>
/// A customizable menu control with extended functionality, 
/// including support for corner customization and styling.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswMenu&gt;
///     &lt;se:StswMenuItem Header="File"&gt;
///         &lt;se:StswMenuItem Header="Open"/&gt;
///         &lt;se:StswMenuItem Header="Save"/&gt;
///     &lt;/se:StswMenuItem&gt;
///     &lt;se:StswMenuItem Header="Edit"&gt;
///         &lt;se:StswMenuItem Header="Undo"/&gt;
///         &lt;se:StswMenuItem Header="Redo"/&gt;
///     &lt;/se:StswMenuItem&gt;
/// &lt;/se:StswMenu&gt;
/// </code>
/// </example>
public class StswMenu : Menu, IStswCornerControl
{
    static StswMenu()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswMenu), new FrameworkPropertyMetadata(typeof(StswMenu)));
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
