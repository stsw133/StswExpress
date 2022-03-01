using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress
{
    /// <summary>
    /// Interaction logic for IconButton.xaml
    /// </summary>
    public partial class IconButton : Button
    {
        public IconButton()
        {
            InitializeComponent();
        }

        /// <summary>
        /// CornerRadius
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty
            = DependencyProperty.Register(
                  nameof(CornerRadius),
                  typeof(double?),
                  typeof(IconButton),
                  new PropertyMetadata(default(double?))
              );
        public double? CornerRadius
        {
            get => (double?)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        /// <summary>
        /// Icon
        /// </summary>
        public static readonly DependencyProperty IconProperty
            = DependencyProperty.Register(
                  nameof(Icon),
                  typeof(ImageSource),
                  typeof(IconButton),
                  new PropertyMetadata(default(ImageSource))
              );
        public ImageSource Icon
        {
            get => (ImageSource)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        /// <summary>
        /// IconMargin
        /// </summary>
        public static readonly DependencyProperty IconMarginProperty
            = DependencyProperty.Register(
                  nameof(IconMargin),
                  typeof(Thickness),
                  typeof(IconButton),
                  new PropertyMetadata(default(Thickness))
              );
        public Thickness IconMargin
        {
            get => (Thickness)GetValue(IconMarginProperty);
            set => SetValue(IconMarginProperty, value);
        }

        /// <summary>
        /// IconSize
        /// </summary>
        public static readonly DependencyProperty IconSizeProperty
            = DependencyProperty.Register(
                  nameof(IconSize),
                  typeof(double?),
                  typeof(IconButton),
                  new PropertyMetadata(default(double?))
              );
        public double? IconSize
        {
            get => (double?)GetValue(IconSizeProperty);
            set => SetValue(IconSizeProperty, value);
        }

        /// <summary>
        /// LabelPadding
        /// </summary>
        public static readonly DependencyProperty LabelPaddingProperty
            = DependencyProperty.Register(
                  nameof(LabelPadding),
                  typeof(Thickness),
                  typeof(IconButton),
                  new PropertyMetadata(default(Thickness))
              );
        public Thickness LabelPadding
        {
            get => (Thickness)GetValue(LabelPaddingProperty);
            set => SetValue(LabelPaddingProperty, value);
        }

        /// <summary>
        /// Orientation
        /// </summary>
        public static readonly DependencyProperty OrientationProperty
            = DependencyProperty.Register(
                  nameof(Orientation),
                  typeof(Orientation),
                  typeof(IconButton),
                  new PropertyMetadata(default(Orientation))
              );
        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        /// <summary>
        /// PanelMargin
        /// </summary>
        public static readonly DependencyProperty PanelMarginProperty
            = DependencyProperty.Register(
                  nameof(PanelMargin),
                  typeof(Thickness),
                  typeof(IconButton),
                  new PropertyMetadata(default(Thickness))
              );
        public Thickness PanelMargin
        {
            get => (Thickness)GetValue(PanelMarginProperty);
            set => SetValue(PanelMarginProperty, value);
        }

        /// <summary>
        /// SubIcon
        /// </summary>
        public static readonly DependencyProperty SubIconProperty
            = DependencyProperty.Register(
                  nameof(SubIcon),
                  typeof(ImageSource),
                  typeof(IconButton),
                  new PropertyMetadata(default(ImageSource))
              );
        public ImageSource SubIcon
        {
            get => (ImageSource)GetValue(SubIconProperty);
            set => SetValue(SubIconProperty, value);
        }

        /// <summary>
        /// SubText
        /// </summary>
        public static readonly DependencyProperty SubTextProperty
            = DependencyProperty.Register(
                  nameof(SubText),
                  typeof(string),
                  typeof(IconButton),
                  new PropertyMetadata(default(string))
              );
        public string SubText
        {
            get => (string)GetValue(SubTextProperty);
            set => SetValue(SubTextProperty, value);
        }

        /// <summary>
        /// Text
        /// </summary>
        public static readonly DependencyProperty TextProperty
            = DependencyProperty.Register(
                  nameof(Text),
                  typeof(string),
                  typeof(IconButton),
                  new PropertyMetadata(default(string))
              );
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
    }
}
