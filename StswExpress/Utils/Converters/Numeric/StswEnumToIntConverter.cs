using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// Converts an <see cref="Enum"/> value to its corresponding integer representation and vice versa.
/// This is useful for bindings where you need to work with enums as integers.
/// </summary>
[Stsw("0.9.0")]
public class StswEnumToIntConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswEnumToIntConverter Instance => instance ??= new StswEnumToIntConverter();
    private static StswEnumToIntConverter? instance;

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <summary>
    /// Converts an <see cref="Enum"/> value to an <see cref="int"/> or an <see cref="int"/> back to an <see cref="Enum"/>.
    /// </summary>
    /// <param name="value">The value to convert, which can be an <see cref="Enum"/> or an <see cref="int"/>.</param>
    /// <param name="targetType">The target type of the conversion (should be <see cref="int"/> or an <see cref="Enum"/>).</param>
    /// <param name="parameter">The expected enum type (required when converting from <see cref="int"/> to <see cref="Enum"/>).</param>
    /// <param name="culture">The culture used for the conversion (not used in this implementation).</param>
    /// <returns>
    /// - If `value` is an `Enum`, returns its integer value.
    /// - If `value` is an `int` and `parameter` is an enum type, returns the corresponding enum value.
    /// - If `value` is a `string` representing an enum name, returns the corresponding enum value.
    /// - Otherwise, returns <see cref="Binding.DoNothing"/>.
    /// </returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return ConvertEnum(value, targetType, parameter);
    }

    /// <summary>
    /// Converts an <see cref="Enum"/> value back to an integer or converts an integer back to an <see cref="Enum"/>.
    /// </summary>
    /// <param name="value">The value to convert back, which can be an <see cref="Enum"/> or an <see cref="int"/>.</param>
    /// <param name="targetType">The target type of the conversion.</param>
    /// <param name="parameter">The expected enum type (required when converting from <see cref="int"/> to <see cref="Enum"/>).</param>
    /// <param name="culture">The culture used for the conversion (not used in this implementation).</param>
    /// <returns>
    /// - If `value` is an `Enum`, returns its integer value.
    /// - If `value` is an `int` and `parameter` is an enum type, returns the corresponding enum value.
    /// - If `value` is a `string` representing an enum name, returns the corresponding enum value.
    /// - Otherwise, returns <see cref="Binding.DoNothing"/>.
    /// </returns>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return ConvertEnum(value, targetType, parameter);
    }

    /// <summary>
    /// Handles conversion between <see cref="Enum"/>, <see cref="int"/>, and <see cref="string"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="targetType">The target conversion type.</param>
    /// <param name="parameter">The expected enum type when converting from <see cref="int"/>.</param>
    /// <returns>The converted value or <see cref="Binding.DoNothing"/> if conversion fails.</returns>
    private static object? ConvertEnum(object? value, Type targetType, object? parameter)
    {
        if (value is Enum enumValue)
            return System.Convert.ToInt32(enumValue);

        if (value is int intValue && parameter is Type enumType && enumType.IsEnum)
            return Enum.ToObject(enumType, intValue);

        if (value is string strValue && parameter is Type enumTypeStr && enumTypeStr.IsEnum)
        {
            if (Enum.TryParse(enumTypeStr, strValue, ignoreCase: true, out var result))
                return result;
        }

        return Binding.DoNothing;
    }
}

/* usage:

<TextBlock Text="{Binding MyEnumValue, Converter={x:Static se:StswEnumToIntConverter.Instance}}" />

<ComboBox SelectedValue="{Binding MyEnumIntValue, Converter={x:Static se:StswEnumToIntConverter.Instance}, ConverterParameter={x:Type local:MyEnum}}" />

<TextBlock Text="{Binding MyEnumString, Converter={x:Static se:StswEnumToIntConverter.Instance}, ConverterParameter={x:Type local:MyEnum}}" />

*/
