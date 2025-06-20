using System.Windows;

namespace StswExpress;
/// <summary>
/// A sub-label control that displays an icon alongside text.
/// Supports corner radius customization and icon styling.
/// </summary>
/// <remarks>
/// This control is designed for use in compact UI elements where an icon and text need to be displayed together,
/// such as tooltips, labels, or inline notifications.
/// </remarks>
public class StswSubLabel : StswLabel, IStswSubControl, IStswCornerControl, IStswIconControl
{
    static StswSubLabel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSubLabel), new FrameworkPropertyMetadata(typeof(StswSubLabel)));
    }
}

/* usage:

<se:StswSubLabel Content="Info" IconData="{StaticResource InfoIcon}"/>

*/
