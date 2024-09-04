using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// Provides custom commands for controls.
/// </summary>
public static class StswCommands
{
    /// <summary>
    /// A command to clear the text or selection in a control.
    /// </summary>
    public static readonly RoutedUICommand Clear = new(nameof(Clear), nameof(Clear), typeof(StswCommands));

    /// <summary>
    /// A command to clear the text in a control.
    /// </summary>
    public static readonly RoutedUICommand SelectAll = new(nameof(SelectAll), nameof(SelectAll), typeof(StswCommands));

    /// <summary>
    /// Static constructor to register command bindings for controls.
    /// </summary>
    static StswCommands()
    {
        CommandManager.RegisterClassCommandBinding(typeof(Selector), new CommandBinding(Clear, Clear_Execute, Clear_CanExecute));
        CommandManager.RegisterClassCommandBinding(typeof(StswPasswordBox), new CommandBinding(Clear, Clear_Execute, Clear_CanExecute));
        CommandManager.RegisterClassCommandBinding(typeof(StswSelectionBox), new CommandBinding(Clear, Clear_Execute, Clear_CanExecute));
        CommandManager.RegisterClassCommandBinding(typeof(TextBox), new CommandBinding(Clear, Clear_Execute, Clear_CanExecute));

        CommandManager.RegisterClassCommandBinding(typeof(ToggleButton), new CommandBinding(SelectAll, SelectAll_Execute, SelectAll_CanExecute));
    }

    /// <summary>
    /// Executes the <see cref="Clear"/> command by clearing the text or selection in the target control.
    /// </summary>
    /// <param name="sender">The control that invoked the command.</param>
    /// <param name="e">The event data.</param>
    private static void Clear_Execute(object sender, ExecutedRoutedEventArgs e)
    {
        switch (sender)
        {
            case Selector selector:
                selector.SelectedIndex = -1;
                break;
            case StswPasswordBox stswPasswordBox:
                stswPasswordBox.Password = default;
                break;
            case StswSelectionBox stswSelectionBox:
                stswSelectionBox.SelectedItems.Cast<IStswSelectionItem>().ToList().ForEach(x => x.IsSelected = false);
                stswSelectionBox.SetTextCommand?.Execute(null);
                break;
            case TextBox textBox:
                textBox.Clear();
                break;
        }
    }

    /// <summary>
    /// Determines whether the <see cref="Clear"/> command can execute on the target control.
    /// </summary>
    /// <param name="sender">The control that invoked the command.</param>
    /// <param name="e">The event data.</param>
    private static void Clear_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = sender switch
        {
            Selector selector => selector.SelectedIndex >= 0,
            StswPasswordBox stswPasswordBox => !string.IsNullOrEmpty(stswPasswordBox.Password),
            StswSelectionBox stswSelectionBox => stswSelectionBox.SelectedItems.Count > 0,
            TextBox textBox => !string.IsNullOrEmpty(textBox.Text),
            _ => false,
        };
    }

    /// <summary>
    /// Executes the <see cref="SelectAll"/> command by selecting all checks in the target control.
    /// </summary>
    /// <param name="sender">The control that invoked the command.</param>
    /// <param name="e">The event data.</param>
    private static void SelectAll_Execute(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is ToggleButton toggleButton)
        {
            var itemsControl = StswFn.FindVisualAncestor<IStswSelectionControl>(toggleButton);
            if (itemsControl?.ItemsSource?.Cast<IStswSelectionItem>() is IEnumerable<IStswSelectionItem> items)
                foreach (var item in items.Where(x => x.IsSelected))
                    item.GetType().GetProperty((string)e.Parameter)?.SetValue(item, toggleButton.IsChecked == true);
        }
    }

    /// <summary>
    /// Determines whether the <see cref="SelectAll"/> command can execute on the target control.
    /// </summary>
    /// <param name="sender">The control that invoked the command.</param>
    /// <param name="e">The event data.</param>
    private static void SelectAll_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = sender is ToggleButton;
    }
}
