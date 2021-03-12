using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress.Controls
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
        public ImageSource Icon
        {
            get => (ImageSource)GetValue(pIcon);
            set { SetValue(pIcon, value); }
        }
        public static readonly DependencyProperty pIcon
            = DependencyProperty.Register(
                  nameof(Icon),
                  typeof(ImageSource),
                  typeof(Header),
                  new PropertyMetadata(null)
              );

        /// <summary>
        /// Text
        /// </summary>
        public string Text
        {
            get => (string)GetValue(pText);
            set { SetValue(pText, value); }
        }
        public static readonly DependencyProperty pText
            = DependencyProperty.Register(
                  nameof(Text),
                  typeof(string),
                  typeof(Header),
                  new PropertyMetadata(null)
              );
    }
}
