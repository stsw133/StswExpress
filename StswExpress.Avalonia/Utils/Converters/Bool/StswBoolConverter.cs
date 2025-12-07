using System.Globalization;

namespace StswExpress.Avalonia;
/// <summary>
/// A value converter that transforms a <see cref="bool"/> value to a specified target type.
/// <br/>
/// If the parameter starts with '!', the conversion result is inverted.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;Button Content="Save" IsVisible="{Binding CanSave, Converter={x:Static se:StswBoolConverter.Instance}}"/&gt;
/// &lt;Button Content="Edit" IsVisible="{Binding IsEditing, Converter={x:Static se:StswBoolConverter.Instance}, ConverterParameter='!'}"/&gt;
/// &lt;TextBlock Text="{Binding IsAdmin, Converter={x:Static se:StswBoolConverter.Instance}, TargetType={x:Type sys:Int32}}"/&gt;
/// </code>
/// </example>
public class StswBoolConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswBoolConverter Instance => instance ??= new StswBoolConverter();
    private static StswBoolConverter? instance;
    
    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool val && !bool.TryParse(value?.ToString(), out val))
            return BindingOperations.DoNothing;

        var isReversed = parameter is string param && param.StartsWith('!');
        val ^= isReversed;

        return val.ConvertTo(targetType);
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => BindingOperations.DoNothing;
}
