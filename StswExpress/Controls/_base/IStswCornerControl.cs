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
    /// Gets or sets a value indicating whether corner clipping is enabled for the control.
    /// When set to <see langword="true"/>, content within the control's border area is clipped to match
    /// the border's rounded corners, preventing elements from protruding beyond the border.
    /// </summary>
    public bool CornerClipping { get; set; }
    public static readonly DependencyProperty? CornerClippingProperty;

    /// <summary>
    /// Gets or sets the degree to which the corners of the control's border are rounded by defining
    /// a radius value for each corner independently. This property allows users to control the roundness
    /// of corners, and large radius values are smoothly scaled to blend from corner to corner.
    /// </summary>
    public CornerRadius CornerRadius { get; set; }
    public static readonly DependencyProperty? CornerRadiusProperty;
}
