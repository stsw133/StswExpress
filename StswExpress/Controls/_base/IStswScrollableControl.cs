using System.Windows;

namespace StswExpress;

/// <summary>
/// Defines a contract for scrollable controls.
/// </summary>
public interface IStswScrollableControl
{
    /// <summary>
    /// Gets or sets the data model for properties of the scroll viewer associated with the control.
    /// The <see cref="StswScrollViewerModel"/> class provides customization options for the appearance and behavior of the scroll viewer.
    /// </summary>
    public StswScrollViewerModel ScrollViewer { get; set; }
    public static readonly DependencyProperty? ScrollViewerProperty;
}
