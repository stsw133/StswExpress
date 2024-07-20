using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// Takes a color value as input and changes its alpha based on the provided parameters:
/// Use nothing at the beginning of the converter parameter to set the alpha of the output color.
/// Use '%' at the end of the converter parameter to use percent values.
/// Use a value in the parameter between 0 and 255 (or 0 to 100 in case of percents) to set the alpha of the output color.
/// Examples: '80', '125', '180', '245', '16%', '26%', '36%', '50%'
/// </summary>
public class StswColorAlphaConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswColorAlphaConverter Instance => instance ??= new StswColorAlphaConverter();
    private static StswColorAlphaConverter? instance;

    /// <summary>
    /// Provides the singleton instance of the converter.
    /// </summary>
    /// <param name="serviceProvider">A service provider that can provide services for the markup extension.</param>
    /// <returns>The singleton instance of the converter.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <summary>
    /// Converts the input color to a color with the specified alpha value.
    /// </summary>
    /// <param name="value">The input color value.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter to use for the operation.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>The color with the modified alpha value.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var pmr = parameter?.ToString() ?? string.Empty;
        var color = StswColorConverter.GetColorFromValue(value);

        /// parameters
        var isPercent = pmr.EndsWith('%');
        if (isPercent)
            pmr = pmr.TrimEnd('%');

        /// func
        if (!double.TryParse(pmr, NumberStyles.Number, culture, out var pmrVal))
            pmrVal = isPercent ? 100 : 255;

        var a = Math.Clamp(isPercent ? pmrVal * 255 / 100 : pmrVal, 0, 255);
        color = Color.FromArgb((byte)Math.Round(a), color.R, color.G, color.B);

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
