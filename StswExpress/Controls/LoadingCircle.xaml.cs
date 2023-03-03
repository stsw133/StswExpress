using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// Interaction logic for LoadingCircle.xaml
/// </summary>
public partial class LoadingCircle : UserControl
{
    public LoadingCircle()
    {
        InitializeComponent();
    }
    static LoadingCircle()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(LoadingCircle), new FrameworkPropertyMetadata(typeof(LoadingCircle)));
    }

    #region Properties
    /// Scale
    public static readonly DependencyProperty ScaleProperty
        = DependencyProperty.Register(
            nameof(Scale),
            typeof(GridLength),
            typeof(LoadingCircle),
            new PropertyMetadata(OnScaleChanged)
        );
    public GridLength Scale
    {
        get => (GridLength)GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }
    public static void OnScaleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is LoadingCircle stsw)
        {
            if (stsw.Scale == GridLength.Auto)
            {
                stsw.Height = double.NaN;
                stsw.Width = double.NaN;
            }
            else if (BindingOperations.GetBindingBase(stsw, HeightProperty) == null || BindingOperations.GetBindingBase(stsw, WidthProperty) == null)
            {
                var multiBinding = new MultiBinding();
                multiBinding.Bindings.Add(new Binding(nameof(Settings.Default.iSize)) { Source = Settings.Default });
                multiBinding.Bindings.Add(new Binding(nameof(Scale)) { RelativeSource = new RelativeSource(RelativeSourceMode.Self) });
                multiBinding.Converter = new conv_Calculate();
                multiBinding.ConverterParameter = "*";
                stsw.SetBinding(HeightProperty, multiBinding);
                stsw.SetBinding(WidthProperty, multiBinding);
            }
        }
    }
    #endregion

    #region Style
    /// ForegroundDisabled
    public static readonly DependencyProperty ForegroundDisabledProperty
        = DependencyProperty.Register(
            nameof(ForegroundDisabled),
            typeof(Brush),
            typeof(LoadingCircle)
        );
    public Brush ForegroundDisabled
    {
        get => (Brush)GetValue(ForegroundDisabledProperty);
        set => SetValue(ForegroundDisabledProperty, value);
    }
    #endregion
}
