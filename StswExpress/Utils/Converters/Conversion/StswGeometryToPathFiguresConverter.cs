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
[Stsw("0.15.0", Changes = StswPlannedChanges.None)]
public class StswGeometryToPathFiguresConverter : MarkupExtension, IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static StswGeometryToPathFiguresConverter Instance => _instance ??= new StswGeometryToPathFiguresConverter();
    private static StswGeometryToPathFiguresConverter? _instance;

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

    /// <summary>
    /// Converts a <see cref="Geometry"/> object into its corresponding <see cref="PathFigureCollection"/>.
    /// </summary>
    /// <param name="value">The <see cref="Geometry"/> object to convert.</param>
    /// <param name="targetType">The expected type of the binding target (ignored).</param>
    /// <param name="parameter">An optional converter parameter (ignored).</param>
    /// <param name="culture">The culture to use in the converter (ignored).</param>
    /// <returns>
    /// A <see cref="PathFigureCollection"/> extracted from the provided <see cref="Geometry"/>, 
    /// or <see cref="Binding.DoNothing"/> if conversion is not possible.
    /// </returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is Geometry geometry
            ? geometry.GetFlattenedPathGeometry().Figures
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
