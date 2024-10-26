using System.Windows;

namespace StswExpress;
/// <summary>
/// Represents a control that functions as a sub control and displays an icon.
/// </summary>
public class StswSubLabel : StswLabel, IStswSubControl, IStswCornerControl, IStswIconControl
{
    static StswSubLabel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSubLabel), new FrameworkPropertyMetadata(typeof(StswSubLabel)));
    }
}
