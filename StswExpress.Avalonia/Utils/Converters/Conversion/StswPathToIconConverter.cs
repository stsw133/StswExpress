using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System.Globalization;

namespace StswExpress.Avalonia;
/// <summary>
/// Converts a file path into an icon representation as an <see cref="IImage"/>.
/// This converter extracts the associated icon of a file or folder and converts it into an image source.
/// </summary>
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
        => value is string path && !string.IsNullOrWhiteSpace(path)
            ? StswFnUI.ExtractAssociatedIcon(path)
            : BindingOperations.DoNothing;

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => BindingOperations.DoNothing;
}
