using System;
using System.Collections;
using System.Data;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;

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
