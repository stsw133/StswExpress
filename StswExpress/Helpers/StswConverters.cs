using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;

#region Bool converters
/// <summary>
/// Converts <see cref="bool"/> → targetType.<br/>
/// Use '<c>!</c>' at the beginning of converter parameter to reverse output value.<br/>
/// <br/>
/// When targetType is <see cref="Visibility"/> then output is <c>Visible</c> when <c>true</c>, otherwise <c>Collapsed</c>.<br/>
/// When targetType is anything else then returns <see cref="bool"/> with value depending on converter result.<br/>
/// </summary>
public class StswBoolConverter : MarkupExtension, IValueConverter
{
    private static StswBoolConverter? instance;
    public static StswBoolConverter Instance => instance ??= new StswBoolConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// Convert
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool.TryParse(value?.ToString(), out var val);
        var pmr = parameter?.ToString() ?? string.Empty;

        /// parameters
        bool isReversed = pmr.Contains('!');

        if (isReversed) pmr = pmr.Remove(pmr.IndexOf('!'), 1);

        /// result
        if (targetType == typeof(bool))
            return (val ^ isReversed);
        else if (targetType == typeof(Visibility))
            return (val ^ isReversed) ? Visibility.Visible : Visibility.Collapsed;
        else
            return Binding.DoNothing;
    }

    /// ConvertBack
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}

/// <summary>
/// Compares value parameter to converter parameter.<br/>
/// Use '<c>!</c>' at the beginning of converter parameter to invert output value.<br/>
/// <br/>
/// When targetType is <see cref="Visibility"/> then output is <c>Visible</c> when <c>true</c>, otherwise <c>Collapsed</c>.<br/>
/// When targetType is anything else then returns <see cref="bool"/> with value depending on converter result.<br/>
/// </summary>
public class StswCompareConverter : MarkupExtension, IValueConverter
{
    private static StswCompareConverter? instance;
    public static StswCompareConverter Instance => instance ??= new StswCompareConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

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
/// Checks if value parameter contains converter parameter.<br/>
/// Use '<c>!</c>' at the beginning of converter parameter to invert output value.<br/>
/// <br/>
/// When targetType is <see cref="Visibility"/> then output is <c>Visible</c> when <c>true</c>, otherwise <c>Collapsed</c>.<br/>
/// When targetType is anything else then returns <see cref="bool"/> with value depending on converter result.<br/>
/// </summary>
public class StswContainsConverter : MarkupExtension, IValueConverter
{
    private static StswContainsConverter? instance;
    public static StswContainsConverter Instance => instance ??= new StswContainsConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

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
            var stringValues = enumerable.Cast<object>().Select(x => x?.ToString() ?? string.Empty).ToList();

            if (targetType == typeof(Visibility))
                return (stringValues.Contains(pmr) ^ isReversed) ? Visibility.Visible : Visibility.Collapsed;
            else
                return (stringValues.Contains(pmr) ^ isReversed);
        }
        else if (value?.ToString() != null)
        {
            var stringValue = value.ToString();

            if (targetType == typeof(Visibility))
                return (stringValue?.Contains(pmr) == true ^ isReversed) ? Visibility.Visible : Visibility.Collapsed;
            else
                return (stringValue?.Contains(pmr) == true ^ isReversed);
        }
        return false;
    }

    /// ConvertBack
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}

/// <summary>
/// Checks if value parameter is not null.<br/>
/// <br/>
/// When targetType is <see cref="Visibility"/> then output is <c>Visible</c> when <c>true</c>, otherwise <c>Collapsed</c>.<br/>
/// When targetType is anything else then returns <see cref="bool"/> with value depending on converter result.<br/>
/// </summary>
public class StswNotNullConverter : MarkupExtension, IValueConverter
{
    private static StswNotNullConverter? instance;
    public static StswNotNullConverter Instance => instance ??= new StswNotNullConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

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

#region Color converters
/// <summary>
/// Used for color manipulation and conversion based on the provided parameters:<br/>
/// <br/>
/// Use '<c>!</c>' at the beginning of converter parameter to invert output color.<br/>
/// Use '<c>‼</c>' at the beginning of converter parameter to set black color (when input color is bright) or white color (when input color is dark) as output.<br/>
/// Use '<c>↓</c>' at the beginning of converter parameter to desaturate output color.<br/>
/// Use '<c>?</c>' at the beginning of converter parameter to automatically decide if converter needs to increase or decrease brightness of output color.<br/>
/// Use '<c>@</c>' at the end of converter parameter with value between 00 and FF to set alpha of output color.<br/>
/// Use '<c>#</c>' at the beginning of converter parameter to automatically generate color in output based on value string.<br/>
/// Use value between -1.0 and 1.0 to set brightness of output color.<br/>
/// </summary>
[Obsolete]
public class StswColorConverter : MarkupExtension, IValueConverter
{
    private static StswColorConverter? instance;
    public static StswColorConverter Instance => instance ??= new StswColorConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// Convert
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var pmr = parameter?.ToString() ?? string.Empty;
        //var val = value?.ToString() ?? string.Empty;

