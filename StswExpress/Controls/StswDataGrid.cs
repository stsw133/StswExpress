﻿using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace StswExpress;

public class StswDataGrid : DataGrid
{
    static StswDataGrid()
    {
        //DefaultStyleKeyProperty.OverrideMetadata(typeof(StswDataGrid), new FrameworkPropertyMetadata(typeof(StswDataGrid)));
    }

    #region Events
    /// OnApplyTemplate
    public override void OnApplyTemplate()
    {
        ColumnHeaderStyle = (Style)FindResource("StswColumnHeaderStyle");

        base.OnApplyTemplate();
    }

    /// BtnClearFilters_Click
    private void BtnClearFilters_Click(object sender, RoutedEventArgs e)
    {
        var extDict = new StswDictionary<string, StswFilterBindingData>();
        var bindingDatas = StswFn.FindVisualChildren<StswFilter>(this).Select(x => x.BindingData).ToList();
        for (int i = 0; i < bindingDatas.Count; i++)
            extDict.Add(i.ToString(), bindingDatas[i]);

        StswFilter.ClearColumnFilters(extDict);
    }
    #endregion

    #region Properties
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
            var specialColumnHeader = stsw.FindResource("StswDataGridSpecialColumnHeader") as UniformGrid;
            var specialColumnTemplate = stsw.FindResource("StswDataGridSpecialColumnCellTemplate") as DataTemplate;
            var specialColumn = stsw.Columns.FirstOrDefault(x => x is DataGridTemplateColumn specialColumn && specialColumn.CellTemplate == specialColumnTemplate);

            if (stsw.SpecialColumnVisibility != SpecialColumnVisibilities.Collapsed)
            {
                if (specialColumn == null)
                {
                    /// create special column
                    stsw.Columns.Insert(0, new DataGridTemplateColumn() { CellTemplate = specialColumnTemplate, Header = specialColumnHeader });
                    if (stsw.Columns[0].Header is UniformGrid grid && grid.Children[1] is StswButton button)
                        button.Click += stsw.BtnClearFilters_Click;
                    stsw.FrozenColumnCount++;

                    specialColumn = stsw.Columns[0];
                }

                /// set visibility for header
                if (specialColumn?.Header is not null and UniformGrid header)
                    header.Visibility = stsw.SpecialColumnVisibility == SpecialColumnVisibilities.All ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (specialColumn != null)
            {
                /// remove special column
                stsw.FrozenColumnCount--;
                if (stsw.Columns[0].Header is UniformGrid grid && grid.Children[1] is StswButton button)
                    button.Click -= stsw.BtnClearFilters_Click;
                stsw.Columns.Remove(specialColumn);
            }
        }
    }
    /*
    /// RefreshCommand
    public static readonly DependencyProperty RefreshCommandProperty
        = DependencyProperty.Register(
            nameof(RefreshCommand),
            typeof(ICommand),
            typeof(StswDataGrid),
            new PropertyMetadata(default(ICommand))
        );
    public ICommand RefreshCommand
    {
        get => (ICommand)GetValue(RefreshCommandProperty);
        set => SetValue(RefreshCommandProperty, value);
    }
    */
    #endregion

    #region Style
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
