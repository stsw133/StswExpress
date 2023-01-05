using System.Windows;

namespace StswExpress;

/// <summary>
/// Interaction logic for StswButton.xaml
/// </summary>
public partial class StswButton : StswButtonBase
{
    public StswButton()
    {
        InitializeComponent();
    }
    static StswButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswButton), new FrameworkPropertyMetadata(typeof(StswButton)));
    }
}
