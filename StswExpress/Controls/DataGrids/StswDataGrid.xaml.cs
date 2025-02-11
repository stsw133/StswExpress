using Microsoft.Data.SqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// Represents a control that provides a flexible and powerful way to display and edit data in a tabular format.
/// </summary>
public class StswDataGrid : DataGrid, IStswCornerControl, IStswSelectionControl
{
    public StswDataGrid()
    {
        ApplyFiltersCommand = new StswCommand(ApplyFilters);
        ClearFiltersCommand = new StswCommand(ClearFilters);

        FiltersData = new StswDataGridFiltersDataModel
        {
            Apply = ApplyFilters,
            Clear = ClearFilters,
        };
    }
    static StswDataGrid()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswDataGrid), new FrameworkPropertyMetadata(typeof(StswDataGrid)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswDataGrid), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    protected override DependencyObject GetContainerForItemOverride() => new StswDataGridRow();
    protected override bool IsItemItsOwnContainerOverride(object item) => item is StswDataGridRow;

    #region Events & methods
    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// force styles to refresh
        CellStyle = CellStyle;
        ColumnHeaderStyle = ColumnHeaderStyle;
        RowHeaderStyle = RowHeaderStyle;

        /// attach local event on all discovered FilterBoxes
        var filterBoxes = StswFn.FindVisualChildren<StswFilterBox>(this);
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
    }

    /// <summary>
    /// Custom auto-generating columns for specific property types.
    /// </summary>
    /// <param name="e">Auto-generating column event arguments</param>
    protected override void OnAutoGeneratingColumn(DataGridAutoGeneratingColumnEventArgs e)
    {
        /// if a column with this same binding already exists, skip
        if (Columns.OfType<DataGridBoundColumn>().Any(x => x.Binding is Binding binding && binding.Path.Path == e.PropertyName))
        {
            e.Cancel = true;
            return;
        }

        /// skip generating column if StswIgnoreAutoGenerateColumnAttribute is present
        if (e.PropertyDescriptor is System.ComponentModel.PropertyDescriptor property && property.Attributes[typeof(StswIgnoreAutoGenerateColumnAttribute)] != null)
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

    /// <summary>
    /// Occurs when the ItemsSource property value changes.
    /// </summary>
    /// <param name="oldValue">The old value of the ItemsSource property.</param>
    /// <param name="newValue">The new value of the ItemsSource property.</param>
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        IStswSelectionControl.ItemsSourceChanged(this, newValue);
        base.OnItemsSourceChanged(oldValue, newValue);
    }
    #endregion

    #region Filters
    private StswFilterAggregator _filterAggregator = new();
    public ICommand ClearFiltersCommand { get; }
    public ICommand ApplyFiltersCommand { get; }

    /// <summary>
    /// Clears the filters.
    /// </summary>
    private void ClearFilters()
    {
        var filterBoxes = StswFn.FindVisualChildren<StswFilterBox>(this).ToList();

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
    /// Applies the filters and updates the SQL filter and parameters.
    /// </summary>
    private void ApplyFilters()
    {
        var filterBoxes = StswFn.FindVisualChildren<StswFilterBox>(this).ToList();
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
    /// Allows external controls (e.g. <see cref="StswFilterBox"/>) to register/unregister filter predicates.
    /// </summary>
    public void RegisterExternalFilter(object key, Predicate<object>? filter)
    {
        _filterAggregator.RegisterFilter(key, filter);
        ApplyFilters();
    }

    /// <summary>
    /// Gets or sets the final SQL filter text.
    /// </summary>
    public string SqlFilter
    {
        get => _sqlFilter;
        private set => _sqlFilter = value;
    }
    private string _sqlFilter = "1=1";

    /// <summary>
    /// Gets or sets the final collection of SQL parameters.
    /// </summary>
    public IList<SqlParameter> SqlParameters
    {
        get => _sqlParameters;
        private set => _sqlParameters = value;
    }
    private IList<SqlParameter> _sqlParameters = [];

    /// <summary>
    /// Builds the combined SQL filter from all filter boxes.
    /// </summary>
    /// <param name="filterBoxes"></param>
    private void UpdateSqlFilters(IEnumerable<StswFilterBox> filterBoxes)
    {
        FiltersData ??= new StswDataGridFiltersDataModel();
        FiltersData.SqlFilter = string.Join(" AND ", filterBoxes.Select(x => x.SqlString).Where(x => !string.IsNullOrWhiteSpace(x)));
        FiltersData.SqlParameters = filterBoxes.SelectMany(x =>
            new[]
            {
                new SqlParameter($"{x.SqlParam}1", x.Value1 ?? DBNull.Value),
                new SqlParameter($"{x.SqlParam}2", x.Value2 ?? DBNull.Value)
            })
            .Where(p => p.Value != DBNull.Value)
            .ToList();

        if (string.IsNullOrWhiteSpace(FiltersData.SqlFilter))
            FiltersData.SqlFilter = "1=1";
    }

    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets a value indicating whether the filters are visible.
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
    /// Gets or sets the filters data for the control.
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
    private static void OnFiltersDataChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswDataGrid stsw)
        {
            if (e.NewValue is StswDataGridFiltersDataModel filtersData)
            {
                filtersData.Clear = stsw.ClearFilters;
                filtersData.Apply = stsw.ApplyFilters;
            }
        }
    }

    /// <summary>
    /// Gets or sets the filters type for the control.
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
    /// Gets or sets the command for refreshing the data (if Enter key is pressed inside any filter box).
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
    /// Gets or sets the command parameter for refreshing the data (if Enter key is pressed inside any filter box).
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
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets a value indicating whether corner clipping is enabled for the control.
    /// When set to <see langword="true"/>, content within the control's border area is clipped to match
    /// the border's rounded corners, preventing elements from protruding beyond the border.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the degree to which the corners of the control's border are rounded by defining
    /// a radius value for each corner independently. This property allows users to control the roundness
    /// of corners, and large radius values are smoothly scaled to blend from corner to corner.
    /// </summary>
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
    /// Gets or sets the background brush for the header.
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
    /// Gets or sets the border brush for the header.
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

/// <summary>
/// Data model for <see cref="StswDataGrid"/>'s filters.
/// </summary>
public class StswDataGridFiltersDataModel
{
    /// <summary>
    /// Gets or sets the action for applying the filters.
    /// </summary>
    public Action? Apply { get; internal set; }

    /// <summary>
    /// Gets or sets the action for clearing the filters.
    /// </summary>
    public Action? Clear { get; internal set; }

    /// <summary>
    /// Gets or sets the SQL filter.
    /// </summary>
    public string SqlFilter { get; internal set; } = "1=1";

    /// <summary>
    /// Gets or sets the list of SQL parameters.
    /// </summary>
    public IList<SqlParameter> SqlParameters { get; internal set; } = [];
}

[AttributeUsage(AttributeTargets.Property)]
public class StswIgnoreAutoGenerateColumnAttribute : Attribute
{
}
