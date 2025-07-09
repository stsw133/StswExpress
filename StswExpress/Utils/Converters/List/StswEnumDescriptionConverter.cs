using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// A value converter that retrieves the user-friendly description of an enumeration value.
/// <br/>
/// - Uses the <see cref="DescriptionAttribute"/> to get a human-readable string.  
/// - If no description is found, returns the enumeration value as a string.  
/// - Supports <see cref="Nullable{T}"/> values (e.g., `MyEnum?`).  
/// </summary>
/// <remarks>
/// This converter is useful for displaying user-friendly text in UI elements bound to enumeration values.
/// </remarks>
[Stsw("0.8.0")]
public class StswEnumDescriptionConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswEnumDescriptionConverter Instance => instance ??= new StswEnumDescriptionConverter();
    private static StswEnumDescriptionConverter? instance;

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <summary>
    /// Converts an enumeration value to its description.
    /// </summary>
    /// <param name="value">The enumeration value to convert.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">Unused in this converter.</param>
    /// <param name="culture">The culture to use in the conversion.</param>
    /// <returns>
    /// - The description of the enumeration value if found.  
    /// - The enumeration value as a string if no description is available.  
    /// - <see cref="Binding.DoNothing"/> if the value is not an enumeration.
    /// </returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Enum enumValue)
            return Binding.DoNothing;

        return enumValue.GetDescription();
    }

    /// <summary>
    /// This converter does not support converting back from target value to source value.
    /// </summary>
    /// <param name="value">The value produced by the binding target.</param>
    /// <param name="targetType">The type to convert to.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns><see cref="Binding.DoNothing"/> as the converter does not support converting back.</returns>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => Binding.DoNothing;
}

/* usage:

<TextBlock Text="{Binding Status, Converter={x:Static se:StswEnumDescriptionConverter.Instance}}"/>

<ComboBox ItemsSource="{Binding OrderStatuses}" SelectedItem="{Binding SelectedStatus}">
    <ComboBox.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding Converter={x:Static se:StswEnumDescriptionConverter.Instance}}"/>
        </DataTemplate>
    </ComboBox.ItemTemplate>
</ComboBox>

*/
