/*
using StswExpress.Commons;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StswExpress.Commons.Tests;
public class StswRandomGeneratorTests
{
    private class SimpleModel
    {
        public int IntProp { get; set; }
        public string? StringProp { get; set; }
        public bool BoolProp { get; set; }
    }

    private class ComplexModel
    {
        public SimpleModel? Nested { get; set; }
        public List<int>? IntList { get; set; }
        public DateTime DateProp { get; set; }
    }

    private enum TestEnum
    {
        First,
        Second,
        Third
    }

    private struct StructModel
    {
        public int Value { get; set; }
        public TestEnum EnumValue { get; set; }
    }

    [Fact]
    public void CreateRandomItems_ReturnsCorrectCount()
    {
        var items = StswRandomGenerator.CreateRandomItems<SimpleModel>(5).ToList();
        Assert.Equal(5, items.Count);
    }

    [Fact]
    public void CreateRandomItems_PopulatesProperties()
    {
        var item = StswRandomGenerator.CreateRandomItems<SimpleModel>(1).First();
        Assert.NotNull(item);
        Assert.True(item.IntProp != default || item.StringProp != default || item.BoolProp != default);
    }

    [Fact]
    public void CreateRandomItems_PopulatesNestedObjects()
    {
        var item = StswRandomGenerator.CreateRandomItems<ComplexModel>(1).First();
        Assert.NotNull(item);
        Assert.NotNull(item.Nested);
        Assert.NotNull(item.IntList);
        Assert.True(item.IntList!.Count > 0);
    }

    [Fact]
    public void CreateRandomItems_PopulatesEnum()
    {
        var item = StswRandomGenerator.CreateRandomItems<StructModel>(1).First();
        Assert.True(Enum.IsDefined(typeof(TestEnum), item.EnumValue));
    }

    [Fact]
    public void CreateRandomItems_PopulatesArrays()
    {
        var items = StswRandomGenerator.CreateRandomItems<int[]>(1).First();
        Assert.NotNull(items);
        Assert.True(items.Length > 0);
    }

    [Fact]
    public void CreateRandomItems_PopulatesListOfStructs()
    {
        var items = StswRandomGenerator.CreateRandomItems<List<StructModel>>(1).First();
        Assert.NotNull(items);
        Assert.True(items.Count > 0);
        Assert.True(Enum.IsDefined(typeof(TestEnum), items[0].EnumValue));
    }

    [Fact]
    public void CreateRandomItems_PopulatesNullableTypes()
    {
        var items = StswRandomGenerator.CreateRandomItems<int?>(10).ToList();
        Assert.True(items.Any(i => i == null));
        Assert.True(items.Any(i => i != null));
    }

    [Fact]
    public void CreateRandomItems_ReturnsNullForAbstractOrInterface()
    {
        var items = StswRandomGenerator.CreateRandomItems<IDisposable>(1).First();
        Assert.Null(items);
    }
}
*/