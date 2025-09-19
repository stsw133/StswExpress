using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress.Tests.Utils.Converters;
public class StswCalculateConverterTests
{
    private readonly StswCalculateConverter _converter = StswCalculateConverter.Instance;

    [Theory]
    [InlineData(10, typeof(double), "+5", 15)]
    [InlineData(10, typeof(double), "-3", 7)]
    [InlineData(10, typeof(double), "*2", 20)]
    [InlineData(10, typeof(double), "/2", 5)]
    [InlineData(10, typeof(double), "%3", 1)]
    [InlineData(2, typeof(double), "^3", 8)]
    public void Convert_Double_ReturnsExpected(double input, Type targetType, string parameter, double expected)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(10, typeof(int), "+5", 15)]
    [InlineData(10, typeof(int), "-3", 7)]
    [InlineData(10, typeof(int), "*2", 20)]
    [InlineData(10, typeof(int), "/2", 5)]
    [InlineData(10, typeof(int), "%3", 1)]
    [InlineData(2, typeof(int), "^3", 8)]
    public void Convert_Int_ReturnsExpected(int input, Type targetType, string parameter, int expected)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(10.0, typeof(CornerRadius), "*2", 20.0)]
    [InlineData(5.0, typeof(CornerRadius), "+3", 8.0)]
    public void Convert_DoubleToCornerRadius_ReturnsExpected(double input, Type targetType, string parameter, double expected)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.IsType<CornerRadius>(result);
        var cr = (CornerRadius)result;
        Assert.Equal(expected, cr.TopLeft);
        Assert.Equal(expected, cr.TopRight);
        Assert.Equal(expected, cr.BottomRight);
        Assert.Equal(expected, cr.BottomLeft);
    }

    [Theory]
    [InlineData(10.0, typeof(Thickness), "*2", 20.0)]
    [InlineData(5.0, typeof(Thickness), "+3", 8.0)]
    public void Convert_DoubleToThickness_ReturnsExpected(double input, Type targetType, string parameter, double expected)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.IsType<Thickness>(result);
        var th = (Thickness)result;
        Assert.Equal(expected, th.Left);
        Assert.Equal(expected, th.Top);
        Assert.Equal(expected, th.Right);
        Assert.Equal(expected, th.Bottom);
    }

    [Theory]
    [InlineData(10.0, typeof(GridLength), "*2", 20.0)]
    [InlineData(5.0, typeof(GridLength), "+3", 8.0)]
    public void Convert_DoubleToGridLength_ReturnsExpected(double input, Type targetType, string parameter, double expected)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.IsType<GridLength>(result);
        var gl = (GridLength)result;
        Assert.Equal(expected, gl.Value);
    }

    [Fact]
    public void Convert_CornerRadius_MultiParam_ReturnsExpected()
    {
        var input = new CornerRadius(1, 2, 3, 4);
        var result = _converter.Convert(input, typeof(CornerRadius), "*2,3,4,5", CultureInfo.InvariantCulture);
        Assert.IsType<CornerRadius>(result);
        var cr = (CornerRadius)result;
        Assert.Equal(2, cr.TopLeft);
        Assert.Equal(6, cr.TopRight);
        Assert.Equal(12, cr.BottomRight);
        Assert.Equal(20, cr.BottomLeft);
    }

    [Fact]
    public void Convert_Thickness_MultiParam_ReturnsExpected()
    {
        var input = new Thickness(1, 2, 3, 4);
        var result = _converter.Convert(input, typeof(Thickness), "+2,3,4,5", CultureInfo.InvariantCulture);
        Assert.IsType<Thickness>(result);
        var th = (Thickness)result;
        Assert.Equal(3, th.Left);
        Assert.Equal(5, th.Top);
        Assert.Equal(7, th.Right);
        Assert.Equal(9, th.Bottom);
    }

    [Fact]
    public void Convert_GridLength_AutoOrStar_ReturnsOriginal()
    {
        var auto = new GridLength(0, GridUnitType.Auto);
        var star = new GridLength(1, GridUnitType.Star);
        var resultAuto = _converter.Convert(auto, typeof(GridLength), "*2", CultureInfo.InvariantCulture);
        var resultStar = _converter.Convert(star, typeof(GridLength), "*2", CultureInfo.InvariantCulture);
        Assert.Same(auto, resultAuto);
        Assert.Same(star, resultStar);
    }

    [Fact]
    public void Convert_DateTime_AddDays_ReturnsExpected()
    {
        var dt = new DateTime(2024, 1, 1);
        var result = _converter.Convert(dt, typeof(DateTime), "+5", CultureInfo.InvariantCulture);
        Assert.IsType<DateTime>(result);
        Assert.Equal(new DateTime(2024, 1, 6), result);
    }

    [Fact]
    public void Convert_InvalidParameter_ReturnsBindingDoNothing()
    {
        var result = _converter.Convert(10, typeof(double), "invalid", CultureInfo.InvariantCulture);
        Assert.Equal(Binding.DoNothing, result);
    }

    [Fact]
    public void Convert_NullValue_ReturnsBindingDoNothing()
    {
        var result = _converter.Convert(null, typeof(double), "*2", CultureInfo.InvariantCulture);
        Assert.Equal(Binding.DoNothing, result);
    }

    [Fact]
    public void ConvertBack_Always_ReturnsBindingDoNothing()
    {
        var result = _converter.ConvertBack(10, typeof(double), "*2", CultureInfo.InvariantCulture);
        Assert.Equal(Binding.DoNothing, result);
    }

    [Fact]
    public void ProvideValue_ReturnsSingletonInstance()
    {
        var instance = _converter.ProvideValue(null);
        Assert.Same(StswCalculateConverter.Instance, instance);
    }
}
