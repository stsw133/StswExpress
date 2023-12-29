using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Represents a control displaying rotatable arrow icon.
/// </summary>
public class StswDropArrow : Control
{
    static StswDropArrow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswDropArrow), new FrameworkPropertyMetadata(typeof(StswDropArrow)));
    }

    #region Main properties
    /// <summary>
    /// Gets or sets a value indicating whether or not the drop-down portion of the control is currently open.
    /// </summary>
    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }
    public static readonly DependencyProperty IsDropDownOpenProperty
        = DependencyProperty.Register(
            nameof(IsDropDownOpen),
            typeof(bool),
            typeof(StswDropArrow)
        );
    #endregion
}
