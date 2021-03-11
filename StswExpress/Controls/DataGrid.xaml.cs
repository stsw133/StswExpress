using System.Windows;

namespace StswExpress.Controls
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
        public string HeaderBackground
        {
            get => (string)GetValue(pHeaderBackground);
            set { SetValue(pHeaderBackground, value); }
        }
        public static readonly DependencyProperty pHeaderBackground
            = DependencyProperty.Register(
                  nameof(HeaderBackground),
                  typeof(string),
                  typeof(DataGrid),
                  new PropertyMetadata("#DDD")
              );
    }
}
