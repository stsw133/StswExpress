using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Generate new color based on passed value and the provided seed as parameter.
/// </summary>
public class StswColorGeneratorConverter : MarkupExtension, IValueConverter
{
    private static StswColorGeneratorConverter? instance;
    public static StswColorGeneratorConverter Instance => instance ??= new StswColorGeneratorConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// Convert
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var pmr = parameter?.ToString() ?? string.Empty;
        var val = value?.ToString() ?? string.Empty;
        Color color;

        /// generate new color
        if (!string.IsNullOrEmpty(val))
        {
            int hashCode = val.GetHashCode();
            int r = (hashCode >> 16) & 0xFF;
            int g = (hashCode >> 8) & 0xFF;
            int b = hashCode & 0xFF;

            if (!string.IsNullOrEmpty(pmr) && int.TryParse(pmr, out var brightnessThreshold) && int.TryParse(pmr, out var darknessThreshold))
            {
                if (r > brightnessThreshold)
                    r -= (r - brightnessThreshold) / 2;
                if (g > brightnessThreshold)
                    g -= (g - brightnessThreshold) / 2;
                if (b > brightnessThreshold)
                    b -= (b - brightnessThreshold) / 2;
                if (r < darknessThreshold)
                    r += (darknessThreshold - r) / 2;
                if (g < darknessThreshold)
                    g += (darknessThreshold - g) / 2;
                if (b < darknessThreshold)
                    b += (darknessThreshold - b) / 2;
            }

            color = Color.FromArgb(255, (byte)r, (byte)g, (byte)b);
        }

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
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
