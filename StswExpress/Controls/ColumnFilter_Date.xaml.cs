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
        /// Text
        /// </summary>
        public string Text
        {
            get => (string)GetValue(pText);
            set { SetValue(pText, value); }
        }
        public static readonly DependencyProperty pText
            = DependencyProperty.Register(
                  nameof(Text),
                  typeof(string),
                  typeof(ColumnFilter_Date),
                  new PropertyMetadata(string.Empty)
              );

        /// <summary>
        /// FilterType
        /// </summary>
        public string FilterType
        {
            get => (string)GetValue(pFilterType);
            set { SetValue(pFilterType, value); }
        }
        public static readonly DependencyProperty pFilterType
            = DependencyProperty.Register(
                  nameof(FilterType),
                  typeof(string),
                  typeof(ColumnFilter_Date),
                  new PropertyMetadata(string.Empty)
              );

        /// <summary>
        /// FilterVisibility
        /// </summary>
        public bool FilterVisibility
        {
            get => (bool)GetValue(pFilterVisibility);
            set { SetValue(pFilterVisibility, value); }
        }
        public static readonly DependencyProperty pFilterVisibility
            = DependencyProperty.Register(
                  nameof(FilterVisibility),
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
            set { SetValue(pValue1, value); }
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
            set { SetValue(pValue2, value); }
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
