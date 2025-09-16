using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// Converts <see langword="null"/> values to <see cref="DependencyProperty.UnsetValue"/> to reset WPF bindings properly.
/// <br/>
/// This is useful in controls like <see cref="System.Windows.Controls.Image"/>, where <c>null</c> does not properly remove a binding value.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;Image Source="{Binding SelectedImagePath, Converter={x:Static se:StswNullToUnsetConverter.Instance}}"/&gt;
/// &lt;TextBox Text="{Binding SelectedText, Converter={x:Static se:StswNullToUnsetConverter.Instance}}"/&gt;
/// &lt;Style TargetType="Button"&gt;
///     &lt;Setter Property="Content" Value="{Binding ButtonText, Converter={x:Static se:StswNullToUnsetConverter.Instance}}"/&gt;
/// &lt;/Style&gt;
/// </code>
/// </example>
[StswInfo(null)]
public class StswNullToUnsetConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswNullToUnsetConverter Instance => instance ??= new StswNullToUnsetConverter();
    private static StswNullToUnsetConverter? instance;

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value ?? DependencyProperty.UnsetValue;

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value == DependencyProperty.UnsetValue ? null : Binding.DoNothing;
}
