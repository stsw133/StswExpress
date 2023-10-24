﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Represents a custom window control with additional functionality and customization options.
/// </summary>
[ContentProperty(nameof(Content))]
public class StswPopup : Popup
{
    public StswPopup()
    {
        Child = new ContentControl()
        {
            ContentTemplate = (DataTemplate)FindResource("StswPopupChildTemplate")
        };
    }
    static StswPopup()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswPopup), new FrameworkPropertyMetadata(typeof(StswPopup)));
    }

    #region Main properties
    /// <summary>
    /// 
    /// </summary>
    public object? Content
    {
        get => (object?)GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }
    public static readonly DependencyProperty ContentProperty
        = DependencyProperty.Register(
            nameof(Content),
            typeof(object),
            typeof(StswPopup)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// 
    /// </summary>
    public Brush Background
    {
        get => (Brush)GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }
    public static readonly DependencyProperty BackgroundProperty
        = DependencyProperty.Register(
            nameof(Background),
            typeof(Brush),
            typeof(StswPopup)
        );

    /// <summary>
    /// 
    /// </summary>
    public Brush BorderBrush
    {
        get => (Brush)GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }
    public static readonly DependencyProperty BorderBrushProperty
        = DependencyProperty.Register(
            nameof(BorderBrush),
            typeof(Brush),
            typeof(StswPopup)
        );

    /// <summary>
    /// 
    /// </summary>
    public Thickness BorderThickness
    {
        get => (Thickness)GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }
    public static readonly DependencyProperty BorderThicknessProperty
        = DependencyProperty.Register(
            nameof(BorderThickness),
            typeof(Thickness),
            typeof(StswPopup)
        );

    /// <summary>
    /// Gets or sets the corner radius of the custom window.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswPopup)
        );


    /// <summary>
    /// 
    /// </summary>
    public Thickness Padding
    {
        get => (Thickness)GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }
    public static readonly DependencyProperty PaddingProperty
        = DependencyProperty.Register(
            nameof(Padding),
            typeof(Thickness),
            typeof(StswPopup)
        );
    #endregion
}