using System.Windows;
using System.Windows.Controls;

namespace StswExpress
{
    /// <summary>
    /// Interaction logic for ExtDataGrid.xaml
    /// </summary>
    public partial class ExtDataGrid : DataGrid
    {
        /// <summary>
        /// Background color of the column headers
        /// </summary>
        public static readonly DependencyProperty HeaderBackgroundProperty
            = DependencyProperty.Register(
                  nameof(HeaderBackground),
                  typeof(string),
                  typeof(ExtDataGrid),
                  new PropertyMetadata("#F9F9F9")
              );
        public string HeaderBackground
        {
            get => (string)GetValue(HeaderBackgroundProperty);
            set => SetValue(HeaderBackgroundProperty, value);
        }
    }
}
