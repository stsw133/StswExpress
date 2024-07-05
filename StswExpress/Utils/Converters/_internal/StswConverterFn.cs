using System;
using System.Globalization;
using System.Windows;

namespace StswExpress;
internal static class StswConverterFn
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="val"></param>
    /// <param name="ope"></param>
    /// <param name="pmr"></param>
    /// <returns></returns>
    internal static double Calc(double val, char? ope, double pmr) => ope switch
    {
        '+' => val + pmr,
        '-' => val - pmr,
        '*' => val * pmr,
        '/' => val / pmr,
        '%' => val % pmr,
        '^' => Math.Pow(val, pmr),
        '√' => Math.Pow(val, 1/pmr),
        _ => val
    };

    /// CalculateToCornerRadius
    internal static CornerRadius CalculateToCornerRadius(object? value, char? @operator, object? parameter, CultureInfo culture)
    {
        var pmr = Array.ConvertAll((parameter?.ToString() ?? string.Empty).Split([' ', ','], StringSplitOptions.TrimEntries), i => Convert.ToDouble(i, culture));
        double Apply(double val, double pmr) => Calc(val, @operator, pmr);

        if (value is CornerRadius val1)
        {
            return pmr.Length switch
            {
                4 => new CornerRadius(Apply(val1.TopLeft, pmr[0]), Apply(val1.TopRight, pmr[1]), Apply(val1.BottomRight, pmr[2]), Apply(val1.BottomLeft, pmr[3])),
                2 => new CornerRadius(Apply(val1.TopLeft, pmr[0]), Apply(val1.TopRight, pmr[1]), Apply(val1.BottomRight, pmr[0]), Apply(val1.BottomLeft, pmr[1])),
                1 => new CornerRadius(Apply(val1.TopLeft, pmr[0])),
                _ => new CornerRadius(val1.TopLeft)
            };
        }
        else
        {
            var val2 = Convert.ToDouble(value, culture);
            return pmr.Length switch
            {
                4 => new CornerRadius(Apply(val2, pmr[0]), Apply(val2, pmr[1]), Apply(val2, pmr[2]), Apply(val2, pmr[3])),
                2 => new CornerRadius(Apply(val2, pmr[0]), Apply(val2, pmr[1]), Apply(val2, pmr[0]), Apply(val2, pmr[1])),
                1 => new CornerRadius(Apply(val2, pmr[0])),
                _ => new CornerRadius(val2)
            };
        }
    }

    /// CalculateToGridLength
    internal static GridLength CalculateToGridLength(object? value, char? @operator, object? parameter, CultureInfo culture)
    {
        double Apply(double val, double pmr) => Calc(val, @operator, pmr);

        double.TryParse(value?.ToString(), culture, out var val);
        double.TryParse(parameter?.ToString(), culture, out var pmr);
        return new GridLength(Apply(val, pmr));
    }

    /// CalculateToThickness
    internal static Thickness CalculateToThickness(object? value, char? @operator, object? parameter, CultureInfo culture)
    {
        var pmr = Array.ConvertAll((parameter?.ToString() ?? string.Empty).Split([' ', ','], StringSplitOptions.TrimEntries), i => Convert.ToDouble(i, culture));
        double Apply(double val, double pmr) => Calc(val, @operator, pmr);

        if (value is Thickness val1)
        {
            return pmr.Length switch
            {
                4 => new Thickness(Apply(val1.Left, pmr[0]), Apply(val1.Top, pmr[1]), Apply(val1.Right, pmr[2]), Apply(val1.Bottom, pmr[3])),
                2 => new Thickness(Apply(val1.Left, pmr[0]), Apply(val1.Top, pmr[1]), Apply(val1.Right, pmr[0]), Apply(val1.Bottom, pmr[1])),
                1 => new Thickness(Apply(val1.Left, pmr[0])),
                _ => new Thickness(val1.Left)
            };
        }
        else
        {
            var val2 = Convert.ToDouble(value, culture);
            return pmr.Length switch
            {
                4 => new Thickness(Apply(val2, pmr[0]), Apply(val2, pmr[1]), Apply(val2, pmr[2]), Apply(val2, pmr[3])),
                2 => new Thickness(Apply(val2, pmr[0]), Apply(val2, pmr[1]), Apply(val2, pmr[0]), Apply(val2, pmr[1])),
                1 => new Thickness(Apply(val2, pmr[0])),
                _ => new Thickness(val2)
            };
        }
    }
}
