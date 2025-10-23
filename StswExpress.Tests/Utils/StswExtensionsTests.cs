using System.ComponentModel;
using System.Data;

namespace StswExpress.Commons.Tests.Utils;
public class StswExtensionsTests
{
    #region Convert extensions
    [Fact]
    public void ConvertTo_NullableInt_ReturnsNullForNull()
    {
        object? value = null;
        var result = value.ConvertTo(typeof(int?));
        Assert.Null(result);
    }

    [Fact]
    public void ConvertTo_Enum_ReturnsEnumValue()
    {
        object value = "Friday";
        var result = value.ConvertTo(typeof(DayOfWeek));
        Assert.Equal(DayOfWeek.Friday, result);
    }

    [Fact]
    public void ConvertTo_Primitive_ReturnsConvertedValue()
    {
        object value = "123";
        var result = value.ConvertTo<int>();
        Assert.Equal(123, result);
    }

    [Fact]
    public void InferSqlDbType_IntType_ReturnsSqlDbTypeInt()
    {
        var type = typeof(int);
        var result = type.InferSqlDbType();
        Assert.Equal(SqlDbType.Int, result);
    }

    [Fact]
    public void ToDataTable_Collection_ReturnsDataTable()
    {
        var items = new[] { new { Id = 1, Name = "A" }, new { Id = 2, Name = "B" } };
        var dt = items.ToDataTable();
        Assert.Equal(2, dt.Rows.Count);
        Assert.Equal("A", dt.Rows[0]["Name"]);
    }
    #endregion

    #region DateTime extensions
    [Fact]
    public void GetQuarter_ReturnsCorrectQuarter()
    {
        var dt = new DateTime(2024, 5, 1);
        Assert.Equal(2, dt.GetQuarter());
    }

    [Fact]
    public void IsSameYearAndMonth_ReturnsTrueForSameMonth()
    {
        var dt1 = new DateTime(2024, 6, 1);
        var dt2 = new DateTime(2024, 6, 30);
        Assert.True(dt1.IsSameYearAndMonth(dt2));
    }

    [Fact]
    public void Next_ReturnsNextDayOfWeek()
    {
        var dt = new DateTime(2024, 6, 1); // Saturday
        var nextMonday = dt.Next(DayOfWeek.Monday);
        Assert.Equal(DayOfWeek.Monday, nextMonday.DayOfWeek);
        Assert.True(nextMonday > dt);
    }

    [Fact]
    public void ToEndOfDay_ReturnsEndOfDay()
    {
        var dt = new DateTime(2024, 6, 1, 10, 0, 0);
        var end = dt.ToEndOfDay();
        Assert.Equal(new DateTime(2024, 6, 1, 23, 59, 59, 999).AddTicks(9999), end);
    }

    [Fact]
    public void ToFirstDayOfMonth_ReturnsFirstDay()
    {
        var dt = new DateTime(2024, 6, 15);
        Assert.Equal(new DateTime(2024, 6, 1), dt.ToFirstDayOfMonth());
    }

    [Fact]
    public void ToLastDayOfMonth_ReturnsLastDay()
    {
        var dt = new DateTime(2024, 2, 10);
        Assert.Equal(new DateTime(2024, 2, 29), dt.ToLastDayOfMonth());
    }

    [Fact]
    public void ToUnixTimeSeconds_ReturnsUnixTimestamp()
    {
        var dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        Assert.Equal(0, dt.ToUnixTimeSeconds());
    }
    #endregion

    #region Dictionary extensions
    [Fact]
    public void ChangeKey_ChangesKeySuccessfully()
    {
        var dict = new Dictionary<string, int> { { "old", 1 } };
        var result = dict.ChangeKey("old", "new");
        Assert.True(result);
        Assert.False(dict.ContainsKey("old"));
        Assert.True(dict.ContainsKey("new"));
    }

