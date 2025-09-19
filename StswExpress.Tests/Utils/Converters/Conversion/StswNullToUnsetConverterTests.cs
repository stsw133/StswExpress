using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace StswExpress.Tests.Utils.Converters;
public class StswNullToUnsetConverterTests
{
    private readonly StswNullToUnsetConverter _converter = StswNullToUnsetConverter.Instance;

    public static IEnumerable<object[]> ConvertTestData()
    {
        yield return new object?[] { null, typeof(object), null, DependencyProperty.UnsetValue };
        yield return new object?[] { "value", typeof(object), null, "value" };
        yield return new object?[] { 123, typeof(int), null, 123 };
    }

    [Theory]
    [MemberData(nameof(ConvertTestData))]
    public void Convert_ReturnsExpected(object? input, Type targetType, object? parameter, object expected)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ConvertBack_UnsetValue_ReturnsNull()
    {
        var result = _converter.ConvertBack(DependencyProperty.UnsetValue, typeof(object), null, CultureInfo.InvariantCulture);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("value")]
    [InlineData(123)]
    [InlineData(true)]
    public void ConvertBack_NotUnsetValue_ReturnsBindingDoNothing(object input)
    {
        var result = _converter.ConvertBack(input, typeof(object), null, CultureInfo.InvariantCulture);
        Assert.Equal(Binding.DoNothing, result);
    }

    [Fact]
    public void ProvideValue_ReturnsSingletonInstance()
    {
        var instance = _converter.ProvideValue(null);
        Assert.Same(StswNullToUnsetConverter.Instance, instance);
    }
}
