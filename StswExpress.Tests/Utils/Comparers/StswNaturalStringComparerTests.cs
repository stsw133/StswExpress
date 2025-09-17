namespace StswExpress.Commons.Tests;
public class StswNaturalStringComparerTests
{
    private readonly StswNaturalStringComparer _comparer = new();

    [Theory]
    [InlineData("Item1", "Item2", -1)]
    [InlineData("Item10", "Item2", 1)]
    [InlineData("Item2", "Item2", 0)]
    [InlineData("Item2", "Item10", -1)]
    [InlineData("Item20", "Item3", 1)]
    [InlineData("Item3", "Item20", -1)]
    [InlineData("Item10", "Item10", 0)]
    [InlineData("Item", "Item1", -1)]
    [InlineData("Item1", "Item", 1)]
    [InlineData("Item", "Item", 0)]
    [InlineData("A2B", "A10B", -1)]
    [InlineData("A10B", "A2B", 1)]
    [InlineData("A2B", "A2B", 0)]
    [InlineData(null, "Item1", -1)]
    [InlineData("Item1", null, 1)]
    [InlineData(null, null, 0)]
    public void Compare_ReturnsExpected(string? x, string? y, int expectedSign)
    {
        var result = _comparer.Compare(x, y);
        Assert.Equal(Math.Sign(expectedSign), Math.Sign(result));
    }

    [Fact]
    public void Sort_List_SortsNaturally()
    {
        var items = new List<string> { "Item1", "Item20", "Item3", "Item10", "Item2" };
        items.Sort(_comparer);
        var expected = new List<string> { "Item1", "Item2", "Item3", "Item10", "Item20" };
        Assert.Equal(expected, items);
    }

    [Fact]
    public void Compare_HandlesMixedAlphaNumeric()
    {
        var items = new List<string> { "A2B", "A10B", "A1B", "A2B1", "A2B10" };
        items.Sort(_comparer);
        var expected = new List<string> { "A1B", "A2B", "A2B1", "A2B10", "A10B" };
        Assert.Equal(expected, items);
    }

    [Fact]
    public void Compare_HandlesLeadingZeros()
    {
        var items = new List<string> { "Item01", "Item1", "Item002", "Item2" };
        items.Sort(_comparer);
        var expected = new List<string> { "Item1", "Item01", "Item2", "Item002" };
        Assert.Equal(expected, items);
    }
}
