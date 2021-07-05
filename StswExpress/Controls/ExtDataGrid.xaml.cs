using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress
{
    /// <summary>
    /// Interaction logic for ExtDataGrid.xaml
    /// </summary>
    public partial class ExtDataGrid : DataGrid
    {
        public ExtDataGrid()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Background color of the column headers
        /// </summary>
        public static readonly DependencyProperty HeaderBackgroundProperty
            = DependencyProperty.Register(
                  nameof(HeaderBackground),
                  typeof(Brush),
                  typeof(ExtDataGrid),
                  new PropertyMetadata((SolidColorBrush)new BrushConverter().ConvertFrom("#F9F9F9"))
              );
        public Brush HeaderBackground
        {
            get => (Brush)GetValue(HeaderBackgroundProperty);
            set => SetValue(HeaderBackgroundProperty, value);
        }
    }
}
