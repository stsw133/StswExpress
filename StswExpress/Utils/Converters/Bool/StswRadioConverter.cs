using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows;

namespace StswExpress;
/// <summary>
/// Compares the value parameter to the converter parameter.
/// Use '!' at the beginning of the converter parameter to invert the output value.
/// <br/>
/// When targetType is <see cref="Visibility"/>, the output is <c>Visible</c> when <see langword="true"/>, otherwise <c>Collapsed</c>.
/// When targetType is anything else, it returns <see cref="bool"/> with a value depending on the converter result.
/// </summary>
public class StswRadioConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswRadioConverter Instance => instance ??= new StswRadioConverter();
    private static StswRadioConverter? instance;

    /// <summary>
    /// Provides the singleton instance of the converter.
    /// </summary>
    /// <param name="serviceProvider">A service provider that can provide services for the markup extension.</param>
    /// <returns>The singleton instance of the converter.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <summary>
    /// Compares the value parameter to the converter parameter.
    /// </summary>
    /// <param name="value">The value produced by the binding source.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter to compare against the value.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>
    /// A <see cref="Visibility"/> value if the targetType is <see cref="Visibility"/>;
    /// otherwise, a <see cref="bool"/> value indicating whether the value matches the parameter.
    /// </returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var val = value?.ToString() ?? string.Empty;
        var pmr = parameter?.ToString() ?? string.Empty;

        /// parameters
        var isReversed = pmr.StartsWith('!');
        if (isReversed)
            pmr = pmr[1..];

        /// result
        var result = (val == pmr) ^ isReversed;

        if (targetType == typeof(Visibility))
            return result ? Visibility.Visible : Visibility.Collapsed;
        else
            return result.ConvertTo(targetType);
    }

    /// <summary>
    /// Converts the value back to the converter parameter.
    /// </summary>
    /// <param name="value">The value produced by the binding target.</param>
    /// <param name="targetType">The type to convert to.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>The converter parameter.</returns>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => parameter;
}