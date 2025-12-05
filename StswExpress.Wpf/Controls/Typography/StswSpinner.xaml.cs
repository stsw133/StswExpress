using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress.Wpf;
/// <summary>
/// A loading spinner control for indicating ongoing background processes.
/// Supports different animation styles, scaling, and color customization.
/// </summary>
/// <remarks>
/// The control provides visual feedback for loading states, making it useful for asynchronous operations.
/// </remarks>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswSpinner Type="Dots" Scale="2" Fill="Red"/&gt;
/// </code>
/// </example>
public class StswSpinner : Control
{
    static StswSpinner()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSpinner), new FrameworkPropertyMetadata(typeof(StswSpinner)));
    }

    #region Logic properties
    /// <summary>
    /// Gets or sets the scale of the loading circle.
    /// Determines the overall size of the spinner.
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
            typeof(StswSpinner),
            new FrameworkPropertyMetadata(default(GridLength),
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                OnScaleChanged)
        );
    public static void OnScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswSpinner stsw)
            return;

        IStswIconControl.ScaleChanged(stsw, stsw.Scale);
    }

    /// <summary>
    /// Gets or sets the type of spinner animation.
    /// Allows selecting different visual styles.
    /// </summary>
    public StswSpinnerType Type
    {
        get => (StswSpinnerType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }
    public static readonly DependencyProperty TypeProperty
        = DependencyProperty.Register(
            nameof(Type),
            typeof(StswSpinnerType),
            typeof(StswSpinner)
        );
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
            typeof(StswSpinner),
            new FrameworkPropertyMetadata(default(Brush),
                FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion

    #region Excluded properties
    /// The following properties are hidden from the designer and serialization:
    
    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete($"{nameof(BorderBrush)} is not supported in {nameof(StswSpinner)}.")]
    protected new Brush? BorderBrush
    {
        get => default;
        set => throw new NotSupportedException($"{nameof(BorderBrush)} is not supported in {nameof(StswSpinner)}.");
    }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete($"{nameof(BorderThickness)} is not supported in {nameof(StswSpinner)}.")]
    protected new Thickness? BorderThickness
    {
        get => default;
        set => throw new NotSupportedException($"{nameof(BorderThickness)} is not supported in {nameof(StswSpinner)}.");
    }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete($"{nameof(Foreground)} is not supported in {nameof(StswSpinner)}.")]
    protected new Brush? Foreground
    {
        get => default;
        set => throw new NotSupportedException($"{nameof(Foreground)} is not supported in {nameof(StswSpinner)}.");
    }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete($"{nameof(FontFamily)} is not supported in {nameof(StswSpinner)}.")]
    protected new FontFamily? FontFamily
    {
        get => default;
        set => throw new NotSupportedException($"{nameof(FontFamily)} is not supported in {nameof(StswSpinner)}.");
    }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete($"{nameof(FontSize)} is not supported in {nameof(StswSpinner)}.")]
    protected new double FontSize
    {
        get => default;
        set => throw new NotSupportedException($"{nameof(FontSize)} is not supported in {nameof(StswSpinner)}.");
    }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete($"{nameof(FontStretch)} is not supported in {nameof(StswSpinner)}.")]
    protected new FontStretch FontStretch
    {
        get => default;
        set => throw new NotSupportedException($"{nameof(FontStretch)} is not supported in {nameof(StswSpinner)}.");
    }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete($"{nameof(FontStyle)} is not supported in {nameof(StswSpinner)}.")]
    protected new FontStyle FontStyle
    {
        get => default;
        set => throw new NotSupportedException($"{nameof(FontStyle)} is not supported in {nameof(StswSpinner)}.");
    }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete($"{nameof(FontWeight)} is not supported in {nameof(StswSpinner)}.")]
    protected new FontWeight FontWeight
    {
        get => default;
        set => throw new NotSupportedException($"{nameof(FontWeight)} is not supported in {nameof(StswSpinner)}.");
    }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete($"{nameof(HorizontalContentAlignment)} is not supported in {nameof(StswSpinner)}.")]
    protected new HorizontalAlignment HorizontalContentAlignment
    {
        get => default;
        set => throw new NotSupportedException($"{nameof(HorizontalContentAlignment)} is not supported in {nameof(StswSpinner)}.");
    }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete($"{nameof(VerticalContentAlignment)} is not supported in {nameof(StswSpinner)}.")]
    protected new VerticalAlignment VerticalContentAlignment
    {
        get => default;
        set => throw new NotSupportedException($"{nameof(VerticalContentAlignment)} is not supported in {nameof(StswSpinner)}.");
    }
    #endregion
}
