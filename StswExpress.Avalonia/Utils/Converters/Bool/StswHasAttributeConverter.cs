using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using System.Globalization;
using System.Reflection;

namespace StswExpress.Avalonia;
/// <summary>
/// A value converter that checks if an enumeration field or a property of an object has a specific attribute.
/// It can be used in XAML bindings to control UI visibility or boolean conditions based on attribute presence.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;TextBlock Text="Deprecated Option" IsEnabled="{Binding MyEnumValue, Converter={x:Static local:StswHasAttributeConverter.Instance}, ConverterParameter=System.ObsoleteAttribute}"/&gt;
/// &lt;TextBlock Text="Warning: Deprecated" IsEnabled="{Binding MyObject.DeprecatedProperty, Converter={x:Static local:StswHasAttributeConverter.Instance}, ConverterParameter=System.ObsoleteAttribute}"/&gt;
/// </code>
/// </example>
public class StswHasAttributeConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswHasAttributeConverter Instance => instance ??= new StswHasAttributeConverter();
    private static StswHasAttributeConverter? instance;

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <inheritdoc/>
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

            return field.GetCustomAttribute(attributeType) != null;
        }
        else
        {
            var property = valueType.GetProperties().FirstOrDefault(prop => prop.GetValue(value) == value);
            if (property == null)
                return GetDefaultValue(targetType);

            return property.GetCustomAttribute(attributeType) != null;
        }
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => BindingOperations.DoNothing;

    /// <summary>
    /// Returns the default value based on the target type.
    /// </summary>
    /// <param name="targetType">The target type to determine the default value.</param>
    /// <returns><see cref="Visibility.Collapsed"/> for Visibility, <see langword="false"/> for boolean, and <see langword="null"/> otherwise.</returns>
    private static object? GetDefaultValue(Type targetType)
    {
        if (targetType == typeof(bool))
            return false;
        return null;
    }
}
