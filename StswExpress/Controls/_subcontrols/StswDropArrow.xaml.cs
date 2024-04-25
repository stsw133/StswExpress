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

    #region Logic properties
    /// <summary>
    /// Gets or sets a value indicating whether or not the content of the control is currently expanded.
    /// </summary>
    public bool? IsExpanded
    {
        get => (bool?)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }
    public static readonly DependencyProperty IsExpandedProperty
        = DependencyProperty.Register(
            nameof(IsExpanded),
            typeof(bool?),
            typeof(StswDropArrow)
        );
    #endregion
}
