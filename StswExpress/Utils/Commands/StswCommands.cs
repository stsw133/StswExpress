using System.Windows.Controls;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
public static class StswCommands
{
    public static readonly RoutedUICommand ClearText = new RoutedUICommand(
            "Clear Text",
            nameof(ClearText),
            typeof(StswCommands)
        );

    static StswCommands()
    {
        CommandManager.RegisterClassCommandBinding(typeof(StswPasswordBox), new CommandBinding(ClearText, ExecuteClearText, CanExecuteClearText));
        CommandManager.RegisterClassCommandBinding(typeof(TextBox), new CommandBinding(ClearText, ExecuteClearText, CanExecuteClearText));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void ExecuteClearText(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is TextBox textBox)
            textBox.Clear();
        else if (sender is StswPasswordBox stswPasswordBox)
            stswPasswordBox.Password = default;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
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
