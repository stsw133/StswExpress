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

    #region Properties
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
            if (stsw.Scale == GridLength.Auto)
            {
                stsw.Height = double.NaN;
                stsw.Width = double.NaN;
            }
            else if (BindingOperations.GetBindingBase(stsw, HeightProperty) == null || BindingOperations.GetBindingBase(stsw, WidthProperty) == null)
            {
                var multiBinding = new MultiBinding();
                multiBinding.Bindings.Add(new Binding(nameof(StswSettings.Default.iSize)) { Source = StswSettings.Default });
                multiBinding.Bindings.Add(new Binding(nameof(Scale)) { RelativeSource = new RelativeSource(RelativeSourceMode.Self) });
                multiBinding.Converter = new StswCalculateConverter();
                multiBinding.ConverterParameter = "*";
                stsw.SetBinding(HeightProperty, multiBinding);
                stsw.SetBinding(WidthProperty, multiBinding);
            }
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
            typeof(StswIcon)
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
            typeof(StswIcon)
        );
    public Brush FillDisabled
    {
        get => (Brush)GetValue(FillDisabledProperty);
        set => SetValue(FillDisabledProperty, value);
    }
    #endregion
}
