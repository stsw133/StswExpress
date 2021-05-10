using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StswExpress.Controls
{
    /// <summary>
    /// Interaction logic for ColumnFilter_Text.xaml
    /// </summary>
    public partial class ColumnFilter_Text : StackPanel
    {
        public ColumnFilter_Text()
        {
            InitializeComponent();
        }

        /// <summary>
        /// FilterType
        /// </summary>
        public static readonly DependencyProperty FilterTypeProperty
            = DependencyProperty.Register(
                  nameof(FilterType),
                  typeof(string),
                  typeof(ColumnFilter_Text),
                  new PropertyMetadata(default(string))
              );
        public string FilterType
        {
            get => (string)GetValue(FilterTypeProperty);
            set => SetValue(FilterTypeProperty, value);
        }

        /// <summary>
        /// Header
        /// </summary>
        public static readonly DependencyProperty HeaderProperty
            = DependencyProperty.Register(
                  nameof(Header),
                  typeof(string),
                  typeof(ColumnFilter_Text),
                  new PropertyMetadata(default(string))
              );
        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        /// <summary>
        /// IsFilterVisible
        /// </summary>
        public static readonly DependencyProperty IsFilterVisibleProperty
            = DependencyProperty.Register(
                  nameof(IsFilterVisible),
                  typeof(bool),
                  typeof(ColumnFilter_Text),
                  new PropertyMetadata(true)
              );
        public bool IsFilterVisible
        {
            get => (bool)GetValue(IsFilterVisibleProperty);
            set => SetValue(IsFilterVisibleProperty, value);
        }

        /// <summary>
        /// Value
        /// </summary>
        public static readonly DependencyProperty ValueProperty
            = DependencyProperty.Register(
                  nameof(Value),
                  typeof(string),
                  typeof(ColumnFilter_Text),
                  new PropertyMetadata(default(string))
              );
        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        /// <summary>
        /// Refresh
        /// </summary>
        private void cmdRefresh_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                Globals.Commands.Refresh.Execute(null, Parent as UIElement);
            }
            catch { }
        }
    }
}
