using System.Windows;
using System.Windows.Controls.Primitives;

namespace StswExpress
{
    /// <summary>
    /// Interaction logic for ExtToggleButton.xaml
    /// </summary>
    public partial class ExtToggleButton : ToggleButton
    {
        public ExtToggleButton()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ForMultiBox
        /// </summary>
        public static readonly DependencyProperty ForMultiBoxProperty
            = DependencyProperty.Register(
                  nameof(ForMultiBox),
                  typeof(bool),
                  typeof(ExtToggleButton),
                  new PropertyMetadata(default(bool))
              );
        public bool ForMultiBox
        {
            get => (bool)GetValue(ForMultiBoxProperty);
            set => SetValue(ForMultiBoxProperty, value);
        }
    }
}
