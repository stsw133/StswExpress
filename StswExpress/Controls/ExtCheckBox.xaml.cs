using System.Windows;
using System.Windows.Controls;

namespace StswExpress
{
    /// <summary>
    /// Interaction logic for ExtCheckBox.xaml
    /// </summary>
    public partial class ExtCheckBox : CheckBox
    {
        public ExtCheckBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// IsError
        /// </summary>
        public static readonly DependencyProperty IsErrorProperty
            = DependencyProperty.Register(
                  nameof(IsError),
                  typeof(bool),
                  typeof(ExtCheckBox),
                  new PropertyMetadata(default(bool))
              );
        public bool IsError
        {
            get => (bool)GetValue(IsErrorProperty);
            set => SetValue(IsErrorProperty, value);
        }
    }
}
