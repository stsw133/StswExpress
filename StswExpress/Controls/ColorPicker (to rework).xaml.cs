using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Interaction logic for ColorPicker.xaml
/// </summary>
public partial class ColorPicker : UserControl
{
    public ColorPicker()
    {
        InitializeComponent();
    }

    /// Color
    public static readonly DependencyProperty ColorProperty
        = DependencyProperty.Register(
              nameof(Color),
              typeof(SolidColorBrush),
              typeof(ColorPicker),
              new PropertyMetadata(default(Brushes))
          );
    public SolidColorBrush Color
    {
        get => GetValue(ColorProperty) as SolidColorBrush ?? Brushes.Transparent;
        set => SetValue(ColorProperty, value);
    }

    /// SlidersWidth
    public static readonly DependencyProperty SlidersWidthProperty
        = DependencyProperty.Register(
              nameof(SlidersWidth),
              typeof(double),
              typeof(ColorPicker),
              new PropertyMetadata(default(double))
          );
    public double SlidersWidth
    {
        get => (double)GetValue(SlidersWidthProperty);
        set => SetValue(SlidersWidthProperty, value);
    }

    /// R
    public static readonly DependencyProperty RProperty
        = DependencyProperty.Register(
              nameof(R),
              typeof(byte),
              typeof(ColorPicker),
              new PropertyMetadata((byte)255)
          );
    public byte R
    {
        get => (byte)GetValue(RProperty);
        set => SetValue(RProperty, value);
    }

    /// G
    public static readonly DependencyProperty GProperty
        = DependencyProperty.Register(
              nameof(G),
              typeof(byte),
              typeof(ColorPicker),
              new PropertyMetadata((byte)255)
          );
    public byte G
    {
        get => (byte)GetValue(GProperty);
        set => SetValue(GProperty, value);
    }

    /// B
    public static readonly DependencyProperty BProperty
        = DependencyProperty.Register(
              nameof(B),
              typeof(byte),
              typeof(ColorPicker),
              new PropertyMetadata((byte)255)
          );
    public byte B
    {
        get => (byte)GetValue(BProperty);
        set => SetValue(BProperty, value);
    }

    /// A
    public static readonly DependencyProperty AProperty
        = DependencyProperty.Register(
              nameof(A),
              typeof(byte),
              typeof(ColorPicker),
              new PropertyMetadata((byte)255)
          );
    public byte A
    {
        get => (byte)GetValue(AProperty);
        set => SetValue(AProperty, value);
    }

    /// AlphaSliderVisibility
    public static readonly DependencyProperty AlphaSliderVisibilityProperty
        = DependencyProperty.Register(
              nameof(AlphaSliderVisibility),
              typeof(Visibility),
              typeof(ColorPicker),
              new PropertyMetadata(Visibility.Collapsed)
          );
    public Visibility AlphaSliderVisibility
    {
        get => (Visibility)GetValue(AlphaSliderVisibilityProperty);
        set => SetValue(AlphaSliderVisibilityProperty, value);
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
