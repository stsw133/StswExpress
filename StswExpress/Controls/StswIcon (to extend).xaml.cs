using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Interaction logic for StswIcon.xaml
/// </summary>
public partial class StswIcon : UserControl
{
    public StswIcon()
    {
        InitializeComponent();
    }

    /// HorizontalIconAlignment
    public static readonly DependencyProperty HorizontalIconAlignmentProperty
        = DependencyProperty.Register(
              nameof(HorizontalIconAlignment),
              typeof(HorizontalAlignment),
              typeof(StswIcon),
              new PropertyMetadata(default(HorizontalAlignment))
          );
    public HorizontalAlignment HorizontalIconAlignment
    {
        get => (HorizontalAlignment)GetValue(HorizontalIconAlignmentProperty);
        set => SetValue(HorizontalIconAlignmentProperty, value);
    }

    /// IconData
    public static readonly DependencyProperty IconDataProperty
        = DependencyProperty.Register(
              nameof(IconData),
              typeof(string),
              typeof(StswIcon),
              new PropertyMetadata(default(string?))
          );
    public string? IconData
    {
        get => (string?)GetValue(IconDataProperty);
        set => SetValue(IconDataProperty, value);
    }

    /// IconSource
    public static readonly DependencyProperty IconSourceProperty
        = DependencyProperty.Register(
              nameof(IconSource),
              typeof(ImageSource),
              typeof(StswIcon),
              new PropertyMetadata(default(ImageSource?))
          );
    public ImageSource? IconSource
    {
        get => (ImageSource?)GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    /// Scale
    public static readonly DependencyProperty ScaleProperty
        = DependencyProperty.Register(
              nameof(Scale),
              typeof(double),
              typeof(StswIcon),
              new PropertyMetadata(1.5)
          );
    public double Scale
    {
        get => (double)GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }

    /// SubIconData
    public static readonly DependencyProperty SubIconDataProperty
        = DependencyProperty.Register(
              nameof(SubIconData),
              typeof(string),
              typeof(StswIcon),
              new PropertyMetadata(default(string?))
          );
    public string? SubIconData
    {
        get => (string?)GetValue(SubIconDataProperty);
        set => SetValue(SubIconDataProperty, value);
    }

    /// SubIconSource
    public static readonly DependencyProperty SubIconSourceProperty
        = DependencyProperty.Register(
              nameof(SubIconSource),
              typeof(ImageSource),
              typeof(StswIcon),
              new PropertyMetadata(default(ImageSource?))
          );
    public ImageSource? SubIconSource
    {
        get => (ImageSource?)GetValue(SubIconSourceProperty);
        set => SetValue(SubIconSourceProperty, value);
    }

    /// VerticalIconAlignment
    public static readonly DependencyProperty VerticalIconAlignmentProperty
        = DependencyProperty.Register(
              nameof(VerticalIconAlignment),
              typeof(VerticalAlignment),
              typeof(StswIcon),
              new PropertyMetadata(default(VerticalAlignment))
          );
    public VerticalAlignment VerticalIconAlignment
    {
        get => (VerticalAlignment)GetValue(VerticalIconAlignmentProperty);
        set => SetValue(VerticalIconAlignmentProperty, value);
    }
}
