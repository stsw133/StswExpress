using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace StswExpress.Tests;
public class StswIsTypeConverterTests
{
    private readonly StswIsTypeConverter _converter = StswIsTypeConverter.Instance;

    [Theory]
    [InlineData("test", typeof(bool), typeof(string), true)]
    [InlineData(123, typeof(bool), typeof(int), true)]
    [InlineData(123.45, typeof(bool), typeof(double), true)]
    [InlineData(123, typeof(bool), typeof(double), false)]
    [InlineData(null, typeof(bool), typeof(string), false)]
    public void Convert_TypeParameter_ReturnsExpected(object? input, Type targetType, object parameter, bool expected)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("test", typeof(bool), "System.String", true)]
    [InlineData(123, typeof(bool), "System.Int32", true)]
    [InlineData(123.45, typeof(bool), "System.Double", true)]
    [InlineData(123, typeof(bool), "System.Double", false)]
    [InlineData(null, typeof(bool), "System.String", false)]
    public void Convert_StringParameter_ReturnsExpected(object? input, Type targetType, string parameter, bool expected)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("test", typeof(Visibility), typeof(string), Visibility.Visible)]
    [InlineData(123, typeof(Visibility), typeof(int), Visibility.Visible)]
    [InlineData(123.45, typeof(Visibility), typeof(double), Visibility.Visible)]
    [InlineData(123, typeof(Visibility), typeof(double), Visibility.Collapsed)]
    [InlineData(null, typeof(Visibility), typeof(string), Visibility.Collapsed)]
    public void Convert_TypeParameter_Visibility_ReturnsExpected(object? input, Type targetType, object parameter, Visibility expected)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("test", typeof(Visibility), "System.String", Visibility.Visible)]
    [InlineData(123, typeof(Visibility), "System.Int32", Visibility.Visible)]
    [InlineData(123.45, typeof(Visibility), "System.Double", Visibility.Visible)]
    [InlineData(123, typeof(Visibility), "System.Double", Visibility.Collapsed)]
    [InlineData(null, typeof(Visibility), "System.String", Visibility.Collapsed)]
    public void Convert_StringParameter_Visibility_ReturnsExpected(object? input, Type targetType, string parameter, Visibility expected)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Convert_InvalidParameterType_ReturnsBindingDoNothing()
    {
        var result = _converter.Convert("test", typeof(bool), 123, CultureInfo.InvariantCulture);
        Assert.Equal(Binding.DoNothing, result);
    }

    [Fact]
    public void Convert_InvalidTypeName_ReturnsBindingDoNothing()
    {
        var result = _converter.Convert("test", typeof(bool), "Non.Existing.Type", CultureInfo.InvariantCulture);
        Assert.Equal(Binding.DoNothing, result);
    }

    [Fact]
    public void ConvertBack_Always_ReturnsBindingDoNothing()
    {
        var result = _converter.ConvertBack(true, typeof(bool), null, CultureInfo.InvariantCulture);
        Assert.Equal(Binding.DoNothing, result);
    }

    [Fact]
    public void ProvideValue_ReturnsSingletonInstance()
    {
        var instance = _converter.ProvideValue(null);
        Assert.Same(StswIsTypeConverter.Instance, instance);
    }
}
