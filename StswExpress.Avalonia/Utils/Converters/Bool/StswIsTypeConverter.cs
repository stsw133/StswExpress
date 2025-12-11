using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using System.Globalization;

namespace StswExpress.Avalonia;
/// <summary>
/// A value converter that checks whether the type of the provided value matches the specified type.
/// <br/>
/// - If the parameter is a `Type`, it directly compares the value's type.  
/// - If the parameter is a `string`, it attempts to resolve the type using `Type.GetType()`.  
/// <br/>
/// It returns a <see cref="bool"/> indicating whether the value's type matches the specified type.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;TextBlock Text="This is a text" IsEnabled="{Binding SelectedObject, Converter={x:Static se:StswIsTypeConverter.Instance}, ConverterParameter={x:Type System.String}}"/&gt;
/// &lt;TextBlock Text="This is a number" IsEnabled="{Binding SelectedItem, Converter={x:Static se:StswIsTypeConverter.Instance}, ConverterParameter={x:Type System.Double}}"/&gt;
/// &lt;TextBlock Text="Matches MyCustomClass" IsEnabled="{Binding SelectedItem, Converter={x:Static se:StswIsTypeConverter.Instance}, ConverterParameter={x:Type local:MyCustomClass}}"/&gt;
/// &lt;CheckBox IsChecked="{Binding SelectedItem, Converter={x:Static se:StswIsTypeConverter.Instance}, ConverterParameter={x:Type System.Int32}}"/&gt;
/// </code>
/// </example>
public class StswIsTypeConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswIsTypeConverter Instance => instance ??= new StswIsTypeConverter();
    private static StswIsTypeConverter? instance;

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return false;

        var expectedType = parameter switch
        {
            Type type => type,
            string typeName => Type.GetType(typeName, throwOnError: false, ignoreCase: true),
            _ => null
        };

        if (expectedType == null)
            return BindingOperations.DoNothing;

        var isSameType = expectedType.IsAssignableFrom(value.GetType());

        return isSameType.ConvertTo(targetType);
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => BindingOperations.DoNothing;
}
