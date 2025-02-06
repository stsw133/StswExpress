using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// A value converter that checks whether the type of the provided value matches the specified type.
/// <br/>
/// - If the parameter is a `Type`, it directly compares the value's type.  
/// - If the parameter is a `string`, it attempts to resolve the type using `Type.GetType()`.  
/// <br/>
/// If the target type is <see cref="Visibility"/>, the result is <see cref="Visibility.Visible"/> when the condition is met, otherwise <see cref="Visibility.Collapsed"/>.
/// Otherwise, it returns a <see cref="bool"/> indicating whether the value's type matches the specified type.
/// </summary>
public class StswIsTypeConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswIsTypeConverter Instance => instance ??= new StswIsTypeConverter();
    private static StswIsTypeConverter? instance;

    /// <summary>
    /// Provides the singleton instance of the converter for XAML bindings.
    /// </summary>
    /// <param name="serviceProvider">A service provider that can provide services for the markup extension.</param>
    /// <returns>The singleton instance of the converter.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <summary>
    /// Compares the type of the provided value with the specified type.
    /// </summary>
    /// <param name="value">The source value whose type is checked.</param>
    /// <param name="targetType">The type to convert to.</param>
    /// <param name="parameter">The expected type (as a `Type` or `string`).</param>
    /// <param name="culture">The culture to use in the conversion.</param>
    /// <returns>
    /// - A <see cref="Visibility"/> value if the target type is <see cref="Visibility"/>.
    /// - A <see cref="bool"/> value indicating whether the value's type matches the specified type.
    /// </returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return StswConverterHelper.ConvertToTargetType(false, targetType);

        var expectedType = parameter switch
        {
            Type type => type,
            string typeName => Type.GetType(typeName, throwOnError: false, ignoreCase: true),
            _ => null
        };

        if (expectedType == null)
            return Binding.DoNothing;

        var isSameType = expectedType.IsAssignableFrom(value.GetType());

        return StswConverterHelper.ConvertToTargetType(isSameType, targetType);
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

/* usage:

<TextBlock Text="This is a text" Visibility="{Binding SelectedObject, Converter={x:Static se:StswIsTypeConverter.Instance}, ConverterParameter={x:Type System.String}}"/>

<TextBlock Text="This is a number" Visibility="{Binding SelectedItem, Converter={x:Static se:StswIsTypeConverter.Instance}, ConverterParameter={x:Type System.Double}}"/>

<TextBlock Text="Matches MyCustomClass" Visibility="{Binding SelectedItem, Converter={x:Static se:StswIsTypeConverter.Instance}, ConverterParameter={x:Type local:MyCustomClass}}"/>

<CheckBox IsChecked="{Binding SelectedItem, Converter={x:Static se:StswIsTypeConverter.Instance}, ConverterParameter={x:Type System.Int32}}"/>

*/
