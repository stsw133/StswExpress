using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
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
        if (baseStyle?.Triggers.OfType<DataTrigger>().Any(t => t.Binding is Binding { Converter: StswShowDetailsStateConverter }) == true)
            return;

        var newStyle = new Style(typeof(StswDataGridRow), dataGrid.RowStyle);
        newStyle.Setters.Add(new Setter(DataGridRow.DetailsVisibilityProperty, Visibility.Collapsed));
        newStyle.Triggers.Add(new DataTrigger
        {
            Binding = new Binding
            {
                Mode = BindingMode.OneWay,
                Converter = StswShowDetailsStateConverter.Instance
            },
            Value = true,
            Setters = { new Setter(DataGridRow.DetailsVisibilityProperty, Visibility.Visible) }
        });

        dataGrid.RowStyle = newStyle;
    }
    #endregion
}

/// <summary>
/// Converts an item into its <see cref="IStswDetailedItem.ShowDetails"/> value when available.
/// Returns <see langword="false"/> for items that do not implement <see cref="IStswDetailedItem"/>.
/// </summary>
internal class StswShowDetailsStateConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswShowDetailsStateConverter Instance => instance ??= new StswShowDetailsStateConverter();
    private static StswShowDetailsStateConverter? instance;

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var showDetails = value is IStswDetailedItem detailsItem && (detailsItem.ShowDetails ?? false);

        return targetType == typeof(Visibility)
            ? showDetails ? Visibility.Visible : Visibility.Collapsed
            : showDetails;
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => Binding.DoNothing;
}
