using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;

#region BoolConverters
/// <summary>
/// Converts bool -> targetType : value parameter has to be a bool.
/// Use "!" at the beginning of converter parameter to invert output value.
/// When targetType == "Visibility" then output is "Visible" when true or "Collapsed" when false.
/// When targetType is anything else then returns bool depending on converter result.
/// </summary>
public class conv_Bool : MarkupExtension, IValueConverter
{
    private static conv_Bool? conv;
    public static conv_Bool Conv => conv ??= new conv_Bool();
    public override object ProvideValue(IServiceProvider serviceProvider) => Conv;

    /// Convert
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool.TryParse(value?.ToString(), out var val);
        var pmr = parameter?.ToString() ?? string.Empty;

        /// parameters
        bool isReversed = pmr.Contains('!');

        //if (isReversed) pmr = pmr.Remove(pmr.IndexOf('!'), 1);

        /// result
        if (targetType == typeof(Visibility))
            return (val ^ isReversed) ? Visibility.Visible : Visibility.Collapsed;
        else
            return (val ^ isReversed);
    }

    /// ConvertBack
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}

/// <summary>
/// Compares value parameter to converter parameter.
/// Use "!" at the beginning of converter parameter to invert output value.
/// When targetType = "Visibility" then output is Visible when true or Collapsed when false.
/// When targetType is anything else then returns bool depending on converter result.
/// </summary>
public class conv_Compare : MarkupExtension, IValueConverter
{
    private static conv_Compare? conv;
    public static conv_Compare Conv => conv ??= new conv_Compare();
    public override object ProvideValue(IServiceProvider serviceProvider) => Conv;

    /// Convert
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var val = value?.ToString() ?? string.Empty;
        var pmr = parameter?.ToString() ?? string.Empty;

        /// parameters
        bool isReversed = pmr.Contains('!');

        if (isReversed) pmr = pmr.Remove(pmr.IndexOf('!'), 1);

        /// result
        if (targetType == typeof(Visibility))
            return ((val == pmr) ^ isReversed) ? Visibility.Visible : Visibility.Collapsed;
        else
            return ((val == pmr) ^ isReversed);
    }

    /// ConvertBack
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}

/// <summary>
/// Checks if value parameter contains converter parameter : value parameter has to be a IEnumerable of string.
/// Use "!" at the beginning of converter parameter to invert output value.
/// When targetType = "Visibility" then output is Visible when true or Collapsed when false.
/// When targetType is anything else then returns bool depending on converter result.
/// </summary>
public class conv_Contains : MarkupExtension, IValueConverter
{
    private static conv_Contains? conv;
    public static conv_Contains Conv => conv ??= new conv_Contains();
    public override object ProvideValue(IServiceProvider serviceProvider) => Conv;

    /// Convert
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        //var val = value?.ToString() ?? string.Empty;
        var pmr = parameter?.ToString() ?? string.Empty;

        /// parameters
        bool isReversed = pmr.Contains('!');

        if (isReversed) pmr = pmr.Remove(pmr.IndexOf('!'), 1);

        /// result
        if (value is IEnumerable enumerable)
        {
            var genArgList = value.GetType().GetGenericArguments();
            var type1 = genArgList.First();

            var e = enumerable.GetEnumerator();
            var list = new List<string>();
            while (e.MoveNext())
            {
                if (e.Current == null)
                    continue;

                list.Add(System.Convert.ChangeType(e.Current, type1).ToString() ?? string.Empty);
            }
            if (type1 != null)
            {
                if (targetType == typeof(Visibility))
                    return (list.Contains(pmr) ^ isReversed) ? Visibility.Visible : Visibility.Collapsed;
                else
                    return (list.Contains(pmr) ^ isReversed);
            }
        }
        else if (value?.ToString() != null)
        {
            if (targetType == typeof(Visibility))
                return (value?.ToString()?.Contains(pmr) == true ^ isReversed) ? Visibility.Visible : Visibility.Collapsed;
            else
                return (value?.ToString()?.Contains(pmr) == true ^ isReversed);
        }
        return false;
    }

    /// ConvertBack
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}

