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

public class StswDataGrid : DataGrid
{
    public ICommand ClearFiltersCommand { get; set; }

    public StswDataGrid()
    {
        ClearFiltersCommand = new StswRelayCommand(ActionClear);
    }

    #region Events
    /// OnApplyTemplate
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

    /// ActionClear
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

    /// ActionRefresh
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
    /// AreFiltersVisible
    public static readonly DependencyProperty AreFiltersVisibleProperty
        = DependencyProperty.Register(
            nameof(AreFiltersVisible),
            typeof(bool),
            typeof(StswDataGrid)
        );
    public bool AreFiltersVisible
    {
        get => (bool)GetValue(AreFiltersVisibleProperty);
        set => SetValue(AreFiltersVisibleProperty, value);
    }

    /// FiltersData
    public static readonly DependencyProperty FiltersDataProperty
        = DependencyProperty.Register(
            nameof(FiltersData),
            typeof(StswDataGridFiltersDataModel),
            typeof(StswDataGrid)
        );
    public StswDataGridFiltersDataModel FiltersData
    {
        get => (StswDataGridFiltersDataModel)GetValue(FiltersDataProperty);
        set => SetValue(FiltersDataProperty, value);
    }

    /// RefreshCommand
    public static readonly DependencyProperty RefreshCommandProperty
        = DependencyProperty.Register(
            nameof(RefreshCommand),
            typeof(ICommand),
            typeof(StswDataGrid)
        );
    public ICommand RefreshCommand
    {
        get => (ICommand)GetValue(RefreshCommandProperty);
        set => SetValue(RefreshCommandProperty, value);
    }

    /// SpecialColumnVisibility
    public enum SpecialColumnVisibilities
    {
        Collapsed,
        All,
        OnlyRows
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
    public SpecialColumnVisibilities SpecialColumnVisibility
    {
        get => (SpecialColumnVisibilities)GetValue(SpecialColumnVisibilityProperty);
        set => SetValue(SpecialColumnVisibilityProperty, value);
    }
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
    /// > Header ...
    /// BackgroundHeader
    public static readonly DependencyProperty BackgroundHeaderProperty
        = DependencyProperty.Register(
            nameof(BackgroundHeader),
            typeof(Brush),
            typeof(StswDataGrid)
        );
    public Brush BackgroundHeader
    {
        get => (Brush)GetValue(BackgroundHeaderProperty);
        set => SetValue(BackgroundHeaderProperty, value);
    }
    /// BorderBrushHeader
    public static readonly DependencyProperty BorderBrushHeaderProperty
        = DependencyProperty.Register(
            nameof(BorderBrushHeader),
            typeof(SolidColorBrush),
            typeof(StswDataGrid),
            new PropertyMetadata(default(SolidColorBrush))
        );
    public SolidColorBrush BorderBrushHeader
    {
        get => (SolidColorBrush)GetValue(BorderBrushHeaderProperty);
        set => SetValue(BorderBrushHeaderProperty, value);
    }
    /// ForegroundHeader
    public static readonly DependencyProperty ForegroundHeaderProperty
        = DependencyProperty.Register(
            nameof(ForegroundHeader),
            typeof(SolidColorBrush),
            typeof(StswDataGrid),
            new PropertyMetadata(default(SolidColorBrush))
        );
    public SolidColorBrush ForegroundHeader
    {
        get => (SolidColorBrush)GetValue(ForegroundHeaderProperty);
        set => SetValue(ForegroundHeaderProperty, value);
    }
    #endregion
}

public class StswDataGridFiltersDataModel
{
    public Action? Clear { get; internal set; }
    public Action? Refresh { get; internal set; }
    public string? SqlFilter { get; internal set; }
    public List<(string name, object val)>? SqlParameters { get; internal set; }
}
