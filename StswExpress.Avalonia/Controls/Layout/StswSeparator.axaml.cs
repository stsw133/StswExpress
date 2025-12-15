using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;

namespace StswExpress.Avalonia;
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
public class StswSeparator : TemplatedControl
{
    public StswSeparator()
    {
        UpdateOrientationPseudoClasses();
    }
    static StswSeparator()
    {
        AffectsArrange<StswSeparator>(OrientationProperty);
        AffectsMeasure<StswSeparator>(OrientationProperty, BorderThicknessProperty);
        AffectsRender<StswSeparator>(BorderBrushProperty, BorderThicknessProperty);
    }

    #region Events & methods
    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == OrientationProperty)
            UpdateOrientationPseudoClasses();
    }

    /// <summary>
    /// Updates the pseudo-classes based on the current orientation.
    /// </summary>
    private void UpdateOrientationPseudoClasses()
    {
        PseudoClasses.Set(":horizontal", Orientation == Orientation.Horizontal);
        PseudoClasses.Set(":vertical", Orientation == Orientation.Vertical);
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the orientation of the separator.
    /// When set to <see cref="Orientation.Horizontal"/>, the separator spans horizontally across the layout.
    /// When set to <see cref="Orientation.Vertical"/>, the separator is displayed as a vertical line.
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
    public static readonly StyledProperty<Orientation> OrientationProperty = AvaloniaProperty.Register<StswSeparator, Orientation>(nameof(Orientation), defaultValue: Orientation.Horizontal);
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the thickness of the separator line.
    /// A higher value results in a thicker visual divider.
    /// </summary>
    public new double BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }
    public static new readonly StyledProperty<double> BorderThicknessProperty = AvaloniaProperty.Register<StswSeparator, double>(nameof(BorderThickness), 2d);

    /// <summary>
    /// Gets or sets the brush used to draw separator line.
    /// </summary>
    public new IBrush? BorderBrush
    {
        get => GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }
    public static new readonly StyledProperty<IBrush?> BorderBrushProperty = AvaloniaProperty.Register<StswSeparator, IBrush?>(nameof(BorderBrush));
    #endregion
}
