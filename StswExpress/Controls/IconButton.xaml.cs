using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress.Controls
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
        /// Icon
        /// </summary>
        public ImageSource Icon
        {
            get => (ImageSource)GetValue(pIcon);
            set => SetValue(pIcon, value);
        }
        public static readonly DependencyProperty pIcon
            = DependencyProperty.Register(
                  nameof(Icon),
                  typeof(ImageSource),
                  typeof(IconButton),
                  new PropertyMetadata(null)
              );

        /// <summary>
        /// IconMargin
        /// </summary>
        public double IconMargin
        {
            get => (double)GetValue(pIconMargin);
            set => SetValue(pIconMargin, value);
        }
        public static readonly DependencyProperty pIconMargin
            = DependencyProperty.Register(
                  nameof(IconMargin),
                  typeof(double),
                  typeof(IconButton),
                  new PropertyMetadata(Application.Current.MainWindow.FontSize * 0.15)
              );

        /// <summary>
        /// IconSize
        /// </summary>
        public double IconSize
        {
            get => (double)GetValue(pIconSize);
            set => SetValue(pIconSize, value);
        }
        public static readonly DependencyProperty pIconSize
            = DependencyProperty.Register(
                  nameof(IconSize),
                  typeof(double),
                  typeof(IconButton),
                  new PropertyMetadata(Application.Current.MainWindow.FontSize * 2)
              );

        /// <summary>
        /// IsTextVisible
        /// </summary>
        public bool IsTextVisible
        {
            get => (bool)GetValue(pIsTextVisible);
            set => SetValue(pIsTextVisible, value);
        }
        public static readonly DependencyProperty pIsTextVisible
            = DependencyProperty.Register(
                  nameof(IsTextVisible),
                  typeof(bool),
                  typeof(IconButton),
                  new PropertyMetadata(true)
              );
        
        /// <summary>
        /// Orientation
        /// </summary>
        public Orientation Orientation
        {
            get => (Orientation)GetValue(pOrientation);
            set => SetValue(pOrientation, value);
        }
        public static readonly DependencyProperty pOrientation
            = DependencyProperty.Register(
                  nameof(Orientation),
                  typeof(Orientation),
                  typeof(IconButton),
                  new PropertyMetadata(Orientation.Horizontal)
              );

        /// <summary>
        /// Text
        /// </summary>
        public string Text
        {
            get => (string)GetValue(pText);
            set => SetValue(pText, value);
        }
        public static readonly DependencyProperty pText
            = DependencyProperty.Register(
                  nameof(Text),
                  typeof(string),
                  typeof(IconButton),
                  new PropertyMetadata(string.Empty)
              );
    }
}
