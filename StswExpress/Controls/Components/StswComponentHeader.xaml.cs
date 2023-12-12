using System.Windows;

namespace StswExpress;

/// <summary>
/// Represents a control that functions as a component and displays an icon.
/// </summary>
public class StswComponentHeader : StswHeader, IStswComponentControl, IStswIconControl
{
    static StswComponentHeader()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswComponentHeader), new FrameworkPropertyMetadata(typeof(StswComponentHeader)));
    }
}
