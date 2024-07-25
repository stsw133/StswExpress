using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// Takes a color value as input and changes its saturation based on the provided parameters:
/// Use nothing or '%' at the end of the converter parameter to increase the saturation of the output color.
/// Use a value in the parameter between 0% and 100% to set the saturation of the output color.
/// Examples: '8%', '13%', '18%', '25%'
/// </summary>
public class StswColorSaturationConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswColorSaturationConverter Instance => instance ??= new StswColorSaturationConverter();
    private static StswColorSaturationConverter? instance;

    /// <summary>
    /// Provides the singleton instance of the converter.
    /// </summary>
    /// <param name="serviceProvider">A service provider that can provide services for the markup extension.</param>
    /// <returns>The singleton instance of the converter.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <summary>
    /// Converts a color value to a color with adjusted saturation based on the converter parameter.
    /// </summary>
    /// <param name="value">The input color value.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter to use for adjusting saturation.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>The color with adjusted saturation.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var pmr = parameter?.ToString() ?? string.Empty;
        var color = StswColorConverter.GetColorFromValue(value);

        /// parameters
        pmr = pmr.TrimEnd('%');

        /// func
        if (!double.TryParse(pmr, NumberStyles.Number, culture, out var pmrVal))
            pmrVal = 100;
        pmrVal = 100 - pmrVal;

        color = AdjustSaturation(color, pmrVal);

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
    /// Adjusts the saturation of a <see cref="Color"/> based on the given parameter.
    /// </summary>
    /// <param name="color">The original color.</param>
    /// <param name="pmrVal">The saturation adjustment value.</param>
    /// <returns>The color with adjusted saturation.</returns>
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
