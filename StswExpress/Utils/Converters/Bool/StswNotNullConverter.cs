using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// Checks if the value parameter is not null.
/// <br/>
/// When targetType is <see cref="Visibility"/>, the output is <c>Visible</c> when <see langword="true"/>, otherwise <c>Collapsed</c>.
/// When targetType is anything else, it returns <see cref="bool"/> with a value depending on the converter result.
/// </summary>
public class StswNotNullConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswNotNullConverter Instance => instance ??= new StswNotNullConverter();
    private static StswNotNullConverter? instance;

    /// <summary>
    /// Provides the singleton instance of the converter.
    /// </summary>
    /// <param name="serviceProvider">A service provider that can provide services for the markup extension.</param>
    /// <returns>The singleton instance of the converter.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <summary>
    /// Converts a value to the specified targetType based on whether it is not null.
    /// </summary>
    /// <param name="value">The value produced by the binding source.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>
    /// A <see cref="Visibility"/> value if the targetType is <see cref="Visibility"/>;
    /// otherwise, a <see cref="bool"/> value indicating whether the value is not null.
    /// </returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (targetType == typeof(Visibility))
            return value is not null ? Visibility.Visible : Visibility.Collapsed;
        else
            return (value is not null).ConvertTo(targetType);
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
