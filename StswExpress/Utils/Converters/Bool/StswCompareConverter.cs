using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// A converter that compares a numeric value to a specified threshold and determines if it is
/// greater than, less than, greater than or equal to, or less than or equal to the threshold.<br/>
/// Use one of these: '<c>&gt;</c>', '<c>&gt;=</c>', '<c>&lt;</c>', '<c>&lt;=</c>', '<c>=</c>', '<c>!</c>', '<c>&amp;</c>', '<c>@</c>' at the beginning of converter parameter and number after.<br/>
/// <br/>
/// When targetType is <see cref="Visibility"/> then output is <c>Visible</c> when <see langword="true"/>, otherwise <c>Collapsed</c>.<br/>
/// When targetType is anything else then returns <see cref="bool"/> with value depending on converter result.<br/>
/// </summary>
public class StswCompareConverter : MarkupExtension, IValueConverter
{
    private static StswCompareConverter? instance;
    public static StswCompareConverter Instance => instance ??= new StswCompareConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// Convert
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var input = value?.ToString() ?? string.Empty;
        var pmr = parameter?.ToString() ?? string.Empty;
        var result = false;

        /// parameters
        if (double.TryParse(input, NumberStyles.Number, culture, out var val))
        {
            if (pmr.StartsWith(">=") && double.TryParse(pmr[2..], out var num))
                result = val >= num;
            else if (pmr.StartsWith("<=") && double.TryParse(pmr[2..], out num))
                result = val <= num;
            else if (pmr.StartsWith('>') && double.TryParse(pmr[1..], out num))
                result = val > num;
            else if (pmr.StartsWith('<') && double.TryParse(pmr[1..], out num))
                result = val < num;
            else if (pmr.StartsWith('=') && double.TryParse(pmr[1..], out num))
                result = val == num;
            else if (pmr.StartsWith('!') && double.TryParse(pmr[1..], out num))
                result = val != num;
            else if (pmr.StartsWith('&') && double.TryParse(pmr[1..], out num))
                result = ((int)val & (int)num) > 0;
        }
        else
        {
            if (pmr.StartsWith('@'))
                result = input.Equals(pmr[1..], StringComparison.CurrentCultureIgnoreCase);
            else if (pmr.StartsWith('='))
                result = input.Equals(pmr[1..]);
            else if (pmr.StartsWith('!'))
                result = !input.Equals(pmr[1..]);
        }

        /// result
        if (targetType == typeof(Visibility))
            return result ? Visibility.Visible : Visibility.Collapsed;
        else
            return result.ConvertTo(targetType);
    }

    /// ConvertBack
    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
