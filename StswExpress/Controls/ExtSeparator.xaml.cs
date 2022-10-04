using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Interaction logic for ExtSeparator.xaml
/// </summary>
public partial class ExtSeparator : Separator
{
    public ExtSeparator()
    {
        InitializeComponent();
    }

    /// Orientation
    public static readonly DependencyProperty OrientationProperty
        = DependencyProperty.Register(
              nameof(Orientation),
              typeof(Orientation),
              typeof(ExtSeparator),
              new PropertyMetadata(default(Orientation))
          );
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
}
