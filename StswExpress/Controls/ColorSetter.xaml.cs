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
            get => $"#FF{R:X2}{G:X2}{B:X2}";
            set
            {
                var color = (Color)ColorConverter.ConvertFromString(value);
                R = color.R;
                G = color.G;
                B = color.B;
            }
        }
        public static readonly DependencyProperty pFill
            = DependencyProperty.Register(
                  nameof(Fill),
                  typeof(string),
                  typeof(ColorSetter),
                  new PropertyMetadata("#FFFFFFFF")
              );
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
                  new PropertyMetadata(255)
              );
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
                  new PropertyMetadata(255)
              );
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
                  new PropertyMetadata(255)
              );
    }
}
