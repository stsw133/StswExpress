using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// Represents a control that functions as a sub control and displays an icon.
/// </summary>
[ContentProperty(nameof(Items))]
public class StswSubDrop : StswDropButton, IStswSubControl, IStswCornerControl, IStswDropControl, IStswIconControl
{
    static StswSubDrop()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSubDrop), new FrameworkPropertyMetadata(typeof(StswSubDrop)));
    }

    #region Logic properties
    /// <summary>
    /// Gets or sets the header content of the control.
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
            typeof(StswSubDrop)
        );

    /// <summary>
    /// 
    /// </summary>
    public string? ContentStringFormat
    {
        get => (string?)GetValue(ContentStringFormatProperty);
        set => SetValue(ContentStringFormatProperty, value);
    }
    public static readonly DependencyProperty ContentStringFormatProperty
        = DependencyProperty.Register(
            nameof(ContentStringFormat),
            typeof(string),
            typeof(StswSubDrop)
        );

    /// <summary>
    /// 
    /// </summary>
    public DataTemplate? ContentTemplate
    {
        get => (DataTemplate?)GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }
    public static readonly DependencyProperty ContentTemplateProperty
        = DependencyProperty.Register(
            nameof(ContentTemplate),
            typeof(DataTemplate),
            typeof(StswSubDrop)
        );

    /// <summary>
    /// 
    /// </summary>
    public DataTemplateSelector? ContentTemplateSelector
    {
        get => (DataTemplateSelector?)GetValue(ContentTemplateSelectorProperty);
        set => SetValue(ContentTemplateSelectorProperty, value);
    }
    public static readonly DependencyProperty ContentTemplateSelectorProperty
        = DependencyProperty.Register(
            nameof(ContentTemplateSelector),
            typeof(DataTemplateSelector),
            typeof(StswSubDrop)
        );

    /// <summary>
    /// Gets or sets the geometry used for the icon.
    /// </summary>
    public Geometry? IconData
    {
        get => (Geometry?)GetValue(IconDataProperty);
        set => SetValue(IconDataProperty, value);
    }
    public static readonly DependencyProperty IconDataProperty
        = DependencyProperty.Register(
            nameof(IconData),
            typeof(Geometry),
            typeof(StswSubDrop)
        );

    /// <summary>
    /// Gets or sets the scale of the icon.
    /// </summary>
    public GridLength IconScale
    {
        get => (GridLength)GetValue(IconScaleProperty);
        set => SetValue(IconScaleProperty, value);
    }
    public static readonly DependencyProperty IconScaleProperty
        = DependencyProperty.Register(
            nameof(IconScale),
            typeof(GridLength),
            typeof(StswSubDrop)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the control is in a busy/loading state.
    /// </summary>
    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }
    public static readonly DependencyProperty IsBusyProperty
        = DependencyProperty.Register(
            nameof(IsBusy),
            typeof(bool?),
            typeof(StswSubDrop)
        );

    /// <summary>
    /// Gets or sets whether the the content within the control is visible or not.
    /// </summary>
    public bool IsContentVisible
    {
        get => (bool)GetValue(IsContentVisibleProperty);
        set => SetValue(IsContentVisibleProperty, value);
    }
    public static readonly DependencyProperty IsContentVisibleProperty
        = DependencyProperty.Register(
            nameof(IsContentVisible),
            typeof(bool),
            typeof(StswSubDrop)
        );

    /// <summary>
    /// Gets or sets the orientation of the control.
    /// </summary>
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
    public static readonly DependencyProperty OrientationProperty
        = DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(StswSubDrop)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the fill brush of the icon.
    /// </summary>
    public Brush IconFill
    {
        get => (Brush)GetValue(IconFillProperty);
        set => SetValue(IconFillProperty, value);
    }
    public static readonly DependencyProperty IconFillProperty
        = DependencyProperty.Register(
            nameof(IconFill),
            typeof(Brush),
            typeof(StswSubDrop)
        );

    /// <summary>
    /// Gets or sets the stroke brush of the icon.
    /// </summary>
    public Brush IconStroke
    {
        get => (Brush)GetValue(IconStrokeProperty);
        set => SetValue(IconStrokeProperty, value);
    }
    public static readonly DependencyProperty IconStrokeProperty
        = DependencyProperty.Register(
            nameof(IconStroke),
            typeof(Brush),
            typeof(StswSubDrop)
        );

    /// <summary>
    /// Gets or sets the stroke thickness of the icon.
    /// </summary>
    public double IconStrokeThickness
    {
        get => (double)GetValue(IconStrokeThicknessProperty);
        set => SetValue(IconStrokeThicknessProperty, value);
    }
    public static readonly DependencyProperty IconStrokeThicknessProperty
        = DependencyProperty.Register(
            nameof(IconStrokeThickness),
            typeof(double),
            typeof(StswSubDrop)
        );
    #endregion
}
