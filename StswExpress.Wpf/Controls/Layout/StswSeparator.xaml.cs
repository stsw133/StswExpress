using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress.Wpf;
/// <summary>
/// A visual separator used to divide UI elements.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;StackPanel&gt;
///     &lt;TextBlock Text="Section 1"/&gt;
///     &lt;se:StswSeparator Orientation="Horizontal" BorderThickness="2"/&gt;
///     &lt;TextBlock Text="Section 2"/&gt;
/// &lt;/StackPanel&gt;
/// </code>
/// </example>
public class StswSeparator : Separator
{
    static StswSeparator()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSeparator), new FrameworkPropertyMetadata(typeof(StswSeparator)));
    }

    #region Dependency properties
    /// <summary>
    /// Gets or sets the thickness of the separator line.
    /// A higher value results in a thicker visual divider.
    /// </summary>
    public new double BorderThickness
    {
        get => (double)GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }
    public new static readonly DependencyProperty BorderThicknessProperty
        = DependencyProperty.Register(
            nameof(BorderThickness),
            typeof(double),
            typeof(StswSeparator),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the orientation of the separator.
    /// When set to <see cref="Orientation.Horizontal"/>, the separator spans horizontally across the layout.
    /// When set to <see cref="Orientation.Vertical"/>, the separator is displayed as a vertical line.
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
            typeof(StswSeparator),
            new FrameworkPropertyMetadata(default(Orientation), FrameworkPropertyMetadataOptions.AffectsArrange)
        );
    #endregion

    #region Excluded properties
    /// The following properties are hidden from the designer and serialization
    /// because they are not relevant to the appearance or behavior of the control:

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete($"{nameof(Background)} is not supported in {nameof(StswSeparator)}.")]
    protected new Brush? Background
    {
        get => default;
        set => throw new NotSupportedException($"{nameof(Background)} is not supported in {nameof(StswSeparator)}.");
    }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete($"{nameof(BorderBrush)} is not supported in {nameof(StswSeparator)}.")]
    protected new Brush? Foreground
    {
        get => default;
        set => throw new NotSupportedException($"{nameof(Foreground)} is not supported in {nameof(StswSeparator)}.");
    }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete($"{nameof(BorderBrush)} is not supported in {nameof(StswSeparator)}.")]
    protected new FontFamily? FontFamily
    {
        get => default;
        set => throw new NotSupportedException($"{nameof(FontFamily)} is not supported in {nameof(StswSeparator)}.");
    }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete($"{nameof(FontSize)} is not supported in {nameof(StswSeparator)}.")]
    protected new double FontSize
    {
        get => default;
        set => throw new NotSupportedException($"{nameof(FontSize)} is not supported in {nameof(StswSeparator)}.");
    }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete($"{nameof(FontStyle)} is not supported in {nameof(StswSeparator)}.")]
    protected new FontStretch FontStretch
    {
        get => default;
        set => throw new NotSupportedException($"{nameof(FontStretch)} is not supported in {nameof(StswSeparator)}.");
    }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete($"{nameof(FontStyle)} is not supported in {nameof(StswSeparator)}.")]
    protected new FontWeight FontWeight
    {
        get => default;
        set => throw new NotSupportedException($"{nameof(FontWeight)} is not supported in {nameof(StswSeparator)}.");
    }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete($"{nameof(HorizontalContentAlignment)} is not supported in {nameof(StswSeparator)}.")]
    protected new HorizontalAlignment HorizontalContentAlignment
    {
        get => default;
        set => throw new NotSupportedException($"{nameof(HorizontalContentAlignment)} is not supported in {nameof(StswSeparator)}.");
    }

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete($"{nameof(VerticalContentAlignment)} is not supported in {nameof(StswSeparator)}.")]
    protected new VerticalAlignment VerticalContentAlignment
    {
        get => default;
        set => throw new NotSupportedException($"{nameof(VerticalContentAlignment)} is not supported in {nameof(StswSeparator)}.");
    }
    #endregion
}
