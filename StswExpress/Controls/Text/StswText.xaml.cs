using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Represents a control that is displaying a text.
/// </summary>
public class StswText : TextBlock
{
    static StswText()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswText), new FrameworkPropertyMetadata(typeof(StswText)));
    }
}
