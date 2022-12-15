using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Interaction logic for ExtMenuItem.xaml
/// </summary>
public partial class ExtMenuItem : MenuItem
{
    public ExtMenuItem()
    {
        InitializeComponent();
    }

    public static void IconChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is ExtMenuItem item && item.Image != null)
            item.Icon = new Image { Source = item.Image };
    }

    /// Image
    public static readonly DependencyProperty ImageProperty
        = DependencyProperty.Register(
              nameof(Image),
              typeof(ImageSource),
              typeof(ExtMenuItem),
              new FrameworkPropertyMetadata(default(ImageSource),
                  FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                  IconChanged, null, false, UpdateSourceTrigger.PropertyChanged)
          );
    public ImageSource Image
    {
        get => (ImageSource)GetValue(ImageProperty);
        set => SetValue(ImageProperty, value);
    }
}
