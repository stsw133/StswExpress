using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace StswExpress.Avalonia;
/// <summary>
/// A custom border control that applies clipping to its child element with rounded corners.
/// </summary>
/// <remarks>
/// As a side effect <see cref="StswBorder"/> will surpress any databinding or animation of 
/// its childs <see cref="UIElement.Clip"/> property until the child is removed from <see cref="StswBorder"/>.
/// </remarks>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswBorder CornerClipping="True" CornerRadius="20"&gt;
///     &lt;Image Source="example.jpg"/&gt;
/// &lt;/se:StswBorder&gt;
/// </code>
/// </example>
public class StswBorder : Border
{
    private readonly RectangleGeometry _clipRect = new();

    static StswBorder()
    {
        AffectsRender<StswBorder>(CornerClippingProperty);
    }
    #region Events & methods
    /// <summary>
    /// Applies a rounded clipping region to the child element based on <see cref="CornerRadius"/> and <see cref="BorderThickness"/>.
    /// </summary>
    /// <param name="child">The child element to apply clipping to.</param>
    protected virtual void OnApplyChildClip(Control child)
    {
        _clipRect.RadiusX = _clipRect.RadiusY = Math.Max(0.0, CornerRadius.TopLeft - BorderThickness.Left * 0.5);
        _clipRect.Rect = new Rect(child.Bounds.Size);
        child.Clip = _clipRect;
    }
    #endregion

    #region Style properties
    /// <inheritdoc/>
    public bool CornerClipping
    {
        get => GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly StyledProperty<bool> CornerClippingProperty
        = AvaloniaProperty.Register<StswBorder, bool>(
            nameof(CornerClipping),
            defaultValue: false
        );
    #endregion
}
