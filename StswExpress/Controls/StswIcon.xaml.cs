using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Interaction logic for StswIcon.xaml
/// </summary>
public partial class StswIcon : UserControl
{
    public StswIcon()
    {
        InitializeComponent();
    }
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
            typeof(StswIcon)
        );
    public Brush ForegroundDisabled
    {
        get => (Brush)GetValue(ForegroundDisabledProperty);
        set => SetValue(ForegroundDisabledProperty, value);
    }
    #endregion
}
