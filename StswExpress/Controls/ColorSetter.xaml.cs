using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress
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
                  typeof(SolidColorBrush),
                  typeof(ColorSetter),
                  new PropertyMetadata(Brushes.White)
              );
        public SolidColorBrush Color
        {
            get => (SolidColorBrush)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        /// <summary>
        /// SliderWidth
        /// </summary>
        public static readonly DependencyProperty SliderWidthProperty
            = DependencyProperty.Register(
                  nameof(SliderWidth),
                  typeof(double),
                  typeof(ColorSetter),
                  new PropertyMetadata(256)
              );
        public double SliderWidth
        {
            get => (double)GetValue(SliderWidthProperty);
            set => SetValue(SliderWidthProperty, value);
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

        /// UserControl - LayoutUpdated
        private void UserControl_LayoutUpdated(object sender, EventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                var color = Color.Color;
                R = color.R;
                G = color.G;
                B = color.B;
                A = color.A;
                LayoutUpdated -= UserControl_LayoutUpdated;
            }
        }

        /// Color - ValueChanged
        private void SldColor_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => Color = (SolidColorBrush)new BrushConverter().ConvertFromString($"#{A:X2}{R:X2}{G:X2}{B:X2}");
    }
}
