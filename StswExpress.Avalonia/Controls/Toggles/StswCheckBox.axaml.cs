using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace StswExpress.Avalonia;

/// <summary>
/// A customizable checkbox control that supports three states and read-only mode.
/// Includes icon customization and adjustable scaling for the glyph.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswCheckBox Content="Advanced settings" IsThreeState="True" IsReadOnly="True"/&gt;
/// </code>
/// </example>
public class StswCheckBox : CheckBox
{
    static StswCheckBox()
    {
        IconScaleProperty.Changed.AddClassHandler<StswCheckBox>((s, _) => s.InvalidateMeasure());
        IsReadOnlyProperty.Changed.AddClassHandler<StswCheckBox>((s, e) => s.PseudoClasses.Set(":read-only", e.NewValue is bool b && b));
    }

    /// <inheritdoc/>
    protected override void OnClick()
    {
        if (IsReadOnly)
            return;

        base.OnClick();
    }

    #region Logic properties
    /// <summary>
    /// Gets or sets the scale of the icon inside the checkbox.
    /// </summary>
    public GridLength IconScale
    {
        get => GetValue(IconScaleProperty);
        set => SetValue(IconScaleProperty, value);
    }
    public static readonly StyledProperty<GridLength> IconScaleProperty = AvaloniaProperty.Register<StswCheckBox, GridLength>(nameof(IconScale), new GridLength(1.33));

    /// <summary>
    /// Gets or sets a value indicating whether the checkbox is in read-only mode.
    /// When set to <see langword="true"/>, the checkbox cannot be toggled.
    /// </summary>
    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }
    public static readonly StyledProperty<bool> IsReadOnlyProperty = AvaloniaProperty.Register<StswCheckBox, bool>(nameof(IsReadOnly));
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the brush used to render the checkbox glyph.
    /// </summary>
    public IBrush? GlyphBrush
    {
        get => GetValue(GlyphBrushProperty);
        set => SetValue(GlyphBrushProperty, value);
    }
    public static readonly StyledProperty<IBrush?> GlyphBrushProperty = AvaloniaProperty.Register<StswCheckBox, IBrush?>(nameof(GlyphBrush));

    /// <summary>
    /// Gets or sets the geometry used for the icon when the checkbox is in the checked state.
    /// </summary>
    public Geometry? IconChecked
    {
        get => GetValue(IconCheckedProperty);
        set => SetValue(IconCheckedProperty, value);
    }
    public static readonly StyledProperty<Geometry?> IconCheckedProperty = AvaloniaProperty.Register<StswCheckBox, Geometry?>(nameof(IconChecked));

    /// <summary>
    /// Gets or sets the geometry used for the icon when the checkbox is in the indeterminate state.
    /// </summary>
    public Geometry? IconIndeterminate
    {
        get => GetValue(IconIndeterminateProperty);
        set => SetValue(IconIndeterminateProperty, value);
    }
    public static readonly StyledProperty<Geometry?> IconIndeterminateProperty = AvaloniaProperty.Register<StswCheckBox, Geometry?>(nameof(IconIndeterminate));

    /// <summary>
    /// Gets or sets the geometry used for the icon when the checkbox is in the unchecked state.
    /// </summary>
    public Geometry? IconUnchecked
    {
        get => GetValue(IconUncheckedProperty);
        set => SetValue(IconUncheckedProperty, value);
    }
    public static readonly StyledProperty<Geometry?> IconUncheckedProperty = AvaloniaProperty.Register<StswCheckBox, Geometry?>(nameof(IconUnchecked));
    #endregion
}
