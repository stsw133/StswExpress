using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;

namespace StswExpress;

/// <summary>
/// Represents the thickness of a border with an additional inner value.
/// </summary>
[TypeConverter(typeof(StswThicknessConverter))]
public readonly struct StswThickness : IEquatable<StswThickness>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StswThickness"/> struct with the same value applied to all sides and the inner thickness.
    /// </summary>
    /// <param name="uniformLength">The uniform thickness for all sides and the inside value.</param>
    public StswThickness(double uniformLength) : this(uniformLength, uniformLength, uniformLength, uniformLength, uniformLength)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StswThickness"/> struct with the same value applied to all sides and a separate inside value.
    /// </summary>
    /// <param name="uniformLength">The uniform thickness for all sides.</param>
    /// <param name="inside">The inner thickness value.</param>
    public StswThickness(double uniformLength, double inside) : this(uniformLength, uniformLength, uniformLength, uniformLength, inside)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StswThickness"/> struct with separate horizontal, vertical, and inside values.
    /// </summary>
    /// <param name="horizontal">The thickness for the left and right sides.</param>
    /// <param name="vertical">The thickness for the top and bottom sides.</param>
    /// <param name="inside">The inner thickness value.</param>
    public StswThickness(double horizontal, double vertical, double inside) : this(horizontal, vertical, horizontal, vertical, inside)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StswThickness"/> struct with distinct values for each side.
    /// </summary>
    /// <param name="left">The thickness for the left side.</param>
    /// <param name="top">The thickness for the top side.</param>
    /// <param name="right">The thickness for the right side.</param>
    /// <param name="bottom">The thickness for the bottom side.</param>
    public StswThickness(double left, double top, double right, double bottom) : this(left, top, right, bottom, 0d)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StswThickness"/> struct with distinct values for each side and the inner thickness.
    /// </summary>
    /// <param name="left">The thickness for the left side.</param>
    /// <param name="top">The thickness for the top side.</param>
    /// <param name="right">The thickness for the right side.</param>
    /// <param name="bottom">The thickness for the bottom side.</param>
    /// <param name="inside">The inner thickness value.</param>
    public StswThickness(double left, double top, double right, double bottom, double inside)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
        Inside = inside;
    }

    /// <summary>
    /// Gets or sets the thickness for the left side.
    /// </summary>
    public double Left { get; }

    /// <summary>
    /// Gets or sets the thickness for the top side.
    /// </summary>
    public double Top { get; }

    /// <summary>
    /// Gets or sets the thickness for the right side.
    /// </summary>
    public double Right { get; }

    /// <summary>
    /// Gets or sets the thickness for the bottom side.
    /// </summary>
    public double Bottom { get; }

    /// <summary>
    /// Gets or sets the thickness for the inside value.
    /// </summary>
    public double Inside { get; }

    /// <summary>
    /// Creates a <see cref="System.Windows.Thickness"/> instance using the outer values.
    /// </summary>
    /// <returns>A <see cref="System.Windows.Thickness"/> that contains the outer thickness values.</returns>
    public readonly Thickness Thickness => new(Left, Top, Right, Bottom);

    /// <summary>
    /// Deconstructs the current instance into individual components.
    /// </summary>
    /// <param name="left">The thickness for the left side.</param>
    /// <param name="top">The thickness for the top side.</param>
    /// <param name="right">The thickness for the right side.</param>
    /// <param name="bottom">The thickness for the bottom side.</param>
    /// <param name="inside">The inner thickness value.</param>
    public readonly void Deconstruct(out double left, out double top, out double right, out double bottom, out double inside)
    {
        left = Left;
        top = Top;
        right = Right;
        bottom = Bottom;
        inside = Inside;
    }

    /// <inheritdoc/>
    public override readonly string ToString() => ToString(CultureInfo.CurrentCulture);

    /// <summary>
    /// Converts the current instance to a string using the provided culture.
    /// </summary>
    /// <param name="culture">The culture to use for formatting.</param>
    public readonly string ToString(CultureInfo culture)
    {
        var separator = culture.TextInfo.ListSeparator + " ";
        string Format(double value) => value.ToString("G", culture);
        return string.Join(separator,
        [
            Format(Left),
            Format(Top),
            Format(Right),
            Format(Bottom),
            Format(Inside)
        ]);
    }

    /// <summary>
    /// Determines whether the specified <see cref="StswThickness"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="StswThickness"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="StswThickness"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public readonly bool Equals(StswThickness other)
        => Left.Equals(other.Left)
            && Top.Equals(other.Top)
            && Right.Equals(other.Right)
            && Bottom.Equals(other.Bottom)
            && Inside.Equals(other.Inside);

    /// <inheritdoc/>
    public override readonly bool Equals(object? obj) => obj is StswThickness other && Equals(other);

    /// <inheritdoc/>
    public override readonly int GetHashCode() => HashCode.Combine(Left, Top, Right, Bottom, Inside);

    /// <summary>
    /// Determines whether two specified instances of <see cref="StswThickness"/> are equal.
    /// </summary>
    public static bool operator ==(StswThickness left, StswThickness right) => left.Equals(right);

    /// <summary>
    /// Determines whether two specified instances of <see cref="StswThickness"/> are not equal.
    /// </summary>
    public static bool operator !=(StswThickness left, StswThickness right) => !left.Equals(right);

    /// <summary>
    /// Converts a <see cref="StswThickness"/> to a <see cref="System.Windows.Thickness"/> and vice versa.
    /// </summary>
    /// <param name="t">The <see cref="StswThickness"/> to convert.</param>
    public static explicit operator Thickness(StswThickness t) => t.Thickness;

    /// <summary>
    /// Converts a <see cref="Thickness"/> to a <see cref="StswThickness"/>.
    /// </summary>
    /// <param name="t">The <see cref="System.Windows.Thickness"/> to convert.</param>
    public static explicit operator StswThickness(Thickness t) => new(t.Left, t.Top, t.Right, t.Bottom);
}

