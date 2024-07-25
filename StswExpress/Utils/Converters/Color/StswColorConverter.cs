using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// Converts color values between different types, including <see cref="Color"/>, <see cref="System.Drawing.Color"/>, <see cref="SolidColorBrush"/>, and HTML color strings.
/// </summary>
public class StswColorConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswColorConverter Instance => instance ??= new StswColorConverter();
    private static StswColorConverter? instance;

    /// <summary>
    /// Provides the singleton instance of the converter.
    /// </summary>
    /// <param name="serviceProvider">A service provider that can provide services for the markup extension.</param>
    /// <returns>The singleton instance of the converter.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <summary>
    /// Converts a color value to the specified target type.
    /// </summary>
    /// <param name="value">The input color value.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter to use (not used).</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>The converted color value in the desired target type.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var color = GetColorFromValue(value);
        return GetResultFromColor(color, targetType);
    }

    /// <summary>
    /// This converter does not support converting back from target value to source value.
    /// </summary>
    /// <param name="value">The value produced by the binding target.</param>
    /// <param name="targetType">The type to convert to.</param>
    /// <param name="parameter">The converter parameter to use (not used).</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns><see cref="Binding.DoNothing"/> as the converter does not support converting back.</returns>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => Binding.DoNothing;

    /// <summary>
    /// Converts an input value to a <see cref="Color"/>.
    /// </summary>
    /// <param name="value">The input value.</param>
    /// <returns>The converted <see cref="Color"/>.</returns>
    internal static Color GetColorFromValue(object? value)
    {
        if (value == DependencyProperty.UnsetValue)
            return Colors.Transparent;

        return value switch
        {
            Color c => c,
            System.Drawing.Color d => Color.FromArgb(d.A, d.R, d.G, d.B),
            SolidColorBrush br => br.Color,
            _ => (Color)ColorConverter.ConvertFromString(value?.ToString() ?? "Transparent")
        };
    }

    /// <summary>
    /// Converts a <see cref="Color"/> to the desired target type.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <param name="targetType">The desired target type.</param>
    /// <returns>The converted value in the desired target type.</returns>
    internal static object GetResultFromColor(Color color, Type targetType)
    {
        return targetType switch
        {
            Type t when t == typeof(Color) => color,
            Type t when t == typeof(System.Drawing.Color) => System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B),
            Type t when t == typeof(Brush) || t == typeof(SolidColorBrush) => new SolidColorBrush(color),
            _ => color.ToString()
        };
    }
}
