using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace StswExpress.Tests;
public class StswBoolConverterTests
{
    private readonly StswBoolConverter _converter = StswBoolConverter.Instance;

    [Theory]
    [InlineData(true, typeof(Visibility), null, Visibility.Visible)]
    [InlineData(false, typeof(Visibility), null, Visibility.Collapsed)]
    [InlineData(true, typeof(Visibility), "!", Visibility.Collapsed)]
    [InlineData(false, typeof(Visibility), "!", Visibility.Visible)]
    public void Convert_Visibility_ReturnsExpected(bool input, Type targetType, object? parameter, Visibility expected)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(true, typeof(string), null, "True")]
    [InlineData(false, typeof(string), null, "False")]
    [InlineData(true, typeof(string), "!", "False")]
    [InlineData(false, typeof(string), "!", "True")]
    public void Convert_String_ReturnsExpected(bool input, Type targetType, object? parameter, string expected)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(true, typeof(int), null, 1)]
    [InlineData(false, typeof(int), null, 0)]
    [InlineData(true, typeof(int), "!", 0)]
    [InlineData(false, typeof(int), "!", 1)]
    public void Convert_Int_ReturnsExpected(bool input, Type targetType, object? parameter, int expected)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("true", typeof(bool), null, true)]
    [InlineData("false", typeof(bool), null, false)]
    [InlineData("true", typeof(bool), "!", false)]
    [InlineData("false", typeof(bool), "!", true)]
    public void Convert_StringBoolInput_ReturnsExpected(string input, Type targetType, object? parameter, bool expected)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, typeof(bool), null)]
    [InlineData("", typeof(bool), null)]
    [InlineData("notabool", typeof(int), null)]
    [InlineData(123, typeof(bool), null)]
    public void Convert_InvalidInput_ReturnsBindingDoNothing(object? input, Type targetType, object? parameter)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
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
        Assert.Same(StswBoolConverter.Instance, instance);
    }
}
