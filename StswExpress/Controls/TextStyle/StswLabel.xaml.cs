using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Represents a control functioning as label.
/// </summary>
public class StswLabel : Label
{
    static StswLabel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswLabel), new FrameworkPropertyMetadata(typeof(StswLabel)));
    }
}