/// <summary>
/// Checks if value parameter is not null.
/// When targetType = "Visibility" then output is Visible when true or Collapsed when false.
/// When targetType is anything else then returns bool depending on converter result.
/// </summary>
public class conv_NotNull : MarkupExtension, IValueConverter
{
    private static conv_NotNull? conv;
    public static conv_NotNull Conv => conv ??= new conv_NotNull();
    public override object ProvideValue(IServiceProvider serviceProvider) => Conv;

    /// Convert
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        /// result
        if (targetType == typeof(Visibility))
            return value is not null ? Visibility.Visible : Visibility.Collapsed;
        else
            return value is not null;
    }

    /// ConvertBack
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
#endregion

#region ColorConverters
/// <summary>
/// Changes brightness of hex color using converter parameters described below:
/// Use "!" at the beginning of converter parameter to invert output color.
/// Use "‼" at the beginning of converter parameter to set black color (when input color is bright) or white color (when input color is dark) as output.
/// Use "↓" at the beginning of converter parameter to desaturate output color.
/// Use "?" at the beginning of converter parameter to automatically decide if converter needs to increase or decrease brightness of output color.
/// Use "@" at the end of converter parameter with value between 00 and FF to set alpha of output color.
/// Use "#" at the beginning of converter parameter to automatically generate color in output based on value string.
/// Use value between -1.0 and 1.0 to set brightness of output color.
/// </summary>
[Obsolete]
public class conv_Color : MarkupExtension, IValueConverter
{
    private static conv_Color? conv;
    public static conv_Color Conv => conv ??= new conv_Color();
    public override object ProvideValue(IServiceProvider serviceProvider) => Conv;

    /// Convert
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var pmr = parameter?.ToString() ?? string.Empty;
        var val = value?.ToString() ?? string.Empty;

        /// parameters
        bool invertColor = pmr.Contains('!'),
             contrastColor = pmr.Contains('‼'),
             desaturateColor = pmr.Contains('↓'),
             autoBrightness = pmr.Contains('?'),
             generateColor = pmr.Contains('#');
        int setAlpha = pmr.Contains('@') ? System.Convert.ToInt32(pmr[(pmr.IndexOf('@') + 1)..], 16) : -1;

        if (invertColor) pmr = pmr.Remove(pmr.IndexOf('!'), 1);
        if (contrastColor) pmr = pmr.Remove(pmr.IndexOf('‼'), 1);
        if (desaturateColor) pmr = pmr.Remove(pmr.IndexOf('↓'), 1);
        if (autoBrightness) pmr = pmr.Remove(pmr.IndexOf('?'), 1);
        if (generateColor) pmr = pmr.Remove(pmr.IndexOf('#'), 1);
        if (setAlpha >= 0) pmr = pmr.Remove(pmr.IndexOf('@'));

        var generatedColor = string.Empty;
        var pmrVal = System.Convert.ToDouble(string.IsNullOrEmpty(pmr) ? "0" : pmr, culture);

        /// generate color
        Color color = generateColor
            ? Color.FromArgb(255, 220 - (val.Sum(x => x) * 9797 % 90), 220 - (val.Sum(x => x) * 8989 % 90), 220 - (val.Sum(x => x) * 8383 % 90))
            : ColorTranslator.FromHtml(val);

        /// desaturate color
        if (desaturateColor)
            color = Color.FromArgb(color.A, (color.R + color.G + color.B) / 3, (color.R + color.G + color.B) / 3, (color.R + color.G + color.B) / 3);

        /// invert color
        if (invertColor)
            color = Color.FromArgb(color.A, 255 - color.R, 255 - color.G, 255 - color.B);

        /// contrast color
        if (contrastColor)
            color = color.GetBrightness() < 0.5 ? Color.White : Color.Black;

        /// auto brightness
        if (autoBrightness)
            pmrVal = color.GetBrightness() < 0.5 ? Math.Abs(pmrVal) : Math.Abs(pmrVal) * -1;

