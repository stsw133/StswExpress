using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Interaction logic for CanvasPath.xaml
/// </summary>
public partial class CanvasPath : Viewbox
{
    public CanvasPath()
    {
        InitializeComponent();
    }

    /// Color
    public static readonly DependencyProperty ColorProperty
        = DependencyProperty.Register(
              nameof(Color),
              typeof(Brush),
              typeof(CanvasPath),
              new PropertyMetadata(default(Brushes))
          );
    public Brush Color
    {
        get => (Brush)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// Data
    public static readonly DependencyProperty DataProperty
        = DependencyProperty.Register(
              nameof(Data),
              typeof(string),
              typeof(CanvasPath),
              new PropertyMetadata(default(string))
          );
    public string Data
    {
        get => (string)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    /// Scale
    public static readonly DependencyProperty ScaleProperty
        = DependencyProperty.Register(
              nameof(Scale),
              typeof(double),
              typeof(CanvasPath),
              new PropertyMetadata(1.5)
          );
    public double Scale
    {
        get => (double)GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }
}
