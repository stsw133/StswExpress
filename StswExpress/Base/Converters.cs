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
	/// Converts bool -> targetType : parameter has to be a bool
	/// </summary>
	public class conv_Bool : MarkupExtension, IValueConverter
	{
		private static conv_Bool _conv = null;
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			if (_conv == null)
				_conv = new conv_Bool();
			return _conv;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool val = System.Convert.ToBoolean(value);
			string param = parameter?.ToString();
			if (param?.StartsWith('!') == true)
			{
				val = !val;
				param = param[1..];
			}

			if (targetType == typeof(string))
				return val ? param.Split('~')[0] : param.Split('~')[1];
			else if (targetType == typeof(Visibility))
				return val ? Visibility.Visible : Visibility.Collapsed;
			else
				return val;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool val = System.Convert.ToBoolean(value);
			string param = parameter?.ToString();
			if (param?.StartsWith('!') == true)
			{
				val = !val;
				param = param[1..];
			}

			if (targetType == typeof(string))
				return !val ? param.Split('~')[0] : param.Split('~')[1];
			else if(targetType == typeof(Visibility))
				return !val ? Visibility.Visible : Visibility.Collapsed;
			else
				return !val;
		}
	}

	/// <summary>
	/// Lightens/darkens hex color using parameter from -1.0 to 1.0 : parameter has to be a number
	/// To get font color based on brightness of background color use ! as parameter
	/// </summary>
	public class conv_Color : MarkupExtension, IValueConverter
	{
		private static conv_Color _conv = null;
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			if (_conv == null)
				_conv = new conv_Color();
			return _conv;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Color color = value != null ? ColorTranslator.FromHtml(value.ToString()) : Color.White;

			if (parameter?.ToString() == "!")
				return color.GetBrightness() < 0.5 ? ColorTranslator.ToHtml(Color.White) : ColorTranslator.ToHtml(Color.Black);

			int r = color.R, g = color.G, b = color.B;
			var param = System.Convert.ToDouble(parameter ?? 0, CultureInfo.InvariantCulture);
			r += System.Convert.ToInt32((param > 0 ? 255 - r : r) * param);
			g += System.Convert.ToInt32((param > 0 ? 255 - g : g) * param);
			b += System.Convert.ToInt32((param > 0 ? 255 - b : b) * param);

			return ColorTranslator.ToHtml(Color.FromArgb(r, g, b));
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Color color = value != null ? ColorTranslator.FromHtml(value.ToString()) : Color.White;

			if (parameter?.ToString() == "!")
				return color.GetBrightness() >= 0.5 ? ColorTranslator.ToHtml(Color.White) : ColorTranslator.ToHtml(Color.Black);

			byte r = color.R, g = color.G, b = color.B;
			var param = System.Convert.ToDouble(parameter ?? 0, CultureInfo.InvariantCulture);
			r = System.Convert.ToByte((-255 * param + r) / (1 - param));
			g = System.Convert.ToByte((-255 * param + g) / (1 - param));
			b = System.Convert.ToByte((-255 * param + b) / (1 - param));

			return ColorTranslator.ToHtml(Color.FromArgb(r, g, b));
		}
	}

	/// <summary>
	/// Compares value to parameter
	/// </summary>
	public class conv_Compare : MarkupExtension, IValueConverter
	{
		private static conv_Compare _conv = null;
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			if (_conv == null)
				_conv = new conv_Compare();
			return _conv;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var rev = parameter?.ToString()?.StartsWith('!') ?? false;
			var param = parameter?.ToString()?.TrimStart('!');

			if ((value?.ToString() == param && !rev) || (value?.ToString() != param && rev))
			{
				if (targetType == typeof(Visibility))
					return Visibility.Visible;
				else
					return true;
			}
			else
			{
				if (targetType == typeof(Visibility))
					return Visibility.Collapsed;
				else
					return false;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
	}

	/// <summary>
	/// Checks if value contains parameter
	/// </summary>
	public class conv_Contains : MarkupExtension, IValueConverter
	{
		private static conv_Contains _conv = null;
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			if (_conv == null)
				_conv = new conv_Contains();
			return _conv;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var rev = parameter?.ToString()?.StartsWith('!') ?? false;
			object param = parameter.ToString().StartsWith('!') ? parameter.ToString().TrimStart('!') : parameter;

			if ((value as IEnumerable<string>).Contains(param))
			{
				if (targetType == typeof(Visibility))
					return !rev ? Visibility.Visible : Visibility.Collapsed;
				else
					return !rev;
			}
			else
			{
				if (targetType == typeof(Visibility))
					return rev ? Visibility.Visible : Visibility.Collapsed;
				else
					return rev;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (targetType == typeof(Visibility))
				return Visibility.Collapsed;
			else
				return false;
		}
	}


	/// <summary>
	/// Generates color from string
	/// </summary>
	public class conv_GenerateColor : MarkupExtension, IValueConverter
	{
		private static conv_GenerateColor _conv = null;
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			if (_conv == null)
				_conv = new conv_GenerateColor();
			return _conv;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var col = ColorTranslator.ToHtml(Color.FromArgb(value.ToString().ToCharArray().Select(x => (x - 39) * 100000).Sum() % int.MaxValue));
			return new conv_Color().Convert(col, null, parameter ?? 0, null);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
	}

	/// <summary>
	/// Converts decimal independent of using dot or comma
	/// </summary>
	public class conv_MultiCultureNumber : MarkupExtension, IValueConverter
	{
		private static conv_MultiCultureNumber _conv = null;
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			if (_conv == null)
				_conv = new conv_MultiCultureNumber();
			return _conv;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var ci = CultureInfo.InvariantCulture.Clone() as CultureInfo;
			ci.NumberFormat.NumberDecimalSeparator = ",";
			return ((decimal)value).ToString(ci);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var ci = CultureInfo.InvariantCulture.Clone() as CultureInfo;
			var s = System.Convert.ToString(value);
			decimal d;
			if (decimal.TryParse(s, NumberStyles.Number ^ NumberStyles.AllowThousands, ci, out d))
			{
				return d;
			}
			else
			{
				ci.NumberFormat.NumberDecimalSeparator = ",";
				if (decimal.TryParse(s, NumberStyles.Number ^ NumberStyles.AllowThousands, ci, out d))
					return d;
			}
			return Binding.DoNothing;
		}
	}

	/// <summary>
	/// Checks if value is not null
	/// </summary>
	public class conv_NotNull : MarkupExtension, IValueConverter
	{
		private static conv_NotNull _conv = null;
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

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (targetType == typeof(Visibility))
				return value == null ? Visibility.Visible : Visibility.Collapsed;
			else
				return value == null;
		}
	}

	/// <summary>
	/// Converts double -> value * parameter
	/// Convert Thickness(double, double, double, double) -> Thickness(value * parameter, value * parameter, value * parameter, value * parameter)
	/// </summary>
	public class conv_Size : MarkupExtension, IValueConverter
	{
		private static conv_Size _conv = null;
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			if (_conv == null)
				_conv = new conv_Size();
			return _conv;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var val = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
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
				else
					return new Thickness(val * paramList[0]);
			}
			else
			{
				var param = System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
				return param == 0 ? 0 : val * param;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var val = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
			if (targetType == typeof(Thickness))
			{
				var paramList = Array.ConvertAll(parameter.ToString().Split(';'), i => System.Convert.ToDouble(i, CultureInfo.InvariantCulture));
				if (paramList.Length == 4)
					return new Thickness(paramList[0] == 0 ? 0 : val / paramList[0],
										 paramList[1] == 0 ? 0 : val / paramList[1],
										 paramList[2] == 0 ? 0 : val / paramList[2],
										 paramList[3] == 0 ? 0 : val / paramList[3]);
				else if (paramList.Length == 2)
					return new Thickness(paramList[0] == 0 ? 0 : val / paramList[0],
										 paramList[1] == 0 ? 0 : val / paramList[1],
										 paramList[0] == 0 ? 0 : val / paramList[0],
										 paramList[1] == 0 ? 0 : val / paramList[1]);
				else
					return new Thickness(paramList[0] == 0 ? 0 : val / paramList[0]);
			}
			else
			{
				var param = System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
				return param == 0 ? 0 : val / param;
			}
		}
	}

	/// <summary>
	/// Converts string -> string : parameter has to be like 'string~string~string'
	/// If value=parameter[0] then get string on left side of second ~ else get string on right side
	/// </summary>
	public class conv_StringToString : MarkupExtension, IValueConverter
	{
		private static conv_StringToString _conv = null;
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			if (_conv == null)
				_conv = new conv_StringToString();
			return _conv;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var rev = parameter.ToString().StartsWith("!");
			var param = parameter.ToString().TrimStart('!').Split('~');

			if ((value.ToString() == param[0] && !rev) || (value.ToString() != param[0] && rev))
				return parameter.ToString().Split('~')[1];
			else
				return parameter.ToString().Split('~')[2];
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var rev = parameter.ToString().StartsWith("!");
			var param = parameter.ToString().TrimStart('!').Split('~');

			if ((value.ToString() == param[0] && !rev) || (value.ToString() != param[0] && rev))
				return parameter.ToString().Split('~')[2];
			else
				return parameter.ToString().Split('~')[1];
		}
	}
}
