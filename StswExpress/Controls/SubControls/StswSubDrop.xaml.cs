using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// A drop-down control with an icon, designed for use as a sub-control.
/// Supports custom templates, busy state, content visibility, and customizable icon styles.
/// </summary>
/// <remarks>
/// This control is intended for use in toolbars, menus, and other compact UI areas where 
/// a small, icon-based drop-down button is needed.
/// </remarks>
[ContentProperty(nameof(Items))]
public class StswSubDrop : StswDropButton, IStswSubControl, IStswCornerControl, IStswDropControl, IStswIconControl
{
    static StswSubDrop()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSubDrop), new FrameworkPropertyMetadata(typeof(StswSubDrop)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswSubDrop), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }

    #region Logic properties
    /// <inheritdoc/>
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
    /// Gets or sets a string format applied to the <see cref="Content"/>.
    /// Useful for formatting text-based content.
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
    /// Gets or sets the data template used to display the <see cref="Content"/>.
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
    /// Gets or sets a data template selector for the <see cref="Content"/>.
    /// Allows dynamic selection of templates based on content type.
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
    public static readonly DependencyProperty OrientationProperty
        = DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(StswSubDrop),
            new FrameworkPropertyMetadata(default(Orientation), FrameworkPropertyMetadataOptions.AffectsArrange)
        );
    #endregion

    #region Style properties
    /// <inheritdoc/>
    public Brush IconFill
    {
        get => (Brush)GetValue(IconFillProperty);
        set => SetValue(IconFillProperty, value);
    }
    public static readonly DependencyProperty IconFillProperty
        = DependencyProperty.Register(
            nameof(IconFill),
            typeof(Brush),
            typeof(StswSubDrop),
            new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <inheritdoc/>
    public Brush IconStroke
    {
        get => (Brush)GetValue(IconStrokeProperty);
        set => SetValue(IconStrokeProperty, value);
    }
    public static readonly DependencyProperty IconStrokeProperty
        = DependencyProperty.Register(
            nameof(IconStroke),
            typeof(Brush),
            typeof(StswSubDrop),
            new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <inheritdoc/>
    public double IconStrokeThickness
    {
        get => (double)GetValue(IconStrokeThicknessProperty);
        set => SetValue(IconStrokeThicknessProperty, value);
    }
    public static readonly DependencyProperty IconStrokeThicknessProperty
        = DependencyProperty.Register(
            nameof(IconStrokeThickness),
            typeof(double),
            typeof(StswSubDrop),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}

/* usage:

<se:StswSubDrop Header="Options">
    <MenuItem Header="Action 1"/>
    <MenuItem Header="Action 2"/>
</se:StswSubDrop>

*/
