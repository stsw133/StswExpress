using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Interaction logic for StswDataGrid.xaml
/// </summary>
public partial class StswDataGrid : StswDataGridBase
{
    public StswDataGrid()
    {
        InitializeComponent();
    }
}

public class StswDataGridBase : DataGrid
{
    #region Style
    /// BackgroundHeader
    public static readonly DependencyProperty BackgroundHeaderProperty
        = DependencyProperty.Register(
            nameof(BackgroundHeader),
            typeof(Brush),
            typeof(StswDataGrid),
            new PropertyMetadata(default(SolidColorBrush))
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
            typeof(StswDataGrid),
            new PropertyMetadata(true)
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
            new FrameworkPropertyMetadata(true,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                IsSpecialColumnVisibleChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public bool IsSpecialColumnVisible
    {
        get => (bool)GetValue(IsSpecialColumnVisibleProperty);
        set => SetValue(IsSpecialColumnVisibleProperty, value);
    }
    public static void IsSpecialColumnVisibleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswDataGrid dtg)
            dtg.Columns[0].Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
    }

    /// BtnClearFilters_Click
    private void BtnClearFilters_Click(object sender, RoutedEventArgs e)
    {
        var extDict = new StswDictionary<string, StswColumnFilterData?>();
        var datas = StswExtensions.FindVisualChildren<StswColumnFilter>(this).Select(x => x.Data).ToList();
        for (int i = 0; i < datas.Count(); i++)
            extDict.Add(i.ToString(), datas[i]);
        extDict.ClearColumnFilters();
    }
}
