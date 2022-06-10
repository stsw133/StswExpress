using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress
{
    /// <summary>
    /// Interaction logic for ExtIcon.xaml
    /// </summary>
    public partial class ExtIcon : UserControl
    {
        public ExtIcon()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Icon
        /// </summary>
        public static readonly DependencyProperty IconProperty
            = DependencyProperty.Register(
                  nameof(Icon),
                  typeof(ImageSource),
                  typeof(ExtIcon),
                  new PropertyMetadata(default(ImageSource))
              );
        public ImageSource Icon
        {
            get => (ImageSource)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        /// <summary>
        /// IconScale
        /// </summary>
        public static readonly DependencyProperty IconScaleProperty
            = DependencyProperty.Register(
                  nameof(IconScale),
                  typeof(double),
                  typeof(ExtIcon),
                  new PropertyMetadata(1.5)
              );
        public double IconScale
        {
            get => (double)GetValue(IconScaleProperty);
            set => SetValue(IconScaleProperty, value);
        }

        /// <summary>
        /// SubIcon
        /// </summary>
        public static readonly DependencyProperty SubIconProperty
            = DependencyProperty.Register(
                  nameof(SubIcon),
                  typeof(ImageSource),
                  typeof(ExtIcon),
                  new PropertyMetadata(default(ImageSource))
              );
        public ImageSource SubIcon
        {
            get => (ImageSource)GetValue(SubIconProperty);
            set => SetValue(SubIconProperty, value);
        }
    }
}
