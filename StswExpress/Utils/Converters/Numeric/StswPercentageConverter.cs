using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// Converts a numeric value to a percentage-based string representation.
/// <br/>
/// - If the value is between `0` and `1`, it assumes a fractional percentage (e.g., `0.75` → `"75%"`).
/// - If the value is greater than or equal to `1`, it assumes an absolute percentage (e.g., `75` → `"75%"`).
/// - The parameter specifies the numeric format (e.g., `"N2"`, `"F1"`, `"0.0"`, etc.).
/// </summary>
public class StswPercentageConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswPercentageConverter Instance => instance ??= new StswPercentageConverter();
    private static StswPercentageConverter? instance;

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <summary>
    /// Converts a numeric value to a percentage-based string.
    /// </summary>
    /// <param name="value">The numeric value to convert.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">
    /// (Optional) The numeric format to use (e.g., `"N2"`, `"F1"`, `"0.0"`).
    /// <br/>If omitted, the default format `"0"` is used.
    /// </param>
    /// <param name="culture">The culture to use for numeric formatting.</param>
    /// <returns>A percentage string representation of the input value.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double && value is not float && value is not int)
            return Binding.DoNothing;

        var number = System.Convert.ToDouble(value, culture);
        var percentage = number * 100;
        var format = parameter?.ToString() ?? "0";

        return string.Format(culture, "{0:" + format + "}%", percentage);
    }

    /// <summary>
    /// Converts a percentage-based string back to a numeric value.
    /// </summary>
    /// <param name="value">The percentage string to convert (e.g., `"75%"` → `0.75`).</param>
    /// <param name="targetType">The type to convert to.</param>
    /// <param name="parameter">Not used in ConvertBack.</param>
    /// <param name="culture">The culture to use for parsing.</param>
    /// <returns>A numeric value corresponding to the percentage string.</returns>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string strValue)
            return Binding.DoNothing;

        strValue = strValue.Trim().Replace(culture.NumberFormat.PercentSymbol, "");

        if (double.TryParse(strValue, NumberStyles.Number, culture, out var result))
            return result / 100;

        return Binding.DoNothing;
    }
}

/* usage:

<TextBlock Text="{Binding Completion, Converter={x:Static se:StswPercentageConverter.Instance}}"/>

<TextBlock Text="{Binding Progress, Converter={x:Static se:StswPercentageConverter.Instance}, ConverterParameter='N2'}"/>

<TextBlock Text="{Binding LoadFactor, Converter={x:Static se:StswPercentageConverter.Instance}, ConverterParameter='F1'}"/>

<TextBlock Text="{Binding Accuracy, Mode=TwoWay, Converter={x:Static se:StswPercentageConverter.Instance}, ConverterParameter='0.000'}"/>

*/
