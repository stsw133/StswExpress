using Avalonia.Controls;

namespace StswExpress.Avalonia;

/// <summary>
/// A text control that extends <see cref="TextBlock"/> with additional styling options.
/// Supports inline text elements for rich text formatting.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswText&gt;
///     <Run Text="Styled " FontWeight="Bold"/>
///     <Run Text="Text" Foreground="Blue"/>
/// &lt;/se:StswText&gt;
/// </code>
/// </example>
public class StswText : TextBlock
{
}
