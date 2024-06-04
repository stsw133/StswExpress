using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// Used for type conversion between different types.
/// </summary>
public class StswColorConverter : MarkupExtension, IValueConverter
{
    private static StswColorConverter? instance;
    public static StswColorConverter Instance => instance ??= new StswColorConverter();
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
