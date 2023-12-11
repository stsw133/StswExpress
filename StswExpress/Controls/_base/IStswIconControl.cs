using System.Windows;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Defines a contract for controls with icon.
/// </summary>
public interface IStswIconControl
{
    /// <summary>
    /// Gets or sets the geometry data of the icon.
    /// </summary>
    public Geometry? IconData { get; set; }
    public static readonly DependencyProperty? IconDataProperty;

    /// <summary>
    /// Gets or sets the fill brush of the icon.
    /// </summary>
    public Brush IconFill { get; set; }
    public static readonly DependencyProperty? IconFillProperty;

    /// <summary>
    /// Gets or sets the scale of the icon.
    /// </summary>
    public GridLength IconScale { get; set; }
    public static readonly DependencyProperty? IconScaleProperty;

    /// <summary>
    /// Gets or sets the stroke brush of the icon.
    /// </summary>
    public Brush IconStroke { get; set; }
    public static readonly DependencyProperty? IconStrokeProperty;

    /// <summary>
    /// Gets or sets the stroke thickness of the icon.
    /// </summary>
    public double IconStrokeThickness { get; set; }
    public static readonly DependencyProperty? IconStrokeThicknessProperty;
}
