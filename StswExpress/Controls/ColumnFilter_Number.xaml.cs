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
        /// Text
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(pText); }
            set { SetValue(pText, value); }
        }
        public static readonly DependencyProperty pText
            = DependencyProperty.Register(
                  nameof(Text),
                  typeof(string),
                  typeof(ColumnFilter_Number),
                  new PropertyMetadata(string.Empty)
              );

        /// <summary>
        /// FilterType
        /// </summary>
        public string FilterType
        {
            get { return (string)GetValue(pFilterType); }
            set { SetValue(pFilterType, value); }
        }
        public static readonly DependencyProperty pFilterType
            = DependencyProperty.Register(
                  nameof(FilterType),
                  typeof(string),
                  typeof(ColumnFilter_Number),
                  new PropertyMetadata(string.Empty)
              );

        /// <summary>
        /// FilterVisibility
        /// </summary>
        public bool FilterVisibility
        {
            get { return (bool)GetValue(pFilterVisibility); }
            set { SetValue(pFilterVisibility, value); }
        }
        public static readonly DependencyProperty pFilterVisibility
            = DependencyProperty.Register(
                  nameof(FilterVisibility),
                  typeof(bool),
                  typeof(ColumnFilter_Number),
                  new PropertyMetadata(true)
              );

        /// <summary>
        /// Value
        /// </summary>
        public string Value
        {
            get { return (string)GetValue(pValue); }
            set { SetValue(pValue, value); }
        }
        public static readonly DependencyProperty pValue
            = DependencyProperty.Register(
                  nameof(Value),
                  typeof(string),
                  typeof(ColumnFilter_Number),
                  new PropertyMetadata(string.Empty)
              );

        /// <summary>
        /// Refresh
        /// </summary>
        private void cmdRefresh_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                dynamic W = Window.GetWindow(this);
                W.cmdRefresh_Executed(sender, e);
            } catch { }
        }
    }
}
