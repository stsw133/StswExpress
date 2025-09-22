using System.Runtime.CompilerServices;

namespace StswExpress.Commons;

/// <summary>
/// Represents a range between two <see cref="DateTime"/> values and exposes helper operations for working with ranges.
/// </summary>
[StswInfo("0.21.0", IsTested = false)]
public class StswDateRange : StswObservableObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StswDateRange"/> class.
    /// </summary>
    public StswDateRange() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="StswDateRange"/> class with the specified start and end values.
    /// </summary>
    /// <param name="start">The start <see cref="DateTime"/> value.</param>
    /// <param name="end">The end <see cref="DateTime"/> value.</param>
    public StswDateRange(DateTime start, DateTime end)
    {
        Start = start;
        End = end;
    }

    /// <summary>
    /// Gets or sets the start value of the range.
    /// </summary>
    public DateTime Start
    {
        get => _start;
        set => SetProperty(ref _start, value);
    }
    private DateTime _start;

    /// <summary>
    /// Gets or sets the end value of the range.
    /// </summary>
    public DateTime End
    {
        get => _end;
        set => SetProperty(ref _end, value);
    }
    private DateTime _end;

    /// <summary>
    /// Gets a value indicating whether the range is in chronological order.
    /// </summary>
    public bool IsNormalized => Start <= End;

    /// <summary>
    /// Gets the duration between the start and end values.
    /// </summary>
    public TimeSpan Duration
    {
        get
        {
            var (start, end) = OrderRange(Start, End);
            return end - start;
        }
    }

    /// <summary>
    /// Determines whether the range represents a single instant in time (i.e., start equals end).
    /// </summary>
    public bool IsInstant
    {
        get
        {
            var (start, end) = OrderRange(Start, End);
            return start == end;
        }
    }

    /// <summary>
    /// Attempts to compute the intersection of the current range with the specified <paramref name="other"/> range.
    /// </summary>
    /// <param name="other">The range to intersect with.</param>
    /// <param name="inclusive">Determines whether the range boundaries should be treated as inclusive.</param>
    /// <param name="intersection">When this method returns, contains the intersection range if the ranges intersect; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the ranges intersect; otherwise, <see langword="false"/>.</returns>
    public bool TryIntersect(StswDateRange other, bool inclusive, out StswDateRange? intersection)
    {
        ArgumentNullException.ThrowIfNull(other);

        var (firstStart, firstEnd) = OrderRange(Start, End);
        var (secondStart, secondEnd) = OrderRange(other.Start, other.End);

        var start = firstStart > secondStart ? firstStart : secondStart;
        var end = firstEnd < secondEnd ? firstEnd : secondEnd;

        var hasNonEmpty = inclusive ? start <= end : start < end;
        if (!hasNonEmpty)
        {
            intersection = null;
            return false;
        }

        intersection = new StswDateRange(start, end);
        return true;
    }

    /// <summary>
    /// Convenience: returns the intersection or null if ranges do not intersect.
    /// </summary>
    /// <param name="other">The range to intersect with.</param>
    /// <param name="inclusive">Determines whether the range boundaries should be treated as inclusive.</param>
    /// <returns>The intersection range if the ranges intersect; otherwise, <see langword="null"/>.</returns>
    public StswDateRange? IntersectOrNull(StswDateRange other, bool inclusive = true)
        => TryIntersect(other, inclusive, out var result)
            ? result
            : null;

    /// <summary>
    /// Convenience: returns the intersection or throws if ranges do not intersect.
    /// </summary>
    /// <param name="other">The range to intersect with.</param>
    /// <param name="inclusive">Determines whether the range boundaries should be treated as inclusive.</param>
    /// <returns>The intersection range.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the ranges do not intersect.</exception>
    public StswDateRange Intersect(StswDateRange other, bool inclusive = true)
        => TryIntersect(other, inclusive, out var result)
            ? result!
            : throw new InvalidOperationException("Ranges do not intersect.");

    /// <summary>
    /// Returns a normalized copy of the range in chronological order.
    /// </summary>
    /// <returns>A <see cref="StswDateRange"/> whose start is earlier than or equal to its end.</returns>
    public StswDateRange GetNormalized()
    {
        var (start, end) = OrderRange(Start, End);
        return new StswDateRange(start, end);
    }

    /// <summary>
    /// Normalizes the range in place, ensuring that the start is earlier than or equal to the end.
    /// </summary>
    public void Normalize()
    {
        if (End < Start)
            (Start, End) = (End, Start);
    }

    /// <summary>
    /// Converts the range to a tuple representation.
    /// </summary>
    public (DateTime Start, DateTime End) ToTuple() => (Start, End);

    /// <summary>
    /// Deconstructs the range into start and end values.
    /// </summary>
    public void Deconstruct(out DateTime start, out DateTime end)
    {
        start = Start;
        end = End;
    }

    /// <summary>
    /// Orders the provided start and end values chronologically.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (DateTime Start, DateTime End) OrderRange(DateTime start, DateTime end) => start <= end ? (start, end) : (end, start);

    #region Logical operations
    /// <summary>
    /// Determines whether the specified <paramref name="value"/> lies within the range.
    /// </summary>
    /// <param name="value">The <see cref="DateTime"/> value to check.</param>
    /// <param name="inclusive">Determines whether the range boundaries should be treated as inclusive.</param>
    /// <returns><see langword="true"/> if the value lies within the range; otherwise, <see langword="false"/>.</returns>
    public bool Contains(DateTime value, bool inclusive = true)
    {
        var (start, end) = OrderRange(Start, End);
        return inclusive
            ? value >= start && value <= end
            : value > start && value < end;
    }

    /// <summary>
    /// Determines whether the specified inner range lies entirely within the current range.
    /// </summary>
    /// <param name="innerStart">The start of the inner range.</param>
    /// <param name="innerEnd">The end of the inner range.</param>
    /// <param name="inclusive">Determines whether the range boundaries should be treated as inclusive.</param>
    /// <returns><see langword="true"/> if the inner range lies within the current range; otherwise, <see langword="false"/>.</returns>
    public bool Contains(DateTime innerStart, DateTime innerEnd, bool inclusive = true)
    {
        var (outerStart, outerEnd) = OrderRange(Start, End);
        var (orderedInnerStart, orderedInnerEnd) = OrderRange(innerStart, innerEnd);

        return inclusive
            ? outerStart <= orderedInnerStart && orderedInnerEnd <= outerEnd
            : outerStart < orderedInnerStart && orderedInnerEnd < outerEnd;
    }

    /// <summary>
    /// Determines whether the specified <paramref name="other"/> range lies entirely within the current range.
    /// </summary>
    /// <param name="other">The range to test.</param>
    /// <param name="inclusive">Determines whether the range boundaries should be treated as inclusive.</param>
    /// <returns><see langword="true"/> if the <paramref name="other"/> range lies within the current range; otherwise, <see langword="false"/>.</returns>
    public bool Contains(StswDateRange other, bool inclusive = true)
    {
        ArgumentNullException.ThrowIfNull(other);

        var (outerStart, outerEnd) = OrderRange(Start, End);
        var (innerStart, innerEnd) = OrderRange(other.Start, other.End);

        return inclusive
            ? outerStart <= innerStart && innerEnd <= outerEnd
            : outerStart < innerStart && innerEnd < outerEnd;
    }

    /// <summary>
    /// Determines whether the current range is adjacent to the specified <paramref name="other"/> range.
    /// </summary>
    /// <param name="other">The range to compare.</param>
    /// <returns><see langword="true"/> if the ranges are adjacent; otherwise, <see langword="false"/>.</returns>
    public bool IsAdjacentTo(StswDateRange other)
    {
        ArgumentNullException.ThrowIfNull(other);
        var (A0, A1) = OrderRange(Start, End);
        var (B0, B1) = OrderRange(other.Start, other.End);
        return A1 == B0 || B1 == A0;
    }

    /// <summary>
    /// Determines whether the current range overlaps with the specified start and end values.
    /// </summary>
    /// <param name="otherStart">The start of the other range.</param>
    /// <param name="otherEnd">The end of the other range.</param>
    /// <param name="inclusive">Determines whether the range boundaries should be treated as inclusive.</param>
    /// <returns><see langword="true"/> if the ranges overlap; otherwise, <see langword="false"/>.</returns>
    public bool Overlaps(DateTime otherStart, DateTime otherEnd, bool inclusive = true)
    {
        var (firstStart, firstEnd) = OrderRange(Start, End);
        var (secondStart, secondEnd) = OrderRange(otherStart, otherEnd);

        return inclusive
            ? firstStart <= secondEnd && secondStart <= firstEnd
            : firstStart < secondEnd && secondStart < firstEnd;
    }

    /// <summary>
    /// Determines whether the current range overlaps with the specified <paramref name="other"/> range.
    /// </summary>
    /// <param name="other">The range to compare.</param>
    /// <param name="inclusive">Determines whether the range boundaries should be treated as inclusive.</param>
    /// <returns><see langword="true"/> if the ranges overlap; otherwise, <see langword="false"/>.</returns>
    public bool Overlaps(StswDateRange other, bool inclusive = true)
    {
        ArgumentNullException.ThrowIfNull(other);
        return Overlaps(other.Start, other.End, inclusive);
    }

    /// <summary>
    /// Determines whether any ranges in the specified collection overlap.
    /// </summary>
    /// <param name="ranges">The collection of ranges to test.</param>
    /// <param name="inclusive">Determines whether the range boundaries should be treated as inclusive.</param>
    /// <returns><see langword="true"/> if any ranges overlap; otherwise, <see langword="false"/>.</returns>
    public static bool AnyOverlap(IEnumerable<(DateTime Start, DateTime End)> ranges, bool inclusive = true)
    {
        ArgumentNullException.ThrowIfNull(ranges);

        var normalized = new List<(DateTime Start, DateTime End)>();
        foreach (var (start, end) in ranges)
        {
            var (orderedStart, orderedEnd) = OrderRange(start, end);
            normalized.Add((orderedStart, orderedEnd));
        }

        return AnyOverlapCore(normalized, inclusive);
    }

    /// <summary>
    /// Determines whether any ranges in the specified collection overlap.
    /// </summary>
    /// <param name="ranges">The collection of ranges to test.</param>
    /// <param name="inclusive">Determines whether the range boundaries should be treated as inclusive.</param>
    /// <returns><see langword="true"/> if any ranges overlap; otherwise, <see langword="false"/>.</returns>
    public static bool AnyOverlap(IEnumerable<StswDateRange> ranges, bool inclusive = true)
    {
        ArgumentNullException.ThrowIfNull(ranges);

        var projected = ranges.Select(r =>
        {
            if (r is null)
                throw new ArgumentException("The collection cannot contain null ranges.", nameof(ranges));
            return (r.Start, r.End);
        });

        return AnyOverlap(projected, inclusive);
    }

    /// <summary>
    /// Core implementation of the AnyOverlap method that works on a list of normalized ranges.
    /// </summary>
    /// <param name="ranges">The list of normalized ranges to test.</param>
    /// <param name="inclusive">Determines whether the range boundaries should be treated as inclusive.</param>
    /// <returns><see langword="true"/> if any ranges overlap; otherwise, <see langword="false"/>.</returns>
    private static bool AnyOverlapCore(List<(DateTime Start, DateTime End)> ranges, bool inclusive)
    {
        if (ranges.Count <= 1)
            return false;

        ranges.Sort(static (left, right) => left.Start.CompareTo(right.Start));

        var currentMergedEnd = ranges[0].End;
        for (var i = 1; i < ranges.Count; i++)
        {
            var next = ranges[i];

            var overlaps = inclusive
                ? next.Start <= currentMergedEnd
                : next.Start < currentMergedEnd;

            if (overlaps)
                return true;

            if (next.End > currentMergedEnd)
                currentMergedEnd = next.End;
        }
        return false;
    }
    #endregion

    #region Predefined ranges
    /// <summary>
    /// Creates a date range covering the entire year.
    /// </summary>
    /// <param name="year">The year to generate the range for.</param>
    /// <returns>A <see cref="StswDateRange"/> from January 1st to December 31st of the specified year.</returns>
    public static StswDateRange FromYear(int year)
    {
        var start = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
        var end = new DateTime(year, 12, 31, 23, 59, 59, 999, DateTimeKind.Unspecified).AddTicks(9999);
        return new StswDateRange(start, end);
    }

    /// <summary>
    /// Creates a date range covering the specified quarter of a year.
    /// </summary>
    /// <param name="year">The year of the quarter.</param>
    /// <param name="quarter">The quarter (1–4).</param>
    /// <returns>A <see cref="StswDateRange"/> covering the given quarter.</returns>
    public static StswDateRange FromQuarter(int year, int quarter)
    {
        if (quarter < 1 || quarter > 4)
            throw new ArgumentOutOfRangeException(nameof(quarter), "Quarter must be between 1 and 4.");

        var startMonth = (quarter - 1) * 3 + 1;
        var start = new DateTime(year, startMonth, 1, 0, 0, 0, DateTimeKind.Unspecified);

        var endMonth = startMonth + 2;
        var daysInMonth = DateTime.DaysInMonth(year, endMonth);
        var end = new DateTime(year, endMonth, daysInMonth, 23, 59, 59, 999, DateTimeKind.Unspecified).AddTicks(9999);

        return new StswDateRange(start, end);
    }

    /// <summary>
    /// Creates a date range covering the specified month of a year.
    /// </summary>
    /// <param name="year">The year of the month.</param>
    /// <param name="month">The month (1–12).</param>
    /// <returns>A <see cref="StswDateRange"/> covering the given month.</returns>
    public static StswDateRange FromMonth(int year, int month)
    {
        if (month < 1 || month > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");

        var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Unspecified);

        var daysInMonth = DateTime.DaysInMonth(year, month);
        var end = new DateTime(year, month, daysInMonth, 23, 59, 59, 999, DateTimeKind.Unspecified).AddTicks(9999);

        return new StswDateRange(start, end);
    }
    #endregion

    #region Unique components extraction
    /// <summary>
    /// Returns unique years within this range as DateTime values at the start of each year (YYYY-01-01 00:00:00).
    /// </summary>
    /// <param name="inclusive">Determines whether the range boundaries should be treated as inclusive.</param>
    /// <returns>An enumerable of <see cref="DateTime"/> values representing the start of each unique year in the range.</returns>
    public IEnumerable<DateTime> GetUniqueYears(bool inclusive = true)
    {
        var (rangeStart, rangeEnd) = OrderRange(Start, End);
        if (inclusive ? rangeStart > rangeEnd : rangeStart >= rangeEnd)
            yield break;

        var currentYearStart = new DateTime(rangeStart.Year, 1, 1);
        var lastYearStart = new DateTime(rangeEnd.Year, 1, 1);

        for (var yearStart = currentYearStart; yearStart <= lastYearStart; yearStart = yearStart.AddYears(1))
        {
            var yearEndExclusive = yearStart.AddYears(1);
            var overlaps = inclusive
                ? rangeStart <= yearEndExclusive && yearStart <= rangeEnd
                : rangeStart < yearEndExclusive && yearStart < rangeEnd;

            if (overlaps)
                yield return yearStart;
        }
    }

    /// <summary>
    /// Returns unique months within this range as DateTime values at the start of each month (YYYY-MM-01 00:00:00).
    /// </summary>
    /// <param name="inclusive">Determines whether the range boundaries should be treated as inclusive.</param>
    /// <returns>An enumerable of <see cref="DateTime"/> values representing the start of each unique month in the range.</returns>
    public IEnumerable<DateTime> GetUniqueMonths(bool inclusive = true)
    {
        var (rangeStart, rangeEnd) = OrderRange(Start, End);
        if (inclusive ? rangeStart > rangeEnd : rangeStart >= rangeEnd)
            yield break;

        var currentMonthStart = new DateTime(rangeStart.Year, rangeStart.Month, 1);
        var lastMonthStart = new DateTime(rangeEnd.Year, rangeEnd.Month, 1);

        for (var monthStart = currentMonthStart; monthStart <= lastMonthStart; monthStart = monthStart.AddMonths(1))
        {
            var monthEndExclusive = monthStart.AddMonths(1);
            var overlaps = inclusive
                ? rangeStart <= monthEndExclusive && monthStart <= rangeEnd
                : rangeStart < monthEndExclusive && monthStart < rangeEnd;

            if (overlaps)
                yield return monthStart;
        }
    }

    /// <summary>
    /// Returns unique days within this range as DateTime values at the start of each day (YYYY-MM-DD 00:00:00).
    /// </summary>
    /// <param name="inclusive">Determines whether the range boundaries should be treated as inclusive.</param>
    /// <returns>An enumerable of <see cref="DateTime"/> values representing the start of each unique day in the range.</returns>
    public IEnumerable<DateTime> GetUniqueDays(bool inclusive = true)
    {
        var (rangeStart, rangeEnd) = OrderRange(Start, End);
        if (inclusive ? rangeStart > rangeEnd : rangeStart >= rangeEnd)
            yield break;

        var currentDayStart = rangeStart.Date;
        var lastDayStart = rangeEnd.Date;

        for (var dayStart = currentDayStart; dayStart <= lastDayStart; dayStart = dayStart.AddDays(1))
        {
            var dayEndExclusive = dayStart.AddDays(1);
            var overlaps = inclusive
                ? rangeStart <= dayEndExclusive && dayStart <= rangeEnd
                : rangeStart < dayEndExclusive && dayStart < rangeEnd;

            if (overlaps)
                yield return dayStart;
        }
    }
    #endregion

    #region Overloaded factory methods
    /// <summary>
    /// Returns a string representation of the range in ISO 8601 format.
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"[{Start:o} – {End:o}]";

    /// <summary>
    /// Determines whether the current range is equal to another range.
    /// </summary>
    /// <param name="other">The range to compare with the current range.</param>
    /// <returns><see langword="true"/> if the ranges are equal; otherwise, <see langword="false"/>.</returns>
    public bool Equals(StswDateRange? other)
    {
        if (other is null) return false;
        var (s1, e1) = OrderRange(Start, End);
        var (s2, e2) = OrderRange(other.Start, other.End);
        return s1 == s2 && e1 == e2;
    }
    public override bool Equals(object? obj) => obj is StswDateRange r && Equals(r);

    /// <summary>
    /// Returns a hash code for the range.
    /// </summary>
    /// <returns>A hash code for the range.</returns>
    public override int GetHashCode()
    {
        var (s, e) = OrderRange(Start, End);
        return HashCode.Combine(s, e);
    }
    public static bool operator ==(StswDateRange? a, StswDateRange? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(StswDateRange? a, StswDateRange? b) => !(a == b);
    #endregion
}
