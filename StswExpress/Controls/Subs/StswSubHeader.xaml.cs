using System.Windows;

namespace StswExpress;

/// <summary>
/// Represents a control that functions as a sub control and displays an icon.
/// </summary>
public class StswSubHeader : StswHeader, IStswSubControl, IStswIconControl
{
    static StswSubHeader()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSubHeader), new FrameworkPropertyMetadata(typeof(StswSubHeader)));
    }
}
