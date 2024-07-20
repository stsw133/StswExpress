using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// Used for color manipulation and conversion based on the provided parameters.
/// Examples: 'B-70% S50%', 'A50% B50% S0%'
/// </summary>
public class StswColorAdvancedConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswColorAdvancedConverter Instance => instance ??= new StswColorAdvancedConverter();
    private static StswColorAdvancedConverter? instance;

    /// <summary>
    /// Provides the singleton instance of the converter.
    /// </summary>
    /// <param name="serviceProvider">A service provider that can provide services for the markup extension.</param>
    /// <returns>The singleton instance of the converter.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <summary>
    /// Converts a color value based on advanced parameters for alpha, brightness, and saturation adjustments.
    /// </summary>
    /// <param name="value">The input color value.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter to use for adjustments.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>The color with adjusted properties.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var color = StswColorConverter.GetColorFromValue(value);
        var adjustments = ParseParameter(parameter?.ToString() ?? string.Empty);

        if (adjustments.TryGetValue('A', out var a) && StswColorAlphaConverter.Instance.Convert(color, typeof(Color), a, culture) is Color colorA)
            color = colorA;
        if (adjustments.TryGetValue('B', out var b) && StswColorBrightnessConverter.Instance.Convert(color, typeof(Color), b, culture) is Color colorB)
            color = colorB;
        if (adjustments.TryGetValue('S', out var s) && StswColorSaturationConverter.Instance.Convert(color, typeof(Color), s, culture) is Color colorS)
            color = colorS;

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

    /// <summary>
    /// Parses the parameter string into a dictionary of adjustments.
    /// </summary>
    /// <param name="parameter">The parameter string.</param>
    /// <returns>A dictionary containing the adjustments for alpha, brightness, and saturation.</returns>
    private static Dictionary<char, string> ParseParameter(string parameter)
    {
        var dictionary = new Dictionary<char, string>();
        var regex = new Regex(@"([A-Za-z])([^A-Za-z]*)");
        var matches = regex.Matches(parameter);

        foreach (var match in matches.Cast<Match>())
        {
            var key = match.Groups[1].Value[0];
            var val = match.Groups[2].Value.Trim();
            dictionary[key] = val;
        }

        return dictionary;
    }
}
