using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Defines a contract for sub controls.
/// </summary>
public interface IStswSubControl
{
    /// <summary>
    /// Gets or sets the content of the control.
    /// </summary>
    public object? Content { get; set; }
    public static readonly DependencyProperty? ContentProperty;

    /// <summary>
    /// Gets or sets the visibility of the content within the control.
    /// </summary>
    public Visibility? ContentVisibility { get; set; }
    public static readonly DependencyProperty? ContentVisibilityProperty;

    /// <summary>
    /// Gets or sets the scale of the icon.
    /// </summary>
    public GridLength IconScale { get; set; }
    public static readonly DependencyProperty? IconScaleProperty;

    /// <summary>
    /// Gets or sets a value indicating whether the control is in a busy/loading state.
    /// </summary>
    public bool IsBusy { get; set; }
    public static readonly DependencyProperty? IsBusyProperty;

    /// <summary>
    /// Gets or sets the orientation of the control.
    /// </summary>
    public Orientation Orientation { get; set; }
    public static readonly DependencyProperty? OrientationProperty;
}
