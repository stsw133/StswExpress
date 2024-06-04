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
/// </summary>
/// <remarks>
/// EXAMPLES:  'B-70% S50%'  'A50% B50% S0%'
/// </remarks>
public class StswColorAdvancedConverter : MarkupExtension, IValueConverter
{
    private static StswColorAdvancedConverter? instance;
    public static StswColorAdvancedConverter Instance => instance ??= new StswColorAdvancedConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// Convert
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        Color color;

        /// input
        if (value is Color c)
            color = c;
        else if (value is System.Drawing.Color d)
            color = Color.FromArgb(d.A, d.R, d.G, d.B);
        else if (value is SolidColorBrush br)
            color = br.ToColor();
        else
            color = (Color)ColorConverter.ConvertFromString(value?.ToString() ?? string.Empty);

        /// conversion
        var dictionary = new Dictionary<char, string>();

        var regex = new Regex(@"([A-Za-z])([^A-Za-z]*)");
        var matches = regex.Matches(parameter?.ToString() ?? string.Empty);
        foreach (var match in matches.Cast<Match>())
        {
            var key = match.Groups[1].Value[0];
            var val = match.Groups[2].Value.Trim();

            dictionary[key] = val;
        }

        if (dictionary.TryGetValue('A', out var a))
            color = (Color)StswColorAlphaConverter.Instance.Convert(color, typeof(Color), a, culture);
        if (dictionary.TryGetValue('B', out var b))
            color = (Color)StswColorBrightnessConverter.Instance.Convert(color, typeof(Color), b, culture);
        if (dictionary.TryGetValue('S', out var s))
            color = (Color)StswColorSaturationConverter.Instance.Convert(color, typeof(Color), s, culture);

        /// output
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
