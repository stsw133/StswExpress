using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StswExpress.Controls
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
        public string FilterType
        {
            get => (string)GetValue(pFilterType);
            set => SetValue(pFilterType, value);
        }
        public static readonly DependencyProperty pFilterType
            = DependencyProperty.Register(
                  nameof(FilterType),
                  typeof(string),
                  typeof(ColumnFilter_Date),
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
                  typeof(ColumnFilter_Date),
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
                  typeof(ColumnFilter_Date),
                  new PropertyMetadata(true)
              );

        /// <summary>
        /// Value1
        /// </summary>
        public string Value1
        {
            get => (string)GetValue(pValue1);
            set => SetValue(pValue1, value);
        }
        public static readonly DependencyProperty pValue1
            = DependencyProperty.Register(
                  nameof(Value1),
                  typeof(string),
                  typeof(ColumnFilter_Date),
                  new PropertyMetadata(string.Empty)
              );

        /// <summary>
        /// Value2
        /// </summary>
        public string Value2
        {
            get => (string)GetValue(pValue2);
            set => SetValue(pValue2, value);
        }
        public static readonly DependencyProperty pValue2
            = DependencyProperty.Register(
                  nameof(Value2),
                  typeof(string),
                  typeof(ColumnFilter_Date),
                  new PropertyMetadata(string.Empty)
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
