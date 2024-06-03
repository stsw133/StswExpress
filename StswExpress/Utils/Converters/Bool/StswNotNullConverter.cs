using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// Checks if value parameter is not null.<br/>
/// <br/>
/// When targetType is <see cref="Visibility"/> then output is <c>Visible</c> when <see langword="true"/>, otherwise <c>Collapsed</c>.<br/>
/// When targetType is anything else then returns <see cref="bool"/> with value depending on converter result.<br/>
/// </summary>
public class StswNotNullConverter : MarkupExtension, IValueConverter
{
    private static StswNotNullConverter? instance;
    public static StswNotNullConverter Instance => instance ??= new StswNotNullConverter();
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// Convert
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        /// result
        if (targetType == typeof(Visibility))
            return value is not null ? Visibility.Visible : Visibility.Collapsed;
        else
            return value is not null;
    }

    /// ConvertBack
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
