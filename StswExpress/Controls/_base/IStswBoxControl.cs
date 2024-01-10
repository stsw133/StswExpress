using System.Collections.ObjectModel;
using System.Windows;

namespace StswExpress;

/// <summary>
/// Defines a contract for box controls.
/// </summary>
public interface IStswBoxControl
{
    /// <summary>
    /// Gets or sets the placeholder text to display in the box when no color is selected.
    /// </summary>
    public string? Placeholder { get; set; }
    public static readonly DependencyProperty? PlaceholderProperty;

    /// <summary>
    /// Gets or sets the collection of sub controls to be displayed in the control.
    /// </summary>
    public ObservableCollection<IStswSubControl> SubControls { get; set; }
    public static readonly DependencyProperty? SubControlsProperty;
}
