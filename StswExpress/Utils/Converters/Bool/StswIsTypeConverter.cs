using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// Compares value type to type in parameter.<br/>
/// </summary>
public class StswIsTypeConverter : MarkupExtension, IValueConverter
{
    private static StswIsTypeConverter? instance;
    public static StswIsTypeConverter Instance => instance ??= new StswIsTypeConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// Convert
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value?.GetType() is Type val)
        {
            if (parameter is Type type)
            {
                if (targetType == typeof(Visibility))
                    return val == type ? Visibility.Visible : Visibility.Collapsed;
                else
                    return val == type;
            }
            else return val;
        }
        else return Binding.DoNothing;
    }

    /// ConvertBack
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
