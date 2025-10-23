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
public class StswDataGridStatusColumn : DataGridTemplateColumn
{
    public StswDataGridStatusColumn()
    {
        HeaderTemplate = Application.Current.TryFindResource("StswDataGridStatusColumnHeaderTemplate") as DataTemplate;
        SetCurrentValue(StatusCellTemplateProperty, Application.Current.TryFindResource("StswDataGridStatusColumnCellTemplate") as DataTemplate);

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
    /// Gets or sets the <see cref="DataTemplate"/> used to render the cell content of the status column.
    /// When set to <see langword="null"/>, the column falls back to the default template delivered with the library.
    /// </summary>
    public DataTemplate? StatusCellTemplate
    {
        get => (DataTemplate?)GetValue(StatusCellTemplateProperty);
        set => SetValue(StatusCellTemplateProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="StatusCellTemplate"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty StatusCellTemplateProperty
        = DependencyProperty.Register(
            nameof(StatusCellTemplate),
            typeof(DataTemplate),
            typeof(StswDataGridStatusColumn),
            new PropertyMetadata(null, OnStatusCellTemplateChanged)
        );
    private static void OnStatusCellTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswDataGridStatusColumn stsw)
            return;

        stsw.CellTemplate = e.NewValue as DataTemplate ?? GetDefaultCellTemplate();
    }
    private static DataTemplate? GetDefaultCellTemplate() => Application.Current.TryFindResource("StswDataGridStatusColumnCellTemplate") as DataTemplate;

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
            t.Binding is Binding b && b.Path?.Path == nameof(IStswTrackableItem.ShowDetails)) == true)
            return;

        var newStyle = new Style(typeof(StswDataGridRow), dataGrid.RowStyle);
        newStyle.Setters.Add(new Setter(DataGridRow.DetailsVisibilityProperty, Visibility.Collapsed));
        newStyle.Triggers.Add(new DataTrigger
        {
            Binding = new Binding(nameof(IStswTrackableItem.ShowDetails))
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
