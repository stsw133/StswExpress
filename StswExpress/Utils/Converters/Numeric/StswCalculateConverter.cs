using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// Calculates a numerical value based on another numerical value or a set of values and an operator sign.<br/>
/// Converter supports <see cref="CornerRadius"/>, <see cref="GridLength"/>, <see cref="Thickness"/> and any numeric type (for example <see cref="int"/> and <see cref="double"/>).<br/>
/// </summary>
public class StswCalculateConverter : MarkupExtension, IValueConverter
{
    private static StswCalculateConverter? instance;
    public static StswCalculateConverter Instance => instance ??= new StswCalculateConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// Convert
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        double.TryParse(value?.ToString(), NumberStyles.Number, culture, out var val);
        var pmr = parameter?.ToString() ?? string.Empty;
        var @operator = '*';
        if (pmr.Length > 0 && !char.IsDigit(pmr[0]))
        {
            @operator = pmr[0];
            pmr = pmr[1..];
        }
        double.TryParse(pmr, NumberStyles.Number, culture, out var num);
        
        /// calc
        double Apply(double val, double pmr) => StswConverterFn.Calc(val, @operator, pmr);

        /// result
        if (targetType.In(typeof(CornerRadius), typeof(CornerRadius?)))
            return StswConverterFn.CalculateToCornerRadius(value, @operator, pmr, culture);
        else if (targetType.In(typeof(GridLength), typeof(GridLength?)))
            return StswConverterFn.CalculateToGridLength(value, @operator, pmr, culture);
        else if (targetType.In(typeof(Thickness), typeof(Thickness?)))
            return StswConverterFn.CalculateToThickness(value, @operator, pmr, culture);
        else if (Nullable.GetUnderlyingType(targetType) == null)
            return Apply(val, num);
        else
            return Apply(val, num).ConvertTo(targetType);
    }

    /// ConvertBack
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => Binding.DoNothing;
}
