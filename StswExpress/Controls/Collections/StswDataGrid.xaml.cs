using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Represents a control that provides a flexible and powerful way to display and edit data in a tabular format.
/// </summary>
public class StswDataGrid : DataGrid
{
    public ICommand ClearFiltersCommand { get; set; }

    public StswDataGrid()
    {
        ClearFiltersCommand = new StswRelayCommand(ActionClear);
    }
    /* /// this will cause control to be not visible
    static StswDataGrid()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswDataGrid), new FrameworkPropertyMetadata(typeof(StswDataGrid)));
    }
    */

    #region Events
    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        ColumnHeaderStyle = (Style)FindResource("StswDataGridColumnHeaderStyle");
        FiltersData = new()
        {
            Clear = ActionClear,
            Refresh = ActionRefresh,
            SqlFilter = null,
            SqlParameters = null
        };

        base.OnApplyTemplate();
    }

    /// <summary>
    /// Clears the filters.
    /// </summary>
    private void ActionClear()
    {
        var stswFilters = StswFn.FindVisualChildren<StswFilter>(this).ToList();
        foreach (var stswFilter in stswFilters)
        {
            stswFilter.FilterMode = stswFilter.DefaultFilterMode;
            stswFilter.SelectedItemsBinding = stswFilter.DefaultSelectedItemsBinding?.Clone();
            stswFilter.Value1 = stswFilter.DefaultValue1;
            stswFilter.Value2 = stswFilter.DefaultValue2;
        }
    }

    /// <summary>
    /// Refreshes the filters and updates the SQL filter and parameters.
    /// </summary>
    private void ActionRefresh()
    {
        FiltersData.SqlFilter = string.Empty;
        FiltersData.SqlParameters = new List<(string, object)>();

        var stswFilters = StswFn.FindVisualChildren<StswFilter>(this).ToList();
        foreach (var stswFilter in stswFilters)
        {
            /// Header is StswColumnFilterData
            if (stswFilter?.SqlString != null)
            {
                FiltersData.SqlFilter += " and " + stswFilter.SqlString;
                if (stswFilter.Value1 != null && stswFilter.SqlParam != null)
                    FiltersData.SqlParameters.Add((stswFilter.SqlParam[..(stswFilter.SqlParam.Length > 120 ? 120 : stswFilter.SqlParam.Length)] + "1", stswFilter.Value1 ?? DBNull.Value));
                if (stswFilter.Value2 != null && stswFilter.SqlParam != null)
                    FiltersData.SqlParameters.Add((stswFilter.SqlParam[..(stswFilter.SqlParam.Length > 120 ? 120 : stswFilter.SqlParam.Length)] + "2", stswFilter.Value2 ?? DBNull.Value));
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
    /// Enum with values of the visibility mode for special column.
    /// </summary>
    public enum SpecialColumnVisibilities
    {
        Collapsed,
        All,
        OnlyRows
    }
    /// <summary>
    /// Gets or sets the visibility mode for special column.
    /// </summary>
    public SpecialColumnVisibilities SpecialColumnVisibility
    {
        get => (SpecialColumnVisibilities)GetValue(SpecialColumnVisibilityProperty);
        set => SetValue(SpecialColumnVisibilityProperty, value);
    }
    public static readonly DependencyProperty SpecialColumnVisibilityProperty
        = DependencyProperty.Register(
            nameof(SpecialColumnVisibility),
            typeof(SpecialColumnVisibilities),
            typeof(StswDataGrid),
            new FrameworkPropertyMetadata(default(SpecialColumnVisibilities),
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

            if (stsw.SpecialColumnVisibility != SpecialColumnVisibilities.Collapsed)
            {
                if (specialColumn == null)
                {
                    /// create special column
                    stsw.Columns.Insert(0, new DataGridTemplateColumn() { CellTemplate = specialColumnCellTemplate, HeaderTemplate = specialColumnHeaderTemplate });
                    stsw.FrozenColumnCount++;

                    specialColumn = stsw.Columns[0];
                }

                /// set visibility for header
                if (specialColumn?.Header is UniformGrid header and not null)
                    header.Visibility = stsw.SpecialColumnVisibility == SpecialColumnVisibilities.All ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (specialColumn != null)
            {
                /// remove special column
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
            typeof(StswDataGrid),
            new PropertyMetadata(default(SolidColorBrush))
        );
    #endregion
}

/// <summary>
/// Data model for StswDataGrid's filters.
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
