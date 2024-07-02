using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// Modulo a numerical value to another numerical value or a set of values.<br/>
/// Converter supports <see cref="CornerRadius"/>, <see cref="Thickness"/> and any numeric type (for example <see cref="int"/> and <see cref="double"/>).<br/>
/// </summary>
public class StswModuloConverter : MarkupExtension, IValueConverter
{
    private static StswModuloConverter? instance;
    public static StswModuloConverter Instance => instance ??= new StswModuloConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// Convert
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        double.TryParse(parameter?.ToString(), NumberStyles.Number, culture, out var pmr);
        if (pmr == 0)
            return value;

        /// result
        if (targetType.In(typeof(CornerRadius), typeof(CornerRadius?)))
        {
            var pmrArray = Array.ConvertAll((parameter?.ToString() ?? string.Empty).Split([' ', ',']), i => System.Convert.ToDouble(i, culture));
            if (value is CornerRadius val1)
            {
                return pmrArray.Length switch
                {
                    4 => new CornerRadius(val1.TopLeft % pmrArray[0], val1.TopRight % pmrArray[1], val1.BottomRight % pmrArray[2], val1.BottomLeft % pmrArray[3]),
                    2 => new CornerRadius(val1.TopLeft % pmrArray[0], val1.TopRight % pmrArray[1], val1.BottomRight % pmrArray[0], val1.BottomLeft % pmrArray[1]),
                    1 => new CornerRadius(val1.TopLeft % pmrArray[0]),
                    _ => new CornerRadius(val1.TopLeft)
                };
            }
            else
            {
                var val2 = System.Convert.ToDouble(value, culture);
                return pmrArray.Length switch
                {
                    4 => new CornerRadius(val2 % pmrArray[0], val2 % pmrArray[1], val2 % pmrArray[2], val2 % pmrArray[3]),
                    2 => new CornerRadius(val2 % pmrArray[0], val2 % pmrArray[1], val2 % pmrArray[0], val2 % pmrArray[1]),
                    1 => new CornerRadius(val2 % pmrArray[0]),
                    _ => new CornerRadius(val2)
                };
            }
        }
        else if (targetType.In(typeof(Thickness), typeof(Thickness?)))
        {
            var pmrArray = Array.ConvertAll((parameter?.ToString() ?? string.Empty).Split([' ', ',']), i => System.Convert.ToDouble(i, culture));
            if (value is Thickness val1)
            {
                return pmrArray.Length switch
                {
                    4 => new Thickness(val1.Left % pmrArray[0], val1.Top % pmrArray[1], val1.Right % pmrArray[2], val1.Bottom % pmrArray[3]),
                    2 => new Thickness(val1.Left % pmrArray[0], val1.Top % pmrArray[1], val1.Right % pmrArray[0], val1.Bottom % pmrArray[1]),
                    1 => new Thickness(val1.Left % pmrArray[0]),
                    _ => new Thickness(val1.Left)
                };
            }
            else
            {
                var val2 = System.Convert.ToDouble(value, culture);
                return pmrArray.Length switch
                {
                    4 => new Thickness(val2 % pmrArray[0], val2 % pmrArray[1], val2 % pmrArray[2], val2 % pmrArray[3]),
                    2 => new Thickness(val2 % pmrArray[0], val2 % pmrArray[1], val2 % pmrArray[0], val2 % pmrArray[1]),
                    1 => new Thickness(val2 % pmrArray[0]),
                    _ => new Thickness(val2)
                };
            }
        }
        else if (!double.TryParse(value?.ToString(), culture, out var val)) return value;
        else return (val % pmr).ConvertTo(targetType);
    }

    /// ConvertBack
    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
