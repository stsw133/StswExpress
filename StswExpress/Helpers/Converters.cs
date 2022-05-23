using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress
{
    /// <summary>
    /// Converts bool -> targetType : value parameter has to be a bool
    /// Use "!" at the beginning of converter parameter to invert output value
    /// When targetType = "string" then converter parameter must contains "~" and be like "what_to_display_when_true~what_to_display_when_false"
    /// When targetType = "Visibility" then output is Visible when true or Collapsed when false
    /// When targetType is anything else then returns bool depending on value parameter
    /// </summary>
    public class conv_Bool : MarkupExtension, IValueConverter
    {
        private static conv_Bool? _conv;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_conv == null)
                _conv = new conv_Bool();
            return _conv;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool val = System.Convert.ToBoolean(value, CultureInfo.InvariantCulture);
            string param = parameter?.ToString() ?? string.Empty;

            /// invert
            if (param.StartsWith('!'))
            {
                val = !val;
                param = param[1..];
            }

            /// calculate result
            if (param.Contains('~'))
                return val ? param.Split('~')[0] : param.Split('~')[1];
            else if (targetType == typeof(Visibility))
                return val ? Visibility.Visible : Visibility.Collapsed;
            else
                return val;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
    }

    /// <summary>
    /// Darkens/brightens hex color using converter parameter from -1.0 to 1.0 : converter parameter has to be a number
    /// Use "!" at the beginning of converter parameter to invert input color
    /// Use "‼" at the beginning of converter parameter to get black color (when input color is bright) or white color (when input color is dark)
    /// Use "?" at the beginning of converter parameter to automatically set converter parameter based on brightness of input color
    /// Use "D" at the beginning of converter parameter to automatically darken the input color based on enabled dark mode
    /// Use "G" at the beginning of converter parameter to automatically generate color based on value char array
    /// Use "S" at the beginning of converter parameter to saturate input color
    /// </summary>
    public class conv_Color : MarkupExtension, IValueConverter
    {
        private static conv_Color? _conv;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_conv == null)
                _conv = new conv_Color();
            return _conv;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string val = value?.ToString() ?? string.Empty;
            string paramRaw = parameter?.ToString() ?? string.Empty;

            bool generateRandomColor = paramRaw.Contains('G'),
                desaturateColor = paramRaw.Contains('↓'),
                invertColor = paramRaw.Contains('!'),
                contrastColor = paramRaw.Contains('‼'),
                autoBrightness = paramRaw.Contains('?'),
                generateColor = paramRaw.Contains('#');
            string generatedColor = generateColor ? paramRaw[paramRaw.IndexOf("#")..] : string.Empty;
            paramRaw = paramRaw.IndexOf("#") > 0 ? paramRaw.Remove(paramRaw.IndexOf("#")) : paramRaw;
            paramRaw = new string(paramRaw.Where(c => char.IsDigit(c) || c == '-' || c == '.').ToArray());

            double param = System.Convert.ToDouble(string.IsNullOrEmpty(paramRaw) ? "0" : paramRaw, CultureInfo.InvariantCulture);

            /// generate color
            Color color = generateColor
                    ? ColorTranslator.FromHtml(generatedColor)
                : generateRandomColor
                    ? Color.FromArgb(255, 224 - (val.Sum(x => x) * 9797 % 128), 224 - (val.Sum(x => x) * 8989 % 128), 224 - (val.Sum(x => x) * 8383 % 128))
                    : ColorTranslator.FromHtml(val);

            if (generateRandomColor)
                color = Color.FromArgb(255, color.R, color.G, color.B);

            /// saturate color
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
                param = color.GetBrightness() < 0.5 ? Math.Abs(param) : Math.Abs(param) * -1;

            /// calculate new color
            int r = color.R, g = color.G, b = color.B;
            r += System.Convert.ToInt32((param > 0 ? 255 - r : r) * param);
            g += System.Convert.ToInt32((param > 0 ? 255 - g : g) * param);
            b += System.Convert.ToInt32((param > 0 ? 255 - b : b) * param);
            color = Color.FromArgb(color.A, r, g, b);

            return ColorTranslator.ToHtml(color).Insert(1, color.A.ToString("X2", null));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
    }

    /// <summary>
    /// Compares value parameter to converter parameter
    /// Use "!" at the beginning of converter parameter to invert output value
    /// When targetType = "Visibility" then output is Visible when true or Collapsed when false
    /// When targetType is anything else then returns bool depending on compare result
    /// </summary>
    public class conv_Compare : MarkupExtension, IValueConverter
    {
        private static conv_Compare? _conv;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_conv == null)
                _conv = new conv_Compare();
            return _conv;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool rev = parameter?.ToString()?.StartsWith('!') ?? false;
            string val = value?.ToString() ?? string.Empty;
            string param = parameter?.ToString()?.TrimStart('!') ?? string.Empty;

            /// calculate result
            if (targetType == typeof(Visibility))
                return (val == param && !rev) || (val != param && rev) ? Visibility.Visible : Visibility.Collapsed;
            else
                return (val == param && !rev) || (val != param && rev);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
    }

    /// <summary>
    /// Checks if value parameter contains converter parameter : value parameter has to be a IEnumerable of string
    /// Use "!" at the beginning of converter parameter to invert output value
    /// When targetType = "Visibility" then output is Visible when true or Collapsed when false
    /// When targetType is anything else then returns bool depending on contains result
    /// </summary>
    public class conv_Contains : MarkupExtension, IValueConverter
    {
        private static conv_Contains? _conv;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_conv == null)
                _conv = new conv_Contains();
            return _conv;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool rev = parameter?.ToString()?.StartsWith('!') ?? false;
            object? param = parameter.ToString().StartsWith('!') ? parameter?.ToString()?.TrimStart('!') : parameter;

            /// calculate result
            if (targetType == typeof(Visibility))
                return (((IEnumerable<string>)value)?.Contains(param) == true && !rev) ? Visibility.Visible : Visibility.Collapsed;
            else
                return ((IEnumerable<string>)value)?.Contains(param) == true && !rev;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
    }

    /// <summary>
    /// Converts decimal independent of using dot or comma
    /// </summary>
    public class conv_MultiCultureNumber : MarkupExtension, IValueConverter
    {
        private static conv_MultiCultureNumber? _conv;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_conv == null)
                _conv = new conv_MultiCultureNumber();
            return _conv;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CultureInfo ci = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            ci.NumberFormat.NumberDecimalSeparator = ",";
            return ((decimal)value).ToString(ci);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
    }

    /// <summary>
    /// Checks if value parameter is not null
    /// When targetType = "Visibility" then output is Visible when true or Collapsed when false
    /// When targetType is anything else then returns bool depending on value parameter
    /// </summary>
    public class conv_NotNull : MarkupExtension, IValueConverter
    {
        private static conv_NotNull? _conv;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_conv == null)
                _conv = new conv_NotNull();
            return _conv;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(Visibility))
                return value != null ? Visibility.Visible : Visibility.Collapsed;
            else
                return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
    }

    /// <summary>
    /// Converts double -> value * parameter
    /// Convert Thickness(double, double, double, double) -> Thickness(value * parameter, value * parameter, value * parameter, value * parameter)
    /// </summary>
    public class conv_Size : MarkupExtension, IValueConverter
    {
        private static conv_Size? _conv;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_conv == null)
                _conv = new conv_Size();
            return _conv;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double val = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
            if (targetType == typeof(Thickness))
            {
                var paramList = Array.ConvertAll(parameter.ToString().Split(';'), i => System.Convert.ToDouble(i, CultureInfo.InvariantCulture));
                if (paramList.Length == 4)
                    return new Thickness(val * paramList[0],
                                         val * paramList[1],
                                         val * paramList[2],
                                         val * paramList[3]);
                else if (paramList.Length == 2)
                    return new Thickness(val * paramList[0],
                                         val * paramList[1],
                                         val * paramList[0],
                                         val * paramList[1]);
                else if (paramList.Length == 1)
                    return new Thickness(val * paramList[0]);
                else
                    return new Thickness(val);
            }
            else
            {
                var param = parameter == null ? 1 : System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
                return param == 0 ? 0 : val * param;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
    }

    /// <summary>
    /// Converts string -> string : converter parameter has to be like 'string~string~string'
    /// If value parameter is same as first string of converter parameter then displays second string of converter parameter else displays third string of converter parameter
    /// Use "!" at the beginning of converter parameter to invert output value
    /// </summary>
    public class conv_StringToString : MarkupExtension, IValueConverter
    {
        private static conv_StringToString? _conv;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_conv == null)
                _conv = new conv_StringToString();
            return _conv;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var rev = parameter?.ToString()?.StartsWith("!") ?? false;
            var param = parameter?.ToString()?.TrimStart('!')?.Split('~');

            return ((value.ToString() == param?[0] && !rev) || (value.ToString() != param?[0] && rev) ? parameter?.ToString()?.Split('~')?[1] : parameter?.ToString()?.Split('~')?[2]) ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
    }
}
