using System;
using System.Globalization;
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
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => parameter is Type type ? value?.GetType() == type : Binding.DoNothing;

    /// ConvertBack
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
