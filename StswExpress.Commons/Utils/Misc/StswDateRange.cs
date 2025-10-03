using System.Globalization;
using System.Runtime.CompilerServices;

namespace StswExpress.Commons;

/// <summary>
/// Represents a range between two <see cref="DateTime"/> values and exposes helper operations for working with ranges.
/// </summary>
public class StswDateRange : StswObservableObject, IComparable<StswDateRange>, IEquatable<StswDateRange>
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
    /// Orders the provided start and end values chronologically.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (DateTime Start, DateTime End) OrderRange(DateTime start, DateTime end) => start <= end ? (start, end) : (end, start);

    /// <summary>
    /// Converts the range to a tuple representation.
    /// </summary>
    public (DateTime Start, DateTime End) ToTuple() => (Start, End);

    #region Add operations
    /// <summary>
    /// Adds the specified <paramref name="offset"/> to both the start and end of the range.
    /// </summary>
    /// <param name="offset">The offset to add.</param>
    /// <returns>A new <see cref="StswDateRange"/> with both bounds adjusted by <paramref name="offset"/>.</returns>
    public StswDateRange Add(TimeSpan offset)
    {
        var (s, e) = OrderRange(Start, End);
        return new StswDateRange(s + offset, e + offset);
    }

    /// <summary>
    /// Adds the specified number of years to both the start and end of the range.
    /// </summary>
    /// <param name="years">The number of years to add. Can be negative to subtract years.</param>
    /// <returns>A new <see cref="StswDateRange"/> with both bounds adjusted by the specified number of years.</returns>
    public StswDateRange AddYears(int years)
    {
        var (s, e) = OrderRange(Start, End);
        return new StswDateRange(s.AddYears(years), e.AddYears(years)).GetNormalized();
    }

    /// <summary>
    /// Adds the specified number of months to both the start and end of the range.
    /// </summary>
    /// <param name="months">The number of months to add. Can be negative to subtract months.</param>
    /// <returns>A new <see cref="StswDateRange"/> with both bounds adjusted by the specified number of months.</returns>
    public StswDateRange AddMonths(int months)
    {
        var (s, e) = OrderRange(Start, End);
        return new StswDateRange(s.AddMonths(months), e.AddMonths(months)).GetNormalized();
    }

    /// <summary>
    /// Adds the specified number of days to both the start and end of the range.
    /// </summary>
    /// <param name="days">The number of days to add. Can be negative to subtract days.</param>
    /// <returns>A new <see cref="StswDateRange"/> with both bounds adjusted by the specified number of days.</returns>
    public StswDateRange AddDays(double days) => Add(TimeSpan.FromDays(days));

    /// <summary>
    /// Adds the specified number of hours to both the start and end of the range.
    /// </summary>
    /// <param name="hours">The number of hours to add. Can be negative to subtract hours.</param>
    /// <returns>A new <see cref="StswDateRange"/> with both bounds adjusted by the specified number of hours.</returns>
    public StswDateRange AddHours(double hours) => Add(TimeSpan.FromHours(hours));

    /// <summary>
    /// Adds the specified number of minutes to both the start and end of the range.
    /// </summary>
    /// <param name="minutes">The number of minutes to add. Can be negative to subtract minutes.</param>
    /// <returns>A new <see cref="StswDateRange"/> with both bounds adjusted by the specified number of minutes.</returns>
    public StswDateRange AddMinutes(double minutes) => Add(TimeSpan.FromMinutes(minutes));

    /// <summary>
    /// Adds the specified number of seconds to both the start and end of the range.
    /// </summary>
    /// <param name="seconds">The number of seconds to add. Can be negative to subtract seconds.</param>
    /// <returns>A new <see cref="StswDateRange"/> with both bounds adjusted by the specified number of seconds.</returns>
    public StswDateRange AddSeconds(double seconds) => Add(TimeSpan.FromSeconds(seconds));

    /// <summary>
    /// Adds the specified number of milliseconds to both the start and end of the range.
    /// </summary>
    /// <param name="ms">The number of milliseconds to add. Can be negative to subtract milliseconds.</param>
    /// <returns>A new <see cref="StswDateRange"/> with both bounds adjusted by the specified number of milliseconds.</returns>
    public StswDateRange AddMilliseconds(double ms) => Add(TimeSpan.FromMilliseconds(ms));

    /// <summary>
    /// Adds the specified number of ticks to both the start and end of the range.
    /// </summary>
    /// <param name="ticks">The number of ticks to add. Can be negative to subtract ticks.</param>
    /// <returns>A new <see cref="StswDateRange"/> with both bounds adjusted by the specified number of ticks.</returns>
    public StswDateRange AddTicks(long ticks) => Add(new TimeSpan(ticks));

    /// <summary>
    /// Adds different offsets to the start and end of the range.
    /// </summary>
    /// <param name="startDelta">The offset to add to the start of the range. Can be negative to subtract time.</param>
    /// <param name="endDelta">The offset to add to the end of the range. Can be negative to subtract time.</param>
    /// <returns>A new <see cref="StswDateRange"/> with the start adjusted by <paramref name="startDelta"/> and the end adjusted by <paramref name="endDelta"/>.</returns>
    public StswDateRange AddOffsets(TimeSpan startDelta, TimeSpan endDelta)
    {
        var (s, e) = OrderRange(Start, End);
        return new StswDateRange(s + startDelta, e + endDelta).GetNormalized();
    }
    #endregion

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
        return Contains(other.Start, other.End, inclusive);
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

    #region Transformations
    /// <summary>
    /// Returns a new range whose bounds are clamped to lie within the specified limits.
    /// </summary>
    /// <param name="min">The inclusive (or exclusive) lower bound.</param>
    /// <param name="max">The inclusive (or exclusive) upper bound.</param>
    /// <param name="inclusive">Determines whether <paramref name="min"/> and <paramref name="max"/> are treated as inclusive (<see langword="true"/>) or exclusive (<see langword="false"/>).
    /// When exclusive, one tick is trimmed from each side to keep the bounds outside of the clamp limits.</param>
    /// <returns>A <see cref="StswDateRange"/> limited to the provided bounds.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="inclusive"/> is <see langword="false"/> and the exclusive interval does not contain any representable <see cref="DateTime"/> values.</exception>
    public StswDateRange Clamp(DateTime min, DateTime max, bool inclusive = true)
    {
        var (rangeStart, rangeEnd) = OrderRange(Start, End);
        var (orderedMin, orderedMax) = OrderRange(min, max);

        DateTime effectiveMin;
        DateTime effectiveMax;

        if (inclusive)
        {
            effectiveMin = orderedMin;
            effectiveMax = orderedMax;
        }
        else
        {
            if (orderedMax.Ticks - orderedMin.Ticks <= 1)
                throw new ArgumentException("Exclusive clamp requires at least two ticks between bounds.", nameof(max));

            effectiveMin = orderedMin.AddTicks(1);
            effectiveMax = orderedMax.AddTicks(-1);
        }

        var clampedStart = rangeStart < effectiveMin ? effectiveMin : rangeStart;
        clampedStart = clampedStart > effectiveMax ? effectiveMax : clampedStart;

        var clampedEnd = rangeEnd > effectiveMax ? effectiveMax : rangeEnd;
        clampedEnd = clampedEnd < effectiveMin ? effectiveMin : clampedEnd;

        if (clampedEnd < clampedStart)
            clampedEnd = clampedStart;

        return new StswDateRange(clampedStart, clampedEnd);
    }

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
    /// Splits the current range into consecutive subranges of the specified <paramref name="step"/>.
    /// </summary>
    /// <param name="step">The duration of each segment. Must be a positive, finite <see cref="TimeSpan"/>.</param>
    /// <param name="inclusive">Determines whether the original range boundaries are treated as inclusive (<see langword="true"/>) or exclusive (<see langword="false"/>). When exclusive, the method trims one tick from each side before generating segments. If no values remain, the resulting enumeration is empty.</param>
    /// <returns>An enumerable of <see cref="StswDateRange"/> segments that cover the current range.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="step"/> is not a positive, finite duration.</exception>
    public IEnumerable<StswDateRange> SplitBy(TimeSpan step, bool inclusive = true)
    {
        if (step.Ticks <= 0)
            throw new ArgumentOutOfRangeException(nameof(step), "Step must be positive.");

        var (rangeStart, rangeEnd) = OrderRange(Start, End);
        if (inclusive ? rangeStart > rangeEnd : rangeStart >= rangeEnd)
            yield break;

        var effectiveStart = inclusive ? rangeStart : rangeStart.AddTicks(1);
        var effectiveEnd = inclusive ? rangeEnd : rangeEnd.AddTicks(-1);

        if (effectiveStart > effectiveEnd)
            yield break;

        for (var segStart = effectiveStart; ;)
        {
            var segEnd = segStart + step;
            if (segEnd >= effectiveEnd)
            {
                yield return new StswDateRange(segStart, inclusive ? rangeEnd : effectiveEnd);
                yield break;
            }

            yield return new StswDateRange(segStart, segEnd);
            segStart = segEnd;
        }
    }

    /// <summary>
    /// Attempts to compute the union of the current range with the specified <paramref name="other"/> range.
    /// </summary>
    /// <param name="other">The other range to merge with.</param>
    /// <param name="inclusive">Determines whether the range boundaries should be treated as inclusive.</param>
    /// <param name="allowTouching">If <see langword="true"/>, touching ranges (where the end of one range equals the start of the other) are considered mergeable.</param>
    /// <param name="union">When this method returns, contains the union range if a single-range union is possible; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if a single-range union is possible; otherwise, <see langword="false"/>.</returns>
    public bool TryUnion(StswDateRange other, bool inclusive, bool allowTouching, out StswDateRange? union)
    {
        ArgumentNullException.ThrowIfNull(other);

        var (a0, a1) = OrderRange(Start, End);
        var (b0, b1) = OrderRange(other.Start, other.End);

        var overlaps = inclusive
            ? a0 <= b1 && b0 <= a1
            : a0 < b1 && b0 < a1;

        var touches = (a1 == b0) || (b1 == a0);

        if (!overlaps && !(allowTouching && touches))
        {
            union = null;
            return false;
        }

        var start = a0 <= b0 ? a0 : b0;
        var end = a1 >= b1 ? a1 : b1;
        union = new StswDateRange(start, end);
        return true;
    }

    /// <summary>
    /// Convenience: returns the union or null if a single-range union is not possible.
    /// </summary>
    /// <param name="other">The other range to merge with.</param>
    /// <param name="inclusive">Determines whether the range boundaries should be treated as inclusive.</param>
    /// <param name="allowTouching">If <see langword="true"/>, touching ranges (where the end of one range equals the start of the other) are considered mergeable.</param>
    /// <returns>The union range if a single-range union is possible; otherwise, <see langword="null"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the ranges are disjoint and cannot be merged into a single range.</exception>
    public StswDateRange Union(StswDateRange other, bool inclusive = true, bool allowTouching = false)
        => TryUnion(other, inclusive, allowTouching, out var u)
            ? u!
            : throw new InvalidOperationException("Ranges are disjoint; a single-range union does not exist.");
    #endregion

    #region Unique components extraction
    /// <summary>
    /// Enumerates unique units (years, quarters, months) within the specified range.
    /// </summary>
    /// <param name="start">The start of the range.</param>
    /// <param name="end">The end of the range.</param>
    /// <param name="snapToUnitStart">A function that snaps a <see cref="DateTime"/> to the start of the desired unit (e.g., start of month).</param>
    /// <param name="nextUnitStart">A function that computes the start of the next unit given the start of the current unit (e.g., add one month).</param>
    /// <param name="inclusive">Determines whether the range boundaries should be treated as inclusive.</param>
    /// <returns>An enumerable of <see cref="DateTime"/> values representing the start of each unique unit in the range.</returns>
    private static IEnumerable<DateTime> EnumerateUnits(
        DateTime start,
        DateTime end,
        Func<DateTime, DateTime> snapToUnitStart,
        Func<DateTime, DateTime> nextUnitStart,
        bool inclusive)
    {
        var (rangeStart, rangeEnd) = OrderRange(start, end);
        if (inclusive ? rangeStart > rangeEnd : rangeStart >= rangeEnd)
            yield break;

        var firstUnit = snapToUnitStart(rangeStart);
        var lastUnit = snapToUnitStart(rangeEnd);

        for (var unitStart = firstUnit; unitStart <= lastUnit; unitStart = nextUnitStart(unitStart))
        {
            var unitEndExclusive = nextUnitStart(unitStart);
            var overlaps = inclusive
                ? rangeStart <= unitEndExclusive && unitStart <= rangeEnd
                : rangeStart < unitEndExclusive && unitStart < rangeEnd;

            if (overlaps)
                yield return unitStart;
        }
    }

    /// <summary>
    /// Returns unique years within this range as <see cref="DateTime"/> values at the start of each year (YYYY-01-01 00:00:00).
    /// </summary>
    /// <param name="inclusive">Determines whether the range boundaries should be treated as inclusive.</param>
    /// <returns>An enumerable of <see cref="DateTime"/> values representing the start of each unique year in the range.</returns>
    public IEnumerable<DateTime> GetUniqueYearDates(bool inclusive = true)
        => EnumerateUnits(Start, End, d => new DateTime(d.Year, 1, 1), d => d.AddYears(1), inclusive);

    /// <summary>
    /// Returns unique years within this range as numeric values (YYYY).
    /// </summary>
    /// <param name="inclusive">Determines whether the range boundaries should be treated as inclusive.</param>
    /// <returns>An enumerable of <see cref="int"/> values representing each unique year in the range.</returns>
    public IEnumerable<int> GetUniqueYearValues(bool inclusive = true)
    {
        foreach (var y in EnumerateUnits(Start, End, d => new DateTime(d.Year, 1, 1), d => d.AddYears(1), inclusive))
            yield return y.Year;
    }

    /// <summary>
    /// Returns unique quarters within this range as <see cref="DateTime"/> values at the start of each quarter (YYYY-MM-01 00:00:00).
    /// </summary>
    /// <param name="inclusive">Determines whether the range boundaries should be treated as inclusive.</param>
    /// <returns>An enumerable of <see cref="DateTime"/> values representing the start of each unique quarter in the range.</returns>
    public IEnumerable<DateTime> GetUniqueQuarterDates(bool inclusive = true)
    {
        static int QuarterStartMonth(int m) => (m - 1) / 3 * 3 + 1;
        return EnumerateUnits(Start, End, d => new DateTime(d.Year, QuarterStartMonth(d.Month), 1), d => d.AddMonths(3),
            inclusive);
    }

    /// <summary>
    /// Returns unique quarters within this range.
    /// </summary>
    /// <param name="inclusive">Determines whether the range boundaries should be treated as inclusive.</param>
    /// <returns>An enumerable of tuples describing the year and quarter (1–4).</returns>
    public IEnumerable<(int Year, int Quarter)> GetUniqueQuarterValues(bool inclusive = true)
    {
        static int QuarterStartMonth(int month) => (month - 1) / 3 * 3 + 1;
        foreach (var q in EnumerateUnits(Start, End, d => new DateTime(d.Year, QuarterStartMonth(d.Month), 1), d => d.AddMonths(3), inclusive))
            yield return (q.Year, q.GetQuarter());
    }

    /// <summary>
    /// Returns unique months within this range as <see cref="DateTime"/> values at the start of each month (YYYY-MM-01 00:00:00).
    /// </summary>
    /// <param name="inclusive">Determines whether the range boundaries should be treated as inclusive.</param>
    /// <returns>An enumerable of <see cref="DateTime"/> values representing the start of each unique month in the range.</returns>
    public IEnumerable<DateTime> GetUniqueMonthDates(bool inclusive = true)
        => EnumerateUnits(Start, End, d => new DateTime(d.Year, d.Month, 1), d => d.AddMonths(1), inclusive);

    /// <summary>
    /// Returns unique year-month combinations within this range.
    /// </summary>
    /// <param name="inclusive">Determines whether the range boundaries should be treated as inclusive.</param>
    /// <returns>An enumerable of tuples describing the year and month (1–12).</returns>
    public IEnumerable<(int Year, int Month)> GetUniqueYearMonthValues(bool inclusive = true)
    {
        foreach (var m in EnumerateUnits(Start, End, d => new DateTime(d.Year, d.Month, 1), d => d.AddMonths(1), inclusive))
            yield return (m.Year, m.Month);
    }

    /// <summary>
    /// Returns unique days within this range as <see cref="DateTime"/> values at the start of each day (YYYY-MM-DD 00:00:00).
    /// </summary>
    /// <param name="inclusive">Determines whether the range boundaries should be treated as inclusive.</param>
    /// <returns>An enumerable of <see cref="DateTime"/> values representing the start of each unique day in the range.</returns>
    public IEnumerable<DateTime> GetUniqueDayDates(bool inclusive = true)
        => EnumerateUnits(Start, End, d => new DateTime(d.Year, d.Month, d.Day), d => d.AddDays(1), inclusive);

    /// <summary>
    /// Returns unique year-month-day combinations within this range.
    /// </summary>
    /// <param name="inclusive">Determines whether the range boundaries should be treated as inclusive.</param>
    /// <returns>An enumerable of tuples describing the year, month (1–12), and day (1–31).</returns>
    public IEnumerable<(int Year, int Month, int Day)> GetUniqueDayValues(bool inclusive = true)
    {
        foreach (var d in EnumerateUnits(Start, End, d => new DateTime(d.Year, d.Month, d.Day), d => d.AddDays(1), inclusive))
            yield return (d.Year, d.Month, d.Day);
    }
    #endregion

    /// <summary>
    /// Creates a new <see cref="StswDateRange"/> from the specified start and end values.
    /// </summary>
    /// <param name="other">The range to copy.</param>
    /// <returns>A new <see cref="StswDateRange"/> instance.</returns>
    public int CompareTo(StswDateRange? other)
    {
        if (other is null) return 1;
        var (aStart, aEnd) = OrderRange(Start, End);
        var (bStart, bEnd) = OrderRange(other.Start, other.End);

        int cmp = aStart.CompareTo(bStart);
        if (cmp != 0) return cmp;
        return aEnd.CompareTo(bEnd);
    }

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
    public static bool operator ==(StswDateRange? a, StswDateRange? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(StswDateRange? a, StswDateRange? b) => !(a == b);

    /// <summary>
    /// Returns a hash code for the range.
    /// </summary>
    /// <returns>A hash code for the range.</returns>
    public override int GetHashCode()
    {
        var (s, e) = OrderRange(Start, End);
        return HashCode.Combine(s, e);
    }

    /// <summary>
    /// Returns a string representation of the range in ISO 8601 format.
    /// </summary>
    /// <returns>A string representation of the range.</returns>
    public override string ToString() => ToString("o", CultureInfo.CurrentCulture);

    /// <summary>
    /// Returns a string representation of the range using the specified format for the start and end values.
    /// </summary>
    /// <param name="format">The format string to use for the start and end values. If <see langword="null"/>, the default format is used.</param>
    /// <returns>A string representation of the range.</returns>
    public string ToString(string? format) => ToString(format, CultureInfo.CurrentCulture);

    /// <summary>
    /// Returns a string representation of the range using the specified format and culture for the start and end values.
    /// </summary>
    /// <param name="format">The format string to use for the start and end values. If <see langword="null"/>, the default format is used.</param>
    /// <param name="provider">The format string to use for the start and end values. If <see langword="null"/>, the default format is used.</param>
    /// <returns>A string representation of the range.</returns>
    public string ToString(string? format, IFormatProvider? provider)
    {
        var (s, e) = OrderRange(Start, End);
        return $"[{s.ToString(format, provider)} - {e.ToString(format, provider)}]";
    }
}
