using System;
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
/// To work properly it has to be used with <see cref="StswBindingList{T}"/> as its ItemsSource.
/// </summary>
public class StswDataGrid : DataGrid
{
    public ICommand ClearFiltersCommand { get; set; }

    public StswDataGrid()
    {
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
    /// Clears the filters.
    /// </summary>
    private void ActionClear()
    {
        var StswSqlFilters = StswFn.FindVisualChildren<StswFilterSql>(this).ToList();
        foreach (var StswSqlFilter in StswSqlFilters)
        {
            StswSqlFilter.FilterMode = StswSqlFilter.DefaultFilterMode;
            var itemsSource = StswSqlFilter.ItemsSource?.OfType<IStswSelectionItem>()?.ToList();
            var defaultItemsSource = StswSqlFilter.DefaultItemsSource?.OfType<IStswSelectionItem>()?.ToList();
            itemsSource?.ForEach(x => x.IsSelected = defaultItemsSource?.FirstOrDefault(y => y.Equals(x))?.IsSelected == true);
            StswSqlFilter.Value1 = StswSqlFilter.DefaultValue1;
            StswSqlFilter.Value2 = StswSqlFilter.DefaultValue2;
        }
    }

    /// <summary>
    /// Refreshes the filters and updates the SQL filter and parameters.
    /// </summary>
    private void ActionRefresh()
    {
        FiltersData.SqlFilter = string.Empty;
        FiltersData.SqlParameters = new List<(string, object)>();

        var StswSqlFilters = StswFn.FindVisualChildren<StswFilterSql>(this).ToList();
        foreach (var StswSqlFilter in StswSqlFilters)
        {
            /// Header is StswColumnFilterData
            if (StswSqlFilter?.SqlString != null)
            {
                FiltersData.SqlFilter += " and " + StswSqlFilter.SqlString;
                if (StswSqlFilter.Value1 != null && StswSqlFilter.SqlParam != null)
                    FiltersData.SqlParameters.Add((StswSqlFilter.SqlParam[..(StswSqlFilter.SqlParam.Length > 120 ? 120 : StswSqlFilter.SqlParam.Length)] + "1", StswSqlFilter.Value1 ?? DBNull.Value));
                if (StswSqlFilter.Value2 != null && StswSqlFilter.SqlParam != null)
                    FiltersData.SqlParameters.Add((StswSqlFilter.SqlParam[..(StswSqlFilter.SqlParam.Length > 120 ? 120 : StswSqlFilter.SqlParam.Length)] + "2", StswSqlFilter.Value2 ?? DBNull.Value));
            }
        }

        if (FiltersData.SqlFilter.StartsWith(" and "))
            FiltersData.SqlFilter = FiltersData.SqlFilter[5..];
        if (string.IsNullOrWhiteSpace(FiltersData.SqlFilter))
            FiltersData.SqlFilter = "1=1";
    }
    #endregion

    #region Main properties
    /// <summary>
    /// Gets or sets a value indicating whether the filters are visible.
    /// </summary>
    public bool AreFiltersVisible
    {
        get => (bool)GetValue(AreFiltersVisibleProperty);
        set => SetValue(AreFiltersVisibleProperty, value);
    }
    public static readonly DependencyProperty AreFiltersVisibleProperty
        = DependencyProperty.Register(
            nameof(AreFiltersVisible),
            typeof(bool),
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
    /// Gets or sets the command for refreshing the data.
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
    /// Gets or sets the visibility mode for special column.
    /// </summary>
    public StswSpecialColumnVisibility SpecialColumnVisibility
    {
        get => (StswSpecialColumnVisibility)GetValue(SpecialColumnVisibilityProperty);
        set => SetValue(SpecialColumnVisibilityProperty, value);
    }
    public static readonly DependencyProperty SpecialColumnVisibilityProperty
        = DependencyProperty.Register(
            nameof(SpecialColumnVisibility),
            typeof(StswSpecialColumnVisibility),
            typeof(StswDataGrid),
            new FrameworkPropertyMetadata(default(StswSpecialColumnVisibility),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSpecialColumnVisibilityChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnSpecialColumnVisibilityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswDataGrid stsw)
        {
            var specialColumnCellTemplate = stsw.FindResource("StswDataGridSpecialColumnCellTemplate") as DataTemplate;
            var specialColumnHeaderTemplate = stsw.FindResource("StswDataGridSpecialColumnHeaderTemplate") as DataTemplate;

            var specialColumn = stsw.Columns.FirstOrDefault(x => x is DataGridTemplateColumn specialColumn
                && specialColumn.CellTemplate == specialColumnCellTemplate
                && specialColumn.HeaderTemplate == specialColumnHeaderTemplate);

            if (stsw.SpecialColumnVisibility != StswSpecialColumnVisibility.Collapsed)
            {
                if (specialColumn == null)
                {
                    /// create special column
                    stsw.Columns.Insert(0, new DataGridTemplateColumn() { CellTemplate = specialColumnCellTemplate, HeaderTemplate = specialColumnHeaderTemplate });
                    if (stsw.IsLoaded)
                        stsw.FrozenColumnCount++;

                    specialColumn = stsw.Columns[0];

                    /// make style for cell
                    var newCellStyle = new Style(typeof(DataGridCell));
                    newCellStyle.Setters.Add(new Setter(IsTabStopProperty, false));
                    specialColumn.CellStyle = newCellStyle;
                }
                specialColumn.CanUserReorder = false;
                specialColumn.CanUserResize = false;
                specialColumn.IsReadOnly = true;

                /// set visibility for header
                //if (specialColumn?.HeaderTemplate?.Template is TemplateContent grid and not null)
                //    grid.Visibility = stsw.SpecialColumnVisibility == StswSpecialColumnVisibility.All ? Visibility.Visible : Visibility.Collapsed;

                /// triggers
                var style = stsw.RowStyle ?? new Style(typeof(DataGridRow));
                var newStyle = new Style(typeof(DataGridRow))
                {
                    Resources = style.Resources
                };

                foreach (var setter in style.Setters)
                    newStyle.Setters.Add(setter);

                foreach (var trigger in style.Triggers)
                    newStyle.Triggers.Add(trigger);

                if (style.Triggers.OfType<DataTrigger>().FirstOrDefault(
                        trigger => trigger.Binding is Binding binding &&
                        binding.Path != null && binding.Path.Path == nameof(IStswCollectionItem.ShowDetails)
                    ) == null)
                {
                    var t = new DataTrigger() { Binding = new Binding(nameof(IStswCollectionItem.ShowDetails)), Value = true };
                    t.Setters.Add(new Setter(DataGridRow.DetailsVisibilityProperty, Visibility.Visible));
                    newStyle.Triggers.Add(t);
                }

                if (style.Setters.OfType<Setter>().FirstOrDefault(setter => setter.Property == DataGridRow.DetailsVisibilityProperty) == null)
                    newStyle.Setters.Add(new Setter(DataGridRow.DetailsVisibilityProperty, Visibility.Collapsed));

                stsw.RowStyle = newStyle;
            }
            else if (specialColumn != null)
            {
                /// remove special column
                if (stsw.IsLoaded && stsw.FrozenColumnCount > 0)
                    stsw.FrozenColumnCount--;
                stsw.Columns.Remove(specialColumn);
            }
        }
    }
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the background brush for the header.
    /// </summary>
    public Brush BackgroundHeader
    {
        get => (Brush)GetValue(BackgroundHeaderProperty);
        set => SetValue(BackgroundHeaderProperty, value);
    }
    public static readonly DependencyProperty BackgroundHeaderProperty
        = DependencyProperty.Register(
            nameof(BackgroundHeader),
            typeof(Brush),
            typeof(StswDataGrid)
        );

    /// <summary>
    /// Gets or sets the border brush for the header.
    /// </summary>
    public SolidColorBrush BorderBrushHeader
    {
        get => (SolidColorBrush)GetValue(BorderBrushHeaderProperty);
        set => SetValue(BorderBrushHeaderProperty, value);
    }
    public static readonly DependencyProperty BorderBrushHeaderProperty
        = DependencyProperty.Register(
            nameof(BorderBrushHeader),
            typeof(SolidColorBrush),
            typeof(StswDataGrid)
        );

    /// <summary>
    /// Gets or sets the degree to which the corners of the control are rounded.
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
            typeof(StswDataGrid)
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
    /// Gets or sets the SQL filter.
    /// </summary>
    public string? SqlFilter { get; internal set; }

    /// <summary>
    /// Gets or sets the list of SQL parameters.
    /// </summary>
    public List<(string name, object val)>? SqlParameters { get; internal set; }
}
