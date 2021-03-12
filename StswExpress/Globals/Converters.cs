using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress.Globals
{
	/// <summary>
	/// Convert bool -> !bool , bool -> !Visibility : parameter must be a bool
	/// </summary>
	public class conv_BoolInverted : MarkupExtension, IValueConverter
	{
		private static conv_BoolInverted _conv = null;
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			if (_conv == null)
				_conv = new conv_BoolInverted();
			return _conv;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (targetType == typeof(Visibility))
				return !System.Convert.ToBoolean(value) ? Visibility.Visible : Visibility.Collapsed;
			else
				return !System.Convert.ToBoolean(value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (targetType == typeof(Visibility))
				return !System.Convert.ToBoolean(value) ? Visibility.Collapsed : Visibility.Visible;
			else
				return !System.Convert.ToBoolean(value);
		}
	}

	/// <summary>
	/// Convert bool -> string : parameter must be like 'string~string'
	/// If true then get string on left side of ~ else get string on right side
	/// </summary>
	public class conv_BoolToString : MarkupExtension, IValueConverter
	{
		private static conv_BoolToString _conv = null;
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			if (_conv == null)
				_conv = new conv_BoolToString();
			return _conv;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if ((bool)value) return parameter.ToString().Split('~')[0];
			return parameter.ToString().Split('~')[1];
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return parameter.ToString().Split('~')[1];
		}
	}

	/// <summary>
	/// Convert bool -> Visibility : parameter must be bool
	/// </summary>
	public class conv_BoolToVisibility : MarkupExtension, IValueConverter
	{
		private static conv_BoolToVisibility _conv = null;
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			if (_conv == null)
				_conv = new conv_BoolToVisibility();
			return _conv;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => System.Convert.ToBoolean(value) ? Visibility.Visible : Visibility.Collapsed;

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => System.Convert.ToBoolean(value) ? Visibility.Collapsed : Visibility.Visible;
	}

	/// <summary>
	/// Lighten/darken hex color using parameter from -1.0 to 1.0 : parameter must be a number
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
			Color color = ColorTranslator.FromHtml(value.ToString());

			if (parameter.ToString() == "!")
				return color.GetBrightness() < 0.5 ? Color.White : Color.Black;

			int r = color.R, g = color.G, b = color.B;
			var param = System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
			r += System.Convert.ToInt32((param > 0 ? 255 - r : r) * param);
			g += System.Convert.ToInt32((param > 0 ? 255 - g : g) * param);
			b += System.Convert.ToInt32((param > 0 ? 255 - b : b) * param);

			return ColorTranslator.ToHtml(Color.FromArgb(r, g, b));
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Color color = ColorTranslator.FromHtml(value.ToString());

			if (parameter.ToString() == "!")
				return value;

			byte r = color.R, g = color.G, b = color.B;
			var param = System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
			r = System.Convert.ToByte((-255 * param + r) / (1 - param));
			g = System.Convert.ToByte((-255 * param + g) / (1 - param));
			b = System.Convert.ToByte((-255 * param + b) / (1 - param));
			return ColorTranslator.ToHtml(Color.FromArgb(r, g, b));
		}
	}

	/// <summary>
	/// Compare value to parameter
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
			var rev = parameter?.ToString()?.StartsWith("!") ?? false;
			var param = parameter?.ToString()?.TrimStart('!') ?? string.Empty;

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

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (targetType == typeof(Visibility))
				return Visibility.Collapsed;
			else
				return false;
		}
	}

	/// <summary>
	/// Convert List.Contains(string) : parameter must be a string
	/// If true then returns true or Visibility.Visible else returns false or Visibility.Collapsed
	/// </summary>
	public class conv_ListContains : MarkupExtension, IValueConverter
	{
		private static conv_ListContains _conv = null;
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			if (_conv == null)
				_conv = new conv_ListContains();
			return _conv;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if ((value as List<string>).Contains(parameter.ToString().TrimStart('!')))
			{
				if (targetType == typeof(Visibility))
					return parameter.ToString()[0] != '!' ? Visibility.Visible : Visibility.Collapsed;
				else
					return parameter.ToString()[0] != '!' ? true : false;
			}
			else
			{
				if (targetType == typeof(Visibility))
					return parameter.ToString()[0] != '!' ? Visibility.Collapsed : Visibility.Visible;
				else
					return parameter.ToString()[0] != '!' ? false : true;
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
	/// Convert double -> value * parameter
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
	/// Convert string -> string : parameter must be like 'string~string~string'
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
