using System.Windows;

namespace StswExpress;

/// <summary>
/// Defines a contract for drop-down controls.
/// </summary>
public interface IStswDropControl
{
    /// <summary>
    /// Gets or sets a value indicating whether or not the drop-down portion of the control is currently open.
    /// </summary>
    public bool IsDropDownOpen { get; set; }
    public static readonly DependencyProperty? IsDropDownOpenProperty;

    /// <summary>
    /// Gets or sets the maximum height of the drop-down portion of the control.
    /// </summary>
    public double MaxDropDownHeight { get; set; }
    public static readonly DependencyProperty? MaxDropDownHeightProperty;

    /// <summary>
    /// Gets or sets the data model for properties of the dropdown popup associated with the control.
    /// The <see cref="StswPopupModel"/> class provides customization options for the appearance and behavior of the popup.
    /// </summary>
    public StswPopupModel Popup { get; set; }
    public static readonly DependencyProperty? PopupProperty;
}
