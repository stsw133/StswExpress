using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// Provides custom commands for common UI interactions.
/// </summary>
public static class StswCommands
{
    /// <summary>
    /// A command to select all available items in a selection-based control.
    /// </summary>
    public static readonly RoutedUICommand CheckAllSelected = new(nameof(CheckAllSelected), nameof(CheckAllSelected), typeof(StswCommands));

    /// <summary>
    /// A command to clear the text, password, or selection in a supported control.
    /// </summary>
    public static readonly RoutedUICommand Clear = new(nameof(Clear), nameof(Clear), typeof(StswCommands));

    /// <summary>
    /// A command to close a dialog by triggering the closing mechanism.
    /// </summary>
    public static readonly RoutedUICommand CloseDialog = new(nameof(CloseDialog), nameof(CloseDialog), typeof(StswCommands));

    /// <summary>
    /// A command to deselect all selected items in a selection-based control.
    /// </summary>
    public static readonly RoutedUICommand DeselectAll = new(nameof(DeselectAll), nameof(DeselectAll), typeof(StswCommands));

    /// <summary>
    /// A command to select all available items in a selection-based control.
    /// </summary>
    public static readonly RoutedUICommand SelectAll = new(nameof(SelectAll), nameof(SelectAll), typeof(StswCommands));

    /// <summary>
    /// Static constructor to register command bindings for relevant UI controls.
    /// </summary>
    static StswCommands()
    {
        CommandManager.RegisterClassCommandBinding(typeof(Selector), new CommandBinding(Clear, Clear_Execute, Clear_CanExecute));
        CommandManager.RegisterClassCommandBinding(typeof(StswPasswordBox), new CommandBinding(Clear, Clear_Execute, Clear_CanExecute));
        CommandManager.RegisterClassCommandBinding(typeof(StswSelectionBox), new CommandBinding(Clear, Clear_Execute, Clear_CanExecute));
        CommandManager.RegisterClassCommandBinding(typeof(TextBox), new CommandBinding(Clear, Clear_Execute, Clear_CanExecute));

        CommandManager.RegisterClassCommandBinding(typeof(ButtonBase), new CommandBinding(CloseDialog, CloseDialog_Execute, CloseDialog_CanExecute));

        CommandManager.RegisterClassCommandBinding(typeof(ButtonBase), new CommandBinding(SelectAll, SelectAll_Execute, SelectAll_CanExecute));
        CommandManager.RegisterClassCommandBinding(typeof(ButtonBase), new CommandBinding(DeselectAll, DeselectAll_Execute, DeselectAll_CanExecute));

        CommandManager.RegisterClassCommandBinding(typeof(ToggleButton), new CommandBinding(CheckAllSelected, CheckAllSelected_Execute, CheckAllSelected_CanExecute));
    }

