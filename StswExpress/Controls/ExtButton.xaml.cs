using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Interaction logic for ExtButton.xaml
/// </summary>
public partial class ExtButton : Button
{
    public ExtButton()
    {
        InitializeComponent();
    }

    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
              nameof(CornerRadius),
              typeof(double?),
              typeof(ExtButton),
              new PropertyMetadata(default(double?))
          );
    public double? CornerRadius
    {
        get => (double?)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
}
