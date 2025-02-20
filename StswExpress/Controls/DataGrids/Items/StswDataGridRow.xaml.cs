using System.Windows;
using System.Windows.Controls;

namespace StswExpress;
/// <summary>
/// Represents a row in the <see cref="StswDataGrid"/> with built-in selection state synchronization.
/// When the row is selected or unselected, its associated data model automatically updates the selection state.
/// </summary>
public class StswDataGridRow : DataGridRow
{
    static StswDataGridRow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswDataGridRow), new FrameworkPropertyMetadata(typeof(StswDataGridRow)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswDataGridRow), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    #region Events & methods
    /// <summary>
    /// Handles the row selection event. If the data context implements <see cref="IStswSelectionItem"/>, 
    /// the selection state is updated to reflect that the row is selected.
    /// </summary>
    /// <param name="e">The event data.</param>
    protected override void OnSelected(RoutedEventArgs e)
    {
        base.OnSelected(e);

        if (DataContext is IStswSelectionItem item)
            item.IsSelected = true;
    }

    /// <summary>
    /// Handles the row unselection event. If the data context implements <see cref="IStswSelectionItem"/>, 
    /// the selection state is updated to reflect that the row is not selected.
    /// </summary>
    /// <param name="e">The event data.</param>
    protected override void OnUnselected(RoutedEventArgs e)
    {
        base.OnUnselected(e);

        if (DataContext is IStswSelectionItem item)
            item.IsSelected = false;
    }
    #endregion
}
