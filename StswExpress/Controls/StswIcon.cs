using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;

namespace StswExpress;

public class StswIcon : UserControl
{
    static StswIcon()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswIcon), new FrameworkPropertyMetadata(typeof(StswIcon)));
    }

    #region Main properties
    /// CanvasSize
    public static readonly DependencyProperty CanvasSizeProperty
        = DependencyProperty.Register(
            nameof(CanvasSize),
            typeof(double),
            typeof(StswIcon)
        );
    public double CanvasSize
    {
        get => (double)GetValue(CanvasSizeProperty);
        set => SetValue(CanvasSizeProperty, value);
    }

    /// Data
    public static readonly DependencyProperty DataProperty
        = DependencyProperty.Register(
            nameof(Data),
            typeof(Geometry),
            typeof(StswIcon)
        );
    public Geometry? Data
    {
        get => (Geometry?)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    /// Scale
    public static readonly DependencyProperty ScaleProperty
        = DependencyProperty.Register(
            nameof(Scale),
            typeof(GridLength),
            typeof(StswIcon),
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
        if (obj is StswIcon stsw)
        {
            stsw.Height = stsw.Scale == GridLength.Auto ? double.NaN : stsw.Scale.Value * 12;
            stsw.Width = stsw.Scale == GridLength.Auto ? double.NaN : stsw.Scale.Value * 12;
        }
    }
    #endregion

    #region Style properties
    /// > Fill ...
    /// Fill
    public static readonly DependencyProperty FillProperty
        = DependencyProperty.Register(
            nameof(Fill),
            typeof(Brush),
            typeof(StswIcon)
        );
    public Brush Fill
    {
        get => (Brush)GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    /// > Opacity ...
    /// OpacityDisabled
    public static readonly DependencyProperty OpacityDisabledProperty
        = DependencyProperty.Register(
            nameof(OpacityDisabled),
            typeof(double),
            typeof(StswIcon)
        );
    public double OpacityDisabled
    {
        get => (double)GetValue(OpacityDisabledProperty);
        set => SetValue(OpacityDisabledProperty, value);
    }
    #endregion
}
