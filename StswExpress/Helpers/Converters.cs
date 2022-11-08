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
/// When targetType = "string" then converter parameter must contains "~" and be like "whatToDisplayWhenTrue~whatToDisplayWhenFalse".
/// When targetType = "Visibility" then output is Visible when true or Collapsed when false.
/// When targetType is anything else then returns bool depending on converter result.
/// </summary>
public class conv_Bool : MarkupExtension, IValueConverter
{
    private static conv_Bool? _conv;
    public override object ProvideValue(IServiceProvider serviceProvider) => _conv ??= new conv_Bool();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var rev = parameter?.ToString()?.StartsWith('!') ?? false;
        var pmr = parameter?.ToString()?.TrimStart('!') ?? string.Empty;
        var val = System.Convert.ToBoolean(value, culture);

        /// calculate result
        if (pmr.Contains('~'))
            return (val ^ rev) ? pmr.Split('~')[0] : pmr.Split('~')[1];
        else if (targetType == typeof(Visibility))
            return (val ^ rev) ? Visibility.Visible : Visibility.Collapsed;
        else
            return (val ^ rev);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
}

/// <summary>
/// Compares value parameter to converter parameter.
/// Use "!" at the beginning of converter parameter to invert output value.
/// When targetType = "Visibility" then output is Visible when true or Collapsed when false.
/// When targetType is anything else then returns bool depending on converter result.
/// </summary>
public class conv_Compare : MarkupExtension, IValueConverter
{
    private static conv_Compare? _conv;
    public override object ProvideValue(IServiceProvider serviceProvider) => _conv ??= new conv_Compare();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var rev = parameter?.ToString()?.StartsWith('!') ?? false;
        var pmr = parameter?.ToString()?.TrimStart('!') ?? string.Empty;
        var val = System.Convert.ToString(value, culture);

        /// calculate result
        if (pmr.Contains('~'))
        {
            var pmrArr = pmr.Split('~');
            return ((val == pmrArr[0]) ^ rev) ? pmrArr[1] : pmrArr[2];
        }
        else if (targetType == typeof(Visibility))
            return ((val == pmr) ^ rev) ? Visibility.Visible : Visibility.Collapsed;
        else
            return ((val == pmr) ^ rev);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
}

/// <summary>
/// Checks if value parameter contains converter parameter : value parameter has to be a IEnumerable of string.
/// Use "!" at the beginning of converter parameter to invert output value.
/// When targetType = "Visibility" then output is Visible when true or Collapsed when false.
/// When targetType is anything else then returns bool depending on converter result.
/// </summary>
public class conv_Contains : MarkupExtension, IValueConverter
{
    private static conv_Contains? _conv;
    public override object ProvideValue(IServiceProvider serviceProvider) => _conv ??= new conv_Contains();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var rev = parameter?.ToString()?.StartsWith('!') ?? false;
        var pmr = parameter?.ToString()?.TrimStart('!') ?? string.Empty;

        /// calculate result
        if (value is IEnumerable val)
        {
            var genArgList = value.GetType().GetGenericArguments();
            var type1 = genArgList.First();

            var e = val.GetEnumerator();
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
                    return (list.Contains(pmr) ^ rev) ? Visibility.Visible : Visibility.Collapsed;
                else
                    return (list.Contains(pmr) ^ rev);
            }
        }
        else if (value?.ToString() != null)
        {
            if (targetType == typeof(Visibility))
                return (value.ToString().Contains(pmr) ^ rev) ? Visibility.Visible : Visibility.Collapsed;
            else
                return (value.ToString().Contains(pmr) ^ rev);
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
}

/// <summary>
/// Checks if value parameter is not null.
/// When targetType = "Visibility" then output is Visible when true or Collapsed when false.
/// When targetType is anything else then returns bool depending on converter result.
/// </summary>
public class conv_NotNull : MarkupExtension, IValueConverter
{
    private static conv_NotNull? _conv;
    public override object ProvideValue(IServiceProvider serviceProvider) => _conv ??= new conv_NotNull();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (targetType == typeof(Visibility))
            return value != null ? Visibility.Visible : Visibility.Collapsed;
        else
            return value != null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
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
public class conv_Color : MarkupExtension, IValueConverter
{
    private static conv_Color? _conv;
    public override object ProvideValue(IServiceProvider serviceProvider) => _conv ??= new conv_Color();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var pmr = parameter?.ToString() ?? string.Empty;
        var val = value?.ToString() ?? string.Empty;

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

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
}
#endregion

#region FormatConverters
/// <summary>
/// Converts decimal independent of using dot or comma.
/// </summary>
public class conv_MultiCultureNumber : MarkupExtension, IValueConverter
{
    private static conv_MultiCultureNumber? _conv;
    public override object ProvideValue(IServiceProvider serviceProvider) => _conv ??= new conv_MultiCultureNumber();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var ci = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        ci.NumberFormat.NumberDecimalSeparator = ",";
        return ((decimal)value).ToString(ci);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
}
#endregion

#region NumericConverters
/// <summary>
/// Converts double -> value + parameter.
/// Convert Thickness(double, double, double, double) -> Thickness(value + parameter, value + parameter, value + parameter, value + parameter).
/// </summary>
public class conv_Add : MarkupExtension, IValueConverter
{
    private static conv_Add? _conv;
    public override object ProvideValue(IServiceProvider serviceProvider) => _conv ??= new conv_Add();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var val = System.Convert.ToDouble(value, culture);

