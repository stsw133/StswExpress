using System.Windows;
using System.Windows.Controls;

namespace StswExpress;
/// <summary>
/// 
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
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnSelected(RoutedEventArgs e)
    {
        base.OnSelected(e);

        if (DataContext is IStswSelectionItem item)
            item.IsSelected = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnUnselected(RoutedEventArgs e)
    {
        base.OnUnselected(e);

        if (DataContext is IStswSelectionItem item)
            item.IsSelected = false;
    }
    #endregion
}
