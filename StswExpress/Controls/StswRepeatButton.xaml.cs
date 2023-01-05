using System.Windows;

namespace StswExpress;

/// <summary>
/// Interaction logic for StswRepeatButton.xaml
/// </summary>
public partial class StswRepeatButton : StswRepeatButtonBase
{
    public StswRepeatButton()
    {
        InitializeComponent();
    }
    static StswRepeatButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswRepeatButton), new FrameworkPropertyMetadata(typeof(StswRepeatButton)));
    }
}
