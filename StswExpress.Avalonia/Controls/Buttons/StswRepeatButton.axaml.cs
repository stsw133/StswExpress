using Avalonia.Controls;

namespace StswExpress.Avalonia;
/// <summary>
/// Represents a button control that continuously triggers an action while it is pressed and held.
/// This control extends <see cref="RepeatButton"/>, providing additional styling options such as corner rounding.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswRepeatButton Command="{Binding MyCommand}" Content="Hold Me"/&gt;
/// </code>
/// </example>
public class StswRepeatButton : RepeatButton
{
}
