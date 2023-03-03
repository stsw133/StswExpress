﻿using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Interaction logic for StswButton.xaml
/// </summary>
public partial class StswButton : Button
{
    public StswButton()
    {
        InitializeComponent();
    }
    static StswButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswButton), new FrameworkPropertyMetadata(typeof(StswButton)));
    }

    #region Events
    /// StswButtonHyperlink_Click
    private void StswButtonHyperlink_Click(object sender, RoutedEventArgs e)
    {
        if (sender is StswButton stsw && !string.IsNullOrEmpty(stsw.Hyperlink))
        {
            var url = stsw.Hyperlink.Replace("&", "^&");
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
    }
    #endregion

    #region Properties
    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswButton)
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// Hyperlink
    public static readonly DependencyProperty HyperlinkProperty
        = DependencyProperty.Register(
            nameof(Hyperlink),
            typeof(string),
            typeof(StswButton),
            new FrameworkPropertyMetadata(default(string?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnHyperlinkChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public string? Hyperlink
    {
        get => (string?)GetValue(HyperlinkProperty);
        set => SetValue(HyperlinkProperty, value);
    }
    public static void OnHyperlinkChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswButton stsw)
        {
            if (e.OldValue != null)
                stsw.Click -= stsw.StswButtonHyperlink_Click;
            if (e.NewValue != null)
                stsw.Click += stsw.StswButtonHyperlink_Click;
        }
    }
    #endregion

    #region Style
    /// BackgroundDisabled
    public static readonly DependencyProperty BackgroundDisabledProperty
        = DependencyProperty.Register(
            nameof(BackgroundDisabled),
            typeof(Brush),
            typeof(StswButton)
        );
    public Brush BackgroundDisabled
    {
        get => (Brush)GetValue(BackgroundDisabledProperty);
        set => SetValue(BackgroundDisabledProperty, value);
    }
    /// BorderBrushDisabled
    public static readonly DependencyProperty BorderBrushDisabledProperty
        = DependencyProperty.Register(
            nameof(BorderBrushDisabled),
            typeof(Brush),
            typeof(StswButton)
        );
    public Brush BorderBrushDisabled
    {
        get => (Brush)GetValue(BorderBrushDisabledProperty);
        set => SetValue(BorderBrushDisabledProperty, value);
    }
    /// ForegroundDisabled
    public static readonly DependencyProperty ForegroundDisabledProperty
        = DependencyProperty.Register(
            nameof(ForegroundDisabled),
            typeof(Brush),
            typeof(StswButton)
        );
    public Brush ForegroundDisabled
    {
        get => (Brush)GetValue(ForegroundDisabledProperty);
        set => SetValue(ForegroundDisabledProperty, value);
    }

    /// BackgroundMouseOver
    public static readonly DependencyProperty BackgroundMouseOverProperty
        = DependencyProperty.Register(
            nameof(BackgroundMouseOver),
            typeof(Brush),
            typeof(StswButton)
        );
    public Brush BackgroundMouseOver
    {
        get => (Brush)GetValue(BackgroundMouseOverProperty);
        set => SetValue(BackgroundMouseOverProperty, value);
    }
    /// BorderBrushMouseOver
    public static readonly DependencyProperty BorderBrushMouseOverProperty
        = DependencyProperty.Register(
            nameof(BorderBrushMouseOver),
            typeof(Brush),
            typeof(StswButton)
        );
    public Brush BorderBrushMouseOver
    {
        get => (Brush)GetValue(BorderBrushMouseOverProperty);
        set => SetValue(BorderBrushMouseOverProperty, value);
    }
    /// ForegroundMouseOver
    public static readonly DependencyProperty ForegroundMouseOverProperty
        = DependencyProperty.Register(
            nameof(ForegroundMouseOver),
            typeof(Brush),
            typeof(StswButton)
        );
    public Brush ForegroundMouseOver
    {
        get => (Brush)GetValue(ForegroundMouseOverProperty);
        set => SetValue(ForegroundMouseOverProperty, value);
    }

    /// BackgroundPressed
    public static readonly DependencyProperty BackgroundPressedProperty
        = DependencyProperty.Register(
            nameof(BackgroundPressed),
            typeof(Brush),
            typeof(StswButton)
        );
    public Brush BackgroundPressed
    {
        get => (Brush)GetValue(BackgroundPressedProperty);
        set => SetValue(BackgroundPressedProperty, value);
    }
    /// BorderBrushPressed
    public static readonly DependencyProperty BorderBrushPressedProperty
        = DependencyProperty.Register(
            nameof(BorderBrushPressed),
            typeof(Brush),
            typeof(StswButton)
        );
    public Brush BorderBrushPressed
    {
        get => (Brush)GetValue(BorderBrushPressedProperty);
        set => SetValue(BorderBrushPressedProperty, value);
    }
    /// ForegroundPressed
    public static readonly DependencyProperty ForegroundPressedProperty
        = DependencyProperty.Register(
            nameof(ForegroundPressed),
            typeof(Brush),
            typeof(StswButton)
        );
    public Brush ForegroundPressed
    {
        get => (Brush)GetValue(ForegroundPressedProperty);
        set => SetValue(ForegroundPressedProperty, value);
    }

    /// BorderBrushDefaulted
    public static readonly DependencyProperty BorderBrushDefaultedProperty
        = DependencyProperty.Register(
            nameof(BorderBrushDefaulted),
            typeof(Brush),
            typeof(StswButton)
        );
    public Brush BorderBrushDefaulted
    {
        get => (Brush)GetValue(BorderBrushDefaultedProperty);
        set => SetValue(BorderBrushDefaultedProperty, value);
    }
    #endregion
}