    /// <summary>
    /// Executes the <see cref="CheckAllSelected"/> command by selecting all available items in a selection-based control.
    /// </summary>
    /// <param name="sender">The control that invoked the command.</param>
    /// <param name="e">The event data, where the parameter is the name of the property to set.</param>
    private static void CheckAllSelected_Execute(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is ToggleButton toggleButton)
        {
            var itemsControl = StswAppFn.FindVisualAncestor<IStswSelectionControl>(toggleButton);
            if (itemsControl?.ItemsSource?.Cast<IStswSelectionItem>() is IEnumerable<IStswSelectionItem> items)
                foreach (var item in items.Where(x => x.IsSelected))
                    item.GetType().GetProperty((string)e.Parameter)?.SetValue(item, toggleButton.IsChecked == true);
        }
    }

    /// <summary>
    /// Determines whether the <see cref="CheckAllSelected"/> command can execute on the target control.
    /// </summary>
    /// <param name="sender">The control that invoked the command.</param>
    /// <param name="e">The event data.</param>
    private static void CheckAllSelected_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = sender is ToggleButton;
    }

    /// <summary>
    /// Determines whether the <see cref="Clear"/> command can execute on the target control.
    /// </summary>
    /// <param name="sender">The control that invoked the command.</param>
    /// <param name="e">The event data.</param>
    private static void Clear_Execute(object sender, ExecutedRoutedEventArgs e)
    {
        switch (e.Parameter ?? sender)
        {
            case Selector selector:
                selector.SelectedIndex = -1;
                break;
            case StswPasswordBox stswPasswordBox:
                stswPasswordBox.Password = default;
                break;
            case StswSelectionBox stswSelectionBox:
                stswSelectionBox.ItemsSource.Cast<IStswSelectionItem>().ToList().ForEach(x => x.IsSelected = false);
                stswSelectionBox.UpdateTextCommand?.Execute(null);
                break;
            case TextBox textBox:
                textBox.Clear();
                break;
        }
    }

    /// <summary>
    /// Executes the <see cref="CloseDialog"/> command by closing the associated dialog window.
    /// </summary>
    /// <param name="sender">The control that invoked the command.</param>
    /// <param name="e">The event data, which may contain a parameter for the dialog result.</param>
    private static void Clear_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = (e.Parameter ?? sender) switch
        {
            Selector selector => selector.SelectedIndex >= 0,
            StswPasswordBox stswPasswordBox => !string.IsNullOrEmpty(stswPasswordBox.Password),
            StswSelectionBox stswSelectionBox => stswSelectionBox.ItemsSource?.Cast<IStswSelectionItem>()?.Any(x => x.IsSelected) == true,
            TextBox textBox => !string.IsNullOrEmpty(textBox.Text),
            _ => false,
        };
    }

    /// <summary>
    /// Executes the <see cref="CloseDialog"/> command by closing the associated dialog window.
    /// </summary>
    /// <param name="sender">The control that invoked the command.</param>
    /// <param name="e">The event data, which may contain a parameter for the dialog result.</param>
    private static void CloseDialog_Execute(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is ButtonBase buttonBase)
            StswContentDialog.Close(e.Parameter);
    }

    /// <summary>
    /// Determines whether the <see cref="CloseDialog"/> command can execute on the target control.
    /// </summary>
    /// <param name="sender">The control that invoked the command.</param>
    /// <param name="e">The event data.</param>
    private static void CloseDialog_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = sender is ButtonBase;
    }

    /// <summary>
    /// Executes the <see cref="DeselectAll"/> command by deselecting all selected items in a selection-based control.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void DeselectAll_Execute(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Parameter is IEnumerable items)
            items.Cast<IStswSelectionItem>().ToList().ForEach(x => x.IsSelected = false);
        else if (sender is ItemsControl itemsControl)
            itemsControl.ItemsSource.Cast<IStswSelectionItem>().ToList().ForEach(x => x.IsSelected = false);
    }

    /// <summary>
    /// Determines whether the <see cref="DeselectAll"/> command can execute on the target control.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void DeselectAll_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        if (e.Parameter is IEnumerable items)
            e.CanExecute = items.Cast<IStswSelectionItem>().Any(x => x.IsSelected);
        else if (sender is ItemsControl itemsControl)
            e.CanExecute = itemsControl.ItemsSource?.Cast<IStswSelectionItem>()?.Any(x => x.IsSelected) == true;
    }

    /// <summary>
    /// Executes the <see cref="SelectAll"/> command by selecting all available items in a selection-based control.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void SelectAll_Execute(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Parameter is IEnumerable items)
            items.Cast<IStswSelectionItem>().ToList().ForEach(x => x.IsSelected = true);
        else if (sender is ItemsControl itemsControl)
            itemsControl.ItemsSource.Cast<IStswSelectionItem>().ToList().ForEach(x => x.IsSelected = true);
    }

    /// <summary>
    /// Determines whether the <see cref="SelectAll"/> command can execute on the target control.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void SelectAll_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        if (e.Parameter is IEnumerable items)
            e.CanExecute = items.Cast<IStswSelectionItem>().Any(x => !x.IsSelected);
        else if (sender is ItemsControl itemsControl)
            e.CanExecute = itemsControl.ItemsSource?.Cast<IStswSelectionItem>()?.Any(x => !x.IsSelected) == true;
    }
}
