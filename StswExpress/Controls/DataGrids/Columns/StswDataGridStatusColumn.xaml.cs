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
public class StswDataGridStatusColumn : DataGridTemplateColumn
{
    public StswDataGridStatusColumn()
    {
        Dispatcher.CurrentDispatcher.InvokeAsync(InitializeColumn, DispatcherPriority.Background);
    }

    /// <summary>
    /// Initializes the status column by setting up templates, styles, and behaviors.
    /// This method is invoked asynchronously to ensure it runs after the <see cref="DataGrid"/> is fully loaded.
    /// </summary>
    private void InitializeColumn()
    {
        HeaderTemplate ??= Application.Current.TryFindResource("StswDataGridStatusColumnHeaderTemplate") as DataTemplate;
        CellTemplate ??= Application.Current.TryFindResource("StswDataGridStatusColumnCellTemplate") as DataTemplate;

        if (GetDataGridOwner() is not StswDataGrid dataGrid)
            return;

        var baseCellStyle = Application.Current.TryFindResource("StswDataGridCellStyle") as Style ?? new Style(typeof(DataGridCell));
        CellStyle = new Style(typeof(DataGridCell), baseCellStyle)
        {
            Setters = { new Setter(Control.IsTabStopProperty, false) }
        };

        CanUserReorder = false;
        CanUserResize = false;
        IsReadOnly = true;

        var baseStyle = dataGrid.RowStyle;
        if (baseStyle == null || baseStyle.BasedOn == null)
        {
            baseStyle = new Style(typeof(StswDataGridRow));
            dataGrid.RowStyle = baseStyle;
        }
        var newStyle = new Style(typeof(StswDataGridRow), baseStyle);

        if (!baseStyle.Triggers.OfType<DataTrigger>().Any(x => x.Binding is Binding binding && binding.Path?.Path == nameof(IStswCollectionItem.ShowDetails)))
            newStyle.Triggers.Add(new DataTrigger
            {
                Binding = new Binding(nameof(IStswCollectionItem.ShowDetails)),
                Value = true,
                Setters = { new Setter(StswDataGridRow.DetailsVisibilityProperty, Visibility.Visible) }
            });

        if (!baseStyle.Setters.OfType<Setter>().Any(setter => setter.Property == StswDataGridRow.DetailsVisibilityProperty))
            newStyle.Setters.Add(new Setter(StswDataGridRow.DetailsVisibilityProperty, Visibility.Collapsed));

        dataGrid.RowStyle = newStyle;
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
}

/* usage:

<se:StswDataGridStatusColumn/>

*/
