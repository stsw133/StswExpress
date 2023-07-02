using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;

public class StswLoadingCircle : UserControl
{
    static StswLoadingCircle()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswLoadingCircle), new FrameworkPropertyMetadata(typeof(StswLoadingCircle)));
    }

    #region Main properties
    /// Scale
    public static readonly DependencyProperty ScaleProperty
        = DependencyProperty.Register(
            nameof(Scale),
            typeof(GridLength),
            typeof(StswLoadingCircle),
            new PropertyMetadata(default(GridLength), OnScaleChanged)
        );
    public GridLength Scale
    {
        get => (GridLength)GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }
    public static void OnScaleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswLoadingCircle stsw)
        {
            stsw.Height = stsw.Scale == GridLength.Auto ? double.NaN : stsw.Scale.Value * 12;
            stsw.Width = stsw.Scale == GridLength.Auto ? double.NaN : stsw.Scale.Value * 12;
        }
    }
    #endregion

    #region Spatial properties
    private new Thickness? BorderThickness { get; set; }
    #endregion

    #region Style properties
    private new Brush? Background { get; set; }
    private new Brush? BorderBrush { get; set; }
    private new Brush? Foreground { get; set; }

    /// > Fill ...
    /// Fill
    public static readonly DependencyProperty FillProperty
        = DependencyProperty.Register(
            nameof(Fill),
            typeof(Brush),
            typeof(StswLoadingCircle)
        );
    public Brush Fill
    {
        get => (Brush)GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }
    #endregion
}
