using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// A value converter that checks if an enumeration field or a property of an object has a specific attribute.
/// It can be used in XAML bindings to control UI visibility or boolean conditions based on attribute presence.
/// </summary>
[StswInfo("0.16.0")]
public class StswHasAttributeConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswHasAttributeConverter Instance => instance ??= new StswHasAttributeConverter();
    private static StswHasAttributeConverter? instance;

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <summary>
    /// Checks if the provided enumeration field or property has the specified attribute.
    /// </summary>
    /// <param name="value">The enumeration value or object property to check.</param>
    /// <param name="targetType">The target type for conversion (Visibility or bool).</param>
    /// <param name="parameter">The name of the attribute type to check for.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>
    /// Returns <see cref="Visibility.Visible"/> if the attribute is found and <see cref="Visibility.Collapsed"/> otherwise when targetType is Visibility.
    /// Returns <see langword="true"/> if the attribute is found and <see langword="false"/> otherwise when targetType is bool.
    /// Returns <see langword="null"/> for other target types.
    /// </returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return GetDefaultValue(targetType);

        var attributeTypeName = parameter.ToString() ?? string.Empty;
        var attributeType = Type.GetType(attributeTypeName);
        if (attributeType == null)
            return GetDefaultValue(targetType);

        var valueType = value.GetType();
        if (valueType.IsEnum)
        {
            var field = valueType.GetField(value.ToString()!);
            if (field == null)
                return GetDefaultValue(targetType);

            var hasAttribute = field.GetCustomAttribute(attributeType) != null;
            return GetReturnValue(targetType, hasAttribute);
        }
        else
        {
            var property = valueType.GetProperties().FirstOrDefault(prop => prop.GetValue(value) == value);
            if (property == null)
                return GetDefaultValue(targetType);

            var hasAttribute = property.GetCustomAttribute(attributeType) != null;
            return GetReturnValue(targetType, hasAttribute);
        }
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

    /// <summary>
    /// Returns the default value based on the target type.
    /// </summary>
    /// <param name="targetType">The target type to determine the default value.</param>
    /// <returns><see cref="Visibility.Collapsed"/> for Visibility, <see langword="false"/> for boolean, and <see langword="null"/> otherwise.</returns>
    private object? GetDefaultValue(Type targetType)
    {
        if (targetType == typeof(Visibility))
            return Visibility.Collapsed;
        if (targetType == typeof(bool))
            return false;
        return null;
    }

    /// <summary>
    /// Returns the converted value based on the target type and attribute presence.
    /// </summary>
    /// <param name="targetType">The target type (Visibility or bool).</param>
    /// <param name="hasAttribute">Indicates whether the attribute was found.</param>
    /// <returns>
    /// <see cref="Visibility.Visible"/> if the attribute is found and <see cref="Visibility.Collapsed"/> otherwise when targetType is Visibility.
    /// <see langword="true"/> if the attribute is found and <see langword="false"/> otherwise when targetType is bool.
    /// </returns>
    private object? GetReturnValue(Type targetType, bool hasAttribute)
    {
        return targetType == typeof(Visibility)
            ? (hasAttribute ? Visibility.Visible : Visibility.Collapsed)
            : hasAttribute;
    }
}

/* usage:

<TextBlock Text="Deprecated Option" Visibility="{Binding MyEnumValue, Converter={x:Static local:StswHasAttributeConverter.Instance}, ConverterParameter=System.ObsoleteAttribute}"/>

<TextBlock Text="Warning: Deprecated" Visibility="{Binding MyObject.DeprecatedProperty, Converter={x:Static local:StswHasAttributeConverter.Instance}, ConverterParameter=System.ObsoleteAttribute}"/>

*/