    //[Fact]
    //public void GetValueOrDefault_ReturnsValueOrDefault()
    //{
    //    var dict = new Dictionary<string, int> { { "a", 1 } };
    //    Assert.Equal(1, dict.GetValueOrDefault("a"));
    //    Assert.Equal(0, dict.GetValueOrDefault("b"));
    //}

    [Fact]
    public void ToDictionarySafely_IgnoresDuplicates()
    {
        var items = new[] { "a", "a", "b" };
        var dict = items.ToDictionarySafely(x => x, x => x.Length);
        Assert.Equal(2, dict.Count);
    }
    #endregion

    #region Enum extensions
    private enum TestEnum
    {
        [Description("Test Value")]
        Value
    }

    [Fact]
    public void GetAttributeOfType_ReturnsAttribute()
    {
        var attr = TestEnum.Value.GetAttributeOfType<DescriptionAttribute>();
        Assert.NotNull(attr);
        Assert.Equal("Test Value", attr.Description);
    }

    [Fact]
    public void GetDescription_ReturnsDescription()
    {
        Assert.Equal("Test Value", TestEnum.Value.GetDescription());
    }

    [Fact]
    public void GetNextValue_WrapsAround()
    {
        var next = DayOfWeek.Sunday.GetNextValue();
        Assert.Equal(DayOfWeek.Monday, next);
    }
    #endregion

