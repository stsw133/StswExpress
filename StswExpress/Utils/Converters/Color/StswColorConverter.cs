using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// A powerful color converter that allows multiple transformations on a color value.
/// <br/>
/// Supports conversion between <see cref="Color"/>, <see cref="Brush"/>, <see cref="SolidColorBrush"/>, 
/// <see cref="System.Drawing.Color"/>, and HTML color strings.
/// <br/>
/// Allows multiple chained operations using parameters such as:
/// - `G` (Generate color from input value)
/// - `A` (Alpha adjustment)
/// - `B` (Brightness adjustment)
/// - `S` (Saturation adjustment)
/// <br/>
/// Example usages:
/// - `"S40%"` → Adjust saturation to 40%
/// - `"A20% B-10%"` → Reduce alpha to 20%, decrease brightness by 10%
/// - `"G B15% S25%"` → Generate color from value, then apply brightness & saturation modifications
/// </summary>
[Stsw("0.16.0")]
public class StswColorConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswColorConverter Instance => instance ??= new StswColorConverter();
    private static StswColorConverter? instance;

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <summary>
    /// Converts an input color based on specified parameters.
    /// </summary>
    /// <param name="value">The input value, which can be a color, a brush, a string, or a custom object.</param>
    /// <param name="targetType">The target type for conversion (e.g., <see cref="Color"/>, <see cref="Brush"/>).</param>
    /// <param name="parameter">A space-separated string defining operations to apply on the color.</param>
    /// <param name="culture">The culture for parsing numeric values.</param>
    /// <returns>The transformed color in the target type.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var color = Colors.Transparent;
        var parameters = ParseParameters(parameter);

        if (parameters.ContainsKey('G'))
            color = StswFnUI.GenerateColor(value?.ToString() ?? "", -1);
        else
            color = GetColorFromValue(value);

        if (parameters.TryGetValue('A', out var alphaParam))
            color = ApplyAlpha(color, alphaParam, culture);
        if (parameters.TryGetValue('B', out var brightnessParam))
            color = ApplyBrightness(color, brightnessParam, culture);
        if (parameters.TryGetValue('S', out var saturationParam))
            color = ApplySaturation(color, saturationParam, culture);

        return GetResultFromColor(color, targetType);
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
    /// Parses the converter parameter string and extracts operations to apply on the color.
    /// </summary>
    /// <param name="parameter">The converter parameter string containing transformations (e.g., "B20% A50%").</param>
    /// <returns>A dictionary mapping operation keys ('A', 'B', 'S', 'G') to their respective values.</returns>
    private static Dictionary<char, string> ParseParameters(object? parameter)
    {
        var adjustments = new Dictionary<char, string>();
        var regex = new Regex(@"([A-Za-z])([^A-Za-z]*)");
        foreach (Match match in regex.Matches(parameter?.ToString() ?? string.Empty))
        {
            var key = match.Groups[1].Value[0];
            var val = match.Groups[2].Value.Trim();
            adjustments[key] = val;
        }
        return adjustments;
    }

    /// <summary>
    /// Extracts a <see cref="Color"/> from an input value, handling various formats.
    /// </summary>
    /// <param name="value">
    /// The input value, which can be:
    /// - A <see cref="Color"/> object
    /// - A <see cref="SolidColorBrush"/>
    /// - A <see cref="System.Drawing.Color"/>
    /// - A valid HTML color string (e.g., "#FF5733" or "Red").
    /// </param>
    /// <returns>A valid <see cref="Color"/>. If the value is not recognized, returns <see cref="Colors.Transparent"/>.</returns>
    private static Color GetColorFromValue(object? value)
    {
        if (value == DependencyProperty.UnsetValue)
            return Colors.Transparent;

        return value switch
        {
            Color c => c,
            System.Drawing.Color d => Color.FromArgb(d.A, d.R, d.G, d.B),
            SolidColorBrush br => br.Color,
            _ => (Color) ColorConverter.ConvertFromString(value?.ToString() ?? "Transparent")
        };
    }

    /// <summary>
    /// Converts a <see cref="Color"/> into the desired output type.
    /// </summary>
    /// <param name="color">The source color.</param>
    /// <param name="targetType">
    /// The type to convert to. Supported types:
    /// - <see cref="Color"/>
    /// - <see cref="System.Drawing.Color"/>
    /// - <see cref="SolidColorBrush"/>
    /// - String representation of the color.
    /// </param>
    /// <returns>The converted value in the requested type.</returns>
    private static object GetResultFromColor(Color color, Type targetType)
    {
        return targetType switch
        {
            Type t when t == typeof(Color) || t == typeof(Color?) => color,
            Type t when t == typeof(System.Drawing.Color) || t == typeof(System.Drawing.Color?) => System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B),
            Type t when t == typeof(Brush) || t == typeof(SolidColorBrush) => new SolidColorBrush(color),
            _ => color.ToString()
        };
    }

    /// <summary>
    /// Adjusts the alpha (transparency) of the given color.
    /// </summary>
    /// <param name="color">The original color.</param>
    /// <param name="parameter">The alpha adjustment value (0-255 or 0-100% for percentage).</param>
    /// <param name="culture">The culture used for parsing the numeric value.</param>
    /// <returns>The color with adjusted alpha.</returns>
    private static Color ApplyAlpha(Color color, string parameter, CultureInfo culture)
    {
        var isPercent = parameter.EndsWith('%');
        var pmr = double.TryParse(parameter.TrimEnd('%'), NumberStyles.Number, culture, out var pmrVal) ? pmrVal : (isPercent ? 100 : 255);
        var a = Math.Clamp(isPercent ? pmr * 255 / 100 : pmr, 0, 255);
        return Color.FromArgb((byte)Math.Round(a), color.R, color.G, color.B);
    }

    /// <summary>
    /// Adjusts the brightness of the given color.
    /// </summary>
    /// <param name="color">The original color.</param>
    /// <param name="parameter">
    /// The brightness adjustment value:
    /// - If prefixed with '?', brightness will be adjusted automatically based on the current brightness.
    /// - If ending with '%', the value is treated as a percentage.
    /// - Otherwise, the value is treated as an absolute adjustment (-255 to 255).
    /// </param>
    /// <param name="culture">The culture used for parsing the numeric value.</param>
    /// <returns>The color with adjusted brightness.</returns>
    private static Color ApplyBrightness(Color color, string parameter, CultureInfo culture)
    {
        var isAuto = parameter.StartsWith('?');
        var isPercent = parameter.EndsWith('%');
        var pmr = parameter.TrimStart('?').TrimEnd('%');
        var pmrVal = double.TryParse(pmr, NumberStyles.Number, culture, out var val) ? val : 0;

        if (isAuto)
            pmrVal = color.ToDrawingColor().GetBrightness() < 0.5 ? Math.Abs(pmrVal) : -Math.Abs(pmrVal);

        return AdjustBrightness(color, pmrVal, isPercent);
    }

    /// <summary>
    /// Adjusts the brightness of a given color.
    /// </summary>
    /// <param name="color">The original color.</param>
    /// <param name="pmrVal">The brightness adjustment value (-255 to 255 or percentage).</param>
    /// <param name="isPercent">Indicates whether the value is in percentage.</param>
    /// <returns>The color with modified brightness.</returns>
    private static Color AdjustBrightness(Color color, double pmrVal, bool isPercent)
    {
        double r = color.R, g = color.G, b = color.B;
        if (isPercent)
        {
            r += (pmrVal > 0 ? 255 - r : r) * pmrVal / 100;
            g += (pmrVal > 0 ? 255 - g : g) * pmrVal / 100;
            b += (pmrVal > 0 ? 255 - b : b) * pmrVal / 100;
        }
        return Color.FromArgb(color.A, (byte)Math.Round(Math.Clamp(r, 0, 255)),
                                       (byte)Math.Round(Math.Clamp(g, 0, 255)),
                                       (byte)Math.Round(Math.Clamp(b, 0, 255)));
    }

    /// <summary>
    /// Adjusts the saturation of the given color.
    /// </summary>
    /// <param name="color">The original color.</param>
    /// <param name="parameter">The saturation adjustment value (0-100%).</param>
    /// <param name="culture">The culture used for parsing the numeric value.</param>
    /// <returns>The color with adjusted saturation.</returns>
    private static Color ApplySaturation(Color color, string parameter, CultureInfo culture)
    {
        var pmr = double.TryParse(parameter.TrimEnd('%'), NumberStyles.Number, culture, out var pmrVal) ? 100 - pmrVal : 100;
        return AdjustSaturation(color, pmr);
    }

    /// <summary>
    /// Modifies the saturation of a color.
    /// </summary>
    /// <param name="color">The original color.</param>
    /// <param name="pmrVal">The saturation adjustment value (0-100%).</param>
    /// <returns>The color with modified saturation.</returns>
    private static Color AdjustSaturation(Color color, double pmrVal)
    {
        double r = color.R, g = color.G, b = color.B;
        double avg = (r + g + b) / 3;
        r = color.R - (color.R - avg) * pmrVal / 100;
        g = color.G - (color.G - avg) * pmrVal / 100;
        b = color.B - (color.B - avg) * pmrVal / 100;
        return Color.FromArgb(color.A, (byte)Math.Round(r), (byte)Math.Round(g), (byte)Math.Round(b));
    }
}

/* usage:

<Border Background="{Binding MyBrush, Converter={x:Static se:StswColorConverter.Instance}}" />

<Border Background="{Binding MyBrush, Converter={x:Static se:StswColorConverter.Instance}, ConverterParameter='A50%'}"/>

<Border Background="{Binding MyBrush, Converter={x:Static se:StswColorConverter.Instance}, ConverterParameter='B?20%'}"/>

<TextBlock Foreground="{Binding MyText, Converter={x:Static se:StswColorConverter.Instance}, ConverterParameter='G'}"/>

*/
