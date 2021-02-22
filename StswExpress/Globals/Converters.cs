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
	/// Convert bool -> !bool , bool -> !Visibility : parameter must be bool
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
	/// If true then string is on left side of ~ else on right side
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

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return System.Convert.ToBoolean(value) ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return System.Convert.ToBoolean(value) ? Visibility.Collapsed : Visibility.Visible;
		}
	}

	/// <summary>
	/// Compare string to string : parameter must be string
	/// </summary>
	public class conv_CompareStrings : MarkupExtension, IValueConverter
	{
		private static conv_CompareStrings _conv = null;
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			if (_conv == null)
				_conv = new conv_CompareStrings();
			return _conv;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value?.ToString() == (parameter?.ToString() ?? string.Empty))
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
	/// Convert list.contains(string) : parameter must be string
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
	/// Convert double -> double * double : parameter must be number
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
			return System.Convert.ToDouble(value, CultureInfo.InvariantCulture) * System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return System.Convert.ToDouble(value, CultureInfo.InvariantCulture) / System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
		}
	}

	/// <summary>
	/// Lighten/darken hex color using parameter from -1.0 to 1.0 : parameter must be number
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
}
