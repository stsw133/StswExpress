﻿using System.Windows;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Defines a contract for controls with an icon.
/// </summary>
[StswInfo("0.4.0")]
public interface IStswIconControl
{
    /// <summary>
    /// Gets or sets the geometry data that defines the icon shape.
    /// </summary>
    public Geometry? IconData { get; set; }
    public static readonly DependencyProperty? IconDataProperty;

    /// <summary>
    /// Gets or sets the brush used to fill the icon.
    /// </summary>
    public Brush IconFill { get; set; }
    public static readonly DependencyProperty? IconFillProperty;

    /// <summary>
    /// Gets or sets the scale of the icon, allowing size adjustments.
    /// </summary>
    public GridLength IconScale { get; set; }
    public static readonly DependencyProperty? IconScaleProperty;

    /// <summary>
    /// Gets or sets the brush used for the icon's stroke (outline).
    /// </summary>
    public Brush IconStroke { get; set; }
    public static readonly DependencyProperty? IconStrokeProperty;

    /// <summary>
    /// Gets or sets the thickness of the icon's stroke.
    /// </summary>
    public double IconStrokeThickness { get; set; }
    public static readonly DependencyProperty? IconStrokeThicknessProperty;
}
