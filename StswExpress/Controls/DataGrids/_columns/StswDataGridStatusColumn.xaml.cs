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
        CellTemplate = GetDefaultCellTemplate();

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

    #region Events & methods
    /// <inheritdoc/>
    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == CellTemplateProperty && e.NewValue is null)
            CellTemplate = GetDefaultCellTemplate();
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
    /// Gets the default cell template for the status column from application resources.
    /// </summary>
    /// <returns>The default <see cref="DataTemplate"/> for the cell, or null if not found.</returns>
    private static DataTemplate? GetDefaultCellTemplate() => Application.Current.TryFindResource("StswDataGridStatusColumnCellTemplate") as DataTemplate;

    /// <summary>
    /// Extends the row style of the <see cref="StswDataGrid"/> to include a trigger for showing details.
    /// </summary>
    private void TryExtendRowStyle()
    {
        var dataGrid = GetDataGridOwner();
        if (dataGrid == null)
            return;

        var baseStyle = dataGrid.RowStyle;
        if (baseStyle?.Triggers.OfType<DataTrigger>().Any(IsShowDetailsTrigger) == true)
            return;

        var newStyle = new Style(typeof(StswDataGridRow), dataGrid.RowStyle);
        newStyle.Setters.Add(new Setter(DataGridRow.DetailsVisibilityProperty, Visibility.Collapsed));

        var showDetailsTrigger = new DataTrigger
        {
            Binding = new Binding
            {
                Mode = BindingMode.OneWay,
                Path = new PropertyPath(nameof(StswDataGridRow.DataContext)),
                RelativeSource = new RelativeSource(RelativeSourceMode.Self),
                Converter = StswIsTypeConverter.Instance,
                ConverterParameter = typeof(IStswDetailedItem)
            },
            Value = true
        };

        showDetailsTrigger.Setters.Add(new Setter(DataGridRow.DetailsVisibilityProperty, new Binding
        {
            Path = new PropertyPath($"{nameof(StswDataGridRow.DataContext)}.{nameof(IStswDetailedItem.ShowDetails)}"),
            RelativeSource = new RelativeSource(RelativeSourceMode.Self),
            Converter = StswBoolConverter.Instance,
            TargetNullValue = Visibility.Collapsed,
            FallbackValue = Visibility.Collapsed
        }));

        newStyle.Triggers.Add(showDetailsTrigger);

        dataGrid.RowStyle = newStyle;
    }

    /// <summary>
    /// Determines whether the given <see cref="DataTrigger"/> is the one used to show details.
    /// </summary>
    /// <param name="trigger">The trigger to check.</param>
    /// <returns><see langword="true"/> if the trigger is for showing details; otherwise, <see langword="false"/>.</returns>
    private static bool IsShowDetailsTrigger(DataTrigger trigger)
    {
        if (trigger.Binding is not Binding binding)
            return false;

        if (binding.Path?.Path == nameof(IStswDetailedItem.ShowDetails))
            return true;

        if (binding.Converter == StswIsTypeConverter.Instance
         && Equals(binding.ConverterParameter, typeof(IStswDetailedItem))
         && binding.Path?.Path == nameof(StswDataGridRow.DataContext)
         && trigger.Value is bool boolValue && boolValue
         && trigger.Setters.OfType<Setter>().Any(IsShowDetailsSetter))
            return true;

        return false;
    }

    /// <summary>
    /// Determines whether the given <see cref="Setter"/> is the one used to show details.
    /// </summary>
    /// <param name="setter">The setter to check.</param>
    /// <returns><see langword="true"/> if the setter is for showing details; otherwise, <see langword="false"/>.</returns>
    private static bool IsShowDetailsSetter(Setter setter)
    {
        if (setter.Property != DataGridRow.DetailsVisibilityProperty)
            return false;

        return setter.Value switch
        {
            Binding binding => binding.Path?.Path == $"{nameof(StswDataGridRow.DataContext)}.{nameof(IStswDetailedItem.ShowDetails)}" &&
                               binding.RelativeSource?.Mode == RelativeSourceMode.Self &&
                               binding.Converter == StswBoolConverter.Instance,
            System.Windows.Visibility => true,
            _ => false
        };
    }
    #endregion
}
