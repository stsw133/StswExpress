using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace StswExpress.Tests;
public class StswContainsConverterTests
{
    private readonly StswContainsConverter _converter = StswContainsConverter.Instance;

    [Theory]
    [InlineData("Item1", typeof(bool), "Item1", true)]
    [InlineData("Item2", typeof(bool), "Item1", false)]
    [InlineData("Item1,Item2", typeof(bool), "Item1", true)]
    [InlineData("Item2,Item3", typeof(bool), "Item1", false)]
    [InlineData("Item1,Item2", typeof(bool), "Item1,Item2", true)]
    [InlineData("Item1,Item2", typeof(bool), "Item1,Item3", true)]
    [InlineData("Item2,Item3", typeof(bool), "Item1,Item3", true)]
    [InlineData("Item2,Item3", typeof(bool), "Item1,Item4", false)]
    [InlineData("Item1,Item5", typeof(bool), "Item1,!Item5", false)]
    [InlineData("Item1,Item2", typeof(bool), "Item1,!Item5", true)]
    [InlineData("Item5", typeof(bool), "!Item5", false)]
    [InlineData("Item1", typeof(bool), "!Item5", true)]
    public void Convert_StringInput_ReturnsExpected(string input, Type targetType, object? parameter, bool expected)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(new[] { "Item1", "Item2" }, typeof(bool), "Item1", true)]
    [InlineData(new[] { "Item2", "Item3" }, typeof(bool), "Item1", false)]
    [InlineData(new[] { "Item1", "Item2" }, typeof(bool), "Item1,Item2", true)]
    [InlineData(new[] { "Item1", "Item5" }, typeof(bool), "Item1,!Item5", false)]
    [InlineData(new[] { "Item1", "Item2" }, typeof(bool), "Item1,!Item5", true)]
    public void Convert_EnumerableInput_ReturnsExpected(IEnumerable<string> input, Type targetType, object? parameter, bool expected)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Item1", typeof(Visibility), "Item1", Visibility.Visible)]
    [InlineData("Item2", typeof(Visibility), "Item1", Visibility.Collapsed)]
    [InlineData("Item1,Item2", typeof(Visibility), "Item1", Visibility.Visible)]
    [InlineData("Item1,Item5", typeof(Visibility), "Item1,!Item5", Visibility.Collapsed)]
    [InlineData("Item1,Item2", typeof(Visibility), "Item1,!Item5", Visibility.Visible)]
    public void Convert_Visibility_ReturnsExpected(string input, Type targetType, object? parameter, Visibility expected)
    {
        var result = _converter.Convert(input, targetType, parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Convert_NullInput_ReturnsFalseOrCollapsed()
    {
        var resultBool = _converter.Convert(null, typeof(bool), "Item1", CultureInfo.InvariantCulture);
        var resultVis = _converter.Convert(null, typeof(Visibility), "Item1", CultureInfo.InvariantCulture);
        Assert.False((bool)resultBool);
        Assert.Equal(Visibility.Collapsed, resultVis);
    }

    [Fact]
    public void ConvertBack_Always_ReturnsBindingDoNothing()
    {
        var result = _converter.ConvertBack("Item1", typeof(bool), "Item1", CultureInfo.InvariantCulture);
        Assert.Equal(Binding.DoNothing, result);
    }

    [Fact]
    public void ProvideValue_ReturnsSingletonInstance()
    {
        var instance = _converter.ProvideValue(null);
        Assert.Same(StswContainsConverter.Instance, instance);
    }
}
