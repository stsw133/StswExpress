using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress.Controls
{
    /// <summary>
    /// Interaction logic for ColorSetter.xaml
    /// </summary>
    public partial class ColorSetter : UserControl
    {
        public ColorSetter()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Fill
        /// </summary>
        public string Fill
        {
            get => (string)GetValue(pFill);
            set { SetValue(pFill, value); }
        }
        public static readonly DependencyProperty pFill
            = DependencyProperty.Register(
                  nameof(Fill),
                  typeof(string),
                  typeof(ColorSetter),
                  new PropertyMetadata("#FFFFFFFF")
              );

        /// <summary>
        /// R
        /// </summary>
        public byte R
        {
            get => (byte)GetValue(pR);
            set { SetValue(pR, value); }
        }
        public static readonly DependencyProperty pR
            = DependencyProperty.Register(
                  nameof(R),
                  typeof(byte),
                  typeof(ColorSetter),
                  new PropertyMetadata((byte)255)
              );

        /// <summary>
        /// G
        /// </summary>
        public byte G
        {
            get => (byte)GetValue(pG);
            set { SetValue(pG, value); }
        }
        public static readonly DependencyProperty pG
            = DependencyProperty.Register(
                  nameof(G),
                  typeof(byte),
                  typeof(ColorSetter),
                  new PropertyMetadata((byte)255)
              );

        /// <summary>
        /// B
        /// </summary>
        public byte B
        {
            get => (byte)GetValue(pB);
            set { SetValue(pB, value); }
        }
        public static readonly DependencyProperty pB
            = DependencyProperty.Register(
                  nameof(B),
                  typeof(byte),
                  typeof(ColorSetter),
                  new PropertyMetadata((byte)255)
              );

        /// <summary>
        /// A
        /// </summary>
        public byte A
        {
            get => (byte)GetValue(pA);
            set { SetValue(pA, value); }
        }
        public static readonly DependencyProperty pA
            = DependencyProperty.Register(
                  nameof(A),
                  typeof(byte),
                  typeof(ColorSetter),
                  new PropertyMetadata((byte)255)
              );

        /// <summary>
        /// ShowAlphaSlider
        /// </summary>
        public bool ShowAlphaSlider
        {
            get => (bool)GetValue(pShowAlphaSlider);
            set { SetValue(pShowAlphaSlider, value); }
        }
        public static readonly DependencyProperty pShowAlphaSlider
            = DependencyProperty.Register(
                  nameof(ShowAlphaSlider),
                  typeof(bool),
                  typeof(ColorSetter),
                  new PropertyMetadata(false)
              );

        /// <summary>
		/// Loaded
		/// </summary>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var color = (Color)ColorConverter.ConvertFromString(Fill);
            R = color.R;
            G = color.G;
            B = color.B;
            A = color.A;
        }

        /// <summary>
        /// ValueChanged
        /// </summary>
        private void color_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Fill = $"#{A:X2}{R:X2}{G:X2}{B:X2}";
        }
    }
}
