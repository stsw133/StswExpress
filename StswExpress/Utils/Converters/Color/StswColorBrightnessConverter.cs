using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// Takes a color value as input and changes its brightness based on the provided parameters:
/// Use nothing or '+' at the beginning of the converter parameter to increase the brightness of the output color.
/// Use '-' at the beginning of the converter parameter to decrease the brightness of the output color.
/// Use '?' at the beginning of the converter parameter to automatically decide if the converter needs to increase or decrease the brightness of the output color.
/// Use '%' at the end of the converter parameter to use percent values.
/// Use a value in the parameter between -255 and 255 (or -100 to 100 in the case of percents) to set the brightness of the output color.
/// Examples: '16', '+25', '-36', '?49', '8%', '+13%', '-18%', '?25%'
/// </summary>
public class StswColorBrightnessConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswColorBrightnessConverter Instance => instance ??= new StswColorBrightnessConverter();
    private static StswColorBrightnessConverter? instance;

    /// <summary>
    /// Provides the singleton instance of the converter.
    /// </summary>
    /// <param name="serviceProvider">A service provider that can provide services for the markup extension.</param>
    /// <returns>The singleton instance of the converter.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <summary>
    /// Converts a color value to a brighter or darker color based on the converter parameter.
    /// </summary>
    /// <param name="value">The input color value.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter to use for adjusting brightness.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>The color with the adjusted brightness.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var pmr = parameter?.ToString() ?? string.Empty;
        var color = StswColorConverter.GetColorFromValue(value);

        /// parameters
        var isAuto = pmr.StartsWith('?');
        var isPercent = pmr.EndsWith('%');
        if (isAuto) pmr = pmr.TrimStart('?');
        if (isPercent) pmr = pmr.TrimEnd('%');

        /// func
        if (!double.TryParse(pmr, NumberStyles.Number, culture, out var pmrVal))
            pmrVal = 0;

        if (isAuto)
            pmrVal = color.ToDrawingColor().GetBrightness() < 0.5 ? Math.Abs(pmrVal) : -Math.Abs(pmrVal);

        color = AdjustBrightness(color, pmrVal, isPercent);

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
    /// Adjusts the brightness of a <see cref="Color"/> based on the given parameter.
    /// </summary>
    /// <param name="color">The original color.</param>
    /// <param name="pmrVal">The brightness adjustment value.</param>
    /// <param name="isPercent">Indicates whether the adjustment value is in percent.</param>
    /// <returns>The color with adjusted brightness.</returns>
    private static Color AdjustBrightness(Color color, double pmrVal, bool isPercent)
    {
        double r = color.R, g = color.G, b = color.B;

        if (isPercent)
        {
            r += (pmrVal > 0 ? 255 - r : r) * pmrVal / 100;
            g += (pmrVal > 0 ? 255 - g : g) * pmrVal / 100;
            b += (pmrVal > 0 ? 255 - b : b) * pmrVal / 100;
        }
        else
        {
            var minMax = pmrVal > 0 ? new[] { r, g, b }.Min() : new[] { r, g, b }.Max();
            var multiplier = pmrVal > 0 ? 255 - minMax : minMax;

            if ((pmrVal > 0 && minMax != 255) || (pmrVal < 0 && minMax != 0))
            {
                r += (pmrVal > 0 ? 255 - r : r) / multiplier * pmrVal;
                g += (pmrVal > 0 ? 255 - g : g) / multiplier * pmrVal;
                b += (pmrVal > 0 ? 255 - b : b) / multiplier * pmrVal;
            }
        }

        r = Math.Clamp(r, 0, 255);
        g = Math.Clamp(g, 0, 255);
        b = Math.Clamp(b, 0, 255);

        return Color.FromArgb(color.A, (byte)Math.Round(r), (byte)Math.Round(g), (byte)Math.Round(b));
    }
}
