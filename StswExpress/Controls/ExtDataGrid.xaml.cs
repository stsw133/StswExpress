﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Interaction logic for ExtDataGrid.xaml
/// </summary>
public partial class ExtDataGrid : DataGrid
{
    public ExtDataGrid()
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
              typeof(ExtDataGrid),
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
              typeof(ExtDataGrid),
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
              typeof(ExtDataGrid),
              new PropertyMetadata(default(SolidColorBrush))
          );
    public SolidColorBrush HeaderForeground
    {
        get => (SolidColorBrush)GetValue(HeaderForegroundProperty);
        set => SetValue(HeaderForegroundProperty, value);
    }

    /// ID
    public static readonly DependencyProperty IDProperty
        = DependencyProperty.Register(
              nameof(ID),
              typeof(int),
              typeof(ExtDataGrid),
              new PropertyMetadata(default(int))
          );
    public int ID
    {
        get => (int)GetValue(IDProperty);
        set => SetValue(IDProperty, value);
    }
}

public class SetMinWidthToAutoAttachedBehaviour
{
    public static bool GetSetMinWidthToAuto(DependencyObject obj)
    {
        return (bool)obj.GetValue(SetMinWidthToAutoProperty);
    }

    public static void SetSetMinWidthToAuto(DependencyObject obj, bool value)
    {
        obj.SetValue(SetMinWidthToAutoProperty, value);
    }

    // Using a DependencyProperty as the backing store for SetMinWidthToAuto.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty SetMinWidthToAutoProperty =
        DependencyProperty.RegisterAttached("SetMinWidthToAuto", typeof(bool), typeof(SetMinWidthToAutoAttachedBehaviour), new UIPropertyMetadata(false, WireUpLoadedEvent));

    public static void WireUpLoadedEvent(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var grid = (DataGrid)d;

        var doIt = (bool)e.NewValue;

        if (doIt)
        {
            grid.Loaded += SetMinWidths;
        }
    }

    public static void SetMinWidths(object source, EventArgs e)
    {
        var grid = (DataGrid)source;

        foreach (var column in grid.Columns)
        {
            column.MinWidth = column.ActualWidth;
            column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
        }
    }
}

