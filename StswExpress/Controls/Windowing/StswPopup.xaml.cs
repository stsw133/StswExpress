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
    public static readonly DependencyProperty BackgroundProperty
        = DependencyProperty.RegisterAttached(
            nameof(Control.Background),
            typeof(Brush),
            typeof(StswPopup),
            new PropertyMetadata(default, OnBackgroundChanged)
        );
    public static Brush GetBackground(DependencyObject obj) => (Brush)obj.GetValue(BackgroundProperty);
    public static void SetBackground(DependencyObject obj, Brush value) => obj.SetValue(BackgroundProperty, value);
    private static void OnBackgroundChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is IStswDropControl stsw)
        {
            if (stsw.GetPopup() is StswPopup popup)
                SetBackground(popup, (Brush)e.NewValue);
        }
    }

    /// <summary>
    /// Gets or sets the border brush for the popup.
    /// </summary>
    public static readonly DependencyProperty BorderBrushProperty
        = DependencyProperty.RegisterAttached(
            nameof(Control.BorderBrush),
            typeof(Brush),
            typeof(StswPopup),
            new PropertyMetadata(default, OnBorderBrushChanged)
        );
    public static Brush GetBorderBrush(DependencyObject obj) => (Brush)obj.GetValue(BorderBrushProperty);
    public static void SetBorderBrush(DependencyObject obj, Brush value) => obj.SetValue(BorderBrushProperty, value);
    private static void OnBorderBrushChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is IStswDropControl stsw)
        {
            if (stsw.GetPopup() is StswPopup popup)
                SetBorderBrush(popup, (Brush)e.NewValue);
        }
    }

    /// <summary>
    /// Gets or sets the thickness of the border for the popup.
    /// </summary>
    public static readonly DependencyProperty BorderThicknessProperty
        = DependencyProperty.RegisterAttached(
            nameof(Control.BorderThickness),
            typeof(Thickness),
            typeof(StswPopup),
            new PropertyMetadata(new Thickness(2), OnBorderThicknessChanged)
        );
    public static Thickness GetBorderThickness(DependencyObject obj) => (Thickness)obj.GetValue(BorderThicknessProperty);
    public static void SetBorderThickness(DependencyObject obj, Thickness value) => obj.SetValue(BorderThicknessProperty, value);
    private static void OnBorderThicknessChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is IStswDropControl stsw)
        {
            if (stsw.GetPopup() is StswPopup popup)
                SetBorderThickness(popup, (Thickness)e.NewValue);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether corner clipping is enabled for the popup.
    /// </summary>
    public static readonly DependencyProperty CornerClippingProperty
        = DependencyProperty.RegisterAttached(
            nameof(IStswCornerControl.CornerClipping),
            typeof(bool),
            typeof(StswPopup),
            new PropertyMetadata(true, OnCornerClippingChanged)
        );
    public static bool GetCornerClipping(DependencyObject obj) => (bool)obj.GetValue(CornerClippingProperty);
    public static void SetCornerClipping(DependencyObject obj, bool value) => obj.SetValue(CornerClippingProperty, value);
    private static void OnCornerClippingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is IStswDropControl stsw)
        {
            if (stsw.GetPopup() is StswPopup popup)
                SetCornerClipping(popup, (bool)e.NewValue);
        }
    }

    /// <summary>
    /// Gets or sets the corner radius for the popup.
    /// </summary>
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.RegisterAttached(
            nameof(IStswCornerControl.CornerRadius),
            typeof(CornerRadius),
            typeof(StswPopup),
            new PropertyMetadata(new CornerRadius(10), OnCornerRadiusChanged)
        );
    public static CornerRadius GetCornerRadius(DependencyObject obj) => (CornerRadius)obj.GetValue(CornerRadiusProperty);
    public static void SetCornerRadius(DependencyObject obj, CornerRadius value) => obj.SetValue(CornerRadiusProperty, value);
    private static void OnCornerRadiusChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is IStswDropControl stsw)
        {
            if (stsw.GetPopup() is StswPopup popup)
                SetCornerRadius(popup, (CornerRadius)e.NewValue);
        }
    }

    /// <summary>
    /// Gets or sets the padding for the popup.
    /// </summary>
    public static readonly DependencyProperty PaddingProperty
        = DependencyProperty.RegisterAttached(
            nameof(Control.Padding),
            typeof(Thickness),
            typeof(StswPopup),
            new PropertyMetadata(new Thickness(0), OnPaddingChanged)
        );
    public static Thickness GetPadding(DependencyObject obj) => (Thickness)obj.GetValue(PaddingProperty);
    public static void SetPadding(DependencyObject obj, Thickness value) => obj.SetValue(PaddingProperty, value);
    private static void OnPaddingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is IStswDropControl stsw)
        {
            if (stsw.GetPopup() is StswPopup popup)
                SetPadding(popup, (Thickness)e.NewValue);
        }
    }
    #endregion
}
