using System.Windows;
using System.Windows.Controls;

namespace StswExpress;
/// <summary>
/// Represents a control that can be used to visually divide content in a user interface.
/// </summary>
public class StswGridSplitter : GridSplitter
{
    static StswGridSplitter()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswGridSplitter), new FrameworkPropertyMetadata(typeof(StswGridSplitter)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswGridSplitter), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }
}