        /// calculate new color
        int r = color.R, g = color.G, b = color.B;
        r += System.Convert.ToInt32((pmrVal > 0 ? 255 - r : r) * pmrVal);
        g += System.Convert.ToInt32((pmrVal > 0 ? 255 - g : g) * pmrVal);
        b += System.Convert.ToInt32((pmrVal > 0 ? 255 - b : b) * pmrVal);
        color = Color.FromArgb(setAlpha.Between(0, 255) ? setAlpha : color.A, r, g, b);

        return ColorTranslator.ToHtml(color).Insert(1, color.A.ToString("X2", null));
    }

    /// ConvertBack
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
/// <summary>
/// Changes brightness of hex color using converter parameters described below:
/// Use nothing or "+" at the beginning of converter parameter to increase brightness of output color.
/// Use "-" at the beginning of converter parameter to decrease brightness of output color.
/// Use "?" at the beginning of converter parameter to automatically decide if converter needs to increase or decrease brightness of output color.
/// Use "%" at the end of converter parameter to use percent values.
/// Use value in parameter between -255 and 255 (or -100 to 100 in case of percents) to set brightness of output color.
/// EXAMPLES:  '16'  '+25'  '-36'  '?49'  '8%'  '+13%'  '-18%'  '?25%'
/// </summary>
public class conv_ColorBrightness : MarkupExtension, IValueConverter
{
    private static conv_ColorBrightness? conv;
    public static conv_ColorBrightness Conv => conv ??= new conv_ColorBrightness();
    public override object ProvideValue(IServiceProvider serviceProvider) => Conv;

    /// Convert
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var pmr = parameter?.ToString() ?? string.Empty;
        var val = value?.ToString() ?? string.Empty;

        /// parameters
        bool isAuto = pmr.Contains('?'),
             isPercent = pmr.Contains('%');

        if (isAuto) pmr = pmr.Remove(pmr.IndexOf('?'), 1);
        if (isPercent) pmr = pmr.Remove(pmr.IndexOf('%'), 1);

        /// value as color and parameter as number
        var color = ColorTranslator.FromHtml(val);

        if (!double.TryParse(pmr, NumberStyles.Number, culture, out var pmrVal))
            pmrVal = 0;

        if (isAuto)
            pmrVal = color.GetBrightness() < 0.5 ? Math.Abs(pmrVal) : Math.Abs(pmrVal) * -1;

        /// calculate new color
        double r = color.R, g = color.G, b = color.B;

        if (isPercent)
        {
            r += (pmrVal > 0 ? 255 - r : r) * pmrVal / 100;
            g += (pmrVal > 0 ? 255 - g : g) * pmrVal / 100;
            b += (pmrVal > 0 ? 255 - b : b) * pmrVal / 100;
        }
        else
        {
            var minMax = pmrVal > 0 ? new[] { r, g, b }.Min() : new[] { r, g, b }.Max();
            var multiplier = pmrVal > 0 ? 255 - minMax : minMax;

            if ((pmrVal > 0 && minMax != 255) || (pmrVal < 0 && minMax != 0))
            {
                r += (pmrVal > 0 ? 255 - r : r) / multiplier * pmrVal;
                g += (pmrVal > 0 ? 255 - g : g) / multiplier * pmrVal;
                b += (pmrVal > 0 ? 255 - b : b) / multiplier * pmrVal;
            }
        }
        r = Math.Clamp(r, 0, 255);
        g = Math.Clamp(g, 0, 255);
        b = Math.Clamp(b, 0, 255);

        /// result
        color = Color.FromArgb(color.A, (byte)r, (byte)g, (byte)b);

        return ColorTranslator.ToHtml(color).Insert(1, color.A.ToString("X2", null));
    }

    /// ConvertBack
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
#endregion

#region FormatConverters
/// <summary>
/// Converts decimal independent of using dot or comma.
/// </summary>
public class conv_MultiCultureNumber : MarkupExtension, IValueConverter
{
    private static conv_MultiCultureNumber? conv;
    public static conv_MultiCultureNumber Conv => conv ??= new conv_MultiCultureNumber();
    public override object ProvideValue(IServiceProvider serviceProvider) => Conv;

