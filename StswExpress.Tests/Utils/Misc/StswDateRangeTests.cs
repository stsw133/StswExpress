using System;
using System.Collections.Generic;
using System.Linq;

namespace StswExpress.Commons.Tests.Utils.Misc;
public class StswDateRangeTests
{
    [Fact]
    public void Constructor_Default_InitializesToMinValues()
    {
        var range = new StswDateRange();
        Assert.Equal(default, range.Start);
        Assert.Equal(default, range.End);
    }

    [Fact]
    public void Constructor_WithStartEnd_SetsProperties()
    {
        var start = new DateTime(2020, 1, 1);
        var end = new DateTime(2020, 12, 31);
        var range = new StswDateRange(start, end);
        Assert.Equal(start, range.Start);
        Assert.Equal(end, range.End);
    }

    [Theory]
    [InlineData("2020-01-01", "2020-12-31", true)]
    [InlineData("2020-12-31", "2020-01-01", false)]
    public void IsNormalized_ReturnsExpected(string s, string e, bool expected)
    {
        var start = DateTime.Parse(s);
        var end = DateTime.Parse(e);
        var range = new StswDateRange(start, end);
        Assert.Equal(expected, range.IsNormalized);
    }

    [Theory]
    [InlineData("2020-01-01", "2020-01-01", 0)]
    [InlineData("2020-01-01", "2020-01-02", 1)]
    [InlineData("2020-01-02", "2020-01-01", 1)]
    public void Duration_ReturnsExpectedDays(string s, string e, int expectedDays)
    {
        var start = DateTime.Parse(s);
        var end = DateTime.Parse(e);
        var range = new StswDateRange(start, end);
        Assert.Equal(TimeSpan.FromDays(expectedDays), range.Duration);
    }

    [Theory]
    [InlineData("2020-01-01", "2020-01-01", true)]
    [InlineData("2020-01-01", "2020-01-02", false)]
    public void IsInstant_ReturnsExpected(string s, string e, bool expected)
    {
        var start = DateTime.Parse(s);
        var end = DateTime.Parse(e);
        var range = new StswDateRange(start, end);
        Assert.Equal(expected, range.IsInstant);
    }

    [Fact]
    public void TryIntersect_IntersectingRanges_ReturnsTrueAndIntersection()
    {
        var r1 = new StswDateRange(new DateTime(2020, 1, 1), new DateTime(2020, 1, 10));
        var r2 = new StswDateRange(new DateTime(2020, 1, 5), new DateTime(2020, 1, 15));
        var result = r1.TryIntersect(r2, true, out var intersection);
        Assert.True(result);
        Assert.NotNull(intersection);
        Assert.Equal(new DateTime(2020, 1, 5), intersection!.Start);
        Assert.Equal(new DateTime(2020, 1, 10), intersection.End);
    }

    [Fact]
    public void TryIntersect_NonIntersectingRanges_ReturnsFalse()
    {
        var r1 = new StswDateRange(new DateTime(2020, 1, 1), new DateTime(2020, 1, 10));
        var r2 = new StswDateRange(new DateTime(2020, 1, 11), new DateTime(2020, 1, 15));
        var result = r1.TryIntersect(r2, true, out var intersection);
        Assert.False(result);
        Assert.Null(intersection);
    }

    [Fact]
    public void IntersectOrNull_ReturnsIntersectionOrNull()
    {
        var r1 = new StswDateRange(new DateTime(2020, 1, 1), new DateTime(2020, 1, 10));
        var r2 = new StswDateRange(new DateTime(2020, 1, 5), new DateTime(2020, 1, 15));
        var r3 = new StswDateRange(new DateTime(2020, 1, 11), new DateTime(2020, 1, 15));
        Assert.NotNull(r1.IntersectOrNull(r2));
        Assert.Null(r1.IntersectOrNull(r3));
    }

    [Fact]
    public void Intersect_ThrowsIfNoIntersection()
    {
        var r1 = new StswDateRange(new DateTime(2020, 1, 1), new DateTime(2020, 1, 10));
        var r2 = new StswDateRange(new DateTime(2020, 1, 11), new DateTime(2020, 1, 15));
        Assert.Throws<InvalidOperationException>(() => r1.Intersect(r2));
    }

