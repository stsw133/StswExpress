using System.Globalization;
using System.Windows.Data;

namespace StswExpress.Tests;
public class StswPercentageConverterTests
{
    private readonly StswPercentageConverter _converter = StswPercentageConverter.Instance;

    [Theory]
    [InlineData(0.75, typeof(string), null, "75%")]
    [InlineData(0.75, typeof(string), "N2", "75.00%")]
    [InlineData(0.75, typeof(string), "F1", "75.0%")]
    [InlineData(0.75, typeof(string), "0.000", "75.000%")]
    [InlineData(75, typeof(string), null, "7500%")]
    [InlineData(1, typeof(string), null, "100%")]
    [InlineData(0, typeof(string), null, "0%")]
    [InlineData(-0.5, typeof(string), null, "-50%")]
    [InlineData(0.1234, typeof(string), "N1", "12.3%")]
    public void Convert_ValidNumeric_ReturnsExpected(object input, Type targetType, object? parameter, string expected)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("75%", typeof(double), null, 0.75)]
    [InlineData("75.00%", typeof(double), "N2", 0.75)]
    [InlineData("75.0%", typeof(double), "F1", 0.75)]
    [InlineData("75.000%", typeof(double), "0.000", 0.75)]
    [InlineData("100%", typeof(double), null, 1.0)]
    [InlineData("0%", typeof(double), null, 0.0)]
    [InlineData("-50%", typeof(double), null, -0.5)]
    [InlineData("12.3%", typeof(double), "N1", 0.123)]
    public void ConvertBack_ValidString_ReturnsExpected(string input, Type targetType, object? parameter, double expected)
    {
        var result = _converter.ConvertBack(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("notanumber")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" %")]
    public void ConvertBack_InvalidString_ReturnsBindingDoNothing(object? input)
    {
        var result = _converter.ConvertBack(input, typeof(double), null, CultureInfo.InvariantCulture);
        Assert.Equal(Binding.DoNothing, result);
    }

    [Theory]
    [InlineData("notanumber")]
    [InlineData("")]
    [InlineData(null)]
    public void Convert_InvalidInput_ReturnsBindingDoNothing(object? input)
    {
        var result = _converter.Convert(input, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal(Binding.DoNothing, result);
    }

    [Fact]
    public void ProvideValue_ReturnsSingletonInstance()
    {
        var instance = _converter.ProvideValue(null);
        Assert.Same(StswPercentageConverter.Instance, instance);
    }
}
