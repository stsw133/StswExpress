﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Interaction logic for CanvasIcon.xaml
/// </summary>
public partial class CanvasIcon : Viewbox
{
    public CanvasIcon()
    {
        InitializeComponent();
    }

    /// CanvasSize
    public static readonly DependencyProperty CanvasSizeProperty
        = DependencyProperty.Register(
              nameof(CanvasSize),
              typeof(double),
              typeof(CanvasIcon),
              new PropertyMetadata(24d)
          );
    public double CanvasSize
    {
        get => (double)GetValue(CanvasSizeProperty);
        set => SetValue(CanvasSizeProperty, value);
    }

    /// Color
    public static readonly DependencyProperty ColorProperty
        = DependencyProperty.Register(
              nameof(Color),
              typeof(Brush),
              typeof(CanvasIcon),
              new PropertyMetadata(default(Brushes))
          );
    public Brush Color
    {
        get => (Brush)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// Data
    public static readonly DependencyProperty DataProperty
        = DependencyProperty.Register(
              nameof(Data),
              typeof(Geometry),
              typeof(CanvasIcon),
              new PropertyMetadata(default(Geometry?))
          );
    public Geometry? Data
    {
        get => (Geometry?)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    /// Scale
    public static readonly DependencyProperty ScaleProperty
        = DependencyProperty.Register(
              nameof(Scale),
              typeof(double),
              typeof(CanvasIcon),
              new PropertyMetadata(1.5)
          );
    public double Scale
    {
        get => (double)GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }
}