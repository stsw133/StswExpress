using System.Windows;
using System.Windows.Controls;

namespace StswExpress
{
    /// <summary>
    /// Interaction logic for LoadingCircle.xaml
    /// </summary>
    public partial class LoadingCircle : UserControl
    {
        public LoadingCircle()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Size
        /// </summary>
        public static readonly DependencyProperty SizeProperty
            = DependencyProperty.Register(
                  nameof(Size),
                  typeof(double),
                  typeof(LoadingCircle),
                  new PropertyMetadata(Application.Current.MainWindow.FontSize)
              );
        public double Size
        {
            get => (double)GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        /// <summary>
        /// Text
        /// </summary>
        public static readonly DependencyProperty TextProperty
            = DependencyProperty.Register(
                  nameof(Text),
                  typeof(string),
                  typeof(LoadingCircle),
                  new PropertyMetadata(default(string))
              );
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
    }
}
