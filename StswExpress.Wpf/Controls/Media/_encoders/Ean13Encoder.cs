using System;
using System.Collections.Generic;
using System.Linq;

namespace StswExpress.Wpf;

/// <summary>
/// Encoder for EAN-13 barcodes.
/// </summary>
internal static class Ean13Encoder
{
    private static readonly string[] LeftOdd =
    [
        "0001101", "0011001", "0010011", "0111101", "0100011",
        "0110001", "0101111", "0111011", "0110111", "0001011"
    ];

    private static readonly string[] LeftEven =
    [
        "0100111", "0110011", "0011011", "0100001", "0011101",
        "0111001", "0000101", "0010001", "0001001", "0010111"
    ];

    private static readonly string[] Right =
    [
        "1110010", "1100110", "1101100", "1000010", "1011100",
        "1001110", "1010000", "1000100", "1001000", "1110100"
    ];

    // Parity patterns for the left side based on the first digit.
    private static readonly string[] Parities =
    [
        "LLLLLL",
        "LLGLGG",
        "LLGGLG",
        "LLGGGL",
        "LGLLGG",
        "LGGLLG",
        "LGGGLL",
        "LGLGLG",
        "LGLGGL",
        "LGGLGL"
    ];

    /// <summary>
    /// Builds the EAN-13 barcode pattern for the given content.
    /// </summary>
    /// <param name="content">The content to encode.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Thrown when content is invalid.</exception>
    public static List<bool> BuildPattern(string? content)
    {
        content ??= string.Empty;

        if (!content.All(char.IsDigit))
            throw new ArgumentException("EAN-13 accepts only digits.");

        var digits = content.Select(ch => ch - '0').ToList();

        if (digits.Count is not (12 or 13))
            throw new ArgumentException("EAN-13 requires 12 data digits or 13 including checksum.");

        if (digits.Count == 12)
        {
            var checksum = CalculateChecksum(digits);
            digits.Add(checksum);
        }
        else
        {
            var checksum = CalculateChecksum(digits.Take(12));
            if (checksum != digits[12])
                throw new ArgumentException("Invalid EAN-13 checksum.");
        }

        var result = new List<bool>();
        result.AddRange(new bool[10]);

        void AppendBits(string bits)
        {
            foreach (var bit in bits)
                result.Add(bit == '1');
        }

        AppendBits("101");

        var parity = Parities[digits[0]];
        for (var i = 0; i < 6; i++)
        {
            var digit = digits[i + 1];
            var encoding = parity[i] == 'L' ? LeftOdd[digit] : LeftEven[digit];
            AppendBits(encoding);
        }

        AppendBits("01010");

        for (var i = 7; i < 13; i++)
        {
            var digit = digits[i];
            AppendBits(Right[digit]);
        }

        AppendBits("101");
        result.AddRange(new bool[10]);

        return result;
    }

    /// <summary>
    /// Calculates the EAN-13 checksum digit for the given digits.
    /// </summary>
    /// <param name="digits">The first 12 digits of the EAN-13 code.</param>
    /// <returns>The checksum digit.</returns>
    /// <exception cref="ArgumentException">Thrown when the number of digits is not 12.</exception>
    private static int CalculateChecksum(IEnumerable<int> digits)
    {
        var list = digits.ToList();
        if (list.Count != 12)
            throw new ArgumentException("Checksum calculation requires exactly 12 digits.");

        var sum = 0;
        for (var i = 0; i < list.Count; i++)
        {
            var weight = (i % 2 == 0) ? 1 : 3;
            sum += list[i] * weight;
        }

        return (10 - (sum % 10)) % 10;
    }
}