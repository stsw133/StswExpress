using System.Globalization;
using System.Windows.Data;

namespace StswExpress.Tests;
public class StswIfElseConverterTests
{
    private readonly StswIfElseConverter _converter = StswIfElseConverter.Instance;

    [Theory]
    [InlineData("Admin", typeof(string), "Admin~Yes~Editor~Partially~No", "Yes")]
    [InlineData("Editor", typeof(string), "Admin~Yes~Editor~Partially~No", "Partially")]
    [InlineData("User", typeof(string), "Admin~Yes~Editor~Partially~No", "No")]
    [InlineData("Admin", typeof(string), "Admin ? Yes : Editor ? Partially : No", "Yes")]
    [InlineData("Editor", typeof(string), "Admin ? Yes : Editor ? Partially : No", "Partially")]
    [InlineData("User", typeof(string), "Admin ? Yes : Editor ? Partially : No", "No")]
    [InlineData("Owner", typeof(string), "Admin||Owner ? Visible : Collapsed", "Visible")]
    [InlineData("Admin", typeof(string), "Admin||Owner ? Visible : Collapsed", "Visible")]
    [InlineData("User", typeof(string), "Admin||Owner ? Visible : Collapsed", "Collapsed")]
    [InlineData("Admin", typeof(string), "Admin~Yes~No", "Yes")]
    [InlineData("User", typeof(string), "Admin~Yes~No", "No")]
    [InlineData("Admin", typeof(string), "Admin~Yes", "Yes")]
    [InlineData("User", typeof(string), "Admin~Yes", null)]
    public void Convert_TildeAndTernaryChains_ReturnsExpected(string input, Type targetType, object? parameter, object expected)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Admin", typeof(string), null, null)]
    [InlineData(null, typeof(string), "Admin~Yes~No", "No")]
    public void Convert_NullParameterOrValue_ReturnsExpected(object? input, Type targetType, object? parameter, object expected)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
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
