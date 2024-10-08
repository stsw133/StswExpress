﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// Represents a control that provides a flexible and powerful way to display and edit data in a tabular format.
/// To work properly it has to be used with <see cref="StswBindingList{T}"/> as its ItemsSource.
/// </summary>
public class StswDataGrid : DataGrid, IStswCornerControl, IStswSelectionControl
{
    public ICommand ClearFiltersCommand { get; set; }

    public StswDataGrid()
    {
        Columns.CollectionChanged += Columns_CollectionChanged;

        ClearFiltersCommand = new StswCommand(ActionClear);
    }
    static StswDataGrid()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswDataGrid), new FrameworkPropertyMetadata(typeof(StswDataGrid)));
    }

    #region Events & methods
    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        CellStyle = CellStyle;
        ColumnHeaderStyle = ColumnHeaderStyle;
        RowHeaderStyle = RowHeaderStyle;

        /// filters
        FiltersData = new()
        {
            Clear = ActionClear,
            Refresh = ActionRefresh,
            SqlFilter = null,
            SqlParameters = null
        };
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnSelectedCellsChanged(SelectedCellsChangedEventArgs e)
    {
        base.OnSelectedCellsChanged(e);

        if (UsesSelectionItems && ItemsSource != null)
        {
            if (e.RemovedCells != null)
            {
                foreach (var item in e.RemovedCells.Select(x => x.Item).Distinct())
                    if (item is IStswSelectionItem selectionItem)
                        selectionItem.IsSelected = false;
            }
            if (e.AddedCells != null)
            {
                foreach (var item in e.AddedCells.Select(x => x.Item).Distinct())
                    if (item is IStswSelectionItem selectionItem)
                        selectionItem.IsSelected = true;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void Columns_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems == null)
            return;

        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            foreach (var newColumn in e.NewItems)
                if (newColumn is StswDataGridStatusColumn column)
                    column.DataGridOwner = this;
        }
    }

    /// <summary>
    /// Clears the filters.
    /// </summary>
    private void ActionClear()
    {
        var stswSqlFilters = StswFn.FindVisualChildren<StswFilterBox>(this).ToList();
        foreach (var stswSqlFilter in stswSqlFilters)
        {
            stswSqlFilter.FilterMode = stswSqlFilter.DefaultFilterMode;
            var itemsSource = stswSqlFilter.ItemsSource?.OfType<IStswSelectionItem>()?.ToList();
            var defaultItemsSource = stswSqlFilter.DefaultItemsSource?.OfType<IStswSelectionItem>()?.ToList();
            itemsSource?.ForEach(x => x.IsSelected = defaultItemsSource?.FirstOrDefault(y => y.Equals(x))?.IsSelected == true);
            stswSqlFilter.Value1 = stswSqlFilter.DefaultValue1;
            stswSqlFilter.Value2 = stswSqlFilter.DefaultValue2;
        }
    }

    /// <summary>
    /// Refreshes the filters and updates the SQL filter and parameters.
    /// </summary>
    private void ActionRefresh()
    {
        var stswSqlFilters = StswFn.FindVisualChildren<StswFilterBox>(this).ToList();

        switch (FiltersType)
        {
            /*
            case StswDataGridFiltersType.CollectionView:
                {
                    var view = (ICollectionView)ItemsSource;

                    var filterPredicates = stswSqlFilters.Select(x => x.GenerateFilterPredicate()).Where(predicate => predicate != null).Cast<Predicate<object>>();

                    Predicate<object>? combinedPredicate = item =>
                    {
                        foreach (var predicate in filterPredicates)
                            if (!predicate(item))
                                return false;
                        return true;
                    };

                    view.Filter = combinedPredicate;
                }
                break;
            */
            case StswDataGridFiltersType.SQL:
            default:
                {
                    FiltersData.SqlFilter = string.Empty;
                    FiltersData.SqlParameters = [];

                    foreach (var stswSqlFilter in stswSqlFilters)
                        /// Header is StswColumnFilterData
                        if (stswSqlFilter?.SqlString != null)
                        {
                            FiltersData.SqlFilter += " and " + stswSqlFilter.SqlString;
                            if (stswSqlFilter.Value1 != null && stswSqlFilter.SqlParam != null)
                                FiltersData.SqlParameters.Add(new(stswSqlFilter.SqlParam[..(stswSqlFilter.SqlParam.Length > 120 ? 120 : stswSqlFilter.SqlParam.Length)] + "1", stswSqlFilter.Value1 ?? DBNull.Value));
                            if (stswSqlFilter.Value2 != null && stswSqlFilter.SqlParam != null)
                                FiltersData.SqlParameters.Add(new(stswSqlFilter.SqlParam[..(stswSqlFilter.SqlParam.Length > 120 ? 120 : stswSqlFilter.SqlParam.Length)] + "2", stswSqlFilter.Value2 ?? DBNull.Value));
                        }

                    if (FiltersData.SqlFilter.StartsWith(" and "))
                        FiltersData.SqlFilter = FiltersData.SqlFilter[5..];
                    if (string.IsNullOrWhiteSpace(FiltersData.SqlFilter))
                        FiltersData.SqlFilter = "1=1";
                }
                break;
        }
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
            typeof(StswDataGrid)
        );

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
    /// Gets or sets the command for refreshing the data (if Enter key is pressed inside filter box).
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
    //TODO - double click row command

    /// <summary>
    /// Gets or sets a value indicating whether the control uses selection items that implement
    /// the <see cref="IStswSelectionItem"/> interface to enable advanced selection features.
    /// </summary>
    public bool UsesSelectionItems
    {
        get => (bool)GetValue(UsesSelectionItemsProperty);
        set => SetValue(UsesSelectionItemsProperty, value);
    }
    public static readonly DependencyProperty UsesSelectionItemsProperty
        = DependencyProperty.Register(
            nameof(UsesSelectionItems),
            typeof(bool),
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
    /// Gets or sets the action for clearing the filters.
    /// </summary>
    public Action? Clear { get; internal set; }

    /// <summary>
    /// Gets or sets the action for refreshing the filters.
    /// </summary>
    public Action? Refresh { get; internal set; }

    /// <summary>
    /// Gets or sets the predicate for CollectionView's Filter.
    /// </summary>
    public Predicate<object>? CollectionViewFilter { get; internal set; }

    /// <summary>
    /// Gets or sets the SQL filter.
    /// </summary>
    public string? SqlFilter { get; internal set; }

    /// <summary>
    /// Gets or sets the list of SQL parameters.
    /// </summary>
    public IList<SqlParameter>? SqlParameters { get; internal set; }
}
