using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Takes a color value as input and changes its alpha based on the provided parameters:<br/>
/// <br/>
/// Use nothing at the beginning of converter parameter to set alpha of output color.<br/>
/// Use '<c>%</c>' at the end of converter parameter to use percent values.<br/>
/// Use value in parameter between 0 and 255 (or 0 to 100 in case of percents) to set alpha of output color.<br/>
/// EXAMPLES:  '80'  '125'  '180'  '245'  '16%'  '26%'  '36%'  '50%'
/// </summary>
public class StswColorAlphaConverter : MarkupExtension, IValueConverter
{
    private static StswColorAlphaConverter? instance;
    public static StswColorAlphaConverter Instance => instance ??= new StswColorAlphaConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// Convert
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var pmr = parameter?.ToString() ?? string.Empty;
        Color color;

        /// parameters
        bool isPercent = pmr.EndsWith('%');
        if (isPercent)
            pmr = pmr.Remove(pmr.IndexOf('%'), 1);

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
            pmrVal = isPercent ? 100 : 255;

        /// calculate new color
        double a = Math.Clamp(isPercent ? pmrVal * 255 / 100 : pmrVal, 0, 255);

        color = Color.FromArgb((byte)a, color.R, color.G, color.B);

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
