using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// Takes in an input value and a set of parameters in the form of a string separated by a tilde (~) character.
/// The first part is the condition to evaluate against the input value,
/// the second part is the value to return if the condition is <see langword="true"/>,
/// and the third part is the value to return if the condition is <see langword="false"/>.
/// </summary>
public class StswIfElseConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswIfElseConverter Instance => _instance ??= new StswIfElseConverter();
    private static StswIfElseConverter? _instance;

    /// <summary>
    /// Provides the singleton instance of the converter.
    /// </summary>
    /// <param name="serviceProvider">A service provider that can provide services for the markup extension.</param>
    /// <returns>The singleton instance of the converter.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <summary>
    /// Converts an input value to a specified output value based on the provided parameters.
    /// </summary>
    /// <param name="value">The input value.</param>
    /// <param name="targetType">The type of the target property.</param>
    /// <param name="parameter">A string containing the condition and values separated by a tilde (~) character.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>
    /// The value to return if the condition is <see langword="true"/>, or the value to return if the condition is <see langword="false"/>.
    /// </returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter == null)
            return null;

        var val = value?.ToString() ?? string.Empty;
        var pmr = parameter?.ToString()?.Split('~');

        /// result
        if (pmr?.Length >= 3)
            return val == pmr?[0] ? pmr?[1] : pmr?[2];
        
        return null;
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
