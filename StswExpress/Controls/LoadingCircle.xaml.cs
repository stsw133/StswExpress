using System.Windows;
using System.Windows.Controls;

namespace StswExpress.Controls
{
    /// <summary>
    /// Interaction logic for LoadingCircle.xaml
    /// </summary>
    public partial class LoadingCircle : UserControl
    {
        public LoadingCircle()
        {
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// Size
        /// </summary>
        public double Size
        {
            get => (double)GetValue(pSize);
            set => SetValue(pSize, value);
        }
        public static readonly DependencyProperty pSize
            = DependencyProperty.Register(
                  nameof(Size),
                  typeof(double),
                  typeof(LoadingCircle),
                  new PropertyMetadata(Globals.Properties.iSize)
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
                  typeof(LoadingCircle),
                  new PropertyMetadata(string.Empty)
              );
    }
}
