using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StswExpress.Controls
{
    /// <summary>
    /// Interaction logic for ColumnFilter_Number.xaml
    /// </summary>
    public partial class ColumnFilter_Number : StackPanel
    {
        public ColumnFilter_Number()
        {
            InitializeComponent();
        }

        /// <summary>
        /// FilterType
        /// </summary>
        public string FilterType
        {
            get => (string)GetValue(pFilterType);
            set => SetValue(pFilterType, value);
        }
        public static readonly DependencyProperty pFilterType
            = DependencyProperty.Register(
                  nameof(FilterType),
                  typeof(string),
                  typeof(ColumnFilter_Number),
                  new PropertyMetadata(string.Empty)
              );

        /// <summary>
        /// Header
        /// </summary>
        public string Header
        {
            get => (string)GetValue(pHeader);
            set => SetValue(pHeader, value);
        }
        public static readonly DependencyProperty pHeader
            = DependencyProperty.Register(
                  nameof(Header),
                  typeof(string),
                  typeof(ColumnFilter_Number),
                  new PropertyMetadata(string.Empty)
              );

        /// <summary>
        /// IsFilterVisible
        /// </summary>
        public bool IsFilterVisible
        {
            get => (bool)GetValue(pIsFilterVisible);
            set => SetValue(pIsFilterVisible, value);
        }
        public static readonly DependencyProperty pIsFilterVisible
            = DependencyProperty.Register(
                  nameof(IsFilterVisible),
                  typeof(bool),
                  typeof(ColumnFilter_Number),
                  new PropertyMetadata(true)
              );

        /// <summary>
        /// Value
        /// </summary>
        public double Value
        {
            get => (double)GetValue(pValue);
            set => SetValue(pValue, value);
        }
        public static readonly DependencyProperty pValue
            = DependencyProperty.Register(
                  nameof(Value),
                  typeof(double),
                  typeof(ColumnFilter_Number),
                  new PropertyMetadata(0d)
              );

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
