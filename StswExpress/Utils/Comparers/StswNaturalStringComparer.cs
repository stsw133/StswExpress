using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace StswExpress;

/// <summary>
/// Provides a natural string comparison that sorts strings in a human-expected order,
/// taking into account numerical values within the strings.
/// </summary>
public class StswNaturalStringComparer : IComparer<string>
{
    /// <summary>
    /// Compares two strings using natural sorting, where numerical values are considered as whole numbers
    /// rather than individual characters, ensuring a human-friendly order.
    /// </summary>
    /// <param name="x">The first string to compare.</param>
    /// <param name="y">The second string to compare.</param>
    /// <returns>
    /// A negative value if <paramref name="x"/> is less than <paramref name="y"/>, 
    /// zero if they are equal, or a positive value if <paramref name="x"/> is greater than <paramref name="y"/>.
    /// </returns>
    public int Compare(string? x, string? y)
    {
        if (x == null || y == null)
            return Comparer<string>.Default.Compare(x, y);

        var regex = new Regex(@"\d+|\D+");
        var xParts = regex.Matches(x).Cast<Match>().Select(m => m.Value).ToArray();
        var yParts = regex.Matches(y).Cast<Match>().Select(m => m.Value).ToArray();

        for (var i = 0; i < Math.Min(xParts.Length, yParts.Length); i++)
        {
            var xIsNumber = int.TryParse(xParts[i], out var xNum);
            var yIsNumber = int.TryParse(yParts[i], out var yNum);

            int result;

            if (xIsNumber && yIsNumber)
                result = xNum.CompareTo(yNum);
            else
                result = string.Compare(xParts[i], yParts[i], StringComparison.Ordinal);

            if (result != 0)
                return result;
        }

        return xParts.Length.CompareTo(yParts.Length);
    }
}

/* usage:

List<string> items = new()
{
    "Item1", "Item20", "Item3", "Item10", "Item2"
};
items.Sort(new StswNaturalStringComparer());

*/
