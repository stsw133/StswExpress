using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// Converts a numeric value into a string with a decimal separator that is appropriate for a given culture.
/// </summary>
public class StswMultiCultureNumberConverter : MarkupExtension, IValueConverter
{
    private static StswMultiCultureNumberConverter? instance;
    public static StswMultiCultureNumberConverter Instance => instance ??= new StswMultiCultureNumberConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// Convert
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        /// result
        var ci = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        ci.NumberFormat.NumberDecimalSeparator = ",";
        return ((decimal)value).ToString(ci);
    }

    /// ConvertBack
    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
