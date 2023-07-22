using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Represents a control that extends the <see cref="ScrollViewer"/> class with additional functionality.
/// </summary>
public class RgsScrollViewer : ScrollViewer
{
    static RgsScrollViewer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RgsScrollViewer), new FrameworkPropertyMetadata(typeof(RgsScrollViewer)));
    }
}
