using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// Compares the value's type to the type specified in the parameter.
/// </summary>
public class StswIsTypeConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswIsTypeConverter Instance => instance ??= new StswIsTypeConverter();
    private static StswIsTypeConverter? instance;

    /// <summary>
    /// Provides the singleton instance of the converter.
    /// </summary>
    /// <param name="serviceProvider">A service provider that can provide services for the markup extension.</param>
    /// <returns>The singleton instance of the converter.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <summary>
    /// Compares the value's type to the type specified in the parameter.
    /// </summary>
    /// <param name="value">The value produced by the binding source.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The type to compare with the value's type.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>
    /// A <see cref="Visibility"/> value if the targetType is <see cref="Visibility"/>;
    /// otherwise, a <see cref="bool"/> value indicating whether the value's type matches the specified type.
    /// </returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value?.GetType() is Type valType && parameter is Type paramType)
        {
            bool isSameType = valType == paramType;

            if (targetType == typeof(Visibility))
                return isSameType ? Visibility.Visible : Visibility.Collapsed;
            else
                return isSameType.ConvertTo(targetType);
        }
       
        return Binding.DoNothing;
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
