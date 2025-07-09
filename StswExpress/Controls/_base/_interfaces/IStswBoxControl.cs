using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Defines a contract for box controls, providing properties for error handling, placeholder text, icons, 
/// read-only state, and sub-controls management.
/// </summary>
[Stsw("0.5.0")]
public interface IStswBoxControl
{
    /// <summary>
    /// Gets or sets a read-only collection of validation errors to display in the tooltip of the <see cref="StswSubError"/>.
    /// Provides detailed error messages when validation rules are violated.
    /// </summary>
    public ReadOnlyObservableCollection<ValidationError> Errors { get; set; }
    public static readonly DependencyProperty? ErrorsProperty;

    /// <summary>
    /// Gets or sets a value indicating whether the <see cref="StswSubError"/> is visible within the box when 
    /// there is at least one validation error. This provides a visual indicator of input errors.
    /// </summary>
    public bool HasError { get; set; }
    public static readonly DependencyProperty? HasErrorProperty;

    /// <summary>
    /// Gets or sets the icon displayed inside the box control.
    /// The icon can be an image, a vector element, or any other UI element.
    /// </summary>
    public object? Icon { get; set; }
    public static readonly DependencyProperty? IconProperty;

    /// <summary>
    /// Gets or sets a value indicating whether the control is in read-only mode.
    /// </summary>
    public bool IsReadOnly { get; set; }
    public static readonly DependencyProperty? IsReadOnlyProperty;

    /// <summary>
    /// Gets or sets the placeholder text displayed inside the box when no value is selected.
    /// The placeholder serves as a hint for expected input.
    /// </summary>
    public string? Placeholder { get; set; }
    public static readonly DependencyProperty? PlaceholderProperty;

    /// <summary>
    /// Gets or sets the collection of sub-controls to be displayed inside the control.
    /// Sub-controls can include additional buttons, actions, or decorative elements within the input field.
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
