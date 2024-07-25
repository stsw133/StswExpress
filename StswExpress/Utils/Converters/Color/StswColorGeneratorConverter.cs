using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// Generates a new color based on the passed value and the provided seed (brightness) as a parameter.
/// </summary>
public class StswColorGeneratorConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswColorGeneratorConverter Instance => instance ??= new StswColorGeneratorConverter();
    private static StswColorGeneratorConverter? instance;

    /// <summary>
    /// Provides the singleton instance of the converter.
    /// </summary>
    /// <param name="serviceProvider">A service provider that can provide services for the markup extension.</param>
    /// <returns>The singleton instance of the converter.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <summary>
    /// Converts a value to a new color based on the provided seed (brightness) parameter.
    /// </summary>
    /// <param name="value">The input value used to generate the color.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The seed (brightness) parameter to use for generating the color.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>The generated color based on the input value and seed parameter.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var pmr = parameter?.ToString() ?? string.Empty;
        var val = value?.ToString() ?? string.Empty;
        if (!int.TryParse(pmr, out var seed))
            seed = -1;

        var color = StswFn.GenerateColor(val, seed);

        return StswColorConverter.GetResultFromColor(color, targetType);
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
