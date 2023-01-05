using System.Windows;
using System.Windows.Controls.Primitives;

namespace StswExpress;

/// <summary>
/// Interaction logic for MultiBoxButton.xaml
/// </summary>
public partial class MultiBoxButton : ToggleButton
{
    public MultiBoxButton()
    {
        InitializeComponent();
    }

    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius?),
            typeof(MultiBoxButton),
            new PropertyMetadata(default(CornerRadius?))
        );
    public CornerRadius? CornerRadius
    {
        get => (CornerRadius?)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// ForMultiBox
    internal static readonly DependencyProperty ForMultiBoxProperty
        = DependencyProperty.Register(
              nameof(ForMultiBox),
              typeof(bool),
              typeof(MultiBoxButton),
              new PropertyMetadata(default(bool))
          );
    internal bool ForMultiBox
    {
        get => (bool)GetValue(ForMultiBoxProperty);
        set => SetValue(ForMultiBoxProperty, value);
    }
}
