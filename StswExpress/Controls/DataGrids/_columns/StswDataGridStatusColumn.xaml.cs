using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace StswExpress;
/// <summary>
/// Represents a status column for <see cref="StswDataGrid"/> that visually indicates row states.
/// The column is read-only and automatically binds to row visibility properties.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswDataGridStatusColumn/&gt;
/// </code>
/// </example>
[StswInfo("0.11.0")]
public class StswDataGridStatusColumn : DataGridTemplateColumn
{
    public StswDataGridStatusColumn()
    {
        HeaderTemplate = Application.Current.TryFindResource("StswDataGridStatusColumnHeaderTemplate") as DataTemplate;
        CellTemplate = Application.Current.TryFindResource("StswDataGridStatusColumnCellTemplate") as DataTemplate;

        var baseCellStyle = Application.Current.TryFindResource("StswDataGridCellStyle") as Style ?? new Style(typeof(DataGridCell));
        CellStyle = new Style(typeof(DataGridCell), baseCellStyle)
        {
            Setters = { new Setter(Control.IsTabStopProperty, false) }
        };

        CanUserReorder = false;
        CanUserResize = false;
        IsReadOnly = true;

        Dispatcher.CurrentDispatcher.InvokeAsync(TryExtendRowStyle, DispatcherPriority.Background);
    }

    /// <summary>
    /// Retrieves the parent <see cref="StswDataGrid"/> instance that owns this column.
    /// Uses reflection to access the internal `DataGridOwner` property.
    /// </summary>
    /// <returns>The parent <see cref="StswDataGrid"/> instance, or null if unavailable.</returns>
    private StswDataGrid? GetDataGridOwner()
    {
        var property = typeof(DataGridColumn).GetProperty("DataGridOwner", BindingFlags.NonPublic | BindingFlags.Instance);
        return property?.GetValue(this) as StswDataGrid;
    }

    /// <summary>
    /// Extends the row style of the <see cref="StswDataGrid"/> to include a trigger for showing details.
    /// </summary>
    private void TryExtendRowStyle()
    {
        var dataGrid = GetDataGridOwner();
        if (dataGrid == null)
            return;

        var baseStyle = dataGrid.RowStyle;
        if (baseStyle?.Triggers.OfType<DataTrigger>().Any(t =>
            t.Binding is Binding b && b.Path?.Path == nameof(IStswCollectionItem.ShowDetails)) == true)
            return;

        var newStyle = new Style(typeof(StswDataGridRow), dataGrid.RowStyle);
        newStyle.Setters.Add(new Setter(DataGridRow.DetailsVisibilityProperty, Visibility.Collapsed));
        newStyle.Triggers.Add(new DataTrigger
        {
            Binding = new Binding(nameof(IStswCollectionItem.ShowDetails))
            {
                Mode = BindingMode.OneWay,
                FallbackValue = false,
                TargetNullValue = false
            },
            Value = true,
            Setters = { new Setter(DataGridRow.DetailsVisibilityProperty, Visibility.Visible) }
        });

        dataGrid.RowStyle = newStyle;
    }
}