    /// Convert
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        /// result
        var ci = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        ci.NumberFormat.NumberDecimalSeparator = ",";
        return ((decimal)value).ToString(ci);
    }

    /// ConvertBack
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
#endregion

#region NumericConverters
/// <summary>
/// Converts double -> value + parameter.
/// Convert Thickness(double, double, double, double) -> Thickness(value + parameter, value + parameter, value + parameter, value + parameter).
/// </summary>
public class conv_Add : MarkupExtension, IValueConverter
{
    private static conv_Add? conv;
    public static conv_Add Conv => conv ??= new conv_Add();
    public override object ProvideValue(IServiceProvider serviceProvider) => Conv;

    /// Convert
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        double.TryParse(value?.ToString(), NumberStyles.Number, culture, out var val);
        double.TryParse(parameter?.ToString(), NumberStyles.Number, culture, out var pmr);

        /// result
        if (targetType.In(typeof(CornerRadius), typeof(CornerRadius?)))
        {
            var pmrArray = Array.ConvertAll((parameter?.ToString() ?? string.Empty).Split(new char[] { ' ', ',' }), i => System.Convert.ToDouble(i, culture));
            return pmrArray.Length switch
            {
                4 => new CornerRadius(val + pmrArray[0], val + pmrArray[1], val + pmrArray[2], val + pmrArray[3]),
                2 => new CornerRadius(val + pmrArray[0], val + pmrArray[1], val + pmrArray[0], val + pmrArray[1]),
                1 => new CornerRadius(val + pmrArray[0]),
                _ => new CornerRadius(val),
            };
        }
        else if (targetType.In(typeof(Thickness), typeof(Thickness?)))
        {
            var pmrArray = Array.ConvertAll((parameter?.ToString() ?? string.Empty).Split(new char[] { ' ', ',' }), i => System.Convert.ToDouble(i, culture));
            return pmrArray.Length switch
            {
                4 => new Thickness(val + pmrArray[0], val + pmrArray[1], val + pmrArray[2], val + pmrArray[3]),
                2 => new Thickness(val + pmrArray[0], val + pmrArray[1], val + pmrArray[0], val + pmrArray[1]),
                1 => new Thickness(val + pmrArray[0]),
                _ => new Thickness(val)
            };
        }
        else return val + pmr;
    }

    /// ConvertBack
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}

/// <summary>
/// Converts doubles -> value calculated by parameter.
/// </summary>
[Obsolete]
public class conv_Calculate : MarkupExtension, IMultiValueConverter
{
    private static conv_Calculate? conv;
    public static conv_Calculate Conv => conv ??= new conv_Calculate();
    public override object ProvideValue(IServiceProvider serviceProvider) => Conv;

    /// Convert
    public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
    {
        /// result
        if (value.Any(x => x == null))
            return new object[] { Binding.DoNothing };

        var a = value.Length > 0 ? string.Join(parameter?.ToString(), value).Replace(",", ".") : "1";
        var b = a.Contains('{') ? 0 : System.Convert.ToDouble(new DataTable().Compute(a, string.Empty), culture);

        if (targetType.In(typeof(Thickness), typeof(Thickness?)))
            return new Thickness(b);
        else if (targetType == typeof(string))
            return b.ToString();
        else
            return b;
    }

    /// ConvertBack
    public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture) => new object[] { Binding.DoNothing };
}

/// <summary>
/// Converts double -> value * parameter.
/// Convert Thickness(double, double, double, double) -> Thickness(value * parameter, value * parameter, value * parameter, value * parameter).
/// </summary>
public class conv_Multiply : MarkupExtension, IValueConverter
{
    private static conv_Multiply? conv;
    public static conv_Multiply Conv => conv ??= new conv_Multiply();
    public override object ProvideValue(IServiceProvider serviceProvider) => Conv;

    /// Convert
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        double.TryParse(value?.ToString(), NumberStyles.Number, culture, out var val);
        double.TryParse(parameter?.ToString(), NumberStyles.Number, culture, out var pmr);

