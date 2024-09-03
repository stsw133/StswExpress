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
    public int Compare(string? x, string? y)
    {
        if (x == null || y == null)
            return Comparer<string>.Default.Compare(x, y);

        var regex = new Regex(@"\d+|\D+");
        var xParts = regex.Matches(x).Cast<Match>().Select(m => m.Value).ToArray();
        var yParts = regex.Matches(y).Cast<Match>().Select(m => m.Value).ToArray();

        for (int i = 0; i < Math.Min(xParts.Length, yParts.Length); i++)
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
