using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress
{
    /// <summary>
    /// Interaction logic for ExtMenuItem.xaml
    /// </summary>
    public partial class ExtMenuItem : MenuItem
    {
        public ExtMenuItem()
        {
            InitializeComponent();
        }

        /// Image
        public static readonly DependencyProperty ImageProperty
            = DependencyProperty.Register(
                  nameof(Image),
                  typeof(ImageSource),
                  typeof(ExtMenuItem),
                  new PropertyMetadata(default(ImageSource))
              );
        public ImageSource Image
        {
            get => (ImageSource)GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }

        /// Loaded
        private void MenuItem_Loaded(object sender, RoutedEventArgs e)
        {
            if (Image != null)
                Icon = new Image
                {
                    Source = Image
                };
        }
    }
}