        /// result
        if (targetType.In(typeof(CornerRadius), typeof(CornerRadius?)))
        {
            var pmrArray = Array.ConvertAll((parameter?.ToString() ?? string.Empty).Split(new char[] { ' ', ',' }), i => System.Convert.ToDouble(i, culture));
            return pmrArray.Length switch
            {
                4 => new CornerRadius(val * pmrArray[0], val * pmrArray[1], val * pmrArray[2], val * pmrArray[3]),
                2 => new CornerRadius(val * pmrArray[0], val * pmrArray[1], val * pmrArray[0], val * pmrArray[1]),
                1 => new CornerRadius(val * pmrArray[0]),
                _ => new CornerRadius(val)
            };
        }
        else if (targetType.In(typeof(Thickness), typeof(Thickness?)))
        {
            var pmrArray = Array.ConvertAll((parameter?.ToString() ?? string.Empty).Split(new char[] { ' ', ',' }), i => System.Convert.ToDouble(i, culture));
            return pmrArray.Length switch
            {
                4 => new Thickness(val * pmrArray[0], val * pmrArray[1], val * pmrArray[2], val * pmrArray[3]),
                2 => new Thickness(val * pmrArray[0], val * pmrArray[1], val * pmrArray[0], val * pmrArray[1]),
                1 => new Thickness(val * pmrArray[0]),
                _ => new Thickness(val)
            };
        }
        else return val * pmr;
    }

    /// ConvertBack
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}

/// <summary>
/// Sum values of certain field (name in parameter) in list of elements.
/// </summary>
public class conv_Sum : MarkupExtension, IValueConverter
{
    private static conv_Sum? conv;
    public static conv_Sum Conv => conv ??= new conv_Sum();
    public override object ProvideValue(IServiceProvider serviceProvider) => Conv;

    /// Convert
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var pmr = parameter?.ToString() ?? string.Empty;

        /// result
        var result = 0d;
        
        if (value is IEnumerable enumerable)
            foreach (var item in enumerable)
            {
                if (item.GetType().GetProperty(pmr) is not null and System.Reflection.PropertyInfo prop)
                    result += System.Convert.ToDouble(prop.GetValue(item), culture);
            }
        else if (value is DataTable dataTable)
            foreach (DataRow row in dataTable.Rows)
            {
                if (double.TryParse(row?[pmr]?.ToString(), NumberStyles.Number, culture, out var val))
                    result += val;
            }

        return result;
    }

    /// ConvertBack
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
#endregion

#region SpecialConverters
/// <summary>
/// Checks if value parameter is null and returns UnsetValue if it is.
/// </summary>
public class conv_NullToUnset : MarkupExtension, IValueConverter
{
    private static conv_NullToUnset? conv;
    public static conv_NullToUnset Conv => conv ??= new conv_NullToUnset();
    public override object ProvideValue(IServiceProvider serviceProvider) => Conv;

    /// Convert
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value ?? DependencyProperty.UnsetValue;

    /// ConvertBack
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
#endregion

#region UniversalConverters
/// <summary>
/// Compares binded value to first element of parameter, then displays second or third element based on compare result
/// Converter parameter has to be like 'valueToCompare~whatToDisplayWhenTrue~whatToDisplayWhenFalse'.
/// If binded value is equal to "valueToCompare" then displays "whatToDisplayWhenTrue" else displays "whatToDisplayWhenFalse".
/// </summary>
public class conv_IfElse : MarkupExtension, IValueConverter
{
    private static conv_IfElse? conv;
    public static conv_IfElse Conv => conv ??= new conv_IfElse();
    public override object ProvideValue(IServiceProvider serviceProvider) => Conv;

    /// Convert
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var val = value?.ToString() ?? string.Empty;
        var pmr = parameter?.ToString()?.Split('~') ?? new string[3];

        /// result
        return ((val == pmr?[0]) ? pmr?[1] : pmr?[2]) ?? string.Empty;
    }

    /// ConvertBack
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
#endregion
