using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Defines a contract for component controls.
/// </summary>
public interface IStswComponent
{
    //public Visibility? ContentVisibility { get; set; }
    //public static readonly DependencyProperty? ContentVisibilityProperty;

    public GridLength? IconScale { get; set; }
    public static readonly DependencyProperty? IconScaleProperty;

    public Orientation Orientation { get; set; }
    public static readonly DependencyProperty? OrientationProperty;
}
