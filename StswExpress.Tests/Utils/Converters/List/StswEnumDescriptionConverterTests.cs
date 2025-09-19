using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace StswExpress.Tests.Utils.Converters;
public class StswEnumDescriptionConverterTests
{
    private readonly StswEnumDescriptionConverter _converter = StswEnumDescriptionConverter.Instance;

    public enum TestEnum
    {
        [Description("First Value")]
        First,
        Second,
        [Description("Third Value")]
        Third
    }

    public enum NullableTestEnum
    {
        [Description("Nullable First")]
        First,
        Second
    }

    [Theory]
    [InlineData(TestEnum.First, "First Value")]
    [InlineData(TestEnum.Second, "Second")]
    [InlineData(TestEnum.Third, "Third Value")]
    public void Convert_Enum_ReturnsDescriptionOrName(TestEnum input, string expected)
    {
        var result = _converter.Convert(input, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Convert_NullableEnum_ReturnsDescriptionOrName()
    {
        TestEnum? value = TestEnum.First;
        var result = _converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal("First Value", result);

        value = null;
        var resultNull = _converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal(Binding.DoNothing, resultNull);
    }

    [Fact]
    public void Convert_InvalidType_ReturnsBindingDoNothing()
    {
        var result = _converter.Convert("notanenum", typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal(Binding.DoNothing, result);
    }

    [Fact]
    public void ConvertBack_Always_ReturnsBindingDoNothing()
    {
        var result = _converter.ConvertBack("anything", typeof(TestEnum), null, CultureInfo.InvariantCulture);
        Assert.Equal(Binding.DoNothing, result);
    }

    [Fact]
    public void ProvideValue_ReturnsSingletonInstance()
    {
        var instance = _converter.ProvideValue(null);
        Assert.Same(StswEnumDescriptionConverter.Instance, instance);
    }
}
