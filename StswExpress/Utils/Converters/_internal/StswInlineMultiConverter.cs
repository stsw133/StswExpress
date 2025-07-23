using System;
using System.Globalization;
using System.Windows.Data;

namespace StswExpress;
/// <summary>
/// Provides a way to use inline delegate-based multi-value converters in XAML.
/// </summary>
/// <param name="convert">The convert delegate.</param>
/// <param name="convertBack">The convert back delegate.</param>
[StswInfo("0.3.0")]
internal class StswInlineMultiConverter(StswInlineMultiConverter.ConvertDelegate convert, StswInlineMultiConverter.ConvertBackDelegate? convertBack = null) : IMultiValueConverter
{
    /// <summary>
    /// Delegate for the Convert method.
    /// </summary>
    /// <param name="values">The array of values that the source bindings in the MultiBinding produces.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>A converted value.</returns>
    public delegate object ConvertDelegate(object[] values, Type targetType, object parameter, CultureInfo culture);
    private ConvertDelegate _convert { get; } = convert ?? throw new ArgumentNullException(nameof(convert));

    /// <summary>
    /// Delegate for the ConvertBack method.
    /// </summary>
    /// <param name="value">The value that the binding target produces.</param>
    /// <param name="targetTypes">The array of types to convert to.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>An array of values to be passed to the source bindings.</returns>
    public delegate object[] ConvertBackDelegate(object value, Type[] targetTypes, object parameter, CultureInfo culture);
    private ConvertBackDelegate? _convertBack { get; } = convertBack;

    /// <summary>
    /// Converts source values to a value for the binding target.
    /// </summary>
    /// <param name="values">The array of values that the source bindings in the MultiBinding produces.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>A converted value.</returns>
    public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture) => _convert(values, targetType, parameter, culture);

    /// <summary>
    /// Converts a binding target value to the source binding values.
    /// </summary>
    /// <param name="value">The value that the binding target produces.</param>
    /// <param name="targetTypes">The array of types to convert to.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>An array of values to be passed to the source bindings.</returns>
    /// <exception cref="NotImplementedException">Thrown when the convert back delegate is not provided.</exception>
    public object[]? ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return (_convertBack != null)
            ? _convertBack(value, targetTypes, parameter, culture)
            : throw new NotImplementedException();
    }
}