using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace StswExpress.Tests.Utils.Converters;
public class StswLinqConverterTests
{
    private readonly StswLinqConverter _converter = StswLinqConverter.Instance;

    private class TestItem
    {
        public bool IsEnabled { get; set; }
        public int Age { get; set; }
        public string Status { get; set; } = "";
        public double Price { get; set; }
        public string Category { get; set; } = "";
        public int Quantity { get; set; }
        public string Name { get; set; } = "";
        public Visibility Visibility { get; set; }
    }

    private static List<TestItem> GetTestItems() => new()
    {
        new TestItem { IsEnabled = true, Age = 20, Status = "Active", Price = 100.5, Category = "Electronics", Quantity = 5, Name = "John", Visibility = Visibility.Visible },
        new TestItem { IsEnabled = false, Age = 17, Status = "Inactive", Price = 200.0, Category = "Books", Quantity = 10, Name = "Jane", Visibility = Visibility.Collapsed },
        new TestItem { IsEnabled = true, Age = 30, Status = "Active", Price = 300.0, Category = "Electronics", Quantity = 15, Name = "Doe", Visibility = Visibility.Collapsed }
    };

    [Theory]
    [InlineData("any IsEnabled == true", true)]
    [InlineData("any Age > 25", true)]
    [InlineData("any Status == Inactive", true)]
    [InlineData("any Status == Unknown", false)]
    [InlineData("any Age < 10", false)]
    public void Convert_Any_ReturnsExpected(string parameter, bool expected)
    {
        var result = _converter.Convert(GetTestItems(), typeof(bool), parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("count IsEnabled == true", 2)]
    [InlineData("count Age >= 18", 2)]
    [InlineData("count Status == Active", 2)]
    [InlineData("count Category == Electronics", 2)]
    [InlineData("count Name == Jane", 1)]
    [InlineData("count Visibility == Collapsed", 2)]
    public void Convert_Count_ReturnsExpected(string parameter, int expected)
    {
        var result = _converter.Convert(GetTestItems(), typeof(int), parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("sum Price", 600.5)]
    [InlineData("sum Quantity", 30)]
    [InlineData("sum Age", 67)]
    public void Convert_Sum_ReturnsExpected(string parameter, double expected)
    {
        var result = _converter.Convert(GetTestItems(), typeof(double), parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("average Price", 200.16666666666666)]
    [InlineData("average Quantity", 10)]
    [InlineData("average Age", 22.333333333333332)]
    public void Convert_Average_ReturnsExpected(string parameter, double expected)
    {
        var result = _converter.Convert(GetTestItems(), typeof(double), parameter, CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("where IsEnabled == true", 2)]
    [InlineData("where Status == Inactive", 1)]
    [InlineData("where Category == Electronics", 2)]
    [InlineData("where Age < 25", 2)]
    public void Convert_Where_ReturnsExpectedCount(string parameter, int expectedCount)
    {
        var result = _converter.Convert(GetTestItems(), typeof(IEnumerable), parameter, CultureInfo.InvariantCulture);
        var enumerable = Assert.IsAssignableFrom<IEnumerable>(result);
        Assert.Equal(expectedCount, enumerable.Cast<object>().Count());
    }

    [Fact]
    public void Convert_UnknownCommand_ReturnsErrorMessage()
    {
        var result = _converter.Convert(GetTestItems(), typeof(object), "foobar", CultureInfo.InvariantCulture);
        Assert.StartsWith("Unknown command", result?.ToString());
    }

    [Fact]
    public void Convert_InvalidParameter_ReturnsNull()
    {
        var result = _converter.Convert(GetTestItems(), typeof(object), null, CultureInfo.InvariantCulture);
        Assert.Null(result);
    }

    [Fact]
    public void Convert_InvalidCollection_ReturnsNull()
    {
        var result = _converter.Convert("notacollection", typeof(object), "any IsEnabled == true", CultureInfo.InvariantCulture);
        Assert.Null(result);
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
        Assert.Same(StswLinqConverter.Instance, instance);
    }

    [Fact]
    public void Convert_WithICollectionView_Works()
    {
        var items = GetTestItems();
        var collectionView = new ListCollectionView(items);
        var result = _converter.Convert(collectionView, typeof(bool), "any IsEnabled == true", CultureInfo.InvariantCulture);
        Assert.True((bool)result);
    }
}
