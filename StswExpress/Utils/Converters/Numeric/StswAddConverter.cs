using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// Adds a numerical value to another numerical value or a set of values.<br/>
/// Converter supports <see cref="CornerRadius"/>, <see cref="GridLength"/>,<see cref="Thickness"/> and any numeric type (for example <see cref="int"/> and <see cref="double"/>).<br/>
/// </summary>
[Obsolete($"Please use '{nameof(StswCalculateConverter)}' instead.")]
public class StswAddConverter : MarkupExtension, IValueConverter
{
    private static StswAddConverter? instance;
    public static StswAddConverter Instance => instance ??= new StswAddConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// Convert
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        double.TryParse(value?.ToString(), NumberStyles.Number, culture, out var val);
        double.TryParse(parameter?.ToString(), NumberStyles.Number, culture, out var num);

        /// result
        if (targetType.In(typeof(CornerRadius), typeof(CornerRadius?)))
            return StswConverterFn.CalculateToCornerRadius(value, '+', parameter, culture);
        else if (targetType.In(typeof(GridLength), typeof(GridLength?)))
            return StswConverterFn.CalculateToGridLength(value, '+', parameter, culture);
        else if (targetType.In(typeof(Thickness), typeof(Thickness?)))
            return StswConverterFn.CalculateToThickness(value, '+', parameter, culture);
        else if (Nullable.GetUnderlyingType(targetType) == null)
            return val + num;
        else
            return (val + num).ConvertTo(targetType);
    }

    /// ConvertBack
    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
