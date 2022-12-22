using System.Windows;
using System.Windows.Controls.Primitives;

namespace StswExpress;

/// <summary>
/// Interaction logic for StswToggleButton.xaml
/// </summary>
public partial class StswToggleButton : ToggleButton
{
    public StswToggleButton()
    {
        InitializeComponent();
    }

    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius?),
            typeof(StswToggleButton),
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
              typeof(StswToggleButton),
              new PropertyMetadata(default(bool))
          );
    internal bool ForMultiBox
    {
        get => (bool)GetValue(ForMultiBoxProperty);
        set => SetValue(ForMultiBoxProperty, value);
    }
}
