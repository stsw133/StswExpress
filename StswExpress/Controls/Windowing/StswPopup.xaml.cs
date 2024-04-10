using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Represents a custom popup control with additional functionality and customization options.
/// </summary>
[ContentProperty(nameof(Content))]
public class StswPopup : Popup, /*IStswCornerControl,*/ IStswScrollableControl
{
    public StswPopup()
    {
        Child ??= new ContentControl()
        {
            ContentTemplate = (DataTemplate)FindResource("StswPopupChildTemplate")
        };
    }
    static StswPopup()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswPopup), new FrameworkPropertyMetadata(typeof(StswPopup)));
    }

    #region Events & methods
    /// <summary>
    /// Gets a <see cref="StswScrollViewer"/> of the control.
    /// </summary>
    public StswScrollViewer GetScrollViewer() => (StswScrollViewer)GetTemplateChild("PART_ScrollViewer");
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the data used to generate the child elements of this control.
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
            new PropertyMetadata(default)
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
            new PropertyMetadata(default)
        );
    public static Brush GetBorderBrush(DependencyObject obj) => (Brush)obj.GetValue(BorderBrushProperty);
    public static void SetBorderBrush(DependencyObject obj, Brush value) => obj.SetValue(BorderBrushProperty, value);

    /// <summary>
    /// Gets or sets the thickness of the border for the popup.
    /// </summary>
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
            new PropertyMetadata(new Thickness(2))
        );
    public static Thickness GetBorderThickness(DependencyObject obj) => (Thickness)obj.GetValue(BorderThicknessProperty);
    public static void SetBorderThickness(DependencyObject obj, Thickness value) => obj.SetValue(BorderThicknessProperty, value);

    /// <summary>
    /// Gets or sets a value indicating whether corner clipping is enabled for the popup.
    /// </summary>
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
            new PropertyMetadata(true)
        );
    public static bool GetCornerClipping(DependencyObject obj) => (bool)obj.GetValue(CornerClippingProperty);
    public static void SetCornerClipping(DependencyObject obj, bool value) => obj.SetValue(CornerClippingProperty, value);

    /// <summary>
    /// Gets or sets the corner radius for the popup.
    /// </summary>
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
            new PropertyMetadata(new CornerRadius(10))
        );
    public static CornerRadius GetCornerRadius(DependencyObject obj) => (CornerRadius)obj.GetValue(CornerRadiusProperty);
    public static void SetCornerRadius(DependencyObject obj, CornerRadius value) => obj.SetValue(CornerRadiusProperty, value);

    /// <summary>
    /// Gets or sets the padding for the popup.
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
            new PropertyMetadata(new Thickness(0))
        );
    public static Thickness GetPadding(DependencyObject obj) => (Thickness)obj.GetValue(PaddingProperty);
    public static void SetPadding(DependencyObject obj, Thickness value) => obj.SetValue(PaddingProperty, value);
    #endregion
}
