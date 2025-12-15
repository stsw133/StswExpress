using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress.Wpf;
/// <summary>
/// A converter that returns the inverted value for supported types.
/// <br/>
/// For <see cref="bool"/> values, the result is logical negation, for numbers the sign is flipped,
/// strings are reversed character by character, and lists are returned in reversed order.
/// </summary>
public class StswInvertConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswInvertConverter Instance => instance ??= new StswInvertConverter();
    private static StswInvertConverter? instance;

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => Invert(value, culture);

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => Invert(value, culture);

    /// <summary>
    /// Inverts the given value based on its type.
    /// </summary>
    /// <param name="value">The value to invert.</param>
    /// <param name="culture">The culture info.</param>
    /// <returns>The inverted value.</returns>
    private static object? Invert(object? value, CultureInfo culture)
    {
        switch (value)
        {
            case null:
                return null;
            case bool boolValue:
                return !boolValue;
            case string text:
                return new string([.. text.Reverse()]);
            case Array array:
                return InvertArray(array);
            case IList list:
                return InvertList(list);
            default:
                if (value.GetType() is Type type && type.IsNumericType())
                    return InvertNumber(value, Type.GetTypeCode(type));

                if (value is IEnumerable enumerable)
                    return enumerable.Cast<object?>().Reverse().ToList();

                return Binding.DoNothing;
        }
    }

    /// <summary>
    /// Determines whether the specified TypeCode represents a numeric type.
    /// </summary>
    /// <param name="array">The array to invert.</param>
    /// <returns>The inverted array.</returns>
    private static Array InvertArray(Array array)
    {
        var elementType = array.GetType().GetElementType() ?? typeof(object);
        var reversedArray = Array.CreateInstance(elementType, array.Length);
        for (var i = 0; i < array.Length; i++)
            reversedArray.SetValue(array.GetValue(array.Length - i - 1), i);

        return reversedArray;
    }

    /// <summary>
    /// Determines whether the specified TypeCode represents a numeric type.
    /// </summary>
    /// <param name="list">The list to invert.</param>
    /// <returns>The inverted list.</returns>
    private static ArrayList InvertList(IList list)
    {
        var reversedList = new ArrayList(list);
        reversedList.Reverse();
        return reversedList;
    }

    /// <summary>
    /// Determines whether the specified TypeCode represents a numeric type.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="typeCode">The TypeCode to check.</param>
    /// <returns><c>true</c> if the TypeCode is numeric; otherwise, <c>false</c>.</returns>
    private static object InvertNumber(object value, TypeCode typeCode) => typeCode switch
    {
        TypeCode.SByte => -(sbyte)value,
        TypeCode.Int16 => -(short)value,
        TypeCode.Int32 => -(int)value,
        TypeCode.Int64 => -(long)value,
        TypeCode.Byte => -(double)(byte)value,
        TypeCode.UInt16 => -(double)(ushort)value,
        TypeCode.UInt32 => -(double)(uint)value,
        TypeCode.UInt64 => -(double)(ulong)value,
        TypeCode.Single => -(float)value,
        TypeCode.Double => -(double)value,
        TypeCode.Decimal => -(decimal)value,
        _ => Binding.DoNothing
    };
}
