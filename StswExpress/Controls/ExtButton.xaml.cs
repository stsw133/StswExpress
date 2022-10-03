using System.Windows;
using System.Windows.Controls;

namespace StswExpress
{
    /// <summary>
    /// Interaction logic for ExtButton.xaml
    /// </summary>
    public partial class ExtButton : Button
    {
        public ExtButton()
        {
            InitializeComponent();
        }

        /// CornerRadius
        public static readonly DependencyProperty CornerRadiusProperty
            = DependencyProperty.Register(
                  nameof(CornerRadius),
                  typeof(double?),
                  typeof(ExtButton),
                  new PropertyMetadata(default(double?))
              );
        public double? CornerRadius
        {
            get => (double?)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        /// Property1
        public static readonly DependencyProperty Property1Property
            = DependencyProperty.Register(
                  nameof(Property1),
                  typeof(int),
                  typeof(ExtButton),
                  new PropertyMetadata(default(int))
              );
        public int Property1
        {
            get => (int)GetValue(Property1Property);
            set => SetValue(Property1Property, value);
        }
    }
}
