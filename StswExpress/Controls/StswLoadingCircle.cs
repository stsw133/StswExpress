using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;

namespace StswExpress;

public class StswLoadingCircle : UserControl
{
    static StswLoadingCircle()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswLoadingCircle), new FrameworkPropertyMetadata(typeof(StswLoadingCircle)));
    }

    #region Properties
    /// Scale
    public static readonly DependencyProperty ScaleProperty
        = DependencyProperty.Register(
            nameof(Scale),
            typeof(GridLength),
            typeof(StswLoadingCircle),
            new FrameworkPropertyMetadata(default(GridLength),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnScaleChanged, null, false, UpdateSourceTrigger.PropertyChanged)
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

    #region Style
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
    /// FillDisabled
    public static readonly DependencyProperty FillDisabledProperty
        = DependencyProperty.Register(
            nameof(FillDisabled),
            typeof(Brush),
            typeof(StswLoadingCircle)
        );
    public Brush FillDisabled
    {
        get => (Brush)GetValue(FillDisabledProperty);
        set => SetValue(FillDisabledProperty, value);
    }
    #endregion
}
