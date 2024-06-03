using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// Represents a control displaying vector icon.
/// </summary>
[ContentProperty(nameof(Data))]
public class StswIcon : Control
{
    static StswIcon()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswIcon), new FrameworkPropertyMetadata(typeof(StswIcon)));
    }

    #region Logic properties
    /// <summary>
    /// Gets or sets the canvas size of the icon.
    /// </summary>
    public double CanvasSize
    {
        get => (double)GetValue(CanvasSizeProperty);
        set => SetValue(CanvasSizeProperty, value);
    }
    public static readonly DependencyProperty CanvasSizeProperty
        = DependencyProperty.Register(
            nameof(CanvasSize),
            typeof(double),
            typeof(StswIcon),
            new FrameworkPropertyMetadata(24d)
        );

    /// <summary>
    /// Gets or sets the geometry data of the icon.
    /// </summary>
    public Geometry? Data
    {
        get => (Geometry?)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }
    public static readonly DependencyProperty DataProperty
        = DependencyProperty.Register(
            nameof(Data),
            typeof(Geometry),
            typeof(StswIcon)
        );

    /// <summary>
    /// Gets or sets the scale of the icon.
    /// </summary>
    public GridLength Scale
    {
        get => (GridLength)GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }
    public static readonly DependencyProperty ScaleProperty
        = DependencyProperty.Register(
            nameof(Scale),
            typeof(GridLength),
            typeof(StswIcon),
            new PropertyMetadata(default(GridLength), OnScaleChanged)
        );
    public static void OnScaleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswIcon stsw)
        {
            stsw.Height = stsw.Scale.IsStar ? double.NaN : stsw.Scale!.Value * 12;
            stsw.Width = stsw.Scale.IsStar ? double.NaN : stsw.Scale!.Value * 12;
        }
    }
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the fill brush of the icon.
    /// </summary>
    public Brush Fill
    {
        get => (Brush)GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }
    public static readonly DependencyProperty FillProperty
        = DependencyProperty.Register(
            nameof(Fill),
            typeof(Brush),
            typeof(StswIcon)
        );

    /// <summary>
    /// Gets or sets the stroke brush of the icon.
    /// </summary>
    public Brush Stroke
    {
        get => (Brush)GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }
    public static readonly DependencyProperty StrokeProperty
        = DependencyProperty.Register(
            nameof(Stroke),
            typeof(Brush),
            typeof(StswIcon)
        );

    /// <summary>
    /// Gets or sets the stroke thickness of the icon.
    /// </summary>
    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }
    public static readonly DependencyProperty StrokeThicknessProperty
        = DependencyProperty.Register(
            nameof(StrokeThickness),
            typeof(double),
            typeof(StswIcon)
        );
    #endregion

    #region Excluded properties
    /// The following properties are hidden from the designer and serialization:
    
    [Browsable(false)]
    [Bindable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new Brush? BorderBrush { get; private set; }

    [Browsable(false)]
    [Bindable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new Thickness? BorderThickness { get; private set; }

    [Browsable(false)]
    [Bindable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new Brush? Foreground { get; private set; }

    [Browsable(false)]
    [Bindable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new FontFamily? FontFamily { get; private set; }

    [Browsable(false)]
    [Bindable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new double FontSize { get; private set; }

    [Browsable(false)]
    [Bindable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new FontStretch FontStretch { get; private set; }

    [Browsable(false)]
    [Bindable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new FontWeight FontWeight { get; private set; }

    [Browsable(false)]
    [Bindable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new HorizontalAlignment HorizontalContentAlignment { get; private set; }

    [Browsable(false)]
    [Bindable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new VerticalAlignment VerticalContentAlignment { get; private set; }
    #endregion
}
