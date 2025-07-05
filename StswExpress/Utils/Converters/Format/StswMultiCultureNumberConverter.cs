using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// Converts a numeric value into a string with a culture-specific decimal separator, 
/// allowing both dot (`.`) and comma (`,`) as valid input separators.
/// <br/>
/// This converter is particularly useful in `DataGrid` or `TextBox` bindings where users 
/// may enter decimal numbers using different separator conventions.
/// </summary>
[Stsw(null, Changes = StswPlannedChanges.None)]
public class StswMultiCultureNumberConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswMultiCultureNumberConverter Instance => instance ??= new StswMultiCultureNumberConverter();
    private static StswMultiCultureNumberConverter? instance;

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <summary>
    /// Converts a numeric value into a string formatted according to the culture, 
    /// ensuring the decimal separator is correctly applied.
    /// </summary>
    /// <param name="value">The numeric value to convert.</param>
    /// <param name="targetType">The type of the target property.</param>
    /// <param name="parameter">An optional parameter (not used).</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>A string representation of the number with a culture-specific decimal separator.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || !decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var val))
            return Binding.DoNothing;

        var ci = (CultureInfo)culture.Clone();
        ci.NumberFormat.NumberDecimalSeparator = culture.NumberFormat.NumberDecimalSeparator;

        return val.ToString("G", ci);
    }

    /// <summary>
    /// Converts a string with a flexible decimal separator (dot or comma) into a numeric value.
    /// This allows users to enter numbers using either `.` or `,` as the decimal separator.
    /// </summary>
    /// <param name="value">The string representation of the number.</param>
    /// <param name="targetType">The type to convert to (e.g., `decimal`, `double`, `float`).</param>
    /// <param name="parameter">An optional parameter (not used).</param>
    /// <param name="culture">The culture to use in the conversion.</param>
    /// <returns>The converted numeric value or <see cref="Binding.DoNothing"/> if conversion fails.</returns>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string input || string.IsNullOrWhiteSpace(input))
            return Binding.DoNothing;

        var normalizedInput = input.Replace(',', '.');
        if (decimal.TryParse(normalizedInput, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return System.Convert.ChangeType(result, targetType, culture);

        return Binding.DoNothing;
    }
}

/* usage:

<TextBox Text="{Binding Amount, Converter={x:Static se:StswMultiCultureNumberConverter.Instance}}"/>

<DataGridTextColumn Binding="{Binding Price, Converter={x:Static se:StswMultiCultureNumberConverter.Instance}}"/>

*/