using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// Converts a range specified by the parameter to a list of integers for data binding purposes.
/// The converter expects the parameter to be a string representing a range in the format "start-end" or "end" (e.g., "2-5" or "3").
/// The generated list contains integers in the specified range using <see cref="Enumerable.Range"/>.
/// </summary>
/// <remarks>
/// This can be useful for populating collection controls such as ComboBox or ListBox.
/// </remarks>
internal class StswListFromRangeConverter : MarkupExtension, IValueConverter
{
    private static StswListFromRangeConverter? instance;
    public static StswListFromRangeConverter Instance => instance ??= new StswListFromRangeConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// Convert
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter?.ToString()?.Split('-') is string[] param)
        {
            if (param.Length >= 2)
            {
                var param1 = int.Parse(param[0]);
                var param2 = int.Parse(param[1]);
                return Enumerable.Range(param1, param2 - param1);
            }
            else return Enumerable.Range(0, int.Parse(param[0]));
        }
        return null;
    }

    /// ConvertBack
    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