        /// calculate result
        if (targetType == typeof(Thickness))
        {
            var pmr = Array.ConvertAll(parameter.ToString().Split(new char[] { ' ', ',' }), i => System.Convert.ToDouble(i, culture));
            if (pmr.Length == 4)
                return new Thickness(val + pmr[0], val + pmr[1], val + pmr[2], val + pmr[3]);
            else if (pmr.Length == 2)
                return new Thickness(val + pmr[0], val + pmr[1], val + pmr[0], val + pmr[1]);
            else if (pmr.Length == 1)
                return new Thickness(val + pmr[0]);
            else
                return new Thickness(val);
        }
        else
        {
            var pmr = System.Convert.ToDouble(parameter, culture);
            return val + pmr;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
}

/// <summary>
/// Converts doubles -> value calculated by parameter.
/// </summary>
public class conv_Calculate : MarkupExtension, IMultiValueConverter
{
    private static conv_Calculate? _conv;
    public override object ProvideValue(IServiceProvider serviceProvider) => _conv ??= new conv_Calculate();

    public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
    {
        var a = value.Length > 0 ? string.Join(parameter?.ToString(), value).Replace(",", ".") : "1";
        var b = a.Contains('{') ? 0 : System.Convert.ToDouble(new DataTable().Compute(a, string.Empty), culture);

        /// calculate result
        if (targetType == typeof(Thickness))
            return new Thickness(b);
        else
            return b;
    }

    public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture) => new[] { value };
}

/// <summary>
/// Converts double -> value * parameter.
/// Convert Thickness(double, double, double, double) -> Thickness(value * parameter, value * parameter, value * parameter, value * parameter).
/// </summary>
public class conv_Multiply : MarkupExtension, IValueConverter
{
    private static conv_Multiply? _conv;
    public override object ProvideValue(IServiceProvider serviceProvider) => _conv ??= new conv_Multiply();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var val = System.Convert.ToDouble(value, culture);

        /// calculate result
        if (targetType == typeof(Thickness))
        {
            var pmr = Array.ConvertAll(parameter.ToString().Split(new char[] { ' ', ',' }), i => System.Convert.ToDouble(i, culture));
            if (pmr.Length == 4)
                return new Thickness(val * pmr[0], val * pmr[1], val * pmr[2], val * pmr[3]);
            else if (pmr.Length == 2)
                return new Thickness(val * pmr[0], val * pmr[1], val * pmr[0], val * pmr[1]);
            else if (pmr.Length == 1)
                return new Thickness(val * pmr[0]);
            else
                return new Thickness(val);
        }
        else
        {
            var pmr = System.Convert.ToDouble(parameter, culture);
            return val * pmr;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
}

/// <summary>
/// Sum values of certain field (name in parameter) in list of elements.
/// </summary>
public class conv_Sum : MarkupExtension, IValueConverter
{
    private static conv_Sum? _conv;
    public override object ProvideValue(IServiceProvider serviceProvider) => _conv ??= new conv_Sum();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var val = value as dynamic;
        var res = 0d;

        /// calculate result
        if (val != null)
            foreach (var item in val)
            {
                var fld = item?.GetType().GetProperty(parameter.ToString());
                res += System.Convert.ToDouble(fld.GetValue(item));
            }
        return res;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
}
#endregion

#region StringConverters
/// <summary>
/// Converts string -> string : converter parameter has to be like 'stringCondition~whatToDisplayWhenTrue~whatToDisplayWhenFalse'.
/// Use "!" at the beginning of converter parameter to invert output value.
/// If value == stringCondition then displays "whatToDisplayWhenTrue" else displays "whatToDisplayWhenFalse".
/// </summary>
public class conv_StringToString : MarkupExtension, IValueConverter
{
    private static conv_StringToString? _conv;
    public override object ProvideValue(IServiceProvider serviceProvider) => _conv ??= new conv_StringToString();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var rev = parameter?.ToString()?.StartsWith('!') ?? false;
        var pmr = parameter?.ToString()?.TrimStart('!')?.Split('~') ?? new string[3];
        var val = System.Convert.ToString(value, culture);

        return ((val == pmr[0]) ^ rev) ? pmr[1] : pmr[2];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
}
#endregion
