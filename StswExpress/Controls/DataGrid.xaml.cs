using System.Windows;

namespace StswExpress
{
    /// <summary>
    /// Interaction logic for DataGrid.xaml
    /// </summary>
    public partial class DataGrid : System.Windows.Controls.DataGrid
    {
        public DataGrid()
        {
            InitializeComponent();
        }

        /// <summary>
        /// HeaderBackground
        /// </summary>
        public static readonly DependencyProperty HeaderBackgroundProperty
            = DependencyProperty.Register(
                  nameof(HeaderBackground),
                  typeof(string),
                  typeof(DataGrid),
                  new PropertyMetadata("#F9F9F9")
              );
        public string HeaderBackground
        {
            get => (string)GetValue(HeaderBackgroundProperty);
            set => SetValue(HeaderBackgroundProperty, value);
        }
    }
}
