using System;
using System.ComponentModel;
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
        /// Color
        /// </summary>
        public string Color
        {
            get => (string)GetValue(pColor);
            set => SetValue(pColor, value);
        }
        public static readonly DependencyProperty pColor
            = DependencyProperty.Register(
                  nameof(Color),
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
            set => SetValue(pR, value);
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
            set => SetValue(pG, value);
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
            set => SetValue(pB, value);
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
            set => SetValue(pA, value);
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
            set => SetValue(pShowAlphaSlider, value);
        }
        public static readonly DependencyProperty pShowAlphaSlider
            = DependencyProperty.Register(
                  nameof(ShowAlphaSlider),
                  typeof(bool),
                  typeof(ColorSetter),
                  new PropertyMetadata(false)
              );

        /// <summary>
        /// SliderWidth
        /// </summary>
        public int SliderWidth
        {
            get => (int)GetValue(pSliderWidth);
            set => SetValue(pSliderWidth, value);
        }
        public static readonly DependencyProperty pSliderWidth
            = DependencyProperty.Register(
                  nameof(SliderWidth),
                  typeof(int),
                  typeof(ColorSetter),
                  new PropertyMetadata(0)
              );

        /// <summary>
		/// LayoutUpdated
		/// </summary>
        private void UserControl_LayoutUpdated(object sender, EventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                var color = (Color)ColorConverter.ConvertFromString(Color);
                R = color.R;
                G = color.G;
                B = color.B;
                A = color.A;
                LayoutUpdated -= UserControl_LayoutUpdated;
            }
        }

        /// <summary>
        /// ValueChanged
        /// </summary>
        private void color_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Color = $"#{A:X2}{R:X2}{G:X2}{B:X2}";
        }
    }
}