        /// parameters
        bool invertColor = pmr.Contains('!'),
             contrastColor = pmr.Contains('‼'),
             desaturateColor = pmr.Contains('↓'),
             autoBrightness = pmr.Contains('?'),
             percentBrightness = pmr.Contains('%'),
             generateColor = pmr.Contains('#');
        int setAlpha = pmr.Contains('@') ? System.Convert.ToInt32(pmr[(pmr.IndexOf('@') + 1)..], 16) : -1;

        if (invertColor) pmr = pmr.Remove(pmr.IndexOf('!'), 1);
        if (contrastColor) pmr = pmr.Remove(pmr.IndexOf('‼'), 1);
        if (desaturateColor) pmr = pmr.Remove(pmr.IndexOf('↓'), 1);
        if (autoBrightness) pmr = pmr.Remove(pmr.IndexOf('?'), 1);
        if (percentBrightness) pmr = pmr.Remove(pmr.IndexOf('%'), 1);
        if (generateColor) pmr = pmr.Remove(pmr.IndexOf('#'), 1);
        if (setAlpha >= 0) pmr = pmr.Remove(pmr.IndexOf('@'));

        var generatedColor = string.Empty;
        var pmrVal = System.Convert.ToDouble(string.IsNullOrEmpty(pmr) ? "0" : pmr, culture);

        /// generate color
        Color color;
        if (generateColor)
        {
            var val = value?.ToString() ?? string.Empty;
            color = Color.FromArgb(255, (byte)(220 - (val.Sum(x => x) * 9797 % 90)), (byte)(220 - (val.Sum(x => x) * 8989 % 90)), (byte)(220 - (val.Sum(x => x) * 8383 % 90)));
        }
        else if (value is Color c)
            color = c;
        else if (value is System.Drawing.Color d)
            color = Color.FromArgb(d.A, d.R, d.G, d.B);
        else if (value is SolidColorBrush br)
            color = br.ToColor();
        else
            color = (Color)ColorConverter.ConvertFromString(value?.ToString() ?? string.Empty);

        /// desaturate color
        if (desaturateColor)
            color = Color.FromArgb(color.A, (byte)((color.R + color.G + color.B) / 3), (byte)((color.R + color.G + color.B) / 3), (byte)((color.R + color.G + color.B) / 3));

        /// invert color
        if (invertColor)
            color = Color.FromArgb(color.A, (byte)(255 - color.R), (byte)(255 - color.G), (byte)(255 - color.B));

        /// contrast color
        if (contrastColor)
        {
            color.GetHsl(out var h, out var s, out var l);
            color = l < 50 ? Color.FromRgb(255, 255 ,255) : Color.FromRgb(0, 0, 0);
        }

        /// brightness
        color = (Color)ColorConverter.ConvertFromString(StswColorBrightnessConverter.Instance.Convert(color.ToHtml(), targetType, $"{(autoBrightness ? "?" : string.Empty)}{pmrVal}{(percentBrightness ? "%" : string.Empty)}", culture).ToString());
        color = Color.FromArgb((byte)(setAlpha.Between(0, 255) ? setAlpha : color.A), color.R, color.G, color.B);

        return color.ToHtml();
    }

    /// ConvertBack
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}

/// <summary>
/// Takes a color value as input and changes its brightness based on the provided parameters:<br/>
/// <br/>
/// Use nothing or '<c>+</c>' at the beginning of converter parameter to increase brightness of output color.<br/>
/// Use '<c>-</c>' at the beginning of converter parameter to decrease brightness of output color.<br/>
/// Use '<c>?</c>' at the beginning of converter parameter to automatically decide if converter needs to increase or decrease brightness of output color.<br/>
/// Use '<c>%</c>' at the end of converter parameter to use percent values.<br/>
/// Use value in parameter between -255 and 255 (or -100 to 100 in case of percents) to set brightness of output color.<br/>
/// EXAMPLES:  '16'  '+25'  '-36'  '?49'  '8%'  '+13%'  '-18%'  '?25%'
/// </summary>
public class StswColorBrightnessConverter : MarkupExtension, IValueConverter
{
    private static StswColorBrightnessConverter? instance;
    public static StswColorBrightnessConverter Instance => instance ??= new StswColorBrightnessConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// Convert
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var pmr = parameter?.ToString() ?? string.Empty;
        //var val = value?.ToString() ?? string.Empty;

