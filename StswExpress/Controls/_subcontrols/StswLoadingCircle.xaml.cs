using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Represents a control displaying loading circle.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public class StswLoadingCircle : Control
{
    static StswLoadingCircle()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswLoadingCircle), new FrameworkPropertyMetadata(typeof(StswLoadingCircle)));
    }

    #region Logic properties
    /// <summary>
    /// Gets or sets the scale of the loading circle.
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
            typeof(StswLoadingCircle),
            new PropertyMetadata(default(GridLength), OnScaleChanged)
        );
    public static void OnScaleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswLoadingCircle stsw)
        {
            stsw.Height = stsw.Scale.IsStar ? double.NaN : stsw.Scale!.Value * 12;
            stsw.Width = stsw.Scale.IsStar ? double.NaN : stsw.Scale!.Value * 12;
        }
    }
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the fill brush of the loading circle.
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
            typeof(StswLoadingCircle)
        );
    #endregion

    #region Excluded properties
    /// The following properties are hidden from the designer and serialization:

    [Browsable(false)]
    [Bindable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    protected new Brush? Background { get; private set; }

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
    protected new Thickness Padding { get; private set; }

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
