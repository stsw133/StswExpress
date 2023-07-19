using System.Windows;

namespace StswExpress;

/// <summary>
/// Defines a contract for component controls.
/// </summary>
public interface IStswComponent
{
    /// <summary>
    /// Gets or sets the scale of the icon.
    /// </summary>
    public GridLength? IconScale { get; set; }
}
