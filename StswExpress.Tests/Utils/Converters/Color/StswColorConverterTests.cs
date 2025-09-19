using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Xunit;

namespace StswExpress.Tests.Utils.Converters;
public class StswColorConverterTests
{
    private readonly StswColorConverter _converter = StswColorConverter.Instance;

    [Theory]
    [InlineData("#FF0000", typeof(Color), null, 255, 255, 0, 0)] // Red
    [InlineData("Blue", typeof(Color), null, 255, 0, 0, 255)]
    [InlineData("#8000FF00", typeof(Color), null, 128, 0, 255, 0)] // Semi-transparent green
    public void Convert_StringToColor_ReturnsExpected(string input, Type targetType, object? parameter, byte a, byte r, byte g, byte b)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.IsType<Color>(result);
        var color = (Color)result;
        Assert.Equal(a, color.A);
        Assert.Equal(r, color.R);
        Assert.Equal(g, color.G);
        Assert.Equal(b, color.B);
    }

    [Theory]
    [InlineData(typeof(SolidColorBrush))]
    [InlineData(typeof(Brush))]
    public void Convert_ColorToBrush_ReturnsSolidColorBrush(Type targetType)
    {
        var color = Colors.Red;
        var result = _converter.Convert(color, targetType, null, CultureInfo.InvariantCulture);
        Assert.IsType<SolidColorBrush>(result);
        var brush = (SolidColorBrush)result;
        Assert.Equal(color, brush.Color);
    }

    [Fact]
    public void Convert_ColorToDrawingColor_ReturnsExpected()
    {
        var color = Color.FromArgb(128, 10, 20, 30);
        var result = _converter.Convert(color, typeof(System.Drawing.Color), null, CultureInfo.InvariantCulture);
        Assert.IsType<System.Drawing.Color>(result);
        var drawingColor = (System.Drawing.Color)result;
        Assert.Equal(color.A, drawingColor.A);
        Assert.Equal(color.R, drawingColor.R);
        Assert.Equal(color.G, drawingColor.G);
        Assert.Equal(color.B, drawingColor.B);
    }

    [Theory]
    [InlineData("A50%", 128)]
    [InlineData("A10%", 26)]
    [InlineData("A255", 255)]
    [InlineData("A0", 0)]
    public void Convert_AlphaAdjustment_ReturnsExpected(string parameter, byte expectedAlpha)
    {
        var color = Colors.Red;
        var result = _converter.Convert(color, typeof(Color), parameter, CultureInfo.InvariantCulture);
        var adjusted = (Color)result;
        Assert.Equal(expectedAlpha, adjusted.A);
        Assert.Equal(color.R, adjusted.R);
        Assert.Equal(color.G, adjusted.G);
        Assert.Equal(color.B, adjusted.B);
    }

    [Theory]
    [InlineData("B20%", 255, 51, 51)] // Brighten by 20%
    [InlineData("B-20%", 204, 0, 0)] // Darken by 20%
    public void Convert_BrightnessAdjustment_Percent_ReturnsExpected(string parameter, byte expectedR, byte expectedG, byte expectedB)
    {
        var color = Color.FromArgb(255, 255, 0, 0); // Red
        var result = _converter.Convert(color, typeof(Color), parameter, CultureInfo.InvariantCulture);
        var adjusted = (Color)result;
        Assert.Equal(expectedR, adjusted.R);
        Assert.Equal(expectedG, adjusted.G);
        Assert.Equal(expectedB, adjusted.B);
    }

    [Theory]
    [InlineData("S0%", 255, 85, 85)] // Desaturate to gray
    [InlineData("S100%", 255, 0, 0)] // Full saturation (original)
    [InlineData("S50%", 255, 42, 42)] // Half saturation
    public void Convert_SaturationAdjustment_ReturnsExpected(string parameter, byte expectedR, byte expectedG, byte expectedB)
    {
        var color = Color.FromArgb(255, 255, 0, 0); // Red
        var result = _converter.Convert(color, typeof(Color), parameter, CultureInfo.InvariantCulture);
        var adjusted = (Color)result;
        Assert.Equal(expectedR, adjusted.R);
        Assert.Equal(expectedG, adjusted.G);
        Assert.Equal(expectedB, adjusted.B);
    }

    [Fact]
    public void Convert_GenerateColor_ReturnsConsistentColor()
    {
        var value = "TestSeed";
        var param = "G";
        var color1 = (Color)_converter.Convert(value, typeof(Color), param, CultureInfo.InvariantCulture);
        var color2 = (Color)_converter.Convert(value, typeof(Color), param, CultureInfo.InvariantCulture);
        Assert.Equal(color1, color2);
    }

    [Fact]
    public void Convert_GenerateColorWithSeed_ReturnsDifferentColors()
    {
        var value = "TestSeed";
        var color1 = (Color)_converter.Convert(value, typeof(Color), "G1", CultureInfo.InvariantCulture);
        var color2 = (Color)_converter.Convert(value, typeof(Color), "G2", CultureInfo.InvariantCulture);
        Assert.NotEqual(color1, color2);
    }

    [Fact]
    public void Convert_InvalidInput_ReturnsTransparent()
    {
        var result = _converter.Convert("notacolor", typeof(Color), null, CultureInfo.InvariantCulture);
        Assert.Equal(Colors.Transparent, result);
    }

    [Fact]
    public void ConvertBack_Always_ReturnsBindingDoNothing()
    {
        var result = _converter.ConvertBack(Colors.Red, typeof(Color), null, CultureInfo.InvariantCulture);
        Assert.Equal(Binding.DoNothing, result);
    }

    [Fact]
    public void ProvideValue_ReturnsSingletonInstance()
    {
        var instance = _converter.ProvideValue(null);
        Assert.Same(StswColorConverter.Instance, instance);
    }
}
