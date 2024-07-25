using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// Converts a range specified by the parameter to a list of integers for data binding purposes.
/// The converter expects the parameter to be a string representing a range in the format "start-end" or "end" (e.g., "2-5" or "3").
/// The generated list contains integers in the specified range using <see cref="Enumerable.Range"/>.
/// </summary>
/// <remarks>
/// This can be useful for populating collection controls such as ComboBox or ListBox.
/// </remarks>
[Obsolete("Will be propably replaced with XAML MarkupExtension in the future.")]
public class StswListFromRangeConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswListFromRangeConverter Instance => instance ??= new StswListFromRangeConverter();
    private static StswListFromRangeConverter? instance;

    /// <summary>
    /// Provides the singleton instance of the converter.
    /// </summary>
    /// <param name="serviceProvider">A service provider that can provide services for the markup extension.</param>
    /// <returns>The singleton instance of the converter.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <summary>
    /// Converts a range specified by the parameter to a list of integers.
    /// </summary>
    /// <param name="value">The value produced by the binding source.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">A string representing the range (e.g., "2-5" or "3").</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>A list of integers in the specified range.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter?.ToString()?.Split('-') is string[] param)
        {
            if (param.Length == 2 && int.TryParse(param[0], out var start) && int.TryParse(param[1], out var end))
            {
                return Enumerable.Range(start, end - start + 1);
            }
            else if (param.Length == 1 && int.TryParse(param[0], out var count))
            {
                return Enumerable.Range(0, count);
            }
        }
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
