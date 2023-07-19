using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Represents a control that can be used to visually divide content in a user interface.
/// </summary>
public class StswSeparator : Separator
{
    static StswSeparator()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSeparator), new FrameworkPropertyMetadata(typeof(StswSeparator)));
    }

    #region Events
    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        OnBorderThicknessChanged(this, new DependencyPropertyChangedEventArgs());

        base.OnApplyTemplate();
    }
    #endregion

    #region Main properties
    /// <summary>
    /// Gets or sets the orientation of the control.
    /// </summary>
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
    public static readonly DependencyProperty OrientationProperty
        = DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(StswSeparator)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the thickness of the separator.
    /// </summary>
    public new double BorderThickness
    {
        get => (double)GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }
    public new static readonly DependencyProperty BorderThicknessProperty
        = DependencyProperty.Register(
            nameof(BorderThickness),
            typeof(double),
            typeof(StswSeparator),
            new PropertyMetadata(default(double), OnBorderThicknessChanged)
        );
    public static void OnBorderThicknessChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswSeparator stsw)
        {
            if (stsw.GetTemplateChild("PART_MainBorder") is Border border)
                border.BorderThickness = stsw.Orientation == Orientation.Horizontal
                    ? new Thickness(0, stsw.BorderThickness, 0, 0)
                    : new Thickness(stsw.BorderThickness, 0, 0, 0);
        }
    }

    /// <summary>
    /// Gets or sets the degree to which the corners of the control are rounded.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswSeparator)
        );
    #endregion
}
