using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Interaction logic for ExtDataGrid.xaml
/// </summary>
public partial class ExtDataGrid : DataGrid
{
    public ExtDataGrid()
    {
        InitializeComponent();
    }

    /// <summary>
    /// HeaderBackground
    /// </summary>
    public static readonly DependencyProperty HeaderBackgroundProperty
        = DependencyProperty.Register(
              nameof(HeaderBackground),
              typeof(Brush),
              typeof(ExtDataGrid),
              new PropertyMetadata(default(SolidColorBrush))
          );
    public Brush HeaderBackground
    {
        get => (Brush)GetValue(HeaderBackgroundProperty);
        set => SetValue(HeaderBackgroundProperty, value);
    }

    /// <summary>
    /// HeaderBorderBrush
    /// </summary>
    public static readonly DependencyProperty HeaderBorderBrushProperty
        = DependencyProperty.Register(
              nameof(HeaderBorderBrush),
              typeof(SolidColorBrush),
              typeof(ExtDataGrid),
              new PropertyMetadata(default(SolidColorBrush))
          );
    public SolidColorBrush HeaderBorderBrush
    {
        get => (SolidColorBrush)GetValue(HeaderBorderBrushProperty);
        set => SetValue(HeaderBorderBrushProperty, value);
    }

    /// <summary>
    /// HeaderForeground
    /// </summary>
    public static readonly DependencyProperty HeaderForegroundProperty
        = DependencyProperty.Register(
              nameof(HeaderForeground),
              typeof(SolidColorBrush),
              typeof(ExtDataGrid),
              new PropertyMetadata(default(SolidColorBrush))
          );
    public SolidColorBrush HeaderForeground
    {
        get => (SolidColorBrush)GetValue(HeaderForegroundProperty);
        set => SetValue(HeaderForegroundProperty, value);
    }
}
