using System.Globalization;
using System.Threading;
using System.Windows;

namespace StswExpress;

/// <summary>
/// Interaction logic for StswDatePicker.xaml
/// </summary>
public partial class StswDatePicker : StswDatePickerBase
{
    public StswDatePicker()
    {
        InitializeComponent();
    }
    static StswDatePicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswDatePicker), new FrameworkPropertyMetadata(typeof(StswDatePicker)));
    }
}
