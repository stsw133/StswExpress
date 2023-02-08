using System.Windows;
using System.Windows.Controls;

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

    /// Scale
    public static readonly DependencyProperty ScaleProperty
        = DependencyProperty.Register(
            nameof(Scale),
            typeof(double),
            typeof(LoadingCircle),
            new PropertyMetadata(1.5d)
        );
    public double Scale
    {
        get => (double)GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }
}
