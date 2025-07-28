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
[StswInfo("0.10.0")]
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
    public int Compare(string? x, string? y)
    {
        if (x == null || y == null)
            return Comparer<string>.Default.Compare(x, y);

        var xParts = _tokenRegex.Matches(x).Cast<Match>().Select(m => m.Value).ToArray();
        var yParts = _tokenRegex.Matches(y).Cast<Match>().Select(m => m.Value).ToArray();

        for (var i = 0; i < Math.Min(xParts.Length, yParts.Length); i++)
        {
            var xIsNumber = int.TryParse(xParts[i], out var xNum);
            var yIsNumber = int.TryParse(yParts[i], out var yNum);

            var result = xIsNumber && yIsNumber ? xNum.CompareTo(yNum) : string.Compare(xParts[i], yParts[i], StringComparison.Ordinal);
            if (result != 0)
                return result;
        }

        return xParts.Length.CompareTo(yParts.Length);
    }
}
