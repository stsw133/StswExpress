using System;
using System.IO;
using System.Text.RegularExpressions;

namespace StswExpress.Wpf;

/// <summary>
/// Provides helper methods for filtering file paths based on specified filter patterns.
/// </summary>
internal static class StswPathFilterHelper
{
    /// <summary>
    /// Determines whether the provided file path is allowed by the given filter string.
    /// </summary>
    /// <param name="path">The full path of the file to check.</param>
    /// <param name="filter">The filter string using the file dialog format.</param>
    /// <returns><see langword="true"/> if the path is allowed by the filter or if no filter is provided; otherwise, <see langword="false"/>.</returns>
    public static bool IsFileAllowed(string path, string? filter) => MatchesFilter(Path.GetFileName(path), filter);

    /// <summary>
    /// Checks if the given file name matches the specified filter pattern.
    /// </summary>
    /// <param name="fileName">The name of the file to check.</param>
    /// <param name="filter">The filter pattern, which may include wildcards and multiple patterns separated by '|'.</param>
    /// <returns><see langword="true"/> if the file name matches the filter; otherwise, <see langword="false"/>.</returns>
    public static bool MatchesFilter(string fileName, string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
            return true;

        var parts = filter.Split('|');

        for (var i = 1; i < parts.Length; i += 2)
        {
            foreach (var mask in parts[i].Split([';'], StringSplitOptions.RemoveEmptyEntries))
            {
                if (mask is "*" or "*.*")
                    return true;

                if (Like(fileName, mask))
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines if the input string matches the specified pattern using wildcard characters.
    /// </summary>
    /// <param name="input">The input string to match.</param>
    /// <param name="pattern">The pattern containing wildcard characters ('*' and '?').</param>
    /// <returns><see langword="true"/> if the input matches the pattern; otherwise, <see langword="false"/>.</returns>
    private static bool Like(string input, string pattern)
    {
        var regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
        return Regex.IsMatch(input, regexPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }
}
