using System.Text.RegularExpressions;

namespace StswExpress.Commons;

/// <summary>
/// Provides a natural string comparison that sorts strings in a human-expected order,
/// taking into account numerical values within the strings.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// List<string> items = new()
/// {
///     "Item1", "Item20", "Item3", "Item10", "Item2"
/// };
/// items.Sort(new StswNaturalStringComparer());
/// </code>
/// </example>
[StswInfo("0.10.0", "0.21.0")]
public partial class StswNaturalStringComparer : IComparer<string>
{
    [GeneratedRegex(@"\d+|\D+", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex TokenRegex();
    private static readonly Regex _tokenRegex = TokenRegex();

    /// <summary>
    /// Compares two strings using natural sorting, where numerical values are considered as whole numbers
    /// rather than individual characters, ensuring a human-friendly order.
    /// </summary>
    /// <param name="x">The first string to compare.</param>
    /// <param name="y">The second string to compare.</param>
    /// <returns>A signed integer that indicates the relative order of the strings.</returns>
    [StswInfo("0.10.0", "0.21.0")]
    public int Compare(string? x, string? y)
    {
        if (x == null || y == null)
            return Comparer<string>.Default.Compare(x, y);

        var xParts = _tokenRegex.Matches(x);
        var yParts = _tokenRegex.Matches(y);

        var count = Math.Min(xParts.Count, yParts.Count);
        for (var i = 0; i < count; i++)
        {
            var xs = xParts[i].Value;
            var ys = yParts[i].Value;

            var xn = IsAllDigits(xs);
            var yn = IsAllDigits(ys);

            int result;
            if (xn && yn)
                result = CompareNumericStrings(xs, ys);
            else if (!xn && !yn)
                result = string.Compare(xs, ys, StringComparison.Ordinal);
            else
                result = xn ? 1 : -1;

            if (result != 0)
                return result;
        }

        return xParts.Count.CompareTo(yParts.Count);
    }

    /// <summary>
    /// Checks if the given string consists entirely of digit characters.
    /// </summary>
    /// <param name="s">The string to check.</param>
    /// <returns><see langword="true"/> if the string contains only digits; otherwise, <see langword="false"/>.</returns>
    [StswInfo("0.21.0")]
    private static bool IsAllDigits(string s)
    {
        for (var i = 0; i < s.Length; i++)
            if (s[i] < '0' || s[i] > '9')
                return false;
        return s.Length > 0;
    }

    /// <summary>
    /// Compares two numeric strings without parsing to integers:
    /// 1) compare numeric value by trimming leading zeros and then by length and lexicographically;
    /// 2) if equal value, shorter original token (fewer leading zeros) is considered smaller.
    /// </summary>
    /// <param name="a">The first numeric string to compare.</param>
    /// <param name="b">The second numeric string to compare.</param>
    /// <returns>A signed integer that indicates the relative order of the numeric strings.</returns>
    [StswInfo("0.21.0")]
    private static int CompareNumericStrings(string a, string b)
    {
        var ia = 0; while (ia < a.Length && a[ia] == '0') ia++;
        var ib = 0; while (ib < b.Length && b[ib] == '0') ib++;

        var lena = a.Length - ia;
        var lenb = b.Length - ib;

        if (lena == 0) lena = 1;
        if (lenb == 0) lenb = 1;

        if (lena != lenb)
            return lena.CompareTo(lenb);

        int comp = 0;
        if (!(a.Length == ia && b.Length == ib))
        {
            var lenToCompare = a.Length - ia;
            comp = string.CompareOrdinal(a, ia, b, ib, lenToCompare);
            if (comp != 0)
                return comp;
        }

        if (comp != 0)
            return comp;

        return a.Length.CompareTo(b.Length);
    }
}
