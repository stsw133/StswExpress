﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Interaction logic for Header.xaml
/// </summary>
public partial class Header : UserControl
{
    public Header()
    {
        InitializeComponent();
    }

    /// HorizontalIconAlignment
    public static readonly DependencyProperty HorizontalIconAlignmentProperty
        = DependencyProperty.Register(
              nameof(HorizontalIconAlignment),
              typeof(HorizontalAlignment),
              typeof(Header),
              new PropertyMetadata(default(HorizontalAlignment))
          );
    public HorizontalAlignment HorizontalIconAlignment
    {
        get => (HorizontalAlignment)GetValue(HorizontalIconAlignmentProperty);
        set => SetValue(HorizontalIconAlignmentProperty, value);
    }

    /// IconColor
    public static readonly DependencyProperty IconColorProperty
        = DependencyProperty.Register(
              nameof(IconColor),
              typeof(Brush),
              typeof(Header),
              new PropertyMetadata(default(Brushes))
          );
    public Brush IconColor
    {
        get => (Brush)GetValue(IconColorProperty);
        set => SetValue(IconColorProperty, value);
    }

    /// IconData
    public static readonly DependencyProperty IconDataProperty
        = DependencyProperty.Register(
              nameof(IconData),
              typeof(Geometry),
              typeof(Header),
              new PropertyMetadata(default(Geometry?))
          );
    public Geometry? IconData
    {
        get => (Geometry?)GetValue(IconDataProperty);
        set => SetValue(IconDataProperty, value);
    }

    /// IconScale
    public static readonly DependencyProperty IconScaleProperty
        = DependencyProperty.Register(
              nameof(IconScale),
              typeof(double),
              typeof(Header),
              new PropertyMetadata(1.5)
          );
    public double IconScale
    {
        get => (double)GetValue(IconScaleProperty);
        set => SetValue(IconScaleProperty, value);
    }

    /// IconSource
    public static readonly DependencyProperty IconSourceProperty
        = DependencyProperty.Register(
              nameof(IconSource),
              typeof(ImageSource),
              typeof(Header),
              new PropertyMetadata(default(ImageSource?))
          );
    public ImageSource? IconSource
    {
        get => (ImageSource?)GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    /// SubIconColor
    public static readonly DependencyProperty SubIconColorProperty
        = DependencyProperty.Register(
              nameof(SubIconColor),
              typeof(Brush),
              typeof(Header),
              new PropertyMetadata(default(Brushes))
          );
    public Brush SubIconColor
    {
        get => (Brush)GetValue(SubIconColorProperty);
        set => SetValue(SubIconColorProperty, value);
    }

    /// SubIconData
    public static readonly DependencyProperty SubIconDataProperty
        = DependencyProperty.Register(
              nameof(SubIconData),
              typeof(Geometry),
              typeof(Header),
              new PropertyMetadata(default(Geometry?))
          );
    public Geometry? SubIconData
    {
        get => (Geometry?)GetValue(SubIconDataProperty);
        set => SetValue(SubIconDataProperty, value);
    }

    /// SubIconSource
    public static readonly DependencyProperty SubIconSourceProperty
        = DependencyProperty.Register(
              nameof(SubIconSource),
              typeof(ImageSource),
              typeof(Header),
              new PropertyMetadata(default(ImageSource?))
          );
    public ImageSource? SubIconSource
    {
        get => (ImageSource?)GetValue(SubIconSourceProperty);
        set => SetValue(SubIconSourceProperty, value);
    }

    /// SubText1
    public static readonly DependencyProperty SubText1Property
        = DependencyProperty.Register(
              nameof(SubText1),
              typeof(string),
              typeof(Header),
              new PropertyMetadata(default(string))
          );
    public string SubText1
    {
        get => (string)GetValue(SubText1Property);
        set => SetValue(SubText1Property, value);
    }

    /// SubTextColor1
    public static readonly DependencyProperty SubTextColor1Property
        = DependencyProperty.Register(
              nameof(SubTextColor1),
              typeof(SolidColorBrush),
              typeof(Header),
              new PropertyMetadata(default(SolidColorBrush))
          );
    public SolidColorBrush SubTextColor1
    {
        get => (SolidColorBrush)GetValue(SubTextColor1Property);
        set => SetValue(SubTextColor1Property, value);
    }

    /// SubText2
    public static readonly DependencyProperty SubText2Property
        = DependencyProperty.Register(
              nameof(SubText2),
              typeof(string),
              typeof(Header),
              new PropertyMetadata(default(string))
          );
    public string SubText2
    {
        get => (string)GetValue(SubText2Property);
        set => SetValue(SubText2Property, value);
    }

    /// SubTextColor2
    public static readonly DependencyProperty SubTextColor2Property
        = DependencyProperty.Register(
              nameof(SubTextColor2),
              typeof(SolidColorBrush),
              typeof(Header),
              new PropertyMetadata(default(SolidColorBrush))
          );
    public SolidColorBrush SubTextColor2
    {
        get => (SolidColorBrush)GetValue(SubTextColor2Property);
        set => SetValue(SubTextColor2Property, value);
    }

    /// Text
    public static readonly DependencyProperty TextProperty
        = DependencyProperty.Register(
              nameof(Text),
              typeof(string),
              typeof(Header),
              new PropertyMetadata(default(string))
          );
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// VerticalIconAlignment
    public static readonly DependencyProperty VerticalIconAlignmentProperty
        = DependencyProperty.Register(
              nameof(VerticalIconAlignment),
              typeof(VerticalAlignment),
              typeof(Header),
              new PropertyMetadata(default(VerticalAlignment))
          );
    public VerticalAlignment VerticalIconAlignment
    {
        get => (VerticalAlignment)GetValue(VerticalIconAlignmentProperty);
        set => SetValue(VerticalIconAlignmentProperty, value);
    }
}
