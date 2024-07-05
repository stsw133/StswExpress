using System;
using System.Globalization;
using System.Windows.Data;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
internal class StswInlineMultiConverter(StswInlineMultiConverter.ConvertDelegate convert, StswInlineMultiConverter.ConvertBackDelegate? convertBack = null) : IMultiValueConverter
{
    private ConvertDelegate _convert { get; } = convert ?? throw new ArgumentNullException(nameof(convert));
    public delegate object ConvertDelegate(object[] values, Type targetType, object parameter, CultureInfo culture);

    private ConvertBackDelegate? _convertBack { get; } = convertBack;
    public delegate object[] ConvertBackDelegate(object value, Type[] targetTypes, object parameter, CultureInfo culture);

    /// Convert
    public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture) => _convert(values, targetType, parameter, culture);

    /// ConvertBack
    public object[]? ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => (_convertBack != null) ? _convertBack(value, targetTypes, parameter, culture) : throw new NotImplementedException();
}