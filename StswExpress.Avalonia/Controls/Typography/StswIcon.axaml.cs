using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using System.ComponentModel;

namespace StswExpress.Avalonia;

/// <summary>
/// A control for displaying vector-based icons.
/// Supports scaling, rotation, stroke thickness, and color customization.
/// </summary>
/// <remarks>
/// The control allows for various transformations, including scaling and rotation, making it a versatile choice for UI design.
/// </remarks>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswIcon Data="{StaticResource HomeIcon}" Fill="Blue" Stroke="Black" StrokeThickness="1"/&gt;
/// </code>
/// </example>
public class StswIcon : TemplatedControl
{
    static StswIcon()
    {
        AffectsMeasure<StswIcon>(CanvasSizeProperty, ScaleProperty);
        AffectsRender<StswIcon>(DataProperty, FillProperty, IsRotatedProperty, ScaleProperty, StrokeProperty, StrokeThicknessProperty);
    }

    #region Events & methods
    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        var baseSize = 12d;
        var scale = Scale.IsAbsolute ? Scale.Value : 1d;
        var s = baseSize * scale;

        var desired = new Size(s, s);
        base.MeasureOverride(desired);

        return desired;
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the canvas size of the icon.
    /// This determines the width and height of the drawing area for the icon.
    /// </summary>
    public double CanvasSize
    {
        get => GetValue(CanvasSizeProperty)!;
        set => SetValue(CanvasSizeProperty, value);
    }
    public static readonly StyledProperty<double> CanvasSizeProperty = AvaloniaProperty.Register<StswIcon, double>(nameof(CanvasSize), 24d);

    /// <summary>
    /// Gets or sets the geometry data of the icon.
    /// This defines the vector path used to render the icon.
    /// </summary>
    public Geometry? Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }
    public static readonly StyledProperty<Geometry?> DataProperty = AvaloniaProperty.Register<StswIcon, Geometry?>(nameof(Data));

    /// <summary>
    /// Gets or sets a value indicating whether the icon is rotated.
    /// When set to <see langword="true"/>, the icon rotates <c>180</c> degrees; otherwise, it resets to <c>0</c> degrees.
    /// </summary>
    public bool IsRotated
    {
        get => GetValue(IsRotatedProperty);
        set => SetValue(IsRotatedProperty, value);
    }
    public static readonly StyledProperty<bool> IsRotatedProperty = AvaloniaProperty.Register<StswIcon, bool>(nameof(IsRotated));

    /// <summary>
    /// Gets or sets the scale of the icon.
    /// The scale adjusts the icon's dimensions relative to its default size.
    /// </summary>
    public GridLength Scale
    {
        get => GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }
    public static readonly StyledProperty<GridLength> ScaleProperty = AvaloniaProperty.Register<StswIcon, GridLength>(nameof(Scale), new GridLength(1.5));
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the fill brush of the icon.
    /// This brush is used to paint the interior of the icon's geometry.
    /// </summary>
    public IBrush? Fill
    {
        get => GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }
    public static readonly StyledProperty<IBrush?> FillProperty = AvaloniaProperty.Register<StswIcon, IBrush?>(nameof(Fill));

    /// <summary>
    /// Gets or sets the stroke brush of the icon.
    /// This brush is used to paint the outline of the icon's geometry.
    /// </summary>
    public IBrush? Stroke
    {
        get => GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }
    public static readonly StyledProperty<IBrush?> StrokeProperty = AvaloniaProperty.Register<StswIcon, IBrush?>(nameof(Stroke));

    /// <summary>
    /// Gets or sets the thickness of the icon's stroke.
    /// Determines the width of the outline drawn around the icon.
    /// </summary>
    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }
    public static readonly StyledProperty<double> StrokeThicknessProperty = AvaloniaProperty.Register<StswIcon, double>(nameof(StrokeThickness), 1d);
    #endregion

    #region Excluded properties
    /// The following properties are hidden from the designer and serialization:
    
    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete($"{nameof(BorderBrush)} is not supported in {nameof(StswIcon)}.")]
    protected new IBrush? BorderBrush
    {
        get => default;
        set => throw new NotSupportedException($"{nameof(BorderBrush)} is not supported in {nameof(StswIcon)}.");
    }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete($"{nameof(BorderThickness)} is not supported in {nameof(StswIcon)}.")]
    protected new Thickness BorderThickness
    {
        get => default;
        set => throw new NotSupportedException($"{nameof(BorderThickness)} is not supported in {nameof(StswIcon)}.");
    }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete($"{nameof(Foreground)} is not supported in {nameof(StswIcon)}.")]
    protected new IBrush? Foreground
    {
        get => default;
        set => throw new NotSupportedException($"{nameof(Foreground)} is not supported in {nameof(StswIcon)}.");
    }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete($"{nameof(FontFamily)} is not supported in {nameof(StswIcon)}.")]
    protected new FontFamily? FontFamily
    {
        get => default;
        set => throw new NotSupportedException($"{nameof(FontFamily)} is not supported in {nameof(StswIcon)}.");
    }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete($"{nameof(FontSize)} is not supported in {nameof(StswIcon)}.")]
    protected new double FontSize
    {
        get => default;
        set => throw new NotSupportedException($"{nameof(FontSize)} is not supported in {nameof(StswIcon)}.");
    }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete($"{nameof(FontStretch)} is not supported in {nameof(StswIcon)}.")]
    protected new FontStretch FontStretch
    {
        get => default;
        set => throw new NotSupportedException($"{nameof(FontStretch)} is not supported in {nameof(StswIcon)}.");
    }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete($"{nameof(FontStyle)} is not supported in {nameof(StswIcon)}.")]
    protected new FontStyle FontStyle
    {
        get => default;
        set => throw new NotSupportedException($"{nameof(FontStyle)} is not supported in {nameof(StswIcon)}.");
    }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete($"{nameof(FontWeight)} is not supported in {nameof(StswIcon)}.")]
    protected new FontWeight FontWeight
    {
        get => default;
        set => throw new NotSupportedException($"{nameof(FontWeight)} is not supported in {nameof(StswIcon)}.");
    }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete($"{nameof(HorizontalContentAlignment)} is not supported in {nameof(StswIcon)}.")]
    protected new HorizontalAlignment HorizontalContentAlignment
    {
        get => default;
        set => throw new NotSupportedException($"{nameof(HorizontalContentAlignment)} is not supported in {nameof(StswIcon)}.");
    }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete($"{nameof(VerticalContentAlignment)} is not supported in {nameof(StswIcon)}.")]
    protected new VerticalAlignment VerticalContentAlignment
    {
        get => default;
        set => throw new NotSupportedException($"{nameof(VerticalContentAlignment)} is not supported in {nameof(StswIcon)}.");
    }
    #endregion
}