    #region List extensions
    [Fact]
    public void AddIfNotContains_AddsIfNotPresent()
    {
        var list = new List<int> { 1 };
        list.AddIfNotContains(2);
        Assert.Contains(2, list);
        list.AddIfNotContains(1);
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public void AddRange_AddsMultipleItems()
    {
        var list = new List<int> { 1 };
        list.AddRange([2, 3]);
        Assert.Equal(new[] { 1, 2, 3 }, list);
    }

    [Fact]
    public void Batch_SplitsIntoBatches()
    {
        var items = new[] { 1, 2, 3, 4, 5 };
        var batches = items.Batch(2);
        Assert.Collection(batches,
            b => Assert.Equal([1, 2], b),
            b => Assert.Equal([3, 4], b),
            b => Assert.Equal([5], b));
    }

    [Fact]
    public void ForEach_PerformsAction()
    {
        var items = new[] { 1, 2, 3 };
        var sum = 0;
        items.ForEach(x => sum += x);
        Assert.Equal(6, sum);
    }

    [Fact]
    public void RemoveRange_RemovesItems()
    {
        var list = new List<int> { 1, 2, 3, 4 };
        list.RemoveRange(new[] { 2, 4 });
        Assert.Equal(new[] { 1, 3 }, list);
    }

    [Fact]
    public void Replace_ReplacesValues()
    {
        var list = new List<int> { 1, 2, 2, 3 };
        list.Replace(2, 9);
        Assert.Equal(new[] { 1, 9, 9, 3 }, list);
    }

    [Fact]
    public void Shuffle_RandomizesOrder()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };
        list.Shuffle();
        Assert.Equal(5, list.Count);
        Assert.True(list.Contains(1));
    }
    #endregion

    #region Logical extensions
    [Fact]
    public void Between_ReturnsTrueIfInRange()
    {
        Assert.True(5.Between(1, 10));
        int? value = 5;
        Assert.True(value.Between(1, 10));
    }

    [Fact]
    public void In_ReturnsTrueIfContained()
    {
        Assert.True(2.In(new[] { 1, 2, 3 }));
        Assert.True(2.In(1, 2, 3));
    }

    [Fact]
    public void IsListType_ReturnsTrueForList()
    {
        var type = typeof(List<int>);
        Assert.True(type.IsListType(out var innerType));
        Assert.Equal(typeof(int), innerType);
    }

    [Fact]
    public void IsNullOrDefault_ReturnsTrueForNullOrDefault()
    {
        int? value = null;
        Assert.True(value.IsNullOrDefault());
        value = 0;
        Assert.True(value.IsNullOrDefault());
        value = 1;
        Assert.False(value.IsNullOrDefault());
    }

    [Fact]
    public void IsNullOrEmpty_ReturnsTrueForNullOrEmpty()
    {
        IEnumerable<int>? list = null;
        Assert.True(list.IsNullOrEmpty());
        list = new List<int>();
        Assert.True(list.IsNullOrEmpty());
        list = new List<int> { 1 };
        Assert.False(list.IsNullOrEmpty());
    }

    [Fact]
    public void IsNumericType_ReturnsTrueForNumericTypes()
    {
        Assert.True(typeof(int).IsNumericType());
        Assert.False(typeof(string).IsNumericType());
    }

    [Fact]
    public void IsSimilarTo_ReturnsTrueForSimilarObjects()
    {
        var a = new { Id = 1, Name = "A" };
        var b = new { Id = 1, Name = "A" };
        Assert.True(a.IsSimilarTo(b));
    }

    //[Fact]
    //public void IsSimpleType_ReturnsTrueForSimpleTypes()
    //{
    //    Assert.True(StswExtensions.IsSimpleType(typeof(int)));
    //    Assert.True(StswExtensions.IsSimpleType(typeof(string)));
    //    Assert.False(StswExtensions.IsSimpleType(typeof(object)));
    //}
    #endregion

    #region Object extensions
    private class SourceObj { public int Id { get; set; } public string? Name { get; set; } }
    private class TargetObj { public int Id { get; set; } public string? Name { get; set; } }

    [Fact]
    public void CopyFrom_CopiesProperties()
    {
        var src = new SourceObj { Id = 5, Name = "Test" };
        var tgt = new TargetObj();
        tgt.CopyFrom(src);
        Assert.Equal(5, tgt.Id);
        Assert.Equal("Test", tgt.Name);
    }

    [Fact]
    public void DeepCopyWithJson_CreatesCopy()
    {
        var obj = new SourceObj { Id = 1, Name = "A" };
        var copy = obj.DeepCopyWithJson();
        Assert.NotSame(obj, copy);
        Assert.Equal(obj.Id, copy?.Id);
        Assert.Equal(obj.Name, copy?.Name);
    }

    [Fact]
    public void DeepEquals_ReturnsTrueForEqualObjects()
    {
        var a = new SourceObj { Id = 1, Name = "A" };
        var b = new SourceObj { Id = 1, Name = "A" };
        Assert.True(a.DeepEquals(b));
    }
    #endregion

    #region Text extensions
    [Fact]
    public void Capitalize_CapitalizesString()
    {
        Assert.Equal("Test", "test".Capitalize());
        Assert.Equal("T", "t".Capitalize());
    }

    [Fact]
    public void PadLeft_PadsString()
    {
        Assert.Equal("**abc", "abc".PadLeft(5, "*"));
    }

    [Fact]
    public void PadRight_PadsString()
    {
        Assert.Equal("abc**", "abc".PadRight(5, "*"));
    }

    [Fact]
    public void TrimEnd_RemovesSuffix()
    {
        Assert.Equal("abc", "abcxyz".TrimEnd("xyz"));
        Assert.Equal("abcxyz", "abcxyz".TrimEnd("zzz"));
    }

    [Fact]
    public void TrimStart_RemovesPrefix()
    {
        Assert.Equal("xyz", "abcxyz".TrimStart("abc"));
        Assert.Equal("abcxyz", "abcxyz".TrimStart("zzz"));
    }
    #endregion

    #region Universal extensions
    private class PropObj { public int Id { get; set; } = 42; }

    [Fact]
    public void GetPropertyValue_ReturnsValue()
    {
        var obj = new PropObj();
        Assert.Equal(42, obj.GetPropertyValue("Id"));
        Assert.Null(obj.GetPropertyValue("Unknown"));
    }
    #endregion
}
