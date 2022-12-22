using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Interaction logic for StswMenuItem.xaml
/// </summary>
public partial class StswMenuItem : MenuItem
{
    public StswMenuItem()
    {
        InitializeComponent();
        Loaded += (s, e) => {
            if (Image != null)
                Icon = new Image() { Source = Image };
        };
    }

    /// Image
    public static readonly DependencyProperty ImageProperty
        = DependencyProperty.Register(
            nameof(Image),
            typeof(ImageSource),
            typeof(StswMenuItem),
            new PropertyMetadata(default(ImageSource?))
        );
    public ImageSource? Image
    {
        get => (ImageSource?)GetValue(ImageProperty);
        set => SetValue(ImageProperty, value);
    }
}
