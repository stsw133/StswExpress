using System.Globalization;

namespace StswExpress.Commons.Tests.Utils.Misc;
public class StswDateRangeTests
{
    [Fact]
    public void Constructor_SetsStartAndEnd()
    {
        var start = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 12, 31);
        var range = new StswDateRange(start, end);

        Assert.Equal(start, range.Start);
        Assert.Equal(end, range.End);
    }

    [Fact]
    public void IsNormalized_ReturnsTrueIfStartLessThanOrEqualEnd()
    {
        var range = new StswDateRange(new DateTime(2024, 1, 1), new DateTime(2024, 12, 31));
        Assert.True(range.IsNormalized);

        range = new StswDateRange(new DateTime(2024, 12, 31), new DateTime(2024, 1, 1));
        Assert.True(range.GetNormalized().IsNormalized);
    }

    [Fact]
    public void Duration_ReturnsCorrectTimeSpan()
    {
        var range = new StswDateRange(new DateTime(2024, 1, 1), new DateTime(2024, 1, 2));
        Assert.Equal(TimeSpan.FromDays(1), range.Duration);

        range = new StswDateRange(new DateTime(2024, 1, 2), new DateTime(2024, 1, 1));
        Assert.Equal(TimeSpan.FromDays(1), range.Duration);
    }

    [Fact]
    public void IsInstant_ReturnsTrueIfStartEqualsEnd()
    {
        var dt = new DateTime(2024, 1, 1);
        var range = new StswDateRange(dt, dt);
        Assert.True(range.IsInstant);

        range = new StswDateRange(dt, dt.AddTicks(1));
        Assert.False(range.IsInstant);
    }

    [Fact]
    public void Add_AddsOffsetToBothBounds()
    {
        var range = new StswDateRange(new DateTime(2024, 1, 1), new DateTime(2024, 1, 2));
        var offset = TimeSpan.FromDays(5);
        var result = range.Add(offset);

        Assert.Equal(new DateTime(2024, 1, 6), result.Start);
        Assert.Equal(new DateTime(2024, 1, 7), result.End);
    }

    [Fact]
    public void AddYears_AddsYearsToBothBounds()
    {
        var range = new StswDateRange(new DateTime(2020, 2, 29), new DateTime(2020, 3, 1));
        var result = range.AddYears(1);

        Assert.Equal(new DateTime(2021, 2, 28), result.Start);
        Assert.Equal(new DateTime(2021, 3, 1), result.End);
    }

    [Fact]
    public void AddMonths_AddsMonthsToBothBounds()
    {
        var range = new StswDateRange(new DateTime(2024, 1, 31), new DateTime(2024, 2, 29));
        var result = range.AddMonths(1);

        Assert.Equal(new DateTime(2024, 2, 29), result.Start);
        Assert.Equal(new DateTime(2024, 3, 29), result.End);
    }

    [Fact]
    public void AddOffsets_AddsDifferentOffsets()
    {
        var range = new StswDateRange(new DateTime(2024, 1, 1), new DateTime(2024, 1, 2));
        var result = range.AddOffsets(TimeSpan.FromDays(1), TimeSpan.FromDays(2));

        Assert.Equal(new DateTime(2024, 1, 2), result.Start);
        Assert.Equal(new DateTime(2024, 1, 4), result.End);
    }

    [Fact]
    public void Contains_ValueInclusiveAndExclusive()
    {
        var range = new StswDateRange(new DateTime(2024, 1, 1), new DateTime(2024, 1, 10));
        Assert.True(range.Contains(new DateTime(2024, 1, 1)));
        Assert.True(range.Contains(new DateTime(2024, 1, 10)));
        Assert.False(range.Contains(new DateTime(2023, 12, 31)));

        Assert.False(range.Contains(new DateTime(2024, 1, 1), inclusive: false));
        Assert.False(range.Contains(new DateTime(2024, 1, 10), inclusive: false));
        Assert.True(range.Contains(new DateTime(2024, 1, 2), inclusive: false));
    }

    [Fact]
    public void Contains_RangeInclusiveAndExclusive()
    {
        var range = new StswDateRange(new DateTime(2024, 1, 1), new DateTime(2024, 1, 10));
        Assert.True(range.Contains(new DateTime(2024, 1, 2), new DateTime(2024, 1, 9)));
        Assert.False(range.Contains(new DateTime(2023, 12, 31), new DateTime(2024, 1, 2)));

        Assert.False(range.Contains(new DateTime(2024, 1, 1), new DateTime(2024, 1, 10), inclusive: false));
        Assert.True(range.Contains(new DateTime(2024, 1, 2), new DateTime(2024, 1, 9), inclusive: false));
    }

    [Fact]
    public void Overlaps_RangeInclusiveAndExclusive()
    {
        var range = new StswDateRange(new DateTime(2024, 1, 1), new DateTime(2024, 1, 10));
        Assert.True(range.Overlaps(new DateTime(2024, 1, 5), new DateTime(2024, 1, 15)));
        Assert.False(range.Overlaps(new DateTime(2024, 1, 11), new DateTime(2024, 1, 20)));

        Assert.False(range.Overlaps(new DateTime(2024, 1, 10), new DateTime(2024, 1, 15), inclusive: false));
        Assert.True(range.Overlaps(new DateTime(2024, 1, 9), new DateTime(2024, 1, 10), inclusive: false));
    }

    [Fact]
    public void AnyOverlap_ReturnsTrueIfAnyRangesOverlap()
    {
        var ranges = new[]
        {
            new StswDateRange(new DateTime(2024, 1, 1), new DateTime(2024, 1, 5)),
            new StswDateRange(new DateTime(2024, 1, 4), new DateTime(2024, 1, 10)),
            new StswDateRange(new DateTime(2024, 1, 11), new DateTime(2024, 1, 15))
        };
        Assert.True(StswDateRange.AnyOverlap(ranges));
    }

    [Fact]
    public void AnyOverlap_ReturnsFalseIfNoRangesOverlap()
    {
        var ranges = new[]
        {
            new StswDateRange(new DateTime(2024, 1, 1), new DateTime(2024, 1, 5)),
            new StswDateRange(new DateTime(2024, 1, 6), new DateTime(2024, 1, 10)),
            new StswDateRange(new DateTime(2024, 1, 11), new DateTime(2024, 1, 15))
        };
        Assert.False(StswDateRange.AnyOverlap(ranges));
    }

    [Fact]
    public void FromYear_ReturnsFullYearRange()
    {
        var range = StswDateRange.FromYear(2024);
        Assert.Equal(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Unspecified), range.Start);
        Assert.Equal(new DateTime(2024, 12, 31, 23, 59, 59, 999, DateTimeKind.Unspecified).AddTicks(9999), range.End);
    }

    [Fact]
    public void FromQuarter_ReturnsQuarterRange()
    {
        var range = StswDateRange.FromQuarter(2024, 2);
        Assert.Equal(new DateTime(2024, 4, 1, 0, 0, 0, DateTimeKind.Unspecified), range.Start);
        Assert.Equal(new DateTime(2024, 6, 30, 23, 59, 59, 999, DateTimeKind.Unspecified).AddTicks(9999), range.End);
    }

    [Fact]
    public void FromMonth_ReturnsMonthRange()
    {
        var range = StswDateRange.FromMonth(2024, 2);
        Assert.Equal(new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Unspecified), range.Start);
        Assert.Equal(new DateTime(2024, 2, 29, 23, 59, 59, 999, DateTimeKind.Unspecified).AddTicks(9999), range.End);
    }

    [Fact]
    public void Clamp_ClampsRangeWithinBounds()
    {
        var range = new StswDateRange(new DateTime(2024, 1, 1), new DateTime(2024, 1, 10));
        var clamped = range.Clamp(new DateTime(2024, 1, 3), new DateTime(2024, 1, 8));
        Assert.Equal(new DateTime(2024, 1, 3), clamped.Start);
        Assert.Equal(new DateTime(2024, 1, 8), clamped.End);
    }

    [Fact]
    public void GetNormalized_ReturnsChronologicalOrder()
    {
        var range = new StswDateRange(new DateTime(2024, 1, 10), new DateTime(2024, 1, 1));
        var normalized = range.GetNormalized();
        Assert.True(normalized.Start <= normalized.End);
    }

    [Fact]
    public void Normalize_SwapsStartAndEndIfNeeded()
    {
        var range = new StswDateRange(new DateTime(2024, 1, 10), new DateTime(2024, 1, 1));
        range.Normalize();
        Assert.True(range.Start <= range.End);
    }

    [Fact]
    public void TryIntersect_ReturnsIntersectionIfExists()
    {
        var a = new StswDateRange(new DateTime(2024, 1, 1), new DateTime(2024, 1, 10));
        var b = new StswDateRange(new DateTime(2024, 1, 5), new DateTime(2024, 1, 15));
        Assert.True(a.TryIntersect(b, true, out var intersection));
        Assert.Equal(new DateTime(2024, 1, 5), intersection!.Start);
        Assert.Equal(new DateTime(2024, 1, 10), intersection.End);
    }

    [Fact]
    public void IntersectOrNull_ReturnsNullIfNoIntersection()
    {
        var a = new StswDateRange(new DateTime(2024, 1, 1), new DateTime(2024, 1, 10));
        var b = new StswDateRange(new DateTime(2024, 1, 11), new DateTime(2024, 1, 15));
        Assert.Null(a.IntersectOrNull(b));
    }

    [Fact]
    public void Intersect_ThrowsIfNoIntersection()
    {
        var a = new StswDateRange(new DateTime(2024, 1, 1), new DateTime(2024, 1, 10));
        var b = new StswDateRange(new DateTime(2024, 1, 11), new DateTime(2024, 1, 15));
        Assert.Throws<InvalidOperationException>(() => a.Intersect(b));
    }

    [Fact]
    public void SplitBy_SplitsRangeIntoSegments()
    {
        var range = new StswDateRange(new DateTime(2024, 1, 1), new DateTime(2024, 1, 4));
        var segments = range.SplitBy(TimeSpan.FromDays(1)).ToList();

        Assert.Equal(3, segments.Count);
        Assert.Equal(new DateTime(2024, 1, 1), segments[0].Start);
        Assert.Equal(new DateTime(2024, 1, 2), segments[0].End);
        Assert.Equal(new DateTime(2024, 1, 3), segments[2].Start);
        Assert.Equal(new DateTime(2024, 1, 4), segments[2].End);
    }

    [Fact]
    public void TryUnion_ReturnsUnionIfRangesOverlapOrTouch()
    {
        var a = new StswDateRange(new DateTime(2024, 1, 1), new DateTime(2024, 1, 10));
        var b = new StswDateRange(new DateTime(2024, 1, 10), new DateTime(2024, 1, 15));
        Assert.True(a.TryUnion(b, true, true, out var union));
        Assert.Equal(new DateTime(2024, 1, 1), union!.Start);
        Assert.Equal(new DateTime(2024, 1, 15), union.End);
    }

    [Fact]
    public void Union_ThrowsIfRangesAreDisjoint()
    {
        var a = new StswDateRange(new DateTime(2024, 1, 1), new DateTime(2024, 1, 10));
        var b = new StswDateRange(new DateTime(2024, 1, 11), new DateTime(2024, 1, 15));
        Assert.Throws<InvalidOperationException>(() => a.Union(b));
    }

    [Fact]
    public void GetUniqueYearDates_ReturnsStartOfEachYear()
    {
        var range = new StswDateRange(new DateTime(2022, 6, 1), new DateTime(2024, 2, 1));
        var years = range.GetUniqueYearDates().ToList();
        Assert.Contains(new DateTime(2022, 1, 1), years);
        Assert.Contains(new DateTime(2023, 1, 1), years);
        Assert.Contains(new DateTime(2024, 1, 1), years);
    }

    [Fact]
    public void GetUniqueQuarterValues_ReturnsYearAndQuarterTuples()
    {
        var range = new StswDateRange(new DateTime(2024, 1, 1), new DateTime(2024, 9, 1));
        var quarters = range.GetUniqueQuarterValues().ToList();
        Assert.Contains((2024, 1), quarters);
        Assert.Contains((2024, 2), quarters);
        Assert.Contains((2024, 3), quarters);
    }

    [Fact]
    public void GetUniqueMonthDates_ReturnsStartOfEachMonth()
    {
        var range = new StswDateRange(new DateTime(2024, 1, 15), new DateTime(2024, 3, 10));
        var months = range.GetUniqueMonthDates().ToList();
        Assert.Contains(new DateTime(2024, 1, 1), months);
        Assert.Contains(new DateTime(2024, 2, 1), months);
        Assert.Contains(new DateTime(2024, 3, 1), months);
    }

    [Fact]
    public void GetUniqueDayValues_ReturnsYearMonthDayTuples()
    {
        var range = new StswDateRange(new DateTime(2024, 1, 1), new DateTime(2024, 1, 3));
        var days = range.GetUniqueDayValues().ToList();
        Assert.Contains((2024, 1, 1), days);
        Assert.Contains((2024, 1, 2), days);
        Assert.Contains((2024, 1, 3), days);
    }

    [Fact]
    public void CompareTo_ReturnsCorrectOrder()
    {
        var a = new StswDateRange(new DateTime(2024, 1, 1), new DateTime(2024, 1, 10));
        var b = new StswDateRange(new DateTime(2024, 1, 5), new DateTime(2024, 1, 15));
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.True(a.CompareTo(a) == 0);
    }

    [Fact]
    public void Equals_ReturnsTrueForEqualRanges()
    {
        var a = new StswDateRange(new DateTime(2024, 1, 1), new DateTime(2024, 1, 10));
        var b = new StswDateRange(new DateTime(2024, 1, 1), new DateTime(2024, 1, 10));
        Assert.True(a.Equals(b));
        Assert.True(a == b);
        Assert.False(a != b);
    }

    [Fact]
    public void GetHashCode_IsConsistentForEqualRanges()
    {
        var a = new StswDateRange(new DateTime(2024, 1, 1), new DateTime(2024, 1, 10));
        var b = new StswDateRange(new DateTime(2024, 1, 1), new DateTime(2024, 1, 10));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsIsoFormat()
    {
        var range = new StswDateRange(new DateTime(2024, 1, 1, 12, 0, 0), new DateTime(2024, 1, 2, 13, 0, 0));
        var str = range.ToString();
        Assert.StartsWith("[2024-01-01T12:00:00.0000000", str);
        Assert.Contains("2024-01-02T13:00:00.0000000", str);
    }

    [Fact]
    public void ToString_WithFormatAndProvider()
    {
        var range = new StswDateRange(new DateTime(2024, 1, 1, 12, 0, 0), new DateTime(2024, 1, 2, 13, 0, 0));
        var str = range.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
        Assert.Equal("[2024-01-01 12:00 - 2024-01-02 13:00]", str);
    }
}
