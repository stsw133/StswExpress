using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// Checks if the value parameter contains the converter parameter.
/// Use '!' at the beginning of the converter parameter to invert the output value.
/// <br/>
/// When targetType is <see cref="Visibility"/>, the output is <c>Visible</c> when <see langword="true"/>, otherwise <c>Collapsed</c>.
/// When targetType is anything else, it returns <see cref="bool"/> with a value depending on the converter result.
/// </summary>
public class StswContainsConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswContainsConverter Instance => instance ??= new StswContainsConverter();
    private static StswContainsConverter? instance;

    /// <summary>
    /// Provides the singleton instance of the converter.
    /// </summary>
    /// <param name="serviceProvider">A service provider that can provide services for the markup extension.</param>
    /// <returns>The singleton instance of the converter.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <summary>
    /// Checks if the value parameter contains the converter parameter.
    /// </summary>
    /// <param name="value">The value produced by the binding source.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter to check for containment.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>
    /// A <see cref="Visibility"/> value if the targetType is <see cref="Visibility"/>;
    /// otherwise, a <see cref="bool"/> value indicating whether the value contains the parameter.
    /// </returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var val = ConvertToStringList(value);
        var pmr = ConvertToStringList(parameter);

        var allowed = pmr.Where(x => !x.StartsWith('!'));
        var disallowed = pmr.Where(x => x.StartsWith('!')).Select(x => x[1..]);

        /// result
        var result = val.Any(v => allowed.Contains(v)) && val.All(v => !disallowed.Contains(v));

        return targetType switch
        {
            Type t when t == typeof(Visibility) => result ? Visibility.Visible : Visibility.Collapsed,
            _ => result.ConvertTo(targetType)
        };
    }
    private static IEnumerable<string> ConvertToStringList(object? input) => input switch
    {
        null => [],
        string s => [s],
        IEnumerable e => e.Cast<object>().Select(x => x?.ToString() ?? string.Empty),
        _ => [input.ToString() ?? string.Empty],
    };

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
