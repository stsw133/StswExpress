using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Interaction logic for Icon.xaml
/// </summary>
public partial class Icon : UserControl
{
    public Icon()
    {
        InitializeComponent();
    }

    /// Color
    public static readonly DependencyProperty ColorProperty
        = DependencyProperty.Register(
              nameof(Color),
              typeof(Brush),
              typeof(Icon),
              new PropertyMetadata(default(Brushes))
          );
    public Brush Color
    {
        get => (Brush)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// Data
    public static readonly DependencyProperty DataProperty
        = DependencyProperty.Register(
              nameof(Data),
              typeof(Geometry),
              typeof(Icon),
              new PropertyMetadata(default(Geometry?))
          );
    public Geometry? Data
    {
        get => (Geometry?)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    /// HorizontalIconAlignment
    public static readonly DependencyProperty HorizontalIconAlignmentProperty
        = DependencyProperty.Register(
              nameof(HorizontalIconAlignment),
              typeof(HorizontalAlignment),
              typeof(Icon),
              new PropertyMetadata(default(HorizontalAlignment))
          );
    public HorizontalAlignment HorizontalIconAlignment
    {
        get => (HorizontalAlignment)GetValue(HorizontalIconAlignmentProperty);
        set => SetValue(HorizontalIconAlignmentProperty, value);
    }

    /// Source
    public static readonly DependencyProperty SourceProperty
        = DependencyProperty.Register(
              nameof(Source),
              typeof(ImageSource),
              typeof(Icon),
              new PropertyMetadata(default(ImageSource?))
          );
    public ImageSource? Source
    {
        get => (ImageSource?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// Scale
    public static readonly DependencyProperty ScaleProperty
        = DependencyProperty.Register(
              nameof(Scale),
              typeof(double),
              typeof(Icon),
              new PropertyMetadata(1.5)
          );
    public double Scale
    {
        get => (double)GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }

    /// SubColor
    public static readonly DependencyProperty SubColorProperty
        = DependencyProperty.Register(
              nameof(SubColor),
              typeof(Brush),
              typeof(Icon),
              new PropertyMetadata(default(Brushes))
          );
    public Brush SubColor
    {
        get => (Brush)GetValue(SubColorProperty);
        set => SetValue(SubColorProperty, value);
    }

    /// SubData
    public static readonly DependencyProperty SubDataProperty
        = DependencyProperty.Register(
              nameof(SubData),
              typeof(Geometry),
              typeof(Icon),
              new PropertyMetadata(default(Geometry?))
          );
    public Geometry? SubData
    {
        get => (Geometry?)GetValue(SubDataProperty);
        set => SetValue(SubDataProperty, value);
    }

    /// SubSource
    public static readonly DependencyProperty SubSourceProperty
        = DependencyProperty.Register(
              nameof(SubSource),
              typeof(ImageSource),
              typeof(Icon),
              new PropertyMetadata(default(ImageSource?))
          );
    public ImageSource? SubSource
    {
        get => (ImageSource?)GetValue(SubSourceProperty);
        set => SetValue(SubSourceProperty, value);
    }

    /// VerticalIconAlignment
    public static readonly DependencyProperty VerticalIconAlignmentProperty
        = DependencyProperty.Register(
              nameof(VerticalIconAlignment),
              typeof(VerticalAlignment),
              typeof(Icon),
              new PropertyMetadata(default(VerticalAlignment))
          );
    public VerticalAlignment VerticalIconAlignment
    {
        get => (VerticalAlignment)GetValue(VerticalIconAlignmentProperty);
        set => SetValue(VerticalIconAlignmentProperty, value);
    }
}
