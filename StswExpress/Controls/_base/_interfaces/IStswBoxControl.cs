using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Defines a contract for box controls.
/// </summary>
public interface IStswBoxControl
{
    /// <summary>
    /// Gets or sets a collection of errors to display in <see cref="StswSubError"/>'s tooltip.
    /// </summary>
    public ReadOnlyObservableCollection<ValidationError> Errors { get; set; }
    public static readonly DependencyProperty? ErrorsProperty;

    /// <summary>
    /// Gets or sets a value indicating whether the <see cref="StswSubError"/> is visible within the box when there is at least one validation error.
    /// </summary>
    public bool HasError { get; set; }
    public static readonly DependencyProperty? HasErrorProperty;

    /// <summary>
    /// Gets or sets the icon section of the box.
    /// </summary>
    public object? Icon { get; set; }
    public static readonly DependencyProperty? IconProperty;
    
    /// <summary>
    /// Gets or sets a value indicating whether the drop button is in read-only mode.
    /// When set to true, the popup with items is accessible, but all items within the popup are disabled.
    /// </summary>
    public bool IsReadOnly { get; set; }
    public static readonly DependencyProperty? IsReadOnlyProperty;

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
