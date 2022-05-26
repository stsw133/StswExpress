using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress
{
    /// <summary>
    /// Interaction logic for Header.xaml
    /// </summary>
    public partial class Header : StackPanel
    {
        public Header()
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
                  typeof(Header),
                  new PropertyMetadata(default(ImageSource))
              );
        public ImageSource Icon
        {
            get => (ImageSource)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        /// <summary>
        /// IconSize
        /// </summary>
        public static readonly DependencyProperty IconSizeProperty
            = DependencyProperty.Register(
                  nameof(IconSize),
                  typeof(double),
                  typeof(Header),
                  new PropertyMetadata(1.5)
              );
        public double IconSize
        {
            get => (double)GetValue(IconSizeProperty);
            set => SetValue(IconSizeProperty, value);
        }

        /// <summary>
        /// Padding
        /// </summary>
        public static readonly DependencyProperty PaddingProperty
            = DependencyProperty.Register(
                  nameof(Padding),
                  typeof(Thickness),
                  typeof(Header),
                  new PropertyMetadata(new Thickness(5))
              );
        public Thickness Padding
        {
            get => (Thickness)GetValue(PaddingProperty);
            set => SetValue(PaddingProperty, value);
        }

        /// <summary>
        /// SubIcon
        /// </summary>
        public static readonly DependencyProperty SubIconProperty
            = DependencyProperty.Register(
                  nameof(SubIcon),
                  typeof(ImageSource),
                  typeof(Header),
                  new PropertyMetadata(default(ImageSource))
              );
        public ImageSource SubIcon
        {
            get => (ImageSource)GetValue(SubIconProperty);
            set => SetValue(SubIconProperty, value);
        }

        /// <summary>
        /// SubText1
        /// </summary>
        public static readonly DependencyProperty SubText1Property
            = DependencyProperty.Register(
                  nameof(SubText1),
                  typeof(string),
                  typeof(Header),
                  new PropertyMetadata(default(string))
              );
        public string SubText1
        {
            get => (string)GetValue(SubText1Property);
            set => SetValue(SubText1Property, value);
        }

        /// <summary>
        /// SubTextColor1
        /// </summary>
        public static readonly DependencyProperty SubTextColor1Property
            = DependencyProperty.Register(
                  nameof(SubTextColor1),
                  typeof(SolidColorBrush),
                  typeof(Header),
                  new PropertyMetadata(default(SolidColorBrush))
              );
        public SolidColorBrush SubTextColor1
        {
            get => (SolidColorBrush)GetValue(SubTextColor1Property);
            set => SetValue(SubTextColor1Property, value);
        }

        /// <summary>
        /// SubText2
        /// </summary>
        public static readonly DependencyProperty SubText2Property
            = DependencyProperty.Register(
                  nameof(SubText2),
                  typeof(string),
                  typeof(Header),
                  new PropertyMetadata(default(string))
              );
        public string SubText2
        {
            get => (string)GetValue(SubText2Property);
            set => SetValue(SubText2Property, value);
        }

        /// <summary>
        /// SubTextColor2
        /// </summary>
        public static readonly DependencyProperty SubTextColor2Property
            = DependencyProperty.Register(
                  nameof(SubTextColor2),
                  typeof(SolidColorBrush),
                  typeof(Header),
                  new PropertyMetadata(default(SolidColorBrush))
              );
        public SolidColorBrush SubTextColor2
        {
            get => (SolidColorBrush)GetValue(SubTextColor2Property);
            set => SetValue(SubTextColor2Property, value);
        }

        /// <summary>
        /// Text
        /// </summary>
        public static readonly DependencyProperty TextProperty
            = DependencyProperty.Register(
                  nameof(Text),
                  typeof(string),
                  typeof(Header),
                  new PropertyMetadata(default(string))
              );
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
    }
}
