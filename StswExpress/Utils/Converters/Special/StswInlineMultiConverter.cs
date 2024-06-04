using System;
using System.Globalization;
using System.Windows.Data;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
public class StswInlineMultiConverter(StswInlineMultiConverter.ConvertDelegate convert, StswInlineMultiConverter.ConvertBackDelegate? convertBack = null) : IMultiValueConverter
{
    private ConvertDelegate convert { get; } = convert ?? throw new ArgumentNullException(nameof(convert));
    public delegate object ConvertDelegate(object[] values, Type targetType, object parameter, CultureInfo culture);

    private ConvertBackDelegate? convertBack { get; } = convertBack;
    public delegate object[] ConvertBackDelegate(object value, Type[] targetTypes, object parameter, CultureInfo culture);

    /// Convert
    public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture) => convert(values, targetType, parameter, culture);

    /// ConvertBack
    public object[]? ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => (convertBack != null) ? convertBack(value, targetTypes, parameter, culture) : throw new NotImplementedException();
}