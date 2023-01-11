using System.Windows;

namespace StswExpress;
/// <summary>
/// Interaction logic for StswNavigationButton.xaml
/// </summary>
public partial class StswNavigationButton : StswNavigationButtonBase
{
    public StswNavigationButton()
    {
        InitializeComponent();
    }
    static StswNavigationButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswNavigationButton), new FrameworkPropertyMetadata(typeof(StswNavigationButton)));
    }
}
