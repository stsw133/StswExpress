using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Defines a contract for component controls.
/// </summary>
public interface IStswComponentControl
{
    public object? Content { get; set; }
    public static readonly DependencyProperty? ContentProperty;

    public Visibility? ContentVisibility { get; set; }
    public static readonly DependencyProperty? ContentVisibilityProperty;

    public GridLength? IconScale { get; set; }
    public static readonly DependencyProperty? IconScaleProperty;

    public bool IsBusy { get; set; }
    public static readonly DependencyProperty? IsBusyProperty;

    public Orientation Orientation { get; set; }
    public static readonly DependencyProperty? OrientationProperty;
}
