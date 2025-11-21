using System.Globalization;
using System.Windows.Data;

namespace StswExpress.Tests.Utils.Converters;
public class StswIfElseConverterTests
{
    private readonly StswIfElseConverter _converter = StswIfElseConverter.Instance;

    [Theory]
    [InlineData("Admin", typeof(string), "Admin~Yes~Editor~Partially~No", "Yes")]
    [InlineData("Editor", typeof(string), "Admin~Yes~Editor~Partially~No", "Partially")]
    [InlineData("User", typeof(string), "Admin~Yes~Editor~Partially~No", "No")]
    [InlineData("Owner", typeof(string), "Admin||Owner~Visible~Collapsed", "Visible")]
    [InlineData("User", typeof(string), "Admin||Owner~Visible~Collapsed", "Collapsed")]
    [InlineData("Admin", typeof(string), "Admin~Yes~No", "Yes")]
    [InlineData("User", typeof(string), "Admin~Yes~No", "No")]
    [InlineData("Admin", typeof(string), "Admin~Yes", "Yes")]
    [InlineData("User", typeof(string), "Admin~Yes", null)]
    public void Convert_TildeChains_ReturnsExpected(string input, Type targetType, object? parameter, object expected)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, typeof(string), "{x:Null}~1~~2~3", "1")]
    [InlineData("", typeof(string), "{x:Null}~1~~2~3", "2")]
    [InlineData("User", typeof(string), "{x:Null}~1~~2~3", "3")]
    public void Convert_NullAndEmptyConditions_ReturnExpected(object? input, Type targetType, object? parameter, object expected)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Convert_ResultCanBeNull()
    {
        var result = _converter.Convert("User", typeof(string), "{x:Null}~1~~2~{x:Null}", CultureInfo.InvariantCulture);
        Assert.Null(result);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void Convert_TargetTypeConversions_ReturnTypedResults(bool input, bool expected)
    {
        var result = _converter.Convert(input, typeof(bool), "True~True~False", CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Convert_UnmatchedConditionWithoutElse_ReturnsTargetTypeDefault()
    {
        var result = _converter.Convert("User", typeof(int), "Admin~1", CultureInfo.InvariantCulture);
        Assert.Equal(0, result);
    }

    [Fact]
    public void Convert_NullParameter_ReturnsTargetTypeDefault()
    {
        var result = _converter.Convert("Admin", typeof(bool), null, CultureInfo.InvariantCulture);
        Assert.Equal(false, result);
    }


    [Fact]
    public void ConvertBack_Always_ReturnsBindingDoNothing()
    {
        var result = _converter.ConvertBack("Admin", typeof(string), "Admin~Yes~No", CultureInfo.InvariantCulture);
        Assert.Equal(Binding.DoNothing, result);
    }

    [Fact]
    public void ProvideValue_ReturnsSingletonInstance()
    {
        var instance = _converter.ProvideValue(null);
        Assert.Same(StswIfElseConverter.Instance, instance);
    }
}
