using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace StswExpress;

/// <summary>
/// Provides custom commands for common UI interactions.
/// </summary>
[StswInfo("0.9.0", "0.20.0")]
public static class StswCommands
{
    /// <summary>
    /// A command to clear the content of various controls, such as text boxes or item lists.
    /// </summary>
    [StswInfo("0.21.0")]
    public static readonly RoutedUICommand ClearItems = new(nameof(ClearItems), nameof(ClearItems), typeof(StswCommands));

    /// <summary>
    /// A command to deselect all selected items in a selection-based control.
    /// </summary>
    [StswInfo("0.20.0", "0.21.0")]
    public static readonly RoutedUICommand ClearSelection = new(nameof(ClearSelection), nameof(ClearSelection), typeof(StswCommands));

    /// <summary>
    /// A command to clear the content of various controls, such as text boxes or item lists.
    /// </summary>
    [StswInfo("0.9.0", "0.21.0")]
    public static readonly RoutedUICommand ClearText = new(nameof(ClearText), nameof(ClearText), typeof(StswCommands));

    /// <summary>
    /// A command to close a dialog by triggering the closing mechanism.
    /// </summary>
    [StswInfo("0.13.1")]
    public static readonly RoutedUICommand CloseDialog = new(nameof(CloseDialog), nameof(CloseDialog), typeof(StswCommands));

    /// <summary>
    /// A command to deselect all selected items in a selection-based control.
    /// </summary>
    [StswInfo("0.17.0")]
    public static readonly RoutedUICommand DeselectAll = new(nameof(DeselectAll), nameof(DeselectAll), typeof(StswCommands));

    /// <summary>
    /// A command to select all available items in a selection-based control.
    /// </summary>
    [StswInfo("0.17.0")]
    public static readonly RoutedUICommand SelectAll = new(nameof(SelectAll), nameof(SelectAll), typeof(StswCommands));

    /// <summary>
    /// A command to set a specified property for all selected items in a selection-based control.
    /// </summary>
    [StswInfo("0.9.3", "0.20.0")]
    public static readonly RoutedUICommand SetPropertyForSelected = new(nameof(SetPropertyForSelected), nameof(SetPropertyForSelected), typeof(StswCommands));

    /// <summary>
    /// Static constructor to register command bindings for relevant UI controls.
    /// </summary>
    static StswCommands()
    {
        /// clear [items|selection|text] command
        CommandManager.RegisterClassCommandBinding(typeof(ButtonBase), new CommandBinding(ClearItems, ClearText_Execute, ClearText_CanExecute));
        CommandManager.RegisterClassCommandBinding(typeof(ItemsControl), new CommandBinding(ClearItems, ClearText_Execute, ClearText_CanExecute));

        CommandManager.RegisterClassCommandBinding(typeof(Selector), new CommandBinding(ClearSelection, ClearSelection_Execute, ClearSelection_CanExecute));
        CommandManager.RegisterClassCommandBinding(typeof(StswSelectionBox), new CommandBinding(ClearSelection, ClearSelection_Execute, ClearSelection_CanExecute));

        CommandManager.RegisterClassCommandBinding(typeof(ButtonBase), new CommandBinding(ClearText, ClearText_Execute, ClearText_CanExecute));
        CommandManager.RegisterClassCommandBinding(typeof(StswPasswordBox), new CommandBinding(ClearText, ClearText_Execute, ClearText_CanExecute));
        CommandManager.RegisterClassCommandBinding(typeof(TextBox), new CommandBinding(ClearText, ClearText_Execute, ClearText_CanExecute));

        /// close dialog command
        CommandManager.RegisterClassCommandBinding(typeof(ButtonBase), new CommandBinding(CloseDialog, CloseDialog_Execute, CloseDialog_CanExecute));

        /// [select|deselect] all commands
        CommandManager.RegisterClassCommandBinding(typeof(ButtonBase), new CommandBinding(SelectAll, SelectAll_Execute, SelectAll_CanExecute));
        CommandManager.RegisterClassCommandBinding(typeof(ButtonBase), new CommandBinding(DeselectAll, DeselectAll_Execute, DeselectAll_CanExecute));

        /// set property for selected command
        CommandManager.RegisterClassCommandBinding(typeof(ToggleButton), new CommandBinding(SetPropertyForSelected, SetPropertyForSelected_Execute, SetPropertyForSelected_CanExecute));
    }