        /// parameters
        bool isAuto = pmr.Contains('?'),
             isPercent = pmr.Contains('%');

        if (isAuto) pmr = pmr.Remove(pmr.IndexOf('?'), 1);
        if (isPercent) pmr = pmr.Remove(pmr.IndexOf('%'), 1);

        /// value as color and parameter as number
        Color color;
        if (value is Color c)
            color = c;
        else if (value is System.Drawing.Color d)
            color = Color.FromArgb(d.A, d.R, d.G, d.B);
        else if (value is SolidColorBrush br)
            color = br.ToColor();
        else
            color = (Color)ColorConverter.ConvertFromString(value?.ToString() ?? string.Empty);

        if (!double.TryParse(pmr, NumberStyles.Number, culture, out var pmrVal))
            pmrVal = 0;

        if (isAuto)
        {
            color.GetHsl(out var h, out var s, out var l);
            pmrVal = l < 50 ? Math.Abs(pmrVal) : Math.Abs(pmrVal) * -1;
        }

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

        return color.ToHtml();
    }

    /// ConvertBack
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
#endregion

#region Format converters
/// <summary>
/// Converts a numeric value into a string with a decimal separator that is appropriate for a given culture.
/// </summary>
public class StswMultiCultureNumberConverter : MarkupExtension, IValueConverter
{
    private static StswMultiCultureNumberConverter? instance;
    public static StswMultiCultureNumberConverter Instance => instance ??= new StswMultiCultureNumberConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

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

#region Numeric converters
/// <summary>
/// Adds a numerical value to another numerical value or a set of values.<br/>
/// Converter supports <see cref="CornerRadius"/>, <see cref="Thickness"/> and any numeric type (for example <see cref="int"/> and <see cref="double"/>).<br/>
/// </summary>
public class StswAddConverter : MarkupExtension, IValueConverter
{
    private static StswAddConverter? instance;
    public static StswAddConverter Instance => instance ??= new StswAddConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// Convert
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var val = System.Convert.ToDouble(value, culture);
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
            return new CornerRadius(result);
        else if (targetType.In(typeof(Thickness), typeof(Thickness?)))
            return new Thickness(result);
        else if (targetType == typeof(string))
            return result.ToString();
        else
            return result;
    }

    /// ConvertBack
    public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture) => new object[] { Binding.DoNothing };
}

/// <summary>
/// Multiplies a numerical value to another numerical value or a set of values.<br/>
/// Converter supports <see cref="CornerRadius"/>, <see cref="Thickness"/> and any numeric type (for example <see cref="int"/> and <see cref="double"/>).<br/>
/// </summary>
public class StswMultiplyConverter : MarkupExtension, IValueConverter
{
    private static StswMultiplyConverter? instance;
    public static StswMultiplyConverter Instance => instance ??= new StswMultiplyConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// Convert
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var val = System.Convert.ToDouble(value, culture);
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
/// Allows the user to sum up the values of a specified property of each item in a collection or a DataTable.
/// </summary>
public class StswSumConverter : MarkupExtension, IValueConverter
{
    private static StswSumConverter? instance;
    public static StswSumConverter Instance => instance ??= new StswSumConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

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

#region Special converters
/// <summary>
/// Provides a way to convert <c>null</c> values to <see cref="DependencyProperty.UnsetValue"/>.
/// </summary>
public class StswNullToUnsetConverter : MarkupExtension, IValueConverter
{
    private static StswNullToUnsetConverter? instance;
    public static StswNullToUnsetConverter Instance => instance ??= new StswNullToUnsetConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// Convert
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value ?? DependencyProperty.UnsetValue;

    /// ConvertBack
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
#endregion

#region Universal converters
/// <summary>
/// Takes in an input value and a set of parameters in the form of a string separated by a tilde (~) character, which contains three parts:
/// the first part is the condition to evaluate against the input value,
/// the second part is the value to return if the condition is <c>true</c>,
/// and the third part is the value to return if the condition is <c>false</c>.
/// </summary>
public class StswIfElseConverter : MarkupExtension, IValueConverter
{
    private static StswIfElseConverter? instance;
    public static StswIfElseConverter Instance => instance ??= new StswIfElseConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

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
