using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Interaction logic for StswDataGrid.xaml
/// </summary>
public partial class StswDataGrid : DataGrid
{
    public StswDataGrid()
    {
        InitializeComponent();
    }

    /// <summary>
    /// HeaderBackground
    /// </summary>
    public static readonly DependencyProperty HeaderBackgroundProperty
        = DependencyProperty.Register(
              nameof(HeaderBackground),
              typeof(Brush),
              typeof(StswDataGrid),
              new PropertyMetadata(default(SolidColorBrush))
          );
    public Brush HeaderBackground
    {
        get => (Brush)GetValue(HeaderBackgroundProperty);
        set => SetValue(HeaderBackgroundProperty, value);
    }

    /// <summary>
    /// HeaderBorderBrush
    /// </summary>
    public static readonly DependencyProperty HeaderBorderBrushProperty
        = DependencyProperty.Register(
              nameof(HeaderBorderBrush),
              typeof(SolidColorBrush),
              typeof(StswDataGrid),
              new PropertyMetadata(default(SolidColorBrush))
          );
    public SolidColorBrush HeaderBorderBrush
    {
        get => (SolidColorBrush)GetValue(HeaderBorderBrushProperty);
        set => SetValue(HeaderBorderBrushProperty, value);
    }

    /// <summary>
    /// HeaderForeground
    /// </summary>
    public static readonly DependencyProperty HeaderForegroundProperty
        = DependencyProperty.Register(
              nameof(HeaderForeground),
              typeof(SolidColorBrush),
              typeof(StswDataGrid),
              new PropertyMetadata(default(SolidColorBrush))
          );
    public SolidColorBrush HeaderForeground
    {
        get => (SolidColorBrush)GetValue(HeaderForegroundProperty);
        set => SetValue(HeaderForegroundProperty, value);
    }
}

/// <summary>
/// OBSOLETE
/// </summary>
public class SetMinWidthToAutoAttachedBehaviour
{
    public static bool GetSetMinWidthToAuto(DependencyObject obj) => (bool)obj.GetValue(SetMinWidthToAutoProperty);
    public static void SetSetMinWidthToAuto(DependencyObject obj, bool value) => obj.SetValue(SetMinWidthToAutoProperty, value);

    /// Using a DependencyProperty as the backing store for SetMinWidthToAuto. This enables animation, styling, binding, etc...
    public static readonly DependencyProperty SetMinWidthToAutoProperty =
        DependencyProperty.RegisterAttached(
            nameof(SetSetMinWidthToAuto),
            typeof(bool),
            typeof(SetMinWidthToAutoAttachedBehaviour),
            new UIPropertyMetadata(false, WireUpLoadedEvent)
        );

    public static void WireUpLoadedEvent(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if ((bool)e.NewValue)
            ((DataGrid)d).Loaded += SetMinWidths;
    }

    public static void SetMinWidths(object source, EventArgs e)
    {
        foreach (var column in ((DataGrid)source).Columns)
        {
            column.MinWidth = column.ActualWidth;
            column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
        }
    }
}