    [Fact]
    public void GetNormalized_ReturnsChronologicalOrder()
    {
        var r = new StswDateRange(new DateTime(2020, 12, 31), new DateTime(2020, 1, 1));
        var normalized = r.GetNormalized();
        Assert.True(normalized.Start <= normalized.End);
        Assert.Equal(new DateTime(2020, 1, 1), normalized.Start);
        Assert.Equal(new DateTime(2020, 12, 31), normalized.End);
    }

    [Fact]
    public void Normalize_SwapsStartEndIfNeeded()
    {
        var r = new StswDateRange(new DateTime(2020, 12, 31), new DateTime(2020, 1, 1));
        r.Normalize();
        Assert.True(r.Start <= r.End);
        Assert.Equal(new DateTime(2020, 1, 1), r.Start);
        Assert.Equal(new DateTime(2020, 12, 31), r.End);
    }

    [Fact]
    public void ToTuple_ReturnsStartEndTuple()
    {
        var r = new StswDateRange(new DateTime(2020, 1, 1), new DateTime(2020, 12, 31));
        var tuple = r.ToTuple();
        Assert.Equal(r.Start, tuple.Start);
        Assert.Equal(r.End, tuple.End);
    }

    [Fact]
    public void Deconstruct_ReturnsStartEnd()
    {
        var r = new StswDateRange(new DateTime(2020, 1, 1), new DateTime(2020, 12, 31));
        r.Deconstruct(out var s, out var e);
        Assert.Equal(r.Start, s);
        Assert.Equal(r.End, e);
    }

    [Theory]
    [InlineData("2020-01-01", "2020-12-31", "2020-06-01", true)]
    [InlineData("2020-01-01", "2020-12-31", "2019-12-31", false)]
    public void Contains_Value_ReturnsExpected(string s, string e, string v, bool expected)
    {
        var range = new StswDateRange(DateTime.Parse(s), DateTime.Parse(e));
        var value = DateTime.Parse(v);
        Assert.Equal(expected, range.Contains(value));
    }

    [Theory]
    [InlineData("2020-01-01", "2020-12-31", "2020-02-01", "2020-02-28", true)]
    [InlineData("2020-01-01", "2020-12-31", "2019-12-01", "2020-01-01", false)]
    public void Contains_InnerRange_ReturnsExpected(string s, string e, string is_, string ie, bool expected)
    {
        var range = new StswDateRange(DateTime.Parse(s), DateTime.Parse(e));
        Assert.Equal(expected, range.Contains(DateTime.Parse(is_), DateTime.Parse(ie)));
    }

    [Fact]
    public void Contains_OtherRange_ReturnsExpected()
    {
        var outer = new StswDateRange(new DateTime(2020, 1, 1), new DateTime(2020, 12, 31));
        var inner = new StswDateRange(new DateTime(2020, 2, 1), new DateTime(2020, 2, 28));
        var notInner = new StswDateRange(new DateTime(2019, 12, 1), new DateTime(2020, 1, 1));
        Assert.True(outer.Contains(inner));
        Assert.False(outer.Contains(notInner));
    }

    [Fact]
    public void IsAdjacentTo_ReturnsTrueIfAdjacent()
    {
        var r1 = new StswDateRange(new DateTime(2020, 1, 1), new DateTime(2020, 1, 10));
        var r2 = new StswDateRange(new DateTime(2020, 1, 10), new DateTime(2020, 1, 20));
        Assert.True(r1.IsAdjacentTo(r2));
    }

    [Fact]
    public void Overlaps_ReturnsTrueIfOverlapping()
    {
        var r = new StswDateRange(new DateTime(2020, 1, 1), new DateTime(2020, 1, 10));
        Assert.True(r.Overlaps(new DateTime(2020, 1, 5), new DateTime(2020, 1, 15)));
        Assert.False(r.Overlaps(new DateTime(2020, 1, 11), new DateTime(2020, 1, 15)));
    }

