using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// A value converter that checks if the input value contains a specified parameter.
/// <br/>
/// The converter parameter can be a single value or a collection.  
/// - Use `!` before a parameter value to specify disallowed elements.  
/// - If the target type is <see cref="Visibility"/>, the output will be <see cref="Visibility.Visible"/> when the condition is met, otherwise <see cref="Visibility.Collapsed"/>.  
/// - Otherwise, the output is a <see cref="bool"/> value.
/// </summary>
public class StswContainsConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswContainsConverter Instance => instance ??= new StswContainsConverter();
    private static StswContainsConverter? instance;

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <summary>
    /// Checks if the value parameter contains the converter parameter.
    /// </summary>
    /// <param name="value">The source value to check.</param>
    /// <param name="targetType">The type to convert to.</param>
    /// <param name="parameter">The expected value(s) to check against.</param>
    /// <param name="culture">The culture to use in the conversion.</param>
    /// <returns>
    /// - A <see cref="Visibility"/> value if the target type is <see cref="Visibility"/>.
    /// - A <see cref="bool"/> value indicating whether the value contains the parameter.
    /// </returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var val = ConvertToStringList(value);
        var pmr = ConvertToStringList(parameter);

        var allowed = pmr.Where(x => !x.StartsWith('!')).ToHashSet();
        var disallowed = pmr.Where(x => x.StartsWith('!')).Select(x => x[1..]).ToHashSet();

        var result = val.Any(allowed.Contains) && val.All(v => !disallowed.Contains(v));

        return targetType == typeof(Visibility)
            ? result ? Visibility.Visible : Visibility.Collapsed
            : result.ConvertTo(targetType);
    }

    /// <summary>
    /// Converts an object into a collection of strings.
    /// </summary>
    /// <param name="input">The input value.</param>
    /// <returns>A collection of strings representing the input value.</returns>
    private static IEnumerable<string> ConvertToStringList(object? input) => input switch
    {
        null => [],
        string s => s.Split([','], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries),
        IEnumerable e => e.Cast<object>().Select(x => x?.ToString() ?? string.Empty),
        _ => [input.ToString() ?? string.Empty],
    };

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

<TextBlock Text="Found" Visibility="{Binding SelectedItems, Converter={x:Static se:StswContainsConverter.Instance}, ConverterParameter='Item1'}"/>

<TextBlock Text="Match" Visibility="{Binding SelectedItems, Converter={x:Static se:StswContainsConverter.Instance}, ConverterParameter='Item1,Item3'}"/>

<CheckBox Content="Option available" Visibility="{Binding AvailableOptions, Converter={x:Static se:StswContainsConverter.Instance}, ConverterParameter='Premium'}"/>

<TextBlock Text="Acceptable" Visibility="{Binding SelectedItems, Converter={x:Static se:StswContainsConverter.Instance}, ConverterParameter='Item1,!Item5'}"/>

*/
