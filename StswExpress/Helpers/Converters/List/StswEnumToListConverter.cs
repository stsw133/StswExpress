using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// Converts <see cref="bool"/> → targetType.<br/>
/// Use '<c>!</c>' at the beginning of converter parameter to reverse output value.<br/>
/// <br/>
/// When targetType is <see cref="Visibility"/> then output is <c>Visible</c> when <c>true</c>, otherwise <c>Collapsed</c>.<br/>
/// When targetType is anything else then returns <see cref="bool"/> with value depending on converter result.<br/>
/// </summary>
public class StswEnumToListConverter : MarkupExtension, IValueConverter
{
    private static StswEnumToListConverter? instance;
    public static StswEnumToListConverter Instance => instance ??= new StswEnumToListConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// Convert
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var list = new List<StswSelectionItem>();

        if (parameter is Type type)
            foreach (var elem in Enum.GetNames(type))
                list.Add(new()
                {
                    Display = (type.GetMember(elem).First()?.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute)?.Description ?? elem,
                    Value = Enum.Parse(type, elem)
                });

        return list;
    }

    /// ConvertBack
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
