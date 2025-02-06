using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows;

namespace StswExpress;
/// <summary>
/// A value converter that checks whether the input value matches the specified parameter.
/// <br/>
/// - Use `!` at the beginning of the converter parameter to invert the output value.  
/// - If the target type is <see cref="Visibility"/>, the result is <see cref="Visibility.Visible"/> when the condition is met, otherwise <see cref="Visibility.Collapsed"/>.  
/// - Otherwise, it returns a <see cref="bool"/> indicating whether the value matches the parameter.
/// </summary>
public class StswRadioConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswRadioConverter Instance => instance ??= new StswRadioConverter();
    private static StswRadioConverter? instance;

    /// <summary>
    /// Provides the singleton instance of the converter for XAML bindings.
    /// </summary>
    /// <param name="serviceProvider">A service provider that can provide services for the markup extension.</param>
    /// <returns>The singleton instance of the converter.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <summary>
    /// Compares the input value with the converter parameter.
    /// </summary>
    /// <param name="value">The source value to compare.</param>
    /// <param name="targetType">The type to convert to.</param>
    /// <param name="parameter">The expected value.</param>
    /// <param name="culture">The culture to use in the conversion.</param>
    /// <returns>
    /// - <see cref="Visibility.Visible"/> if the value matches the parameter (when targetType is <see cref="Visibility"/>).  
    /// - <see langword="true"/> if the value matches the parameter (when targetType is a boolean type).  
    /// - The boolean result converted to the specified <paramref name="targetType"/>.
    /// </returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var val = value?.ToString() ?? string.Empty;
        var pmr = parameter?.ToString() ?? string.Empty;

        var isReversed = pmr.StartsWith('!');
        if (isReversed)
            pmr = pmr[1..];

        var result = (val == pmr) ^ isReversed;

        return StswConverterHelper.ConvertToTargetType(result, targetType);
    }

    /// <summary>
    /// Converts the value back to the converter parameter.
    /// </summary>
    /// <param name="value">The value produced by the binding target.</param>
    /// <param name="targetType">The type to convert to.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>The converter parameter.</returns>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => parameter;
}

/* usage:

<TabControl SelectedItem="{Binding SelectedTab}">
    <TabItem Header="Option 1" IsSelected="{Binding SelectedTab, Converter={x:Static se:StswRadioConverter.Instance}, ConverterParameter=Tab1}"/>
    <TabItem Header="Option 2" IsSelected="{Binding SelectedTab, Converter={x:Static se:StswRadioConverter.Instance}, ConverterParameter=Tab2}"/>
</TabControl>

<RadioButton Content="Option A" IsChecked="{Binding SelectedOption, Converter={x:Static se:StswRadioConverter.Instance}, ConverterParameter=0}"/>
<RadioButton Content="Option B" IsChecked="{Binding SelectedOption, Converter={x:Static se:StswRadioConverter.Instance}, ConverterParameter=1}"/>

<TextBlock Text="Only for users" Visibility="{Binding UserRole, Converter={x:Static se:StswRadioConverter.Instance}, ConverterParameter=Admin}"/>

<TextBlock Text="Limited access" Visibility="{Binding UserRole, Converter={x:Static se:StswRadioConverter.Instance}, ConverterParameter=!Admin}"/>

*/
