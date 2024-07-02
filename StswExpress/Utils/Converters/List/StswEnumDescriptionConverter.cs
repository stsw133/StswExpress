using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// A converter that converts an enumeration value to its description, using the <see cref="DescriptionAttribute"/>.
/// If no description is found, it returns the enumeration value as a string.
/// </summary>
/// <remarks>
/// This converter can be used in XAML to display user-friendly descriptions for enumeration values.
/// </remarks>
public class StswEnumDescriptionConverter : MarkupExtension, IValueConverter
{
    private static StswEnumDescriptionConverter? instance;
    public static StswEnumDescriptionConverter Instance => instance ??= new StswEnumDescriptionConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// Convert
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var enumValue = (Enum)value;
        if (enumValue == null)
            return null;

        if (enumValue.GetType().GetField(enumValue.ToString())?.GetCustomAttributes(false) is object[] atrArr && atrArr.Length > 0)
            return (atrArr[0] as DescriptionAttribute)?.Description;
        else
            return enumValue.ToString();
    }

    /// ConvertBack
    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
