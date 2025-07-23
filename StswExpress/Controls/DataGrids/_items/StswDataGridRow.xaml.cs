using System.Windows;
using System.Windows.Controls;

namespace StswExpress;
/// <summary>
/// Represents a row in the <see cref="StswDataGrid"/> with built-in selection state synchronization.
/// When the row is selected or unselected, its associated data model automatically updates the selection state.
/// </summary>
[StswInfo("0.15.0")]
public class StswDataGridRow : DataGridRow
{
    static StswDataGridRow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswDataGridRow), new FrameworkPropertyMetadata(typeof(StswDataGridRow)));
    }

    #region Events & methods
    /// <inheritdoc/>
    protected override void OnSelected(RoutedEventArgs e)
    {
        base.OnSelected(e);

        if (DataContext is IStswSelectionItem item)
            item.IsSelected = true;
    }

    /// <inheritdoc/>
    protected override void OnUnselected(RoutedEventArgs e)
    {
        base.OnUnselected(e);

        if (DataContext is IStswSelectionItem item)
            item.IsSelected = false;
    }
    #endregion
}
