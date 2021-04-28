using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StswExpress.Controls
{
    /// <summary>
    /// Interaction logic for ColumnFilter_Check.xaml
    /// </summary>
    public partial class ColumnFilter_Check : StackPanel
    {
        public ColumnFilter_Check()
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
                  typeof(ColumnFilter_Check),
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
                  typeof(ColumnFilter_Check),
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
                  typeof(ColumnFilter_Check),
                  new PropertyMetadata(true)
              );

        /// <summary>
        /// Text
        /// </summary>
        public string Text
        {
            get => (string)GetValue(pText);
            set => SetValue(pText, value);
        }
        public static readonly DependencyProperty pText
            = DependencyProperty.Register(
                  nameof(Text),
                  typeof(string),
                  typeof(ColumnFilter_Check),
                  new PropertyMetadata(string.Empty)
              );

        /// <summary>
        /// Value
        /// </summary>
        public bool? Value
        {
            get => (bool?)GetValue(pValue);
            set => SetValue(pValue, value);
        }
        public static readonly DependencyProperty pValue
            = DependencyProperty.Register(
                  nameof(Value),
                  typeof(bool?),
                  typeof(ColumnFilter_Check),
                  new PropertyMetadata(null)
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
