using System.Windows;

namespace StswExpress;

/// <summary>
/// Interaction logic for StswCheckBox.xaml
/// </summary>
public partial class StswCheckBox : StswCheckBoxBase
{
    public StswCheckBox()
    {
        InitializeComponent();
    }
    static StswCheckBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswCheckBox), new FrameworkPropertyMetadata(typeof(StswCheckBox)));
    }
}
