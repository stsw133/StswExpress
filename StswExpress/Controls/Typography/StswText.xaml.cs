using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// A text control that extends <see cref="TextBlock"/> with additional styling options.
/// Supports inline text elements for rich text formatting.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswText&gt;
///     &lt;Run Text="Styled " FontWeight="Bold"/&gt;
///     &lt;Run Text="Text" Foreground="Blue"/&gt;
/// &lt;/se:StswText&gt;
/// </code>
/// </example>
[ContentProperty(nameof(Inlines))]
public class StswText : TextBlock
{
    static StswText()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswText), new FrameworkPropertyMetadata(typeof(StswText)));
    }
}
