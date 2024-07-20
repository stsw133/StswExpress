using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// Converts a numeric value into a string with a decimal separator that is appropriate for a given culture.
/// </summary>
public class StswMultiCultureNumberConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswMultiCultureNumberConverter Instance => instance ??= new StswMultiCultureNumberConverter();
    private static StswMultiCultureNumberConverter? instance;

    /// <summary>
    /// Provides the singleton instance of the converter.
    /// </summary>
    /// <param name="serviceProvider">A service provider that can provide services for the markup extension.</param>
    /// <returns>The singleton instance of the converter.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <summary>
    /// Converts a numeric value into a string with a decimal separator that is appropriate for a given culture.
    /// </summary>
    /// <param name="value">The numeric value to convert.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>A string representation of the numeric value with a culture-specific decimal separator.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || !decimal.TryParse(value.ToString(), NumberStyles.Any, culture, out var val))
            return value;

        var ci = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        ci.NumberFormat.NumberDecimalSeparator = ",";
        return val.ToString("G", ci);
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
