using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Interaction logic for StswIcon.xaml
/// </summary>
public partial class StswIcon : StswIconBase
{
    public StswIcon()
    {
        InitializeComponent();
    }
    static StswIcon()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswIcon), new FrameworkPropertyMetadata(typeof(StswIcon)));
    }
}

public class StswIconBase : UserControl
{
    #region Style
    /// Color
    public static readonly DependencyProperty ColorProperty
        = DependencyProperty.Register(
            nameof(Color),
            typeof(Brush),
            typeof(StswIconBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush Color
    {
        get => (Brush)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// ColorDisabled
    public static readonly DependencyProperty ColorDisabledProperty
        = DependencyProperty.Register(
            nameof(ColorDisabled),
            typeof(Brush),
            typeof(StswIconBase),
            new PropertyMetadata(default(Brush))
        );
    public Brush ColorDisabled
    {
        get => (Brush)GetValue(ColorDisabledProperty);
        set => SetValue(ColorDisabledProperty, value);
    }
    #endregion

    /// CanvasSize
    public static readonly DependencyProperty CanvasSizeProperty
        = DependencyProperty.Register(
            nameof(CanvasSize),
            typeof(double),
            typeof(StswIconBase),
            new PropertyMetadata(default(double))
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
            typeof(StswIconBase),
            new PropertyMetadata(default(Geometry?))
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
            typeof(double),
            typeof(StswIconBase),
            new PropertyMetadata(default(double))
        );
    public double Scale
    {
        get => (double)GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }
}
