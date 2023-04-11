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
