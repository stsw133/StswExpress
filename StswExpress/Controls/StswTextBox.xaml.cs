using System.Windows;

namespace StswExpress;

/// <summary>
/// Interaction logic for StswTextBox.xaml
/// </summary>
public partial class StswTextBox : StswTextBoxBase
{
    public StswTextBox()
    {
        InitializeComponent();
    }
    static StswTextBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswTextBox), new FrameworkPropertyMetadata(typeof(StswTextBox)));
    }
}
