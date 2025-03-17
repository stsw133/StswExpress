using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// A value converter that conditionally returns different values based on an input condition.
/// <br/>
/// The converter parameter must be a string with three parts separated by a tilde (`~`):  
/// - The first part is the condition to evaluate against the input value.  
/// - The second part is the value to return if the condition is <see langword="true"/>.  
/// - The third part is the value to return if the condition is <see langword="false"/>.  
/// <br/>
/// Example usage: `"Admin~Yes~No"` will return `"Yes"` if the input value is `"Admin"`, otherwise `"No"`.
/// </summary>
public class StswIfElseConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswIfElseConverter Instance => _instance ??= new StswIfElseConverter();
    private static StswIfElseConverter? _instance;

    /// <summary>
    /// Provides the singleton instance of the converter for XAML bindings.
    /// </summary>
    /// <param name="serviceProvider">A service provider that can provide services for the markup extension.</param>
    /// <returns>The singleton instance of the converter.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <summary>
    /// Converts an input value to a specified output value based on the provided parameters.
    /// </summary>
    /// <param name="value">The input value.</param>
    /// <param name="targetType">The type of the target property.</param>
    /// <param name="parameter">A string containing the condition and values separated by a tilde (`~`).</param>
    /// <param name="culture">The culture to use in the conversion.</param>
    /// <returns>
    /// The value to return if the condition is <see langword="true"/>, or the value to return if the condition is <see langword="false"/>.
    /// </returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter == null)
            return null;

        var parts = parameter.ToString()?.Split('~');
        if (parts == null || parts.Length < 3)
            return null;

        var val = value?.ToString() ?? string.Empty;
        return parts.Length switch
        {
            >= 5 => val == parts[0] ? parts[1] : val == parts[2] ? parts[3] : parts[4],
            _ => val == parts[0] ? parts[1] : parts[2]
        };
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

<TextBlock Text="{Binding UserRole, Converter={x:Static se:StswIfElseConverter.Instance}, ConverterParameter='Admin~Yes~No'}"/>

<Button Content="{Binding Status, Converter={x:Static se:StswIfElseConverter.Instance}, ConverterParameter='Active~Stop~Start'}"/>

<TextBlock Text="Only for Admins" Visibility="{Binding UserRole, Converter={x:Static se:StswIfElseConverter.Instance}, ConverterParameter='Admin~Visible~Collapsed'}"/>

<CheckBox IsChecked="{Binding UserRole, Converter={x:Static se:StswIfElseConverter.Instance}, ConverterParameter='Admin~True~False'}"/>

*/
