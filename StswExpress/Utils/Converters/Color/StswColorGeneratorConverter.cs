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
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var pmr = parameter?.ToString() ?? string.Empty;
        var val = value?.ToString() ?? string.Empty;
        if (!int.TryParse(pmr, out var seed))
            seed = -1;

        /// generate new color
        var color = StswFn.GenerateColor(val, seed);

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
