using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace StswExpress;
/// <summary>
/// Represents an advanced data grid control that provides a flexible and powerful way to display and edit data in a tabular format.
/// Supports filtering, sorting, custom column types, SQL-based filtering, and collection-based filtering.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswDataGrid ItemsSource="{Binding Products}"&gt;
///     &lt;se:StswDataGridTextColumn Header="Name" Binding="{Binding Name}"/&gt;
///     &lt;se:StswDataGridDecimalColumn Header="Price" Binding="{Binding Price}"/&gt;
///     &lt;se:StswDataGridDateColumn Header="Added Date" Binding="{Binding AddedDate}"/&gt;
/// &lt;/se:StswDataGrid&gt;
/// </code>
/// </example>
public partial class StswDataGrid : DataGrid, IStswCornerControl, IStswSelectionControl
{
    private readonly StswScrollActionScheduler _scrollActionScheduler;
    private static Type? SqlParameterType { get; set; }
    private static bool SqlClientAvailable { get; set; }

    public StswDataGrid()
    {
        _scrollActionScheduler = new StswScrollActionScheduler(this);
        ApplyFiltersCommand = new StswCommand(ApplyFilters);
        ClearFiltersCommand = new StswCommand(ClearFilters);

        if (SqlClientAvailable)
            FiltersData = new StswDataGridFiltersDataModel
            {
                Apply = ApplyFilters,
                Clear = ClearFilters,
            };
    }
    static StswDataGrid()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswDataGrid), new FrameworkPropertyMetadata(typeof(StswDataGrid)));
        DetectSqlClient();
    }

    protected override DependencyObject GetContainerForItemOverride() => new StswDataGridRow();
    protected override bool IsItemItsOwnContainerOverride(object item) => item is StswDataGridRow;

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// force styles to refresh
        ColumnHeaderStyle = ColumnHeaderStyle;
        RowHeaderStyle = RowHeaderStyle;

        /// attach local event on all discovered FilterBoxes
        var filterBoxes = StswFnUI.FindVisualChildren<StswFilterBox>(this);
        foreach (var filterBox in filterBoxes)
            filterBox.FilterChanged += (_, _) => ApplyFilters();

        /// if we are using CollectionView filters, set aggregator now
        if (FiltersType == StswDataGridFiltersType.CollectionView)
        {
            var collectionView = CollectionViewSource.GetDefaultView(ItemsSource);
            if (collectionView != null)
                collectionView.Filter = _filterAggregator.CombinedFilter;
        }

        ApplyFilters();

        if (ScrollToItemBehavior == StswScrollToItemBehavior.OnSelection && SelectedItem != null)
            _scrollActionScheduler.Schedule(() => ScrollIntoView(SelectedItem), DispatcherPriority.Loaded);

        HasVisibleBackgroundGrid = !Columns.Any(col => col.Width.IsStar) || (Columns.Any(col => col.Width.IsStar) && !HasItems);
    }

    /// <inheritdoc/>
    protected override void OnAutoGeneratingColumn(DataGridAutoGeneratingColumnEventArgs e)
    {
        /// if a column with this same binding already exists, skip
        if (Columns.OfType<DataGridBoundColumn>().Any(x => x.Binding is Binding binding && binding.Path.Path == e.PropertyName))
        {
            e.Cancel = true;
            return;
        }

        /// skip generating column if StswIgnoreAutoGenerateColumnAttribute is present
        if (e.PropertyDescriptor is PropertyDescriptor property && property.Attributes[typeof(StswIgnoreAutoGenerateColumnAttribute)] != null)
        {
            e.Cancel = true;
            return;
        }

        /// must be a DataGridBoundColumn
        if (e.Column is not DataGridBoundColumn boundColumn)
        {
            e.Cancel = true;
            return;
        }

        if (boundColumn.Binding is not Binding binding)
        {
            e.Cancel = true;
            return;
        }

        /// auto-generate columns based on property type
        e.Column = e.PropertyType switch
        {
            Type t when t == typeof(bool) || t == typeof(bool?) => new StswDataGridCheckColumn { Header = e.Column.Header, Binding = binding },
            Type t when t == typeof(Color) || t == typeof(Color?) => new StswDataGridColorColumn { Header = e.Column.Header, Binding = binding },
            Type t when t == typeof(DateTime) || t == typeof(DateTime?) => new StswDataGridDateColumn { Header = e.Column.Header, Binding = binding },
            Type t when t == typeof(decimal) || t == typeof(decimal?) => new StswDataGridDecimalColumn { Header = e.Column.Header, Binding = binding },
            _ => new StswDataGridTextColumn { Header = e.Column.Header, Binding = binding }
        };

        base.OnAutoGeneratingColumn(e);
    }

    /// <inheritdoc/>
    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);

        if (ScrollToItemBehavior == StswScrollToItemBehavior.OnInsert && e.Action == NotifyCollectionChangedAction.Add && e.NewItems?.Count > 0)
            _scrollActionScheduler.Schedule(() => ScrollIntoView(e.NewItems[^1]), DispatcherPriority.Background);

        HasVisibleBackgroundGrid = !Columns.Any(col => col.Width.IsStar) || (Columns.Any(col => col.Width.IsStar) && !HasItems);
    }

    /// <inheritdoc/>
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        IStswSelectionControl.ItemsSourceChanged(this, newValue);
        base.OnItemsSourceChanged(oldValue, newValue);
    }

    /// <inheritdoc/>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.V && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            StswFnUI.PasteFromClipboard(this);
            e.Handled = true;
        }

        base.OnKeyDown(e);
    }

    /// <inheritdoc/>
    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);
        IStswSelectionControl.SelectionChanged(this, e.AddedItems, e.RemovedItems);

        if (ScrollToItemBehavior == StswScrollToItemBehavior.OnSelection && SelectedItem != null)
            _scrollActionScheduler.Schedule(() => ScrollIntoView(SelectedItem), DispatcherPriority.Background);
    }

    /// <summary>
    /// Detects the presence of the Microsoft.Data.SqlClient assembly and retrieves the SqlParameter type.
    /// </summary>
    private static void DetectSqlClient()
    {
        try
        {
            var type = Type.GetType("Microsoft.Data.SqlClient.SqlParameter, Microsoft.Data.SqlClient");
            if (type == null && AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name == "Microsoft.Data.SqlClient") is { } asm)
            {
                type = asm.GetType("Microsoft.Data.SqlClient.SqlParameter");
            }
            if (type == null)
            {
                var dllPath = Path.Combine(AppContext.BaseDirectory, "Microsoft.Data.SqlClient.dll");
                if (File.Exists(dllPath))
                {
                    asm = Assembly.LoadFrom(dllPath);
                    type = asm.GetType("Microsoft.Data.SqlClient.SqlParameter");
                }
            }

            SqlParameterType = type;
            SqlClientAvailable = type != null;
        }
        catch
        {
            SqlClientAvailable = false;
            SqlParameterType = null;
        }
    }
    #endregion

    #region Filters
    private readonly StswFilterAggregator _filterAggregator = new();
    public ICommand ApplyFiltersCommand { get; }
    public ICommand ClearFiltersCommand { get; }

    /// <summary>
    /// Gets or sets the final SQL filter text used for querying the data source.
    /// This property is updated dynamically based on the selected filters.
    /// </summary>
    public string SqlFilter
    {
        get => _sqlFilter;
        private set => _sqlFilter = value;
    }
    private string _sqlFilter = "1=1";

    /// <summary>
    /// Gets or sets the collection of SQL parameters associated with the SQL filter.
    /// These parameters are applied dynamically based on user-selected filters.
    /// </summary>
    public IList<object> SqlParameters
    {
        get => _sqlParameters;
        private set => _sqlParameters = value;
    }
    private IList<object> _sqlParameters = [];

    /// <summary>
    /// Applies the current filtering criteria to the data grid.
    /// Updates either CollectionView-based or SQL-based filtering depending on the selected filter type.
    /// </summary>
    private void ApplyFilters()
    {
        var filterBoxes = StswFnUI.FindVisualChildren<StswFilterBox>(this).ToList();

        if (FiltersType == StswDataGridFiltersType.CollectionView)
        {
            var cv = CollectionViewSource.GetDefaultView(ItemsSource);
            if (cv != null)
            {
                cv.Filter = _filterAggregator.CombinedFilter;
                cv.Refresh();
            }
        }
        else if (FiltersType == StswDataGridFiltersType.SQL)
        {
            UpdateSqlFilters(filterBoxes);
        }
    }

    /// <summary>
    /// Clears all applied filters in the data grid.
    /// Resets each filter box to its default state and applies the updated filtering logic.
    /// </summary>
    private void ClearFilters()
    {
        var filterBoxes = StswFnUI.FindVisualChildren<StswFilterBox>(this).ToList();

        foreach (var filterBox in filterBoxes)
        {
            filterBox.FilterMode = filterBox.DefaultFilterMode;
            filterBox.Value1 = filterBox.DefaultValue1;
            filterBox.Value2 = filterBox.DefaultValue2;

            var itemsSource = filterBox.ItemsSource?.OfType<IStswSelectionItem>()?.ToList();
            var defaultItemsSource = filterBox.DefaultItemsSource?.OfType<IStswSelectionItem>()?.ToList();
            itemsSource?.ForEach(x => x.IsSelected = defaultItemsSource?.FirstOrDefault(y => y.Equals(x))?.IsSelected == true);
        }

        filterBoxes.FirstOrDefault()?.Focus();

        ApplyFilters();
    }

    /// <summary>
    /// Creates a SQL parameter instance using the specified name and value.
    /// </summary>
    /// <param name="name">The name of the SQL parameter.</param>
    /// <param name="value">The value of the SQL parameter. If <see langword="null"/>, it will be set to <see cref="DBNull.Value"/>.</param>
    /// <returns></returns>
    private static object? CreateSqlParameter(string name, object? value)
    {
        if (SqlParameterType == null)
            return null;

        try
        {
            return Activator.CreateInstance(SqlParameterType, name, value ?? DBNull.Value);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Registers an external filter predicate for the data grid.
    /// Used to dynamically filter data based on external conditions (e.g., additional UI elements).
    /// </summary>
    /// <param name="key">The key representing the external filter.</param>
    /// <param name="filter">The filtering predicate to apply.</param>
    public void RegisterExternalFilter(object key, Predicate<object>? filter)
    {
        _filterAggregator.RegisterFilter(key, filter);
        ApplyFilters();
    }

    /// <summary>
    /// Builds and updates the combined SQL filter from all filter boxes.
    /// Generates the SQL condition string and assigns appropriate parameters for filtering.
    /// </summary>
    /// <param name="filterBoxes">The list of filter boxes used to construct the SQL filter.</param>
    private void UpdateSqlFilters(IEnumerable<StswFilterBox> filterBoxes)
    {
        if (!SqlClientAvailable || SqlParameterType == null)
            return;

        FiltersData ??= new StswDataGridFiltersDataModel();

        FiltersData.SqlFilter = string.Join(" AND ", filterBoxes
            .Select(x => x.SqlString)
            .Where(x => !string.IsNullOrWhiteSpace(x)));

        FiltersData.MakeSqlParameters(filterBoxes
            .SelectMany(x => new[]
            {
                CreateSqlParameter($"{x.SqlParam}1", x.Value1),
                CreateSqlParameter($"{x.SqlParam}2", x.Value2)
            })
            .Where(p => p is not null)
            .ToList()!);

        if (string.IsNullOrWhiteSpace(FiltersData.SqlFilter))
            FiltersData.SqlFilter = "1=1";
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets a value indicating whether the filters are visible.
    /// When set to <see langword="true"/>, filtering controls are displayed inside the data grid headers.
    /// </summary>
    public bool? AreFiltersVisible
    {
        get => (bool?)GetValue(AreFiltersVisibleProperty);
        set => SetValue(AreFiltersVisibleProperty, value);
    }
    public static readonly DependencyProperty AreFiltersVisibleProperty
        = DependencyProperty.Register(
            nameof(AreFiltersVisible),
            typeof(bool?),
            typeof(StswDataGrid)
        );

    /// <summary>
    /// Gets or sets the filters data model that stores filter criteria, SQL filters, and related parameters.
    /// </summary>
    public StswDataGridFiltersDataModel FiltersData
    {
        get => (StswDataGridFiltersDataModel)GetValue(FiltersDataProperty);
        set => SetValue(FiltersDataProperty, value);
    }
    public static readonly DependencyProperty FiltersDataProperty
        = DependencyProperty.Register(
            nameof(FiltersData),
            typeof(StswDataGridFiltersDataModel),
            typeof(StswDataGrid),
            new FrameworkPropertyMetadata(
                default(StswDataGridFiltersDataModel),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnFiltersDataChanged)
        );
    private static void OnFiltersDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswDataGrid stsw)
            return;

        if (e.NewValue is StswDataGridFiltersDataModel filtersData)
        {
            filtersData.Clear = stsw.ClearFilters;
            filtersData.Apply = stsw.ApplyFilters;
        }
    }

    /// <summary>
    /// Gets or sets the filtering mode for the data grid.
    /// Supports either collection-based filtering or SQL-based filtering.
    /// </summary>
    public StswDataGridFiltersType FiltersType
    {
        get => (StswDataGridFiltersType)GetValue(FiltersTypeProperty);
        set => SetValue(FiltersTypeProperty, value);
    }
    public static readonly DependencyProperty FiltersTypeProperty
        = DependencyProperty.Register(
            nameof(FiltersType),
            typeof(StswDataGridFiltersType),
            typeof(StswDataGrid)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the background grid is visible.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool HasVisibleBackgroundGrid
    {
        get => (bool)GetValue(HasVisibleBackgroundGridProperty);
        set => SetValue(HasVisibleBackgroundGridProperty, value);
    }
    public static readonly DependencyProperty HasVisibleBackgroundGridProperty
        = DependencyProperty.Register(
            nameof(HasVisibleBackgroundGrid),
            typeof(bool),
            typeof(StswDataGrid),
            new PropertyMetadata(false)
        );

    /// <summary>
    /// Gets or sets the command that refreshes the data grid.
    /// This command is typically executed when the Enter key is pressed inside a filter box.
    /// </summary>
    public ICommand RefreshCommand
    {
        get => (ICommand)GetValue(RefreshCommandProperty);
        set => SetValue(RefreshCommandProperty, value);
    }
    public static readonly DependencyProperty RefreshCommandProperty
        = DependencyProperty.Register(
            nameof(RefreshCommand),
            typeof(ICommand),
            typeof(StswDataGrid)
        );

    /// <summary>
    /// Gets or sets the parameter to be passed to the <see cref="RefreshCommand"/> when executed.
    /// </summary>
    public object? RefreshCommandParameter
    {
        get => (object?)GetValue(RefreshCommandParameterProperty);
        set => SetValue(RefreshCommandParameterProperty, value);
    }
    public static readonly DependencyProperty RefreshCommandParameterProperty
        = DependencyProperty.Register(
            nameof(RefreshCommandParameter),
            typeof(object),
            typeof(StswDataGrid)
        );

    /// <summary>
    /// Gets or sets the behavior for scrolling to an item when it is selected or inserted.
    /// </summary>
    public StswScrollToItemBehavior ScrollToItemBehavior
    {
        get => (StswScrollToItemBehavior)GetValue(ScrollToItemBehaviorProperty);
        set => SetValue(ScrollToItemBehaviorProperty, value);
    }
    public static readonly DependencyProperty ScrollToItemBehaviorProperty
        = DependencyProperty.Register(
            nameof(ScrollToItemBehavior),
            typeof(StswScrollToItemBehavior),
            typeof(StswDataGrid)
        );
    #endregion

    #region Style properties
    /// <inheritdoc/>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty
        = DependencyProperty.Register(
            nameof(CornerClipping),
            typeof(bool),
            typeof(StswDataGrid),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <inheritdoc/>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswDataGrid),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the background brush for the data grid's column headers.
    /// </summary>
    public Brush HeaderBackground
    {
        get => (Brush)GetValue(HeaderBackgroundProperty);
        set => SetValue(HeaderBackgroundProperty, value);
    }
    public static readonly DependencyProperty HeaderBackgroundProperty
        = DependencyProperty.Register(
            nameof(HeaderBackground),
            typeof(Brush),
            typeof(StswDataGrid),
            new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the border brush applied to the column headers.
    /// </summary>
    public SolidColorBrush HeaderBorderBrush
    {
        get => (SolidColorBrush)GetValue(HeaderBorderBrushProperty);
        set => SetValue(HeaderBorderBrushProperty, value);
    }
    public static readonly DependencyProperty HeaderBorderBrushProperty
        = DependencyProperty.Register(
            nameof(HeaderBorderBrush),
            typeof(SolidColorBrush),
            typeof(StswDataGrid),
            new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
