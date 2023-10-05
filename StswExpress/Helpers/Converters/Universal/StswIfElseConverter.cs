using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;

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
