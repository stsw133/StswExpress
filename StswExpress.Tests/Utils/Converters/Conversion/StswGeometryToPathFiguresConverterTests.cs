using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace StswExpress.Tests.Utils.Converters;
public class StswGeometryToPathFiguresConverterTests
{
    private readonly StswGeometryToPathFiguresConverter _converter = StswGeometryToPathFiguresConverter.Instance;

    [Fact]
    public void Convert_NullInput_ReturnsBindingDoNothing()
    {
        var result = _converter.Convert(null, typeof(PathFigureCollection), null, CultureInfo.InvariantCulture);
        Assert.Equal(Binding.DoNothing, result);
    }

    [Fact]
    public void Convert_NonGeometryInput_ReturnsBindingDoNothing()
    {
        var result = _converter.Convert("notageometry", typeof(PathFigureCollection), null, CultureInfo.InvariantCulture);
        Assert.Equal(Binding.DoNothing, result);
    }

    [Fact]
    public void Convert_Geometry_ReturnsPathFigureCollection()
    {
        var geometry = Geometry.Parse("M 0,0 L 10,0 10,10 0,10 Z");
        var result = _converter.Convert(geometry, typeof(PathFigureCollection), null, CultureInfo.InvariantCulture);

        Assert.IsType<PathFigureCollection>(result);
        var figures = (PathFigureCollection)result;
        Assert.NotEmpty(figures);
        Assert.Equal(1, figures.Count);
        Assert.True(figures[0].IsClosed);
    }

    [Fact]
    public void Convert_GeometryWithMultipleFigures_ReturnsAllFigures()
    {
        var geometry = Geometry.Parse("M 0,0 L 10,0 M 20,20 L 30,30");
        var result = _converter.Convert(geometry, typeof(PathFigureCollection), null, CultureInfo.InvariantCulture);

        Assert.IsType<PathFigureCollection>(result);
        var figures = (PathFigureCollection)result;
        Assert.Equal(2, figures.Count);
    }

    [Fact]
    public void ConvertBack_Always_ReturnsBindingDoNothing()
    {
        var geometry = Geometry.Parse("M 0,0 L 10,0 10,10 0,10 Z");
        var result = _converter.ConvertBack(geometry, typeof(PathFigureCollection), null, CultureInfo.InvariantCulture);
        Assert.Equal(Binding.DoNothing, result);
    }

    [Fact]
    public void ProvideValue_ReturnsSingletonInstance()
    {
        var instance = _converter.ProvideValue(null);
        Assert.Same(StswGeometryToPathFiguresConverter.Instance, instance);
    }
}
