using System.Linq;
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

        /// Button: clear filters
        if (Columns[0] is StswDataGridSpecialColumn specialColumn)
            if (specialColumn.Header is UniformGrid grid)
                if (grid.Children?[1] is StswButton button)
                    button.Click += BtnClearFilters_Click;

        base.OnApplyTemplate();
    }

    /// BtnClearFilters_Click
    private void BtnClearFilters_Click(object sender, RoutedEventArgs e)
    {
        var extDict = new StswDictionary<string, StswFilterBindingData>();
        var bindingDatas = StswExtensions.FindVisualChildren<StswFilter>(this).Select(x => x.BindingData).ToList();
        for (int i = 0; i < bindingDatas.Count; i++)
            extDict.Add(i.ToString(), bindingDatas[i]);

        extDict.ClearColumnFilters();
    }
    #endregion

    #region Properties
    /// AreFiltersVisible
    public static readonly DependencyProperty AreFiltersVisibleProperty
        = DependencyProperty.Register(
            nameof(AreFiltersVisible),
            typeof(bool),
            typeof(StswDataGrid),
            new PropertyMetadata(true)
        );
    public bool AreFiltersVisible
    {
        get => (bool)GetValue(AreFiltersVisibleProperty);
        set => SetValue(AreFiltersVisibleProperty, value);
    }

    /// IsSpecialColumnHeaderVisible
    public static readonly DependencyProperty IsSpecialColumnHeaderVisibleProperty
        = DependencyProperty.Register(
            nameof(IsSpecialColumnHeaderVisible),
            typeof(bool),
            typeof(StswDataGrid)
        );
    public bool IsSpecialColumnHeaderVisible
    {
        get => (bool)GetValue(IsSpecialColumnHeaderVisibleProperty);
        set => SetValue(IsSpecialColumnHeaderVisibleProperty, value);
    }

    /// IsSpecialColumnVisible
    public static readonly DependencyProperty IsSpecialColumnVisibleProperty
        = DependencyProperty.Register(
            nameof(IsSpecialColumnVisible),
            typeof(bool),
            typeof(StswDataGrid),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsSpecialColumnVisibleChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public bool IsSpecialColumnVisible
    {
        get => (bool)GetValue(IsSpecialColumnVisibleProperty);
        set => SetValue(IsSpecialColumnVisibleProperty, value);
    }
    public static void OnIsSpecialColumnVisibleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswDataGrid stsw)
        {
            if ((bool)e.NewValue)
            {
                stsw.Columns.Insert(0, new StswDataGridSpecialColumn());
                if (stsw.IsLoaded)
                    ((StswButton)((UniformGrid)stsw.Columns[0].Header).Children[1]).Click += stsw.BtnClearFilters_Click;
                stsw.FrozenColumnCount++;
            }
            else if (stsw.Columns.Any(x => x is StswDataGridSpecialColumn))
            {
                stsw.FrozenColumnCount--;
                if (stsw.IsLoaded)
                    ((StswButton)((UniformGrid)stsw.Columns[0].Header).Children[1]).Click -= stsw.BtnClearFilters_Click;
                stsw.Columns.RemoveAt(0);
            }
            /*
            stsw.Columns[0].MinWidth = (bool)e.NewValue ? stsw.MinColumnWidth : 0;
            stsw.Columns[0].Width = (bool)e.NewValue ? DataGridLength.Auto : 0;
            stsw.Columns[0].CanUserResize = (bool)e.NewValue;
            */
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
