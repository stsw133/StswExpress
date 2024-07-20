using System;
using System.Collections;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// Allows the user to sum up the values of a specified property of each item in a collection or a <see cref="DataTable"/>.
/// </summary>
public class StswSumConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswSumConverter Instance => instance ??= new StswSumConverter();
    private static StswSumConverter? instance;

    /// <summary>
    /// Provides the singleton instance of the converter.
    /// </summary>
    /// <param name="serviceProvider">A service provider that can provide services for the markup extension.</param>
    /// <returns>The singleton instance of the converter.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <summary>
    /// Sums up the values of a specified property of each item in a collection or a <see cref="DataTable"/>.
    /// </summary>
    /// <param name="value">The value produced by the binding source, either a collection or a <see cref="DataTable"/>.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The name of the property whose values will be summed up.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>
    /// The sum of the values of the specified property.
    /// </returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var propertyName = parameter?.ToString() ?? string.Empty;
        var result = 0d;

        if (value is DataTable dataTable)
        {
            result = dataTable.Rows
                              .OfType<DataRow>()
                              .Sum(row => double.TryParse(row[propertyName]?.ToString(), NumberStyles.Number, culture, out var val) ? val : 0);
        }
        else if (value is IEnumerable enumerable)
        {
            result = enumerable.OfType<object>()
                               .Sum(item => item.GetType().GetProperty(propertyName) is PropertyInfo prop
                                            ? System.Convert.ToDouble(prop.GetValue(item), culture)
                                            : 0);
        }

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
