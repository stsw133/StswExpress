using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StswExpress
{
    /// <summary>
    /// Interaction logic for ColumnFilter_Date.xaml
    /// </summary>
    public partial class ColumnFilter_Date : StackPanel
    {
        public ColumnFilter_Date()
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
                  typeof(ColumnFilter_Date),
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
                 typeof(ColumnFilter_Date),
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
                  typeof(ColumnFilter_Date),
                  new PropertyMetadata(true)
              );
        public bool IsFilterVisible
        {
            get => (bool)GetValue(IsFilterVisibleProperty);
            set => SetValue(IsFilterVisibleProperty, value);
        }

        /// <summary>
        /// Value1
        /// </summary>
        public static readonly DependencyProperty Value1Property
            = DependencyProperty.Register(
                  nameof(Value1),
                  typeof(string),
                  typeof(ColumnFilter_Date),
                  new PropertyMetadata(default(string))
              );
        public string Value1
        {
            get => (string)GetValue(Value1Property);
            set => SetValue(Value1Property, value);
        }

        /// <summary>
        /// Value2
        /// </summary>
        public static readonly DependencyProperty Value2Property
            = DependencyProperty.Register(
                  nameof(Value2),
                  typeof(string),
                  typeof(ColumnFilter_Date),
                  new PropertyMetadata(default(string))
              );
        public string Value2
        {
            get => (string)GetValue(Value2Property);
            set => SetValue(Value2Property, value);
        }

        /// <summary>
        /// Refresh
        /// </summary>
        private void cmdRefresh_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                Commands.Refresh.Execute(null, Parent as UIElement);
            }
            catch { }
        }
    }
}
