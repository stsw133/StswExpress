using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress
{
    /// <summary>
    /// Interaction logic for ExtGroupBox.xaml
    /// </summary>
    public partial class ExtGroupBox : GroupBox
    {
        public ExtGroupBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// CornerRadius
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty
            = DependencyProperty.Register(
                  nameof(CornerRadius),
                  typeof(double),
                  typeof(ExtGroupBox),
                  new PropertyMetadata(Settings.Default.iSize * 0.5)
              );
        public double CornerRadius
        {
            get => (double)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        /// <summary>
        /// Icon
        /// </summary>
        public static readonly DependencyProperty IconProperty
            = DependencyProperty.Register(
                  nameof(Icon),
                  typeof(ImageSource),
                  typeof(ExtGroupBox),
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
                  typeof(ExtGroupBox),
                  new PropertyMetadata(new Thickness(Settings.Default.iSize * 0.15))
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
                  typeof(double),
                  typeof(ExtGroupBox),
                  new PropertyMetadata(Settings.Default.iSize * 1.25)
              );
        public double IconSize
        {
            get => (double)GetValue(IconSizeProperty);
            set => SetValue(IconSizeProperty, value);
        }

        /// <summary>
        /// IsTextVisible
        /// </summary>
        public static readonly DependencyProperty IsTextVisibleProperty
            = DependencyProperty.Register(
                  nameof(IsTextVisible),
                  typeof(bool),
                  typeof(ExtGroupBox),
                  new PropertyMetadata(true)
              );
        public bool IsTextVisible
        {
            get => (bool)GetValue(IsTextVisibleProperty);
            set => SetValue(IsTextVisibleProperty, value);
        }

        /// <summary>
        /// LabelPadding
        /// </summary>
        public static readonly DependencyProperty LabelPaddingProperty
            = DependencyProperty.Register(
                  nameof(LabelPadding),
                  typeof(Thickness),
                  typeof(ExtGroupBox),
                  new PropertyMetadata(new Thickness(Settings.Default.iSize * 0.15))
              );
        public Thickness LabelPadding
        {
            get => (Thickness)GetValue(LabelPaddingProperty);
            set => SetValue(LabelPaddingProperty, value);
        }

        /// <summary>
        /// Text
        /// </summary>
        public static readonly DependencyProperty TextProperty
            = DependencyProperty.Register(
                  nameof(Text),
                  typeof(string),
                  typeof(ExtGroupBox),
                  new PropertyMetadata(default(string))
              );
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
    }
}
