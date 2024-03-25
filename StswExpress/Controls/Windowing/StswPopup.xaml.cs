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
public class StswPopup : Popup, IStswCornerControl, IStswScrollableControl
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
    /// Gets or sets the background brush for the control.
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
    /// Gets or sets the border brush for the control.
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
    /// Gets or sets the border thickness for the control.
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
    /// Gets or sets a value indicating whether corner clipping is enabled for the control.
    /// When set to <see langword="true"/>, content within the control's border area is clipped to match
    /// the border's rounded corners, preventing elements from protruding beyond the border.
    /// </summary>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty
        = DependencyProperty.Register(
            nameof(CornerClipping),
            typeof(bool),
            typeof(StswPopup)
        );

    /// <summary>
    /// Gets or sets the degree to which the corners of the control's border are rounded by defining
    /// a radius value for each corner independently. This property allows users to control the roundness
    /// of corners, and large radius values are smoothly scaled to blend from corner to corner.
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
    /// Gets or sets the padding for the control.
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

    /// <summary>
    /// Gets or sets the data model for properties of the scroll viewer associated with the control.
    /// The <see cref="StswScrollViewerModel"/> class provides customization options for the appearance and behavior of the scroll viewer.
    /// </summary>
    public StswScrollViewerModel ScrollViewer
    {
        get => (StswScrollViewerModel)GetValue(ScrollViewerProperty);
        set => SetValue(ScrollViewerProperty, value);
    }
    public static readonly DependencyProperty ScrollViewerProperty
        = DependencyProperty.Register(
            nameof(ScrollViewer),
            typeof(StswScrollViewerModel),
            typeof(StswPopup)
        );
    #endregion
}

/// <summary>
/// Data model for properties of <see cref="StswPopup"/> that is included in a control.
/// </summary>
public class StswPopupModel : DependencyObject, IStswCornerControl
{
    /// <summary>
    /// Gets or sets the background brush for the popup.
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
            typeof(StswPopupModel)
        );

    /// <summary>
    /// Gets or sets the border brush for the popup.
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
            typeof(StswPopupModel)
        );

    /// <summary>
    /// Gets or sets the thickness of the border for the popup.
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
            typeof(StswPopupModel),
            new PropertyMetadata(new Thickness(2))
        );

    /// <summary>
    /// Gets or sets a value indicating whether corner clipping is enabled for the popup.
    /// </summary>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty
        = DependencyProperty.Register(
            nameof(CornerClipping),
            typeof(bool),
            typeof(StswPopupModel),
            new PropertyMetadata(true)
        );

    /// <summary>
    /// Gets or sets the corner radius for the popup.
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
            typeof(StswPopupModel),
            new PropertyMetadata(new CornerRadius(10))
        );

    /// <summary>
    /// Gets or sets the padding for the popup.
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
            typeof(StswPopupModel),
            new PropertyMetadata(new Thickness(0))
        );
}
