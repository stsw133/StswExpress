using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress
{
    /// <summary>
    /// Interaction logic for NumericDescription.xaml
    /// </summary>
    public partial class NumericDescription : StackPanel
    {
        public NumericDescription()
        {
            InitializeComponent();
        }

        /// Description
        public static readonly DependencyProperty DescriptionProperty
            = DependencyProperty.Register(
                  nameof(Description),
                  typeof(string),
                  typeof(NumericDescription),
                  new PropertyMetadata(default(string))
              );
        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        /// Number
        public static readonly DependencyProperty NumberProperty
            = DependencyProperty.Register(
                  nameof(Number),
                  typeof(int),
                  typeof(NumericDescription),
                  new PropertyMetadata(default(int))
              );
        public string Number
        {
            get => (string)GetValue(NumberProperty);
            set => SetValue(NumberProperty, value);
        }

        /// NumberForeground
        public static readonly DependencyProperty NumberForegroundProperty
            = DependencyProperty.Register(
                  nameof(NumberForeground),
                  typeof(SolidColorBrush),
                  typeof(NumericDescription),
                  new PropertyMetadata(default(SolidColorBrush))
              );
        public SolidColorBrush NumberForeground
        {
            get => (SolidColorBrush)GetValue(NumberForegroundProperty);
            set => SetValue(NumberForegroundProperty, value);
        }
    }
}
