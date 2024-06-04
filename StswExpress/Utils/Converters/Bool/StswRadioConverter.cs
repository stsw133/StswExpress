using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows;

namespace StswExpress;

/// <summary>
/// Compares value parameter to converter parameter.<br/>
/// Use '<c>!</c>' at the beginning of converter parameter to invert output value.<br/>
/// <br/>
/// When targetType is <see cref="Visibility"/> then output is <c>Visible</c> when <see langword="true"/>, otherwise <c>Collapsed</c>.<br/>
/// When targetType is anything else then returns <see cref="bool"/> with value depending on converter result.<br/>
/// </summary>
public class StswRadioConverter : MarkupExtension, IValueConverter
{
    private static StswRadioConverter? instance;
    public static StswRadioConverter Instance => instance ??= new StswRadioConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// Convert
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var val = value?.ToString() ?? string.Empty;
        var pmr = parameter?.ToString() ?? string.Empty;

        /// parameters
        var isReversed = pmr.Contains('!');
        if (isReversed)
            pmr = pmr.Remove(pmr.IndexOf('!'), 1);

        /// result
        if (targetType == typeof(Visibility))
            return ((val == pmr) ^ isReversed) ? Visibility.Visible : Visibility.Collapsed;
        else
            return (val == pmr) ^ isReversed;
    }

    /// ConvertBack
    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => parameter;
}