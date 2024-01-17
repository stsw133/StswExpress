using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// Converts an enumeration type to a list of <see cref="StswSelectionItem"/> for data binding purposes.
/// The converter is designed to generate a list of items, each representing an enumeration value, along with its display name and value.
/// The converter expects the enumeration type as the 'parameter' argument.
/// </summary>
/// <remarks>
/// This can be useful for populating collection controls such as ComboBox or ListBox.
/// </remarks>
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
