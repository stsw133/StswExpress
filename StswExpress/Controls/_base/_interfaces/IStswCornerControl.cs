using System.Windows;

namespace StswExpress;
/// <summary>
/// Defines properties for enabling corner clipping and specifying corner radius in controls
/// that implement the <see cref="StswBorder"/> control to clip content within the border area.
/// </summary>
public interface IStswCornerControl
{
    /// <summary>
    /// Gets or sets the thickness of the control's border.
    /// </summary>
    public Thickness BorderThickness { get; set; }
    public static readonly DependencyProperty? BorderThicknessProperty;

    /// <summary>
    /// Gets or sets a value indicating whether the content of the control is clipped to match its rounded corners.
    /// When set to <see langword="true"/>, any content extending beyond the rounded border is visually restricted.
    /// </summary>
    public bool CornerClipping { get; set; }
    public static readonly DependencyProperty? CornerClippingProperty;

    /// <summary>
    /// Gets or sets the radius of the control's corners, allowing independent customization of each corner's roundness.
    /// Larger values result in smoother, more rounded corners.
    /// </summary>
    public CornerRadius CornerRadius { get; set; }
    public static readonly DependencyProperty? CornerRadiusProperty;
}
