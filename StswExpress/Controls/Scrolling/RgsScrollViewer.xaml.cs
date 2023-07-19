using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
public class RgsScrollViewer : ScrollViewer
{
    static RgsScrollViewer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RgsScrollViewer), new FrameworkPropertyMetadata(typeof(RgsScrollViewer)));
    }
}
