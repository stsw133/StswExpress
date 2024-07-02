using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Takes a color value as input and changes its brightness based on the provided parameters:<br/>
/// <br/>
/// Use nothing or '<c>+</c>' at the beginning of converter parameter to increase brightness of output color.<br/>
/// Use '<c>-</c>' at the beginning of converter parameter to decrease brightness of output color.<br/>
/// Use '<c>?</c>' at the beginning of converter parameter to automatically decide if converter needs to increase or decrease brightness of output color.<br/>
/// Use '<c>%</c>' at the end of converter parameter to use percent values.<br/>
/// Use value in parameter between -255 and 255 (or -100 to 100 in case of percents) to set brightness of output color.<br/>
/// EXAMPLES:  '16'  '+25'  '-36'  '?49'  '8%'  '+13%'  '-18%'  '?25%'
/// </summary>
public class StswColorBrightnessConverter : MarkupExtension, IValueConverter
{
    private static StswColorBrightnessConverter? instance;
    public static StswColorBrightnessConverter Instance => instance ??= new StswColorBrightnessConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// Convert
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var pmr = parameter?.ToString() ?? string.Empty;
        Color color;

        /// parameters
        bool isAuto = pmr.StartsWith('?'),
             isPercent = pmr.EndsWith('%');

        if (isAuto) pmr = pmr.Remove(pmr.IndexOf('?'), 1);
        if (isPercent) pmr = pmr.Remove(pmr.IndexOf('%'), 1);

        /// value as color and parameter as number
        if (value is Color c)
            color = c;
        else if (value is System.Drawing.Color d)
            color = Color.FromArgb(d.A, d.R, d.G, d.B);
        else if (value is SolidColorBrush br)
            color = br.ToColor();
        else try
            {
                color = (Color)ColorConverter.ConvertFromString(value?.ToString() ?? "Transparent");
            }
            catch
            {
                return value;
            }

        if (!double.TryParse(pmr, NumberStyles.Number, culture, out var pmrVal))
            pmrVal = 0;
        if (isAuto)
            pmrVal = color.ToDrawingColor().GetBrightness() < 0.5 ? Math.Abs(pmrVal) : Math.Abs(pmrVal) * -1;

        /// calculate new color
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

        color = Color.FromArgb(color.A, (byte)r, (byte)g, (byte)b);

        /// result
        if (targetType == typeof(Color))
            return color;
        else if (targetType == typeof(System.Drawing.Color))
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        else if (targetType.In(typeof(Brush), typeof(SolidColorBrush)))
            return new SolidColorBrush(color);
        else
            return color.ToHtml();
    }

    /// ConvertBack
    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
