using System.Globalization;
using System.Windows.Data;

namespace StswExpress.Tests;
public class StswEnumToIntConverterTests
{
    public enum TestEnum
    {
        Zero = 0,
        One = 1,
        Two = 2
    }

    private readonly StswEnumToIntConverter _converter = StswEnumToIntConverter.Instance;

    [Theory]
    [InlineData(TestEnum.Zero, typeof(int), typeof(TestEnum), 0)]
    [InlineData(TestEnum.One, typeof(int), typeof(TestEnum), 1)]
    [InlineData(TestEnum.Two, typeof(int), typeof(TestEnum), 2)]
    public void Convert_EnumToInt_ReturnsExpected(TestEnum input, Type targetType, object? parameter, int expected)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0, typeof(TestEnum), typeof(TestEnum), TestEnum.Zero)]
    [InlineData(1, typeof(TestEnum), typeof(TestEnum), TestEnum.One)]
    [InlineData(2, typeof(TestEnum), typeof(TestEnum), TestEnum.Two)]
    public void Convert_IntToEnum_ReturnsExpected(int input, Type targetType, object? parameter, TestEnum expected)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Zero", typeof(TestEnum), typeof(TestEnum), TestEnum.Zero)]
    [InlineData("one", typeof(TestEnum), typeof(TestEnum), TestEnum.One)]
    [InlineData("TWO", typeof(TestEnum), typeof(TestEnum), TestEnum.Two)]
    public void Convert_StringToEnum_ReturnsExpected(string input, Type targetType, object? parameter, TestEnum expected)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Convert_InvalidEnumType_ReturnsBindingDoNothing()
    {
        var result = _converter.Convert(1, typeof(int), typeof(int), CultureInfo.InvariantCulture);
        Assert.Equal(Binding.DoNothing, result);
    }

    [Fact]
    public void Convert_InvalidString_ReturnsBindingDoNothing()
    {
        var result = _converter.Convert("NotAValue", typeof(TestEnum), typeof(TestEnum), CultureInfo.InvariantCulture);
        Assert.Equal(Binding.DoNothing, result);
    }

    [Fact]
    public void Convert_NullValue_ReturnsBindingDoNothing()
    {
        var result = _converter.Convert(null, typeof(TestEnum), typeof(TestEnum), CultureInfo.InvariantCulture);
        Assert.Equal(Binding.DoNothing, result);
    }

    [Theory]
    [InlineData(TestEnum.One, typeof(int), typeof(TestEnum), 1)]
    [InlineData(2, typeof(TestEnum), typeof(TestEnum), TestEnum.Two)]
    [InlineData("Zero", typeof(TestEnum), typeof(TestEnum), TestEnum.Zero)]
    public void ConvertBack_SameAsConvert(object input, Type targetType, object? parameter, object expected)
    {
        var result = _converter.ConvertBack(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ProvideValue_ReturnsSingletonInstance()
    {
        var instance = _converter.ProvideValue(null);
        Assert.Same(StswEnumToIntConverter.Instance, instance);
    }
}
