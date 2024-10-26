using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// Represents a control that is displaying a text.
/// </summary>
[ContentProperty(nameof(Inlines))]
public class StswText : TextBlock
{
    static StswText()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswText), new FrameworkPropertyMetadata(typeof(StswText)));
    }
}
