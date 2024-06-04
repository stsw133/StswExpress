using System;
using System.Collections;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// Checks if value parameter contains converter parameter.<br/>
/// Use '<c>!</c>' at the beginning of converter parameter to invert output value.<br/>
/// <br/>
/// When targetType is <see cref="Visibility"/> then output is <c>Visible</c> when <see langword="true"/>, otherwise <c>Collapsed</c>.<br/>
/// When targetType is anything else then returns <see cref="bool"/> with value depending on converter result.<br/>
/// </summary>
public class StswContainsConverter : MarkupExtension, IValueConverter
{
    private static StswContainsConverter? instance;
    public static StswContainsConverter Instance => instance ??= new StswContainsConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// Convert
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        //var val = value?.ToString() ?? string.Empty;
        var pmr = parameter?.ToString() ?? string.Empty;

        /// parameters
        bool isReversed = pmr.Contains('!');
        if (isReversed)
            pmr = pmr.Remove(pmr.IndexOf('!'), 1);

        /// result
        if (value is IEnumerable enumerable)
        {
            var stringValues = enumerable.Cast<object>().Select(x => x?.ToString() ?? string.Empty).ToList();

            if (targetType == typeof(Visibility))
                return (stringValues.Contains(pmr) ^ isReversed) ? Visibility.Visible : Visibility.Collapsed;
            else
                return (stringValues.Contains(pmr) ^ isReversed);
        }
        else if (value?.ToString() != null)
        {
            var stringValue = value.ToString();

            if (targetType == typeof(Visibility))
                return (stringValue?.Contains(pmr) == true ^ isReversed) ? Visibility.Visible : Visibility.Collapsed;
            else
                return (stringValue?.Contains(pmr) == true ^ isReversed);
        }
        return false;
    }

    /// ConvertBack
    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