/// <summary>
/// Provides conversion support for <see cref="StswThickness"/>.
/// </summary>
public class StswThicknessConverter : TypeConverter
{
    /// <inheritdoc/>
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        if (sourceType == typeof(string) || sourceType == typeof(Thickness))
            return true;

        return base.CanConvertFrom(context, sourceType);
    }

    /// <inheritdoc/>
    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        if (destinationType == typeof(string) || destinationType == typeof(Thickness))
            return true;

        return base.CanConvertTo(context, destinationType);
    }

    /// <inheritdoc/>
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is null)
            return default(StswThickness);

        if (value is string s)
        {
            var tokens = s.Split([",", ";", " ", "\t"], StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0)
                return default(StswThickness);

            var parsed = new double[tokens.Length];
            for (var i = 0; i < tokens.Length; i++)
                if (!double.TryParse(tokens[i], NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out parsed[i]))
                    throw new FormatException($"Cannot parse '{tokens[i]}' as double (InvariantCulture).");

            return parsed.Length switch
            {
                1 => new StswThickness(parsed[0]),
                2 => new StswThickness(parsed[0], parsed[1]),
                3 => new StswThickness(parsed[0], parsed[1], parsed[2]),
                4 => new StswThickness(parsed[0], parsed[1], parsed[2], parsed[3]),
                5 => new StswThickness(parsed[0], parsed[1], parsed[2], parsed[3], parsed[4]),
                _ => throw new FormatException($"Cannot parse '{s}' to {nameof(StswThickness)}."),
            };
        }

        if (value is Thickness thickness)
            return new StswThickness(thickness.Left, thickness.Top, thickness.Right, thickness.Bottom);

        return base.ConvertFrom(context, culture, value);
    }

    /// <inheritdoc/>
    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (value is StswThickness thickness)
        {
            if (destinationType == typeof(string))
                return thickness.ToString(CultureInfo.InvariantCulture);

            if (destinationType == typeof(Thickness))
                return thickness.Thickness;
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }
}
