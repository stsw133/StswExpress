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
public class StswNullToUnsetConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswNullToUnsetConverter Instance => instance ??= new StswNullToUnsetConverter();
    private static StswNullToUnsetConverter? instance;

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <summary>
    /// Converts a <see langword="null"/> value to <see cref="DependencyProperty.UnsetValue"/>.
    /// </summary>
    /// <param name="value">The value from the binding source.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter (not used).</param>
    /// <param name="culture">The culture used in the conversion.</param>
    /// <returns><see cref="DependencyProperty.UnsetValue"/> if <paramref name="value"/> is <see langword="null"/>; otherwise, the value itself.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value ?? DependencyProperty.UnsetValue;

    /// <summary>
    /// Converts back <see cref="DependencyProperty.UnsetValue"/> to <see langword="null"/> when needed.
    /// </summary>
    /// <param name="value">The value from the binding target.</param>
    /// <param name="targetType">The type to convert back to.</param>
    /// <param name="parameter">The converter parameter (not used).</param>
    /// <param name="culture">The culture used in the conversion.</param>
    /// <returns><see langword="null"/> if <paramref name="value"/> is <see cref="DependencyProperty.UnsetValue"/>; otherwise, <see cref="Binding.DoNothing"/>.</returns>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value == DependencyProperty.UnsetValue ? null : Binding.DoNothing;
}

/* usage:

<Image Source="{Binding SelectedImagePath, Converter={x:Static se:StswNullToUnsetConverter.Instance}}"/>

<TextBox Text="{Binding SelectedText, Converter={x:Static se:StswNullToUnsetConverter.Instance}}"/>

<Style TargetType="Button">
    <Setter Property="Content" Value="{Binding ButtonText, Converter={x:Static se:StswNullToUnsetConverter.Instance}}"/>
</Style>

*/
