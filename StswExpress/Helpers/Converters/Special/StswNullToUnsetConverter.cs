using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// Provides a way to convert <c>null</c> values to <see cref="DependencyProperty.UnsetValue"/>.
/// </summary>
public class StswNullToUnsetConverter : MarkupExtension, IValueConverter
{
    private static StswNullToUnsetConverter? instance;
    public static StswNullToUnsetConverter Instance => instance ??= new StswNullToUnsetConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// Convert
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value ?? DependencyProperty.UnsetValue;

    /// ConvertBack
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
