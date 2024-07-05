using System.Windows;

namespace TestApp;
/// <summary>
/// Represents a control that allows users to provide value either by entering numeric value or using a "Up" and "Down" buttons.
/// </summary>
public class StswIntegerBox : StswNumberBoxBase<int>
{
    static StswIntegerBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswIntegerBox), new FrameworkPropertyMetadata(typeof(StswIntegerBox)));
    }
}
