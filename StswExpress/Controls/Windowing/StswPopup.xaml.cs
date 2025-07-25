﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// A customizable popup control that supports content, corner radius customization, and different scrolling behaviors.
/// Allows flexible placement and styling options, including animations and background customization.
/// </summary>
/// <remarks>
/// The popup adapts to different scroll types and supports dynamic content updates.
/// </remarks>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswPopup IsOpen="True" CornerRadius="8" ScrollType="ScrollView"&gt;
///     &lt;TextBlock Text="This is a popup message"/&gt;
/// &lt;/se:StswPopup&gt;
/// </code>
/// </example>
[ContentProperty(nameof(Content))]
[StswInfo("0.2.0")]
public class StswPopup : Popup, IStswCornerControl
{
    public StswPopup()
    {
        Init();
    }
    static StswPopup()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswPopup), new FrameworkPropertyMetadata(typeof(StswPopup)));
    }

    #region Events & methods
    /// <summary>
    /// Initializes the popup's child content based on the selected <see cref="ScrollType"/>.
    /// </summary>
    [StswInfo("0.8.0")]
    private void Init()
    {
        Child = new ContentControl
        {
            ContentTemplate = ScrollType switch
            {
                StswScrollType.DirectionView => (DataTemplate)FindResource("StswPopupDirectionViewTemplate"),
                StswScrollType.ScrollView => (DataTemplate)FindResource("StswPopupScrollViewTemplate"),
                _ => throw new System.NotImplementedException()
            }
        };
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the content displayed inside the popup.
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

    /// <summary>
    /// Gets or sets the type of scroll viewer used within the popup content.
    /// Determines whether a directional or standard scroll view is used.
    /// </summary>
    [StswInfo("0.8.0")]
    public StswScrollType ScrollType
    {
        get => (StswScrollType)GetValue(ScrollTypeProperty);
        set => SetValue(ScrollTypeProperty, value);
    }
    public static readonly DependencyProperty ScrollTypeProperty
        = DependencyProperty.Register(
            nameof(ScrollType),
            typeof(StswScrollType),
            typeof(StswPopup),
            new FrameworkPropertyMetadata(StswScrollType.ScrollView,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnScrollTypeChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnScrollTypeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswPopup stsw)
            return;

        stsw.Init();
    }
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the background brush for the popup.
    /// </summary>
    public Brush Background
    {
        get => (Brush)GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }
    public static readonly DependencyProperty BackgroundProperty
        = DependencyProperty.RegisterAttached(
            nameof(Background),
            typeof(Brush),
            typeof(StswPopup),
            new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    public static Brush GetBackground(DependencyObject obj) => (Brush)obj.GetValue(BackgroundProperty);
    public static void SetBackground(DependencyObject obj, Brush value) => obj.SetValue(BackgroundProperty, value);

    /// <summary>
    /// Gets or sets the border brush for the popup.
    /// </summary>
    public Brush BorderBrush
    {
        get => (Brush)GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }
    public static readonly DependencyProperty BorderBrushProperty
        = DependencyProperty.RegisterAttached(
            nameof(BorderBrush),
            typeof(Brush),
            typeof(StswPopup),
            new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    public static Brush GetBorderBrush(DependencyObject obj) => (Brush)obj.GetValue(BorderBrushProperty);
    public static void SetBorderBrush(DependencyObject obj, Brush value) => obj.SetValue(BorderBrushProperty, value);

    /// <inheritdoc/>
    public Thickness BorderThickness
    {
        get => (Thickness)GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }
    public static readonly DependencyProperty BorderThicknessProperty
        = DependencyProperty.RegisterAttached(
            nameof(BorderThickness),
            typeof(Thickness),
            typeof(StswPopup),
            new FrameworkPropertyMetadata(new Thickness(2), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    public static Thickness GetBorderThickness(DependencyObject obj) => (Thickness)obj.GetValue(BorderThicknessProperty);
    public static void SetBorderThickness(DependencyObject obj, Thickness value) => obj.SetValue(BorderThicknessProperty, value);

    /// <inheritdoc/>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty
        = DependencyProperty.RegisterAttached(
            nameof(CornerClipping),
            typeof(bool),
            typeof(StswPopup),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender)
        );
    public static bool GetCornerClipping(DependencyObject obj) => (bool)obj.GetValue(CornerClippingProperty);
    public static void SetCornerClipping(DependencyObject obj, bool value) => obj.SetValue(CornerClippingProperty, value);

    /// <inheritdoc/>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.RegisterAttached(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswPopup),
            new FrameworkPropertyMetadata(new CornerRadius(6), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    public static CornerRadius GetCornerRadius(DependencyObject obj) => (CornerRadius)obj.GetValue(CornerRadiusProperty);
    public static void SetCornerRadius(DependencyObject obj, CornerRadius value) => obj.SetValue(CornerRadiusProperty, value);

    /// <summary>
    /// Gets or sets the padding inside the popup, defining the spacing between its border and content.
    /// </summary>
    public Thickness Padding
    {
        get => (Thickness)GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }
    public static readonly DependencyProperty PaddingProperty
        = DependencyProperty.RegisterAttached(
            nameof(Padding),
            typeof(Thickness),
            typeof(StswPopup),
            new FrameworkPropertyMetadata(new Thickness(0), FrameworkPropertyMetadataOptions.AffectsMeasure)
        );
    public static Thickness GetPadding(DependencyObject obj) => (Thickness)obj.GetValue(PaddingProperty);
    public static void SetPadding(DependencyObject obj, Thickness value) => obj.SetValue(PaddingProperty, value);
    #endregion
}
