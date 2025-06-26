using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// A value converter that compares a numeric, string, or enum value against a specified threshold or condition.
/// <br/>
/// The converter parameter must start with one of the following symbols:
/// <list type="bullet">
/// <item>'&gt;' (greater than)</item>
/// <item>'&gt;=' (greater than or equal to)</item>
/// <item>'&lt;' (less than)</item>
/// <item>'&lt;=' (less than or equal to)</item>
/// <item>'=' (equal to)</item>
/// <item>'!' (not equal to)</item>
/// <item>'&amp;' (bitwise AND, for integer values)</item>
/// <item>'@' (case-insensitive string comparison)</item>
/// </list>
/// <br/>
/// When the target type is <see cref="Visibility"/>, the result is <see cref="Visibility.Visible"/> when the comparison is <see langword="true"/>; otherwise, <see cref="Visibility.Collapsed"/>.
/// Otherwise, the result is a <see cref="bool"/> indicating whether the comparison condition was met.
/// </summary>
public class StswCompareConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswCompareConverter Instance => instance ??= new StswCompareConverter();
    private static StswCompareConverter? instance;

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <summary>
    /// Compares a numeric, string, or enum value with a specified threshold and returns a boolean result or a visibility state.
    /// </summary>
    /// <param name="value">The source value to compare.</param>
    /// <param name="targetType">The type to convert to.</param>
    /// <param name="parameter">A string defining the comparison operator and threshold.</param>
    /// <param name="culture">The culture to use in the conversion.</param>
    /// <returns>
    /// - A <see cref="Visibility"/> value if the target type is <see cref="Visibility"/>.
    /// - A <see cref="bool"/> value indicating the result of the comparison.
    /// </returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        /// handle Enum comparison separately
        if (value is Enum && parameter is Enum enumParam)
            return Equals(value, enumParam)
                ? (targetType == typeof(Visibility) ? Visibility.Visible : true)
                : (targetType == typeof(Visibility) ? Visibility.Collapsed : false);

        /// ...
        if (parameter is not string pmr || pmr.Length < 2)
            return Binding.DoNothing;

        var result = false;
        var input = value?.ToString() ?? string.Empty;
        var spanParam = pmr.AsSpan();

        /// flag enum comparison (e.g. '&2' for bitwise AND)
        if (spanParam[0] == '&' && value is Enum inputEnum && int.TryParse(spanParam[1..], out var flag))
        {
            result = (System.Convert.ToInt32(inputEnum) & flag) > 0;
        }
        /// numeric comparison
        else if (double.TryParse(input, NumberStyles.Number, culture, out var val))
        {
            if (spanParam[0] is '>' or '<' or '=' or '!')
            {
                if (double.TryParse(spanParam[1..], NumberStyles.Number, culture, out var num))
                {
                    result = spanParam[0] switch
                    {
                        '>' when spanParam.Length > 1 && spanParam[1] == '=' => val >= num,
                        '>' => val > num,
                        '<' when spanParam.Length > 1 && spanParam[1] == '=' => val <= num,
                        '<' => val < num,
                        '=' => val == num,
                        '!' => val != num,
                        _ => result
                    };
                }
            }
            else if (spanParam[0] == '&' && double.TryParse(spanParam[1..], out var num))
            {
                result = ((int)val & (int)num) > 0;
            }
        }
        /// string comparison
        else
        {
            var compareTo = pmr.Length > 1 ? pmr[1..] : string.Empty;

            result = spanParam[0] switch
            {
                '=' => input.Equals(compareTo, StringComparison.Ordinal),
                '!' => !input.Equals(compareTo, StringComparison.Ordinal),
                '@' => input.Equals(compareTo, StringComparison.OrdinalIgnoreCase),
                _ => value?.ToString() == parameter?.ToString()
            };
        }

        return targetType == typeof(Visibility)
            ? result ? Visibility.Visible : Visibility.Collapsed
            : result.ConvertTo(targetType);
    }

    /// <summary>
    /// This converter does not support converting back from target value to source value.
    /// </summary>
    /// <param name="value">The value produced by the binding target.</param>
    /// <param name="targetType">The type to convert to.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns><see cref="Binding.DoNothing"/> as the converter does not support converting back.</returns>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => Binding.DoNothing;
}

/* usage:

<TextBlock Text="Only for small values" Visibility="{Binding SomeNumber, Converter={x:Static se:StswCompareConverter.Instance}, ConverterParameter='<=10'}"/>

<Button Content="Administration panel" Visibility="{Binding UserRole, Converter={x:Static se:StswCompareConverter.Instance}, ConverterParameter='=Admin'}"/>

<Button Content="Delete" Visibility="{Binding UserRole, Converter={x:Static se:StswCompareConverter.Instance}, ConverterParameter='!Guest'}"/>

<Button Content="Advanced options" Visibility="{Binding UserPermissions, Converter={x:Static se:StswCompareConverter.Instance}, ConverterParameter='&2'}"/>

*/
