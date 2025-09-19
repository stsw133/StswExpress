using System.Globalization;
using System.Windows.Data;
using Xunit;

namespace StswExpress.Tests.Utils.Converters;
public class StswMultiCultureNumberConverterTests
{
    private readonly StswMultiCultureNumberConverter _converter = StswMultiCultureNumberConverter.Instance;

    [Theory]
    [InlineData(123.45, typeof(string), null, "123.45", "en-US")]
    [InlineData(123.45, typeof(string), null, "123,45", "pl-PL")]
    [InlineData("123.45", typeof(string), null, "123.45", "en-US")]
    [InlineData("123,45", typeof(string), null, "123,45", "pl-PL")]
    public void Convert_Number_ReturnsExpected(object input, Type targetType, object? parameter, string expected, string cultureName)
    {
        var culture = new CultureInfo(cultureName);
        var result = _converter.Convert(input, targetType, parameter, culture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("123.45", typeof(decimal), null, 123.45, "en-US")]
    [InlineData("123,45", typeof(decimal), null, 123.45, "pl-PL")]
    [InlineData("123.45", typeof(double), null, 123.45, "en-US")]
    [InlineData("123,45", typeof(double), null, 123.45, "pl-PL")]
    public void ConvertBack_String_ReturnsExpected(string input, Type targetType, object? parameter, object expected, string cultureName)
    {
        var culture = new CultureInfo(cultureName);
        var result = _converter.ConvertBack(input, targetType, parameter, culture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("notanumber")]
    public void ConvertBack_InvalidInput_ReturnsBindingDoNothing(string input)
    {
        var result = _converter.ConvertBack(input, typeof(decimal), null, CultureInfo.InvariantCulture);
        Assert.Equal(Binding.DoNothing, result);
    }

    [Fact]
    public void Convert_InvalidInput_ReturnsBindingDoNothing()
    {
        var result = _converter.Convert("notanumber", typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal(Binding.DoNothing, result);
    }

    [Fact]
    public void ProvideValue_ReturnsSingletonInstance()
    {
        var instance = _converter.ProvideValue(null);
        Assert.Same(StswMultiCultureNumberConverter.Instance, instance);
    }
}
