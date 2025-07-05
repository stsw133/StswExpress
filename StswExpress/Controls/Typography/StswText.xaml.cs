using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// A text control that extends <see cref="TextBlock"/> with additional styling options.
/// Supports inline text elements for rich text formatting.
/// </summary>
[ContentProperty(nameof(Inlines))]
[Stsw("0.1.0", Changes = StswPlannedChanges.None)]
public class StswText : TextBlock
{
    static StswText()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswText), new FrameworkPropertyMetadata(typeof(StswText)));
    }
}

/* usage:

<se:StswText>
    <Run Text="Styled " FontWeight="Bold"/>
    <Run Text="Text" Foreground="Blue"/>
</se:StswText>

*/
