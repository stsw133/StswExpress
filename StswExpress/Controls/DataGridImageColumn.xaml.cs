using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress
{
    /// <summary>
    /// Interaction logic for DataGridImageColumn.xaml
    /// </summary>
    public partial class DataGridImageColumn : DataGridTemplateColumn
    {
        public DataGridImageColumn()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Image
        /// </summary>
        public static readonly DependencyProperty ImageProperty
            = DependencyProperty.Register(
                  nameof(Image),
                  typeof(ImageSource),
                  typeof(DataGridImageColumn),
                  new FrameworkPropertyMetadata(default(ImageSource))
              );
        public ImageSource Image
        {
            get => (ImageSource)GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }
    }
}
