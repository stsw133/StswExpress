using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// Calculates a mathematical expression based on input values passed as an array and a separator string.
/// Converter supports <see cref="CornerRadius"/>, <see cref="Thickness"/> and any numeric type (for example <see cref="int"/> and <see cref="double"/>).<br/>
/// </summary>
[Obsolete]
public class StswCalculateConverter : MarkupExtension, IMultiValueConverter
{
    private static StswCalculateConverter? instance;
    public static StswCalculateConverter Instance => instance ??= new StswCalculateConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// Convert
    public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
    {
        /// result
        if (value.Any(x => x == null))
            return new object[] { Binding.DoNothing };

        var a = value.Length > 0 ? string.Join(parameter?.ToString(), value).Replace(",", ".") : "1";
        StswFn.TryCalculateString(a, out var result, culture);

        if (targetType.In(typeof(CornerRadius), typeof(CornerRadius?)))
            return new CornerRadius(System.Convert.ToDouble(result));
        else if (targetType.In(typeof(Thickness), typeof(Thickness?)))
            return new Thickness(System.Convert.ToDouble(result));
        else if (targetType == typeof(string))
            return result.ToString();
        else
            return result;
    }

    /// ConvertBack
    public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture) => new object[] { Binding.DoNothing };
}
