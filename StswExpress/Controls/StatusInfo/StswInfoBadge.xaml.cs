using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Represents a control that can be customized to display a number, icon, or a simple dot.
/// </summary>
public class StswInfoBadge : Control, IStswCornerControl
{
    static StswInfoBadge()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswInfoBadge), new FrameworkPropertyMetadata(typeof(StswInfoBadge)));
    }

    #region Events & methods
    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        OnValueChanged(this, new DependencyPropertyChangedEventArgs());
    }

    /// <summary>
    /// Converts a given integer to a string representation, using 'k' for thousands, 'M' for millions,
    /// and 'B' for billions if applicable.
    /// </summary>
    /// <param name="number">The integer to be converted.</param>
    /// <returns>
    /// A string representing the number with 'k' for thousands, 'M' for millions,
    /// and 'B' for billions, or the number itself if it is less than 1000.
    /// </returns>
    public static string SeparateByThousands(int number)
    {
        if (number.Between(0, 999))
            return $"{number}";

        number /= 1000;
        if (number.Between(0, 999))
            return $"{number}k";

        number /= 1000;
        if (number.Between(0, 999))
            return $"{number}M";

        number /= 1000;
        return $"{number}B";
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Represents the displayed value in the control, dynamically updated to show the actual value
    /// or a truncated version followed by a '+' if it exceeds the specified limit.
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
    /// Specifies the format of the displayed information, allowing customization of the presentation style.
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
    /// Gets or sets the geometry used for the icon.
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
    /// Sets a maximum limit for the displayed value.
    /// If the assigned value surpasses this limit,
    /// the displayed value is truncated and appended with a '+'.
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
    /// Represents the type of information displayed, allowing
    /// customization based on different data representations.
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
    /// Defines the numerical value, used to reflect specific information or statistics.
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
    public static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswInfoBadge stsw)
        {
            if (stsw.Limit == null)
                stsw.DisplayedValue = SeparateByThousands(stsw.Value);
            else
                stsw.DisplayedValue = stsw.Value > stsw.Limit.Value ? $"{SeparateByThousands(stsw.Limit.Value)}+" : $"{SeparateByThousands(stsw.Value)}";
        }
    }
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets a value indicating whether corner clipping is enabled for the control.
    /// When set to <see langword="true"/>, content within the control's border area is clipped to match
    /// the border's rounded corners, preventing elements from protruding beyond the border.
    /// </summary>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty
        = DependencyProperty.Register(
            nameof(CornerClipping),
            typeof(bool),
            typeof(StswInfoBadge)
        );

    /// <summary>
    /// Gets or sets the degree to which the corners of the control's border are rounded by defining
    /// a radius value for each corner independently. This property allows users to control the roundness
    /// of corners, and large radius values are smoothly scaled to blend from corner to corner.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswInfoBadge)
        );
    #endregion
}
