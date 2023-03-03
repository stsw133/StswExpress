﻿using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Interaction logic for StswHeader.xaml
/// </summary>
public partial class StswHeader : UserControl
{
    public StswHeader()
    {
        InitializeComponent();

        SetValue(SubTextsProperty, new ObservableCollection<TextBlock>());
    }
    static StswHeader()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswHeader), new FrameworkPropertyMetadata(typeof(StswHeader)));
    }

    #region Properties
    /// HideText
    public static readonly DependencyProperty HideTextProperty
        = DependencyProperty.Register(
            nameof(HideText),
            typeof(bool),
            typeof(StswHeader)
        );
    public bool HideText
    {
        get => (bool)GetValue(HideTextProperty);
        set => SetValue(HideTextProperty, value);
    }

    /// IconData
    public static readonly DependencyProperty IconDataProperty
        = DependencyProperty.Register(
            nameof(IconData),
            typeof(Geometry),
            typeof(StswHeader)
        );
    public Geometry? IconData
    {
        get => (Geometry?)GetValue(IconDataProperty);
        set => SetValue(IconDataProperty, value);
    }
    /// IconForeground
    public static readonly DependencyProperty IconForegroundProperty
        = DependencyProperty.Register(
            nameof(IconForeground),
            typeof(Brush),
            typeof(StswHeader)
        );
    public Brush IconForeground
    {
        get => (Brush)GetValue(IconForegroundProperty);
        set => SetValue(IconForegroundProperty, value);
    }
    /// IconScale
    public static readonly DependencyProperty IconScaleProperty
        = DependencyProperty.Register(
            nameof(IconScale),
            typeof(GridLength?),
            typeof(StswHeader)
        );
    public GridLength? IconScale
    {
        get => (GridLength?)GetValue(IconScaleProperty);
        set => SetValue(IconScaleProperty, value);
    }
    /// IconSource
    public static readonly DependencyProperty IconSourceProperty
        = DependencyProperty.Register(
            nameof(IconSource),
            typeof(ImageSource),
            typeof(StswHeader)
        );
    public ImageSource? IconSource
    {
        get => (ImageSource?)GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    /// IsBusy
    public static readonly DependencyProperty IsBusyProperty
        = DependencyProperty.Register(
            nameof(IsBusy),
            typeof(bool),
            typeof(StswHeader)
        );
    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }

    /// Orientation
    public static readonly DependencyProperty OrientationProperty
        = DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(StswHeader)
        );
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// SubTexts
    public static readonly DependencyProperty SubTextsProperty
        = DependencyProperty.Register(
            nameof(SubTexts),
            typeof(ObservableCollection<TextBlock>),
            typeof(StswHeader)
        );
    public ObservableCollection<TextBlock> SubTexts
    {
        get => (ObservableCollection<TextBlock>)GetValue(SubTextsProperty);
        set => SetValue(SubTextsProperty, value);
    }

    /// TextMargin
    public static readonly DependencyProperty TextMarginProperty
        = DependencyProperty.Register(
            nameof(TextMargin),
            typeof(Thickness),
            typeof(StswHeader)
        );
    public Thickness TextMargin
    {
        get => (Thickness)GetValue(TextMarginProperty);
        set => SetValue(TextMarginProperty, value);
    }
    #endregion

    #region Style
    /// ForegroundDisabled
    public static readonly DependencyProperty ForegroundDisabledProperty
        = DependencyProperty.Register(
            nameof(ForegroundDisabled),
            typeof(Brush),
            typeof(StswHeader)
        );
    public Brush ForegroundDisabled
    {
        get => (Brush)GetValue(ForegroundDisabledProperty);
        set => SetValue(ForegroundDisabledProperty, value);
    }
    #endregion
}