    /// <summary>
    /// Executes the <see cref="ClearItems"/> command by closing the associated dialog window.
    /// </summary>
    /// <param name="sender">The control that invoked the command.</param>
    /// <param name="e">The event data.</param>
    [StswInfo("0.21.0")]
    private static void ClearItems_Execute(object sender, ExecutedRoutedEventArgs e)
    {
        switch (e.Parameter ?? sender)
        {
            case IList list:
                list.Clear();
                break;
            case ItemsControl ic when ic.ItemsSource is null:
                ic.Items.Clear();
                break;
            case ItemsControl ic when ic.ItemsSource is IList srcList:
                srcList.Clear();
                break;
        }
    }

    /// <summary>
    /// Determines whether the <see cref="ClearItems"/> command can execute on the target control.
    /// </summary>
    /// <param name="sender">The control that invoked the command.</param>
    /// <param name="e">The event data.</param>
    [StswInfo("0.21.0")]
    private static void ClearItems_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = (e.Parameter ?? sender) switch
        {
            IList list => list.Count > 0,
            ItemsControl ic when ic.ItemsSource is null => ic.Items.Count > 0,
            ItemsControl ic when ic.ItemsSource is IList srcList => srcList.Count > 0,
            _ => false,
        };
    }

    /// <summary>
    /// Executes the <see cref="ClearSelection"/> command by closing the associated dialog window.
    /// </summary>
    /// <param name="sender">The control that invoked the command.</param>
    /// <param name="e">The event data.</param>
    [StswInfo("0.20.0")]
    private static void ClearSelection_Execute(object sender, ExecutedRoutedEventArgs e)
    {
        switch (e.Parameter ?? sender)
        {
            case Selector selector:
                selector.SelectedIndex = -1;
                break;
            case StswSelectionBox box when box.ItemsSource is IEnumerable src:
                foreach (var item in src.Cast<object>().OfType<IStswSelectionItem>())
                    item.IsSelected = false;
                box.UpdateTextCommand?.Execute(null);
                break;
        }
    }

    /// <summary>
    /// Determines whether the <see cref="ClearSelection"/> command can execute on the target control.
    /// </summary>
    /// <param name="sender">The control that invoked the command.</param>
    /// <param name="e">The event data, which may contain a parameter for the dialog result.</param>
    [StswInfo("0.20.0")]
    private static void ClearSelection_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = (e.Parameter ?? sender) switch
        {
            Selector selector => selector.SelectedIndex >= 0,
            StswSelectionBox stswSelectionBox when stswSelectionBox.ItemsSource is IEnumerable src
                => src.Cast<object>().OfType<IStswSelectionItem>().Any(i => i.IsSelected),
            _ => false,
        };
    }

    /// <summary>
    /// Executes the <see cref="ClearText"/> command by closing the associated dialog window.
    /// </summary>
    /// <param name="sender">The control that invoked the command.</param>
    /// <param name="e">The event data.</param>
    [StswInfo("0.9.0", "0.21.0")]
    private static void ClearText_Execute(object sender, ExecutedRoutedEventArgs e)
    {
        switch (e.Parameter ?? sender)
        {
            case StswPasswordBox pwd:
                pwd.Password = default;
                break;
            case TextBox tb:
                tb.Clear();
                break;
        }
    }

    /// <summary>
    /// Determines whether the <see cref="ClearText"/> command can execute on the target control.
    /// </summary>
    /// <param name="sender">The control that invoked the command.</param>
    /// <param name="e">The event data.</param>
    [StswInfo("0.9.0", "0.20.0")]
    private static void ClearText_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = (e.Parameter ?? sender) switch
        {
            StswPasswordBox pwd => !string.IsNullOrEmpty(pwd.Password),
            TextBox tb => !string.IsNullOrEmpty(tb.Text),
            _ => false,
        };
    }

    /// <summary>
    /// Executes the <see cref="CloseDialog"/> command by closing the associated dialog window.
    /// </summary>
    /// <param name="sender">The control that invoked the command.</param>
    /// <param name="e">The event data, which may contain a parameter for the dialog result.</param>
    [StswInfo("0.13.1")]
    private static void CloseDialog_Execute(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is ButtonBase)
            StswContentDialog.Close(e.Parameter);
    }

    /// <summary>
    /// Determines whether the <see cref="CloseDialog"/> command can execute on the target control.
    /// </summary>
    /// <param name="sender">The control that invoked the command.</param>
    /// <param name="e">The event data.</param>
    [StswInfo("0.13.1")]
    private static void CloseDialog_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = sender is ButtonBase;

    /// <summary>
    /// Executes the <see cref="DeselectAll"/> command by deselecting all selected items in a selection-based control.
    /// </summary>
    /// <param name="sender">The control that invoked the command.</param>
    /// <param name="e">The event data.</param>
    [StswInfo("0.17.0")]
    private static void DeselectAll_Execute(object sender, ExecutedRoutedEventArgs e)
    {
        if (TryGetSelectionItems(sender, e.Parameter, out var items))
            foreach (var it in items)
                it.IsSelected = false;
    }

    /// <summary>
    /// Determines whether the <see cref="DeselectAll"/> command can execute on the target control.
    /// </summary>
    /// <param name="sender">The control that invoked the command.</param>
    /// <param name="e">The event data.</param>
    [StswInfo("0.17.0")]
    private static void DeselectAll_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = TryGetSelectionItems(sender, e.Parameter, out var items) && items.Any(i => i.IsSelected);
    }

    /// <summary>
    /// Executes the <see cref="SelectAll"/> command by selecting all available items in a selection-based control.
    /// </summary>
    /// <param name="sender">The control that invoked the command.</param>
    /// <param name="e">The event data.</param>
    [StswInfo("0.17.0")]
    private static void SelectAll_Execute(object sender, ExecutedRoutedEventArgs e)
    {
        if (TryGetSelectionItems(sender, e.Parameter, out var items))
            foreach (var it in items)
                it.IsSelected = true;
    }

    /// <summary>
    /// Determines whether the <see cref="SelectAll"/> command can execute on the target control.
    /// </summary>
    /// <param name="sender">The control that invoked the command.</param>
    /// <param name="e">The event data.</param>
    [StswInfo("0.17.0")]
    private static void SelectAll_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = TryGetSelectionItems(sender, e.Parameter, out var items) && items.Any(i => !i.IsSelected);
    }

    /// <summary>
    /// Executes the <see cref="SetPropertyForSelected"/> command by selecting all available items in a selection-based control.
    /// </summary>
    /// <param name="sender">The control that invoked the command.</param>
    /// <param name="e">The event data, where the parameter is the name of the property to set.</param>
    [StswInfo("0.9.3", "0.20.0")]
    private static void SetPropertyForSelected_Execute(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is not ToggleButton toggle)
            return;

        var control = StswFnUI.FindVisualAncestor<IStswSelectionControl>(toggle);
        if (control?.ItemsSource is not IEnumerable src)
            return;

        var propName = e.Parameter as string;
        if (string.IsNullOrWhiteSpace(propName))
            return;

        PropertyInfo? prop = null;
        foreach (var obj in src.Cast<object>().OfType<IStswSelectionItem>().Where(x => x.IsSelected))
        {
            prop ??= obj.GetType().GetProperty(propName);
            prop?.SetValue(obj, toggle.IsChecked == true);
        }
    }

    /// <summary>
    /// Determines whether the <see cref="SetPropertyForSelected"/> command can execute on the target control.
    /// </summary>
    /// <param name="sender">The control that invoked the command.</param>
    /// <param name="e">The event data.</param>
    [StswInfo("0.9.3", "0.20.0")]
    private static void SetPropertyForSelected_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = sender is ToggleButton;

    #region Helpers
    /// <summary>
    /// Tries to retrieve selection items from the given sender or parameter.
    /// </summary>
    /// <param name="sender">The control that invoked the command.</param>
    /// <param name="param">An optional parameter that may contain a collection of items.</param>
    /// <param name="items">The retrieved selection items, if found.</param>
    /// <returns><see langword="true"/> if selection items were found; otherwise, <see langword="false"/>.</returns>
    private static bool TryGetSelectionItems(object sender, object? param, out IEnumerable<IStswSelectionItem> items)
    {
        if (param is IEnumerable p)
        {
            items = p.Cast<object>().OfType<IStswSelectionItem>();
            return true;
        }
        if (sender is ItemsControl { ItemsSource: IEnumerable src })
        {
            items = src.Cast<object>().OfType<IStswSelectionItem>();
            return true;
        }
        items = [];
        return false;
    }
    #endregion
}
