using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using System.Collections;
using System.Data;
using System.Globalization;

namespace StswExpress.Avalonia;
/// <summary>
/// A value converter that checks if the input value contains a specified parameter.
/// <br/>
/// The converter parameter can be a single value or a collection.  
/// - Use `!` before a parameter value to specify disallowed elements.  
/// - If the target type is <see cref="Visibility"/>, the output will be <see cref="Visibility.Visible"/> when the condition is met, otherwise <see cref="Visibility.Collapsed"/>.  
/// - Otherwise, the output is a <see cref="bool"/> value.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;TextBlock Text="Found" Visibility="{Binding SelectedItems, Converter={x:Static se:StswContainsConverter.Instance}, ConverterParameter='Item1'}"/&gt;
/// &lt;TextBlock Text="Match" Visibility="{Binding SelectedItems, Converter={x:Static se:StswContainsConverter.Instance}, ConverterParameter='Item1,Item3'}"/&gt;
/// &lt;CheckBox Content="Option available" Visibility="{Binding AvailableOptions, Converter={x:Static se:StswContainsConverter.Instance}, ConverterParameter='Premium'}"/&gt;
/// &lt;TextBlock Text="Acceptable" Visibility="{Binding SelectedItems, Converter={x:Static se:StswContainsConverter.Instance}, ConverterParameter='Item1,!Item5'}"/&gt;
/// </code>
/// </example>
public class StswContainsConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswContainsConverter Instance => instance ??= new StswContainsConverter();
    private static StswContainsConverter? instance;

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var val = ConvertToStringList(value);
        var pmr = ConvertToStringList(parameter);

        var allowed = pmr.Where(x => !x.StartsWith('!')).ToHashSet();
        var disallowed = pmr.Where(x => x.StartsWith('!')).Select(x => x[1..]).ToHashSet();

        var result = val.Any(allowed.Contains) && val.All(v => !disallowed.Contains(v));

        return result.ConvertTo(targetType);
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => BindingOperations.DoNothing;

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
}
