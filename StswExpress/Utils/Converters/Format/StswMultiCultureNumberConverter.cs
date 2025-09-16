using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// Converts a numeric value into a string with a culture-specific decimal separator, 
/// allowing both dot (`.`) and comma (`,`) as valid input separators.
/// <br/>
/// This converter is particularly useful in `DataGrid` or `TextBox` bindings where users 
/// may enter decimal numbers using different separator conventions.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;TextBox Text="{Binding Amount, Converter={x:Static se:StswMultiCultureNumberConverter.Instance}}"/&gt;
/// &lt;DataGridTextColumn Binding="{Binding Price, Converter={x:Static se:StswMultiCultureNumberConverter.Instance}}"/&gt;
/// </code>
/// </example>
[StswInfo(null)]
public class StswMultiCultureNumberConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswMultiCultureNumberConverter Instance => instance ??= new StswMultiCultureNumberConverter();
    private static StswMultiCultureNumberConverter? instance;

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || !decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var val))
            return Binding.DoNothing;

        var ci = (CultureInfo)culture.Clone();
        ci.NumberFormat.NumberDecimalSeparator = culture.NumberFormat.NumberDecimalSeparator;

        return val.ToString("G", ci);
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string input || string.IsNullOrWhiteSpace(input))
            return Binding.DoNothing;

        var normalizedInput = input.Replace(',', '.');
        if (decimal.TryParse(normalizedInput, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return System.Convert.ChangeType(result, targetType, culture);

        return Binding.DoNothing;
    }
}
