using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// A text control that extends <see cref="TextBlock"/> with additional styling options.
/// Supports inline text elements for rich text formatting.
/// </summary>
[ContentProperty(nameof(Inlines))]
public class StswText : TextBlock
{
    static StswText()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswText), new FrameworkPropertyMetadata(typeof(StswText)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswText), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }
}

/* usage:

<se:StswText>
    <Run Text="Styled " FontWeight="Bold"/>
    <Run Text="Text" Foreground="Blue"/>
</se:StswText>

*/
