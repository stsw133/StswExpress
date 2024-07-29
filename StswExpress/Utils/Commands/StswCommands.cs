using System.Windows.Controls;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// Provides custom commands for controls.
/// </summary>
public static class StswCommands
{
    /// <summary>
    /// A command to clear the text in a control.
    /// </summary>
    public static readonly RoutedUICommand ClearText = new("Clear Text", nameof(ClearText), typeof(StswCommands));

    /// <summary>
    /// Static constructor to register command bindings for controls.
    /// </summary>
    static StswCommands()
    {
        CommandManager.RegisterClassCommandBinding(typeof(StswPasswordBox), new CommandBinding(ClearText, ExecuteClearText, CanExecuteClearText));
        CommandManager.RegisterClassCommandBinding(typeof(TextBox), new CommandBinding(ClearText, ExecuteClearText, CanExecuteClearText));
    }

    /// <summary>
    /// Executes the <see cref="ClearText"/> command by clearing the text in the target control.
    /// </summary>
    /// <param name="sender">The control that invoked the command.</param>
    /// <param name="e">The event data.</param>
    private static void ExecuteClearText(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is TextBox textBox)
            textBox.Clear();
        else if (sender is StswPasswordBox stswPasswordBox)
            stswPasswordBox.Password = default;
    }

    /// <summary>
    /// Determines whether the <see cref="ClearText"/> command can execute on the target control.
    /// </summary>
    /// <param name="sender">The control that invoked the command.</param>
    /// <param name="e">The event data.</param>
    private static void CanExecuteClearText(object sender, CanExecuteRoutedEventArgs e)
    {
        if (sender is TextBox textBox)
            e.CanExecute = !string.IsNullOrEmpty(textBox.Text);
        else if (sender is StswPasswordBox stswPasswordBox)
            e.CanExecute = !string.IsNullOrEmpty(stswPasswordBox.Password);
        else
            e.CanExecute = false;
    }
}