    [Fact]
    public void Overlaps_OtherRange_ReturnsExpected()
    {
        var r1 = new StswDateRange(new DateTime(2020, 1, 1), new DateTime(2020, 1, 10));
        var r2 = new StswDateRange(new DateTime(2020, 1, 5), new DateTime(2020, 1, 15));
        var r3 = new StswDateRange(new DateTime(2020, 1, 11), new DateTime(2020, 1, 15));
        Assert.True(r1.Overlaps(r2));
        Assert.False(r1.Overlaps(r3));
    }

    [Fact]
    public void AnyOverlap_TupleRanges_ReturnsTrueIfAnyOverlap()
    {
        var ranges = new List<(DateTime, DateTime)>
        {
            (new DateTime(2020, 1, 1), new DateTime(2020, 1, 10)),
            (new DateTime(2020, 1, 5), new DateTime(2020, 1, 15)),
            (new DateTime(2020, 1, 20), new DateTime(2020, 1, 25))
        };
        Assert.True(StswDateRange.AnyOverlap(ranges));
    }

    [Fact]
    public void AnyOverlap_TupleRanges_ReturnsFalseIfNoOverlap()
    {
        var ranges = new List<(DateTime, DateTime)>
        {
            (new DateTime(2020, 1, 1), new DateTime(2020, 1, 10)),
            (new DateTime(2020, 1, 11), new DateTime(2020, 1, 15)),
            (new DateTime(2020, 1, 20), new DateTime(2020, 1, 25))
        };
        Assert.False(StswDateRange.AnyOverlap(ranges));
    }

    [Fact]
    public void AnyOverlap_RangeObjects_ReturnsTrueIfAnyOverlap()
    {
        var ranges = new List<StswDateRange>
        {
            new StswDateRange(new DateTime(2020, 1, 1), new DateTime(2020, 1, 10)),
            new StswDateRange(new DateTime(2020, 1, 5), new DateTime(2020, 1, 15)),
            new StswDateRange(new DateTime(2020, 1, 20), new DateTime(2020, 1, 25))
        };
        Assert.True(StswDateRange.AnyOverlap(ranges));
    }

    [Fact]
    public void AnyOverlap_RangeObjects_ReturnsFalseIfNoOverlap()
    {
        var ranges = new List<StswDateRange>
        {
            new StswDateRange(new DateTime(2020, 1, 1), new DateTime(2020, 1, 10)),
            new StswDateRange(new DateTime(2020, 1, 11), new DateTime(2020, 1, 15)),
            new StswDateRange(new DateTime(2020, 1, 20), new DateTime(2020, 1, 25))
        };
        Assert.False(StswDateRange.AnyOverlap(ranges));
    }

    [Fact]
    public void GetUniqueYears_ReturnsExpectedYears()
    {
        var r = new StswDateRange(new DateTime(2018, 6, 1), new DateTime(2020, 2, 1));
        var years = r.GetUniqueYears().Select(d => d.Year).ToList();
        Assert.Contains(2018, years);
        Assert.Contains(2019, years);
        Assert.Contains(2020, years);
        Assert.Equal(3, years.Count);
    }

    [Fact]
    public void GetUniqueMonths_ReturnsExpectedMonths()
    {
        var r = new StswDateRange(new DateTime(2020, 1, 15), new DateTime(2020, 3, 10));
        var months = r.GetUniqueMonths().Select(d => d.Month).ToList();
        Assert.Contains(1, months);
        Assert.Contains(2, months);
        Assert.Contains(3, months);
        Assert.Equal(3, months.Count);
    }

    [Fact]
    public void GetUniqueDays_ReturnsExpectedDays()
    {
        var r = new StswDateRange(new DateTime(2020, 1, 1), new DateTime(2020, 1, 3));
        var days = r.GetUniqueDays().Select(d => d.Day).ToList();
        Assert.Contains(1, days);
        Assert.Contains(2, days);
        Assert.Contains(3, days);
        Assert.Equal(3, days.Count);
    }

    [Fact]
    public void ToString_ReturnsIso8601Format()
    {
        var r = new StswDateRange(new DateTime(2020, 1, 1, 12, 0, 0), new DateTime(2020, 1, 2, 13, 0, 0));
        var str = r.ToString();
        Assert.StartsWith("[2020-01-01T12:00:00.0000000", str);
        Assert.Contains("2020-01-02T13:00:00.0000000", str);
    }

