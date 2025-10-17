using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// A badge control for displaying numerical values, icons, or dots.
/// Supports customizable limits, formatting, icon integration, and rounded corner styling.
/// </summary>
/// <remarks>
/// This control allows displaying notifications, counters, or status indicators in a compact format.
/// </remarks>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswInfoBadge Value="1500" Limit="999" Format="Number"/&gt;
/// </code>
/// </example>
public class StswInfoBadge : Control, IStswCornerControl
{
    static StswInfoBadge()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswInfoBadge), new FrameworkPropertyMetadata(typeof(StswInfoBadge)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        UpdateValue();
    }

    /// <summary>
    /// Converts a numerical value into a shortened string representation.
    /// Uses 'k' for thousands, 'M' for millions, and 'B' for billions if applicable.
    /// </summary>
    /// <param name="number">The integer value to convert.</param>
    /// <returns>
    /// A formatted string representing the number with 'k' (thousands), 'M' (millions),
    /// or 'B' (billions), or the number itself if it is less than 1000.
    /// </returns>
    public static string SeparateByThousands(int number) => Math.Abs(number) switch
    {
        < 1000 => $"{number}",
        < 1_000_000 => $"{number / 1_000}k",
        < 1_000_000_000 => $"{number / 1_000_000}M",
        _ => $"{number / 1_000_000_000}B"
    };

    /// <summary>
    /// Updates the displayed value based on the <see cref="Value"/> and <see cref="Limit"/> properties.
    /// If the value exceeds the limit, it is truncated and appended with a '+'.
    /// </summary>
    public void UpdateValue()
    {
        DisplayedValue = Value > Limit ? $"{SeparateByThousands(Limit.Value)}+" : $"{SeparateByThousands(Value)}";
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the formatted string representation of the displayed value.
    /// Updates dynamically based on the <see cref="Value"/> and <see cref="Limit"/> properties.
    /// </summary>
    public string? DisplayedValue
    {
        get => (string?)GetValue(DisplayedValueProperty);
        internal set => SetValue(DisplayedValueProperty, value);
    }
    public static readonly DependencyProperty DisplayedValueProperty
        = DependencyProperty.Register(
            nameof(DisplayedValue),
            typeof(string),
            typeof(StswInfoBadge)
        );

    /// <summary>
    /// Gets or sets the format used to display the information.
    /// Determines whether the badge shows a number, icon, or other visual representation.
    /// </summary>
    public StswInfoFormat Format
    {
        get => (StswInfoFormat)GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }
    public static readonly DependencyProperty FormatProperty
        = DependencyProperty.Register(
            nameof(Format),
            typeof(StswInfoFormat),
            typeof(StswInfoBadge)
        );

    /// <summary>
    /// Gets or sets the geometry data for the icon displayed within the badge.
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
            typeof(StswInfoBadge)
        );

    /// <summary>
    /// Gets or sets the maximum value that can be displayed before being truncated with a '+' suffix.
    /// If the value exceeds this limit, it is shown as "<see cref="Limit"/>+".
    /// </summary>
    public int? Limit
    {
        get => (int?)GetValue(LimitProperty);
        set => SetValue(LimitProperty, value);
    }
    public static readonly DependencyProperty LimitProperty
        = DependencyProperty.Register(
            nameof(Limit),
            typeof(int?),
            typeof(StswInfoBadge),
            new FrameworkPropertyMetadata(default(int?), OnValueChanged)
        );

    /// <summary>
    /// Gets or sets the type of information displayed in the badge.
    /// Determines the purpose or meaning of the displayed value.
    /// </summary>
    public StswInfoType Type
    {
        get => (StswInfoType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }
    public static readonly DependencyProperty TypeProperty
        = DependencyProperty.Register(
            nameof(Type),
            typeof(StswInfoType),
            typeof(StswInfoBadge)
        );

    /// <summary>
    /// Gets or sets the numerical value displayed in the badge.
    /// This value is dynamically formatted and constrained by the <see cref="Limit"/> property.
    /// </summary>
    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    public static readonly DependencyProperty ValueProperty
        = DependencyProperty.Register(
            nameof(Value),
            typeof(int),
            typeof(StswInfoBadge),
            new FrameworkPropertyMetadata(default(int), OnValueChanged)
        );
    public static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswInfoBadge stsw)
            return;

        stsw.UpdateValue();
    }
    #endregion

    #region Style properties
    /// <inheritdoc/>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty
        = DependencyProperty.Register(
            nameof(CornerClipping),
            typeof(bool),
            typeof(StswInfoBadge),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <inheritdoc/>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswInfoBadge),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}
