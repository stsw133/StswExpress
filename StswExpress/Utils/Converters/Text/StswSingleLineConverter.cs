using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// A value converter and XAML markup extension that collapses multi-line text into a single line.
/// This converter replaces carriage return ('\r') and newline ('\n') characters with a single space.
/// Use this converter in XAML when binding multi-line string sources to controls that should display a single-line representation.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;DataGridTextColumn Header="Description" Binding="{Binding Description, Converter={x:Static se:StswSingleLineConverter.Instance}}"/&gt;
/// </code>
/// </example>
public class StswSingleLineConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswSingleLineConverter Instance => _instance ??= new StswSingleLineConverter();
    private static StswSingleLineConverter? _instance;

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string text)
            return text.Replace("\r", " ").Replace("\n", " ");
        
        return value;
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => Binding.DoNothing;
}
