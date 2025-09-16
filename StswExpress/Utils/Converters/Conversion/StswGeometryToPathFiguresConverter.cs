using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// Converts a <see cref="Geometry"/> object into a collection of <see cref="PathFigure"/> elements.
/// This allows extracting path data from a geometry for use in path-based UI elements.
/// </summary>
[StswInfo("0.15.0")]
public class StswGeometryToPathFiguresConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswGeometryToPathFiguresConverter Instance => _instance ??= new StswGeometryToPathFiguresConverter();
    private static StswGeometryToPathFiguresConverter? _instance;

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is Geometry geometry
            ? geometry.GetFlattenedPathGeometry().Figures
            : Binding.DoNothing;
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => Binding.DoNothing;
}
