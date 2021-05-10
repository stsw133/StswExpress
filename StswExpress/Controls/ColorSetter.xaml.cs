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
        public static readonly DependencyProperty ColorProperty
            = DependencyProperty.Register(
                  nameof(Color),
                  typeof(string),
                  typeof(ColorSetter),
                  new PropertyMetadata("#FFFFFFFF")
              );
        public string Color
        {
            get => (string)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        /// <summary>
        /// R
        /// </summary>
        public static readonly DependencyProperty RProperty
            = DependencyProperty.Register(
                  nameof(R),
                  typeof(byte),
                  typeof(ColorSetter),
                  new PropertyMetadata((byte)255)
              );
        public byte R
        {
            get => (byte)GetValue(RProperty);
            set => SetValue(RProperty, value);
        }

        /// <summary>
        /// G
        /// </summary>
        public static readonly DependencyProperty GProperty
            = DependencyProperty.Register(
                  nameof(G),
                  typeof(byte),
                  typeof(ColorSetter),
                  new PropertyMetadata((byte)255)
              );
        public byte G
        {
            get => (byte)GetValue(GProperty);
            set => SetValue(GProperty, value);
        }

        /// <summary>
        /// B
        /// </summary>
        public static readonly DependencyProperty BProperty
            = DependencyProperty.Register(
                  nameof(B),
                  typeof(byte),
                  typeof(ColorSetter),
                  new PropertyMetadata((byte)255)
              );
        public byte B
        {
            get => (byte)GetValue(BProperty);
            set => SetValue(BProperty, value);
        }

        /// <summary>
        /// A
        /// </summary>
        public static readonly DependencyProperty AProperty
            = DependencyProperty.Register(
                  nameof(A),
                  typeof(byte),
                  typeof(ColorSetter),
                  new PropertyMetadata((byte)255)
              );
        public byte A
        {
            get => (byte)GetValue(AProperty);
            set => SetValue(AProperty, value);
        }

        /// <summary>
        /// IsAlphaSliderVisible
        /// </summary>
        public static readonly DependencyProperty IsAlphaSliderVisibleProperty
            = DependencyProperty.Register(
                  nameof(IsAlphaSliderVisible),
                  typeof(Visibility),
                  typeof(ColorSetter),
                  new PropertyMetadata(Visibility.Collapsed)
              );
        public Visibility IsAlphaSliderVisible
        {
            get => (Visibility)GetValue(IsAlphaSliderVisibleProperty);
            set => SetValue(IsAlphaSliderVisibleProperty, value);
        }

        /// <summary>
        /// SliderWidth
        /// </summary>
        public static readonly DependencyProperty SliderWidthProperty
            = DependencyProperty.Register(
                  nameof(SliderWidth),
                  typeof(int),
                  typeof(ColorSetter),
                  new PropertyMetadata(256)
              );
        public int SliderWidth
        {
            get => (int)GetValue(SliderWidthProperty);
            set => SetValue(SliderWidthProperty, value);
        }

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
        private void color_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => Color = $"#{A:X2}{R:X2}{G:X2}{B:X2}";
    }
}
