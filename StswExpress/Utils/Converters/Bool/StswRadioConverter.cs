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
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;TabControl SelectedItem="{Binding SelectedTab}"&gt;
///     &lt;TabItem Header="Option 1" IsSelected="{Binding SelectedTab, Converter={x:Static se:StswRadioConverter.Instance}, ConverterParameter=Tab1}"/&gt;
///     &lt;TabItem Header="Option 2" IsSelected="{Binding SelectedTab, Converter={x:Static se:StswRadioConverter.Instance}, ConverterParameter=Tab2}"/&gt;
/// &lt;/TabControl&gt;
/// 
/// &lt;RadioButton Content="Option A" IsChecked="{Binding SelectedOption, Converter={x:Static se:StswRadioConverter.Instance}, ConverterParameter=0}"/&gt;
/// &lt;RadioButton Content="Option B" IsChecked="{Binding SelectedOption, Converter={x:Static se:StswRadioConverter.Instance}, ConverterParameter=1}"/&gt;
/// 
/// &lt;TextBlock Text="Only for users" Visibility="{Binding UserRole, Converter={x:Static se:StswRadioConverter.Instance}, ConverterParameter=Admin}"/&gt;
/// 
/// &lt;TextBlock Text="Limited access" Visibility="{Binding UserRole, Converter={x:Static se:StswRadioConverter.Instance}, ConverterParameter=!Admin}"/&gt;
/// </code>
/// </example>
[StswInfo("0.6.1")]
public class StswRadioConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswRadioConverter Instance => instance ??= new StswRadioConverter();
    private static StswRadioConverter? instance;

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var val = value?.ToString() ?? string.Empty;
        var pmr = parameter?.ToString() ?? string.Empty;

        var isReversed = pmr.StartsWith('!');
        if (isReversed)
            pmr = pmr[1..];

        var result = (val == pmr) ^ isReversed;

        return targetType == typeof(Visibility)
            ? result ? Visibility.Visible : Visibility.Collapsed
            : result.ConvertTo(targetType);
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => parameter;
}
