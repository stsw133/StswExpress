using System.Collections.Generic;
using System.Linq;
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
    /// A command to clear the text in a control.
    /// </summary>
    public static readonly RoutedUICommand SelectAll = new("Select All", nameof(SelectAll), typeof(StswCommands));

    /// <summary>
    /// Static constructor to register command bindings for controls.
    /// </summary>
    static StswCommands()
    {
        CommandManager.RegisterClassCommandBinding(typeof(StswPasswordBox), new CommandBinding(ClearText, ClearText_Execute, ClearText_CanExecute));
        CommandManager.RegisterClassCommandBinding(typeof(TextBox), new CommandBinding(ClearText, ClearText_Execute, ClearText_CanExecute));

        CommandManager.RegisterClassCommandBinding(typeof(CheckBox), new CommandBinding(SelectAll, SelectAll_Execute, SelectAll_CanExecute));
    }

    /// <summary>
    /// Executes the <see cref="ClearText"/> command by clearing the text in the target control.
    /// </summary>
    /// <param name="sender">The control that invoked the command.</param>
    /// <param name="e">The event data.</param>
    private static void ClearText_Execute(object sender, ExecutedRoutedEventArgs e)
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
    private static void ClearText_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        if (sender is TextBox textBox)
            e.CanExecute = !string.IsNullOrEmpty(textBox.Text);
        else if (sender is StswPasswordBox stswPasswordBox)
            e.CanExecute = !string.IsNullOrEmpty(stswPasswordBox.Password);
        else
            e.CanExecute = false;
    }

    /// <summary>
    /// Executes the <see cref="SelectAll"/> command by selecting all checks in the target control.
    /// </summary>
    /// <param name="sender">The control that invoked the command.</param>
    /// <param name="e">The event data.</param>
    private static void SelectAll_Execute(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is CheckBox checkBox)
        {
            var itemsControl = StswFn.FindVisualAncestor<IStswSelectionControl>(checkBox);
            if (itemsControl?.ItemsSource?.Cast<IStswSelectionItem>() is IEnumerable<IStswSelectionItem> items)
                foreach (var item in items.Where(x => x.IsSelected))
                    item.GetType().GetProperty((string)e.Parameter)?.SetValue(item, checkBox.IsChecked == true);
        }
    }

    /// <summary>
    /// Determines whether the <see cref="SelectAll"/> command can execute on the target control.
    /// </summary>
    /// <param name="sender">The control that invoked the command.</param>
    /// <param name="e">The event data.</param>
    private static void SelectAll_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        if (sender is CheckBox)
            e.CanExecute = true;
    }
}
