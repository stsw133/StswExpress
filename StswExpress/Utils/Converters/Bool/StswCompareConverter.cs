using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// A converter that compares a numeric value to a specified threshold and determines if it is
/// greater than, less than, greater than or equal to, or less than or equal to the threshold.
/// Use one of these: '&gt;', '&gt;=', '&lt;', '&lt;=', '=', '!', '&amp;', '@' at the beginning of the converter parameter and a number after.
/// <br/>
/// When targetType is <see cref="Visibility"/>, the output is <c>Visible</c> when <see langword="true"/>, otherwise <c>Collapsed</c>.
/// When targetType is anything else, it returns <see cref="bool"/> with a value depending on the converter result.
/// </summary>
public class StswCompareConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswCompareConverter Instance => instance ??= new StswCompareConverter();
    private static StswCompareConverter? instance;

    /// <summary>
    /// Provides the singleton instance of the converter.
    /// </summary>
    /// <param name="serviceProvider">A service provider that can provide services for the markup extension.</param>
    /// <returns>The singleton instance of the converter.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <summary>
    /// Converts a numeric value to a <see cref="bool"/> or <see cref="Visibility"/> based on a comparison with the parameter.
    /// </summary>
    /// <param name="value">The value produced by the binding source.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter to use for comparison.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>
    /// A <see cref="Visibility"/> value if the targetType is <see cref="Visibility"/>;
    /// otherwise, a <see cref="bool"/> value indicating the result of the comparison.
    /// </returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var input = value?.ToString() ?? string.Empty;
        var pmr = parameter?.ToString() ?? string.Empty;
        var result = false;

        /// parameters
        if (double.TryParse(input, NumberStyles.Number, culture, out var val))
        {
            if (pmr.StartsWith(">=") && double.TryParse(pmr[2..], out var num))
                result = val >= num;
            else if (pmr.StartsWith("<=") && double.TryParse(pmr[2..], out num))
                result = val <= num;
            else if (pmr.StartsWith('>') && double.TryParse(pmr[1..], out num))
                result = val > num;
            else if (pmr.StartsWith('<') && double.TryParse(pmr[1..], out num))
                result = val < num;
            else if (pmr.StartsWith('=') && double.TryParse(pmr[1..], out num))
                result = val == num;
            else if (pmr.StartsWith('!') && double.TryParse(pmr[1..], out num))
                result = val != num;
            else if (pmr.StartsWith('&') && double.TryParse(pmr[1..], out num))
                result = ((int)val & (int)num) > 0;
        }
        else
        {
            if (pmr.StartsWith('@'))
                result = input.Equals(pmr[1..], StringComparison.CurrentCultureIgnoreCase);
            else if (pmr.StartsWith('='))
                result = input.Equals(pmr[1..]);
            else if (pmr.StartsWith('!'))
                result = !input.Equals(pmr[1..]);
            else
                result = value?.ToString() == parameter?.ToString();
        }

        /// result
        if (targetType == typeof(Visibility))
            return result ? Visibility.Visible : Visibility.Collapsed;
        else
            return result.ConvertTo(targetType);
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
