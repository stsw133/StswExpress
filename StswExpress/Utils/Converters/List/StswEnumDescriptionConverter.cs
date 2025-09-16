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
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;TextBlock Text="{Binding Status, Converter={x:Static se:StswEnumDescriptionConverter.Instance}}"/&gt;
/// 
/// &lt;ComboBox ItemsSource="{Binding OrderStatuses}" SelectedItem="{Binding SelectedStatus}"&gt;
///     &lt;ComboBox.ItemTemplate&gt;
///         &lt;DataTemplate&gt;
///             &lt;TextBlock Text="{Binding Converter={x:Static se:StswEnumDescriptionConverter.Instance}}"/&gt;
///         &lt;/DataTemplate&gt;
///     &lt;/ComboBox.ItemTemplate&gt;
/// &lt;/ComboBox&gt;
/// </code>
/// </example>
[StswInfo("0.8.0")]
public class StswEnumDescriptionConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswEnumDescriptionConverter Instance => instance ??= new StswEnumDescriptionConverter();
    private static StswEnumDescriptionConverter? instance;

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Enum enumValue)
            return Binding.DoNothing;

        return enumValue.GetDescription();
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => Binding.DoNothing;
}
