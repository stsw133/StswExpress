using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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

        /// <summary>
        /// IconUri
        /// </summary>
        public static readonly DependencyProperty IconUriProperty
            = DependencyProperty.Register(
                  nameof(IconUri),
                  typeof(string),
                  typeof(ExtMenuItem),
                  new PropertyMetadata(default(string))
              );
        public string IconUri
        {
            get => (string)GetValue(IconUriProperty);
            set => SetValue(IconUriProperty, value);
        }

        /// <summary>
        /// Loaded
        /// </summary>
        private void MenuItem_Loaded(object sender, RoutedEventArgs e)
        {
            if (IconUri != null)
            {
                Icon = new System.Windows.Controls.Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,," + IconUri, UriKind.RelativeOrAbsolute))
                };
            }
        }
    }
}
