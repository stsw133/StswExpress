using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Takes a color value as input and changes its saturation based on the provided parameters:<br/>
/// <br/>
/// Use nothing or '<c>%</c>' at the end of converter parameter to increase saturation of output color.<br/>
/// Use value in parameter between 0% and 100% to set saturation of output color.<br/>
/// EXAMPLES:  '8%'  '13%'  '18%'  '25%'
/// </summary>
public class StswColorSaturationConverter : MarkupExtension, IValueConverter
{
    private static StswColorSaturationConverter? instance;
    public static StswColorSaturationConverter Instance => instance ??= new StswColorSaturationConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// Convert
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var pmr = parameter?.ToString() ?? string.Empty;
        Color color;

        /// parameters
        pmr = pmr.TrimEnd('%');

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
            pmrVal = 100;
        pmrVal = 100 - pmrVal;

        /// calculate new color
        double r = color.R, g = color.G, b = color.B;
        double avg = (r + g + b) / 3;

        r = color.R - (color.R - avg) * pmrVal / 100;
        g = color.G - (color.G - avg) * pmrVal / 100;
        b = color.B - (color.B - avg) * pmrVal / 100;

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
