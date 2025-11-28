using System;
using System.Collections.Generic;

namespace StswExpress;

/// <summary>
/// Simple Code 39 encoder.
/// </summary>
internal static class Code39Encoder
{
    private static readonly Dictionary<char, string> Patterns = new()
    {
        ['0'] = "111221211",
        ['1'] = "211211112",
        ['2'] = "112211112",
        ['3'] = "212211111",
        ['4'] = "111221112",
        ['5'] = "211221111",
        ['6'] = "112221111",
        ['7'] = "111211212",
        ['8'] = "211211211",
        ['9'] = "112211211",

        ['A'] = "211112112",
        ['B'] = "112112112",
        ['C'] = "212112111",
        ['D'] = "111122112",
        ['E'] = "211122111",
        ['F'] = "112122111",
        ['G'] = "111112212",
        ['H'] = "211112211",
        ['I'] = "112112211",
        ['J'] = "111122211",

        ['K'] = "211111122",
        ['L'] = "112111122",
        ['M'] = "212111121",
        ['N'] = "111121122",
        ['O'] = "211121121",
        ['P'] = "112121121",
        ['Q'] = "111111222",
        ['R'] = "211111221",
        ['S'] = "112111221",
        ['T'] = "111121221",

        ['U'] = "221111112",
        ['V'] = "122111112",
        ['W'] = "222111111",
        ['X'] = "121121112",
        ['Y'] = "221121111",
        ['Z'] = "122121111",

        ['-'] = "121111212",
        ['.'] = "221111211",
        [' '] = "122111211",
        ['$'] = "121212111",
        ['/'] = "121211121",
        ['+'] = "121112121",
        ['%'] = "111212121",

        ['*'] = "121121211"
    };

    /// <summary>
    /// Builds the Code 39 pattern for the given content.
    /// </summary>
    /// <param name="content">The content to encode.</param>
    /// <returns>The list of booleans representing the barcode pattern (true = bar, false = space).</returns>
    /// <exception cref="ArgumentException">Thrown when the content contains unsupported characters.</exception>
    public static List<bool> BuildPattern(string? content)
    {
        content ??= string.Empty;
        content = content.ToUpperInvariant();

        var full = "*" + content + "*";
        foreach (var ch in full)
            if (!Patterns.ContainsKey(ch))
                throw new ArgumentException($"Unsupported char for Code39: '{ch}'");

        var result = new List<bool>();
        result.AddRange(new bool[10]);

        var isBar = true;
        foreach (var ch in full)
        {
            var pattern = Patterns[ch];

            for (var i = 0; i < pattern.Length; i++)
            {
                var width = pattern[i] == '1' ? 1 : 3;
                for (var w = 0; w < width; w++)
                    result.Add(isBar);
                isBar = !isBar;
            }

            result.Add(false);
            isBar = true;
        }

        result.AddRange(new bool[10]);

        return result;
    }
}
