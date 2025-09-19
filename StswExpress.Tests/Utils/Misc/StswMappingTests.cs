/*
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace StswExpress.Commons.Tests.Utils.Misc;
public class StswMappingTests
{
    private class SimpleClass
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    private class NestedClass
    {
        public int Id { get; set; }
        public SimpleClass? Inner { get; set; }
    }

    [Fact]
    public void MapToClass_MapsSimpleProperties()
    {
        var dt = new DataTable();
        dt.Columns.Add("Id", typeof(int));
        dt.Columns.Add("Name", typeof(string));
        dt.Rows.Add(1, "Test");

        var result = StswExpress.Commons.StswMapping.MapToClass(dt, typeof(SimpleClass)).Cast<SimpleClass>().ToList();

        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
        Assert.Equal("Test", result[0].Name);
    }

    [Fact]
    public void MapToNestedClass_MapsNestedProperties()
    {
        var dt = new DataTable();
        dt.Columns.Add("Id", typeof(int));
        dt.Columns.Add("Inner|Id", typeof(int));
        dt.Columns.Add("Inner|Name", typeof(string));
        dt.Rows.Add(2, 3, "Nested");

        var result = StswExpress.Commons.StswMapping.MapToNestedClass(dt, typeof(NestedClass), '|').Cast<NestedClass>().ToList();

        Assert.Single(result);
        Assert.Equal(2, result[0].Id);
        Assert.NotNull(result[0].Inner);
        Assert.Equal(3, result[0].Inner!.Id);
        Assert.Equal("Nested", result[0].Inner.Name);
    }

    [Fact]
    public void MapToClass_IgnoresNonMatchingColumns()
    {
        var dt = new DataTable();
        dt.Columns.Add("NonExistent", typeof(int));
        dt.Rows.Add(42);

        var result = StswExpress.Commons.StswMapping.MapToClass(dt, typeof(SimpleClass)).Cast<SimpleClass>().ToList();

        Assert.Single(result);
        Assert.Equal(0, result[0].Id);
        Assert.Null(result[0].Name);
    }

    [Fact]
    public void MapToNestedClass_HandlesNullNestedObject()
    {
        var dt = new DataTable();
        dt.Columns.Add("Id", typeof(int));
        dt.Rows.Add(5);

        var result = StswExpress.Commons.StswMapping.MapToNestedClass(dt, typeof(NestedClass), '|').Cast<NestedClass>().ToList();

        Assert.Single(result);
        Assert.Equal(5, result[0].Id);
        Assert.Null(result[0].Inner);
    }

    [Fact]
    public void CacheProperties_ReturnsCorrectMappings()
    {
        var columnNames = new HashSet<string>(new[] { "Id", "Inner|Id", "Inner|Name" });
        var cache = typeof(NestedClass).GetMethod("GetType") != null
            ? typeof(StswExpress.Commons.StswMapping)
                .GetMethod("CacheProperties", BindingFlags.NonPublic | BindingFlags.Static)!
                .Invoke(null, new object[] { typeof(NestedClass), columnNames, '|', "", null }) as Dictionary<string, PropertyInfo>
            : null;

        Assert.NotNull(cache);
        Assert.Contains("Id", cache!.Keys);
        Assert.Contains("Inner|Id", cache.Keys);
        Assert.Contains("Inner|Name", cache.Keys);
    }

    [Fact]
    public void PrepareColumnMappings_ReturnsMappingsForExistingColumns()
    {
        var dt = new DataTable();
        dt.Columns.Add("Id", typeof(int));
        dt.Columns.Add("Inner|Id", typeof(int));
        dt.Columns.Add("Inner|Name", typeof(string));
        var normalizedNames = new[] { "Id", "Inner|Id", "Inner|Name" };
        var columnNames = new HashSet<string>(normalizedNames);
        var propCache = typeof(StswExpress.Commons.StswMapping)
            .GetMethod("CacheProperties", BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, new object[] { typeof(NestedClass), columnNames, '|', "", null }) as Dictionary<string, PropertyInfo>;

        var mappings = typeof(StswExpress.Commons.StswMapping)
            .GetMethod("PrepareColumnMappings", BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, new object[] { normalizedNames, dt, propCache, '|' }) as List<object>;

        Assert.NotNull(mappings);
        Assert.Equal(3, mappings!.Count);
    }
}
*/