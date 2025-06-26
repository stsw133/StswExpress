using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// A value converter that transforms a <see cref="bool"/> value to a specified target type.
/// <br/>
/// If the parameter starts with '!', the conversion result is inverted.
/// <br/>
/// When converting to <see cref="Visibility"/>, <see langword="true"/> results in <see cref="Visibility.Visible"/>, 
/// while <see langword="false"/> results in <see cref="Visibility.Collapsed"/>.
/// For other target types, the boolean value is directly converted.
/// </summary>
public class StswBoolConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswBoolConverter Instance => instance ??= new StswBoolConverter();
    private static StswBoolConverter? instance;
    
    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <summary>
    /// Converts a <see cref="bool"/> value to a specified target type.
    /// </summary>
    /// <param name="value">The source value to convert.</param>
    /// <param name="targetType">The type to convert to.</param>
    /// <param name="parameter">Optional parameter; if prefixed with '!', inverts the boolean value.</param>
    /// <param name="culture">The culture to use in the conversion.</param>
    /// <returns>
    /// - A <see cref="Visibility"/> value if the target type is <see cref="Visibility"/>.
    /// - A <see cref="bool"/> or its equivalent for other types.
    /// </returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool val && !bool.TryParse(value?.ToString(), out val))
            return Binding.DoNothing;

        var isReversed = parameter is string param && param.StartsWith('!');
        val ^= isReversed;

        return targetType == typeof(Visibility)
            ? val ? Visibility.Visible : Visibility.Collapsed
            : val.ConvertTo(targetType);
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

<Button Content="Save" Visibility="{Binding CanSave, Converter={x:Static se:StswBoolConverter.Instance}}"/>

<Button Content="Edit" Visibility="{Binding IsEditing, Converter={x:Static se:StswBoolConverter.Instance}, ConverterParameter='!' }"/>

<TextBlock Text="{Binding IsAdmin, Converter={x:Static se:StswBoolConverter.Instance}, TargetType={x:Type sys:Int32}}"/>

*/
