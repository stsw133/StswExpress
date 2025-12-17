using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace StswExpress.Avalonia;

/// <summary>
/// A customizable radio button that supports icon customization and optional unchecking.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswRadioBox Content="Option A" GroupName="Settings"/&gt;
/// &lt;se:StswRadioBox Content="Option B" GroupName="Settings" AllowUncheck="True" IsChecked="True"/&gt;
/// </code>
/// </example>
public class StswRadioBox : RadioButton
{
    static StswRadioBox()
    {
        IconScaleProperty.Changed.AddClassHandler<StswRadioBox>((s, _) => s.InvalidateMeasure());
        IsReadOnlyProperty.Changed.AddClassHandler<StswRadioBox>((s, e) => s.PseudoClasses.Set(":read-only", e.NewValue is bool b && b));
    }

    /// <inheritdoc/>
    protected override void OnClick()
    {
        if (IsReadOnly)
            return;

        if (AllowUncheck && IsChecked == true)
        {
            IsChecked = false;
            return;
        }

        base.OnClick();
    }

    #region Logic properties
    /// <summary>
    /// Gets or sets a value indicating whether the radio button can be unchecked when already selected.
    /// </summary>
    public bool AllowUncheck
    {
        get => GetValue(AllowUncheckProperty);
        set => SetValue(AllowUncheckProperty, value);
    }
    public static readonly StyledProperty<bool> AllowUncheckProperty = AvaloniaProperty.Register<StswRadioBox, bool>(nameof(AllowUncheck));

    /// <summary>
    /// Gets or sets the scale of the icon inside the radio button.
    /// </summary>
    public GridLength IconScale
    {
        get => GetValue(IconScaleProperty);
        set => SetValue(IconScaleProperty, value);
    }
    public static readonly StyledProperty<GridLength> IconScaleProperty = AvaloniaProperty.Register<StswRadioBox, GridLength>(nameof(IconScale), new GridLength(1.33));

    /// <summary>
    /// Gets or sets a value indicating whether the radio button is in read-only mode.
    /// </summary>
    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }
    public static readonly StyledProperty<bool> IsReadOnlyProperty = AvaloniaProperty.Register<StswRadioBox, bool>(nameof(IsReadOnly));
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the brush used to render the radio glyph.
    /// </summary>
    public IBrush? GlyphBrush
    {
        get => GetValue(GlyphBrushProperty);
        set => SetValue(GlyphBrushProperty, value);
    }
    public static readonly StyledProperty<IBrush?> GlyphBrushProperty = AvaloniaProperty.Register<StswRadioBox, IBrush?>(nameof(GlyphBrush));

    /// <summary>
    /// Gets or sets the geometry used for the icon when the radio button is in the checked state.
    /// </summary>
    public Geometry? IconChecked
    {
        get => GetValue(IconCheckedProperty);
        set => SetValue(IconCheckedProperty, value);
    }
    public static readonly StyledProperty<Geometry?> IconCheckedProperty = AvaloniaProperty.Register<StswRadioBox, Geometry?>(nameof(IconChecked));

    /// <summary>
    /// Gets or sets the geometry used for the icon when the radio button is in the indeterminate state.
    /// </summary>
    public Geometry? IconIndeterminate
    {
        get => GetValue(IconIndeterminateProperty);
        set => SetValue(IconIndeterminateProperty, value);
    }
    public static readonly StyledProperty<Geometry?> IconIndeterminateProperty = AvaloniaProperty.Register<StswRadioBox, Geometry?>(nameof(IconIndeterminate));

    /// <summary>
    /// Gets or sets the geometry used for the icon when the radio button is in the unchecked state.
    /// </summary>
    public Geometry? IconUnchecked
    {
        get => GetValue(IconUncheckedProperty);
        set => SetValue(IconUncheckedProperty, value);
    }
    public static readonly StyledProperty<Geometry?> IconUncheckedProperty = AvaloniaProperty.Register<StswRadioBox, Geometry?>(nameof(IconUnchecked));
    #endregion
}
