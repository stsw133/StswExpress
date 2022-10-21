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

    /// ID
    public static readonly DependencyProperty IDProperty
        = DependencyProperty.Register(
              nameof(ID),
              typeof(int),
              typeof(ExtButton),
              new PropertyMetadata(default(int))
          );
    public int ID
    {
        get => (int)GetValue(IDProperty);
        set => SetValue(IDProperty, value);
    }
}
