using System;
using System.Collections.Generic;
using System.Linq;

namespace StswExpress.Tests;
public class StswListFromRangeExtensionTests
{
    [Theory]
    [InlineData("1-5", new[] { 1, 2, 3, 4, 5 })]
    [InlineData("0-2", new[] { 0, 1, 2 })]
    [InlineData("5-5", new[] { 5 })]
    [InlineData("3", new[] { 0, 1, 2 })]
    [InlineData("0", new int[0])]
    [InlineData("invalid", new int[0])]
    [InlineData("1-invalid", new int[0])]
    [InlineData("invalid-5", new int[0])]
    [InlineData("", new int[0])]
    public void ProvideValue_ReturnsExpectedList(string definition, int[] expected)
    {
        var ext = new StswListFromRangeExtension(definition);
        var result = ext.ProvideValue(null);

        Assert.IsType<List<int>>(result);
        Assert.Equal(expected, (List<int>)result);
    }

    [Fact]
    public void Definition_DefaultValue_IsZero()
    {
        var ext = new StswListFromRangeExtension("0");
        Assert.Equal("0", ext.Definition);
    }

    [Fact]
    public void ProvideValue_NullDefinition_ReturnsEmpty()
    {
        var ext = new StswListFromRangeExtension(null!);
        var result = ext.ProvideValue(null);
        Assert.IsType<int[]>(result);
        Assert.Empty((int[])result);
    }
}
