using System.Windows;

namespace StswExpress;

/// <summary>
/// Defines a contract for controls with border.
/// </summary>
public interface IStswCorner
{
    public bool CornerClipping { get; set; }
    public static readonly DependencyProperty? CornerClippingProperty;

    public CornerRadius CornerRadius { get; set; }
    public static readonly DependencyProperty? CornerRadiusProperty;
}
