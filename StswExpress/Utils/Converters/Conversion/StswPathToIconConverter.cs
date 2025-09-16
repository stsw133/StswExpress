using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;
/// <summary>
/// Converts a file path into an icon representation as an <see cref="System.Windows.Media.ImageSource"/>.
/// This converter extracts the associated icon of a file or folder and converts it into an image source.
/// </summary>
[StswInfo("0.15.0", "0.20.0")]
public class StswPathToIconConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswPathToIconConverter Instance => instance ??= new StswPathToIconConverter();
    private static StswPathToIconConverter? instance;

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is string path && !string.IsNullOrWhiteSpace(path)
            ? StswFnUI.ExtractAssociatedIcon(path)?.ToImageSource()
            : Binding.DoNothing;
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => Binding.DoNothing;
}
