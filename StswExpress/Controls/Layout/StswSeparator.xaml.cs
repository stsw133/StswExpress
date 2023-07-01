using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

public class StswSeparator : Separator
{
    static StswSeparator()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSeparator), new FrameworkPropertyMetadata(typeof(StswSeparator)));
    }

    #region Events
    /// OnApplyTemplate
    public override void OnApplyTemplate()
    {
        OnBorderThicknessChanged(this, new DependencyPropertyChangedEventArgs());

        base.OnApplyTemplate();
    }
    #endregion

    #region Main properties
    /// Orientation
    public static readonly DependencyProperty OrientationProperty
        = DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(StswSeparator)
        );
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
    #endregion

    #region Spatial properties
    /// > BorderThickness ...
    /// BorderThickness
    public new static readonly DependencyProperty BorderThicknessProperty
        = DependencyProperty.Register(
            nameof(BorderThickness),
            typeof(double),
            typeof(StswSeparator),
            new PropertyMetadata(default(double), OnBorderThicknessChanged)
        );
    public new double BorderThickness
    {
        get => (double)GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }
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

    /// > CornerRadius ...
    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswSeparator)
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    #endregion
}
