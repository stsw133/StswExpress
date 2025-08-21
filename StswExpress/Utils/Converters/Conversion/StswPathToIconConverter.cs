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

    /// <summary>
    /// Converts a file path into its associated icon as an <see cref="System.Windows.Media.ImageSource"/>.
    /// </summary>
    /// <param name="value">The file path as a string.</param>
    /// <param name="targetType">The expected type of the binding target (ignored).</param>
    /// <param name="parameter">An optional converter parameter (ignored).</param>
    /// <param name="culture">The culture to use in the converter (ignored).</param>
    /// <returns>
    /// The extracted icon as an <see cref="System.Windows.Media.ImageSource"/>,
    /// or <see cref="Binding.DoNothing"/> if the conversion fails.
    /// </returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is string path && !string.IsNullOrWhiteSpace(path)
            ? StswFnUI.ExtractAssociatedIcon(path)?.ToImageSource()
            : Binding.DoNothing;
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