    [Fact]
    public void Equals_ReturnsTrueForEqualRanges()
    {
        var r1 = new StswDateRange(new DateTime(2020, 1, 1), new DateTime(2020, 1, 10));
        var r2 = new StswDateRange(new DateTime(2020, 1, 1), new DateTime(2020, 1, 10));
        Assert.True(r1.Equals(r2));
        Assert.True(r1 == r2);
        Assert.False(r1 != r2);
    }

    [Fact]
    public void Equals_ReturnsFalseForDifferentRanges()
    {
        var r1 = new StswDateRange(new DateTime(2020, 1, 1), new DateTime(2020, 1, 10));
        var r2 = new StswDateRange(new DateTime(2020, 1, 2), new DateTime(2020, 1, 10));
        Assert.False(r1.Equals(r2));
        Assert.False(r1 == r2);
        Assert.True(r1 != r2);
    }

    [Fact]
    public void GetHashCode_EqualRanges_SameHash()
    {
        var r1 = new StswDateRange(new DateTime(2020, 1, 1), new DateTime(2020, 1, 10));
        var r2 = new StswDateRange(new DateTime(2020, 1, 1), new DateTime(2020, 1, 10));
        Assert.Equal(r1.GetHashCode(), r2.GetHashCode());
    }

    [Theory]
    [InlineData(2019, "2019-01-01T00:00:00", "2019-12-31T23:59:59.9999999")]
    [InlineData(2020, "2020-01-01T00:00:00", "2020-12-31T23:59:59.9999999")]
    public void FromYear_ReturnsExpectedRange(int year, string expectedStart, string expectedEnd)
    {
        var range = StswDateRange.FromYear(year);
        Assert.Equal(DateTime.Parse(expectedStart), range.Start);
        Assert.Equal(DateTime.Parse(expectedEnd), range.End);
    }

    [Theory]
    [InlineData(2020, 1, "2020-01-01T00:00:00", "2020-03-31T23:59:59.9999999")]
    [InlineData(2020, 2, "2020-04-01T00:00:00", "2020-06-30T23:59:59.9999999")]
    [InlineData(2020, 3, "2020-07-01T00:00:00", "2020-09-30T23:59:59.9999999")]
    [InlineData(2020, 4, "2020-10-01T00:00:00", "2020-12-31T23:59:59.9999999")]
    public void FromQuarter_ReturnsExpectedRange(int year, int quarter, string expectedStart, string expectedEnd)
    {
        var range = StswDateRange.FromQuarter(year, quarter);
        Assert.Equal(DateTime.Parse(expectedStart), range.Start);
        Assert.Equal(DateTime.Parse(expectedEnd), range.End);
    }

    [Theory]
    [InlineData(2020, 1, "2020-01-01T00:00:00", "2020-01-31T23:59:59.9999999")]
    [InlineData(2020, 2, "2020-02-01T00:00:00", "2020-02-29T23:59:59.9999999")]
    [InlineData(2021, 2, "2021-02-01T00:00:00", "2021-02-28T23:59:59.9999999")]
    [InlineData(2020, 12, "2020-12-01T00:00:00", "2020-12-31T23:59:59.9999999")]
    public void FromMonth_ReturnsExpectedRange(int year, int month, string expectedStart, string expectedEnd)
    {
        var range = StswDateRange.FromMonth(year, month);
        Assert.Equal(DateTime.Parse(expectedStart), range.Start);
        Assert.Equal(DateTime.Parse(expectedEnd), range.End);
    }

    [Theory]
    [InlineData(2020, 0)]
    [InlineData(2020, 5)]
    public void FromQuarter_ThrowsForInvalidQuarter(int year, int quarter)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => StswDateRange.FromQuarter(year, quarter));
    }

    [Theory]
    [InlineData(2020, 0)]
    [InlineData(2020, 13)]
    public void FromMonth_ThrowsForInvalidMonth(int year, int month)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => StswDateRange.FromMonth(year, month));
    }
}
