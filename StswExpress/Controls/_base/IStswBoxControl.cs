using System.Collections.ObjectModel;
using System.Windows;

namespace StswExpress;

/// <summary>
/// Defines a contract for box controls.
/// </summary>
public interface IStswBoxControl
{
    /// <summary>
    /// Gets or sets a value indicating whether the error sub control is visible within the box when there is at least one validation error.
    /// </summary>
    public bool IsErrorVisible { get; set; }
    public static readonly DependencyProperty? IsErrorVisibleProperty;

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

    /*
    /// <summary>
    /// Updates the main property associated with the selected value in the control based on user input.
    /// </summary>
    /// <param name="alwaysUpdate">A value indicating whether to force a binding update regardless of changes.</param>
    public void UpdateMainProperty(bool alwaysUpdate);
    */
}
