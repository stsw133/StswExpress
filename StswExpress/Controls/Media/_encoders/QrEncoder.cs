using System;
using System.Collections.Generic;
using System.Text;

namespace StswExpress;

/// <summary>
/// Simple QR Code encoder for Model 2 QR codes (ECC M) with automatic version selection.
/// </summary>
public static class QrEncoder
{
    private const int MinVersion = 1;
    private const int MaxVersion = 40;
    private const int FormatEcLevel = 0b00; // M

    private static readonly int[] EcCodewordsPerBlock =
    [
        -1, 10, 16, 26, 18, 24, 16, 18, 22, 22, 26, 30, 22, 22, 24, 24, 28, 28, 26, 26, 26, 26,
        28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28
    ];

    private static readonly int[] EcBlocks =
    [
        -1, 1, 1, 1, 2, 2, 4, 4, 4, 5, 5, 5, 8, 9, 9, 10, 10, 11, 13, 14, 16, 17, 17, 18, 20,
        21, 23, 25, 26, 28, 29, 31, 33, 35, 37, 38, 40, 43, 45, 47, 49
    ];

    #region Public API

    /// <summary>
    /// Encodes the given text into a QR code matrix (Model 2, ECC M) with automatic version &amp; mask selection.
    /// </summary>
    /// <param name="text">The text to encode.</param>
    /// <param name="encoding">The character encoding to use (default is ISO-8859-1).</param>
    /// <returns>2D boolean array representing the QR code modules (true = black, false = white).</returns>
    public static bool[,] Encode(string text, Encoding? encoding = null)
    {
        encoding ??= Encoding.GetEncoding("ISO-8859-1");

        var version = SelectVersion(text, encoding);
        var size = 17 + 4 * version;

        var data = EncodeDataToCodewords(text, version, encoding);
        var allCodewords = AddErrorCorrection(data, version);

        var modules = new bool[size, size];
        var isFunction = new bool[size, size];

        PlaceFunctionPatterns(modules, isFunction, version);
        PlaceDataBits(modules, isFunction, allCodewords);

        var baseMatrix = (bool[,])modules.Clone();
        bool[,] bestMatrix = baseMatrix;
        var bestMask = 0;
        var bestPenalty = int.MaxValue;

        for (var maskId = 0; maskId < 8; maskId++)
        {
            var candidate = (bool[,])baseMatrix.Clone();
            ApplyMask(candidate, isFunction, maskId);
            var penalty = CalculatePenalty(candidate);

            if (penalty < bestPenalty)
            {
                bestPenalty = penalty;
                bestMask = maskId;
                bestMatrix = candidate;
            }
        }

        modules = bestMatrix;

        AddFormatInformation(modules, bestMask, version);

        return modules;
    }
    #endregion

    #region Data encoding
    /// <summary>
    /// Encodes the input text into data codewords using Byte mode.
    /// </summary>
    /// <param name="text">The input text to encode.</param>
    /// <param name="version">The target QR version.</param>
    /// <param name="encoding">The character encoding to use.</param>
    /// <returns>Array of data codewords.</returns>
    /// <exception cref="ArgumentException">Thrown if the text is too long for ECC M in the selected version.</exception>
    private static byte[] EncodeDataToCodewords(string text, int version, Encoding encoding)
    {
        var bytes = encoding.GetBytes(text);
        var dataCodewords = GetNumDataCodewords(version);
        var capacityBits = dataCodewords * 8;
        var charCountBits = GetCharCountBits(version);
        var neededBits = 4 + charCountBits + bytes.Length * 8;

        if (neededBits > capacityBits)
            throw new ArgumentException("Text too long for the selected QR version at ECC M.");

        var bits = new List<int>();
        bits.AddRange([0, 1, 0, 0]);

        for (var i = charCountBits - 1; i >= 0; i--)
            bits.Add(((bytes.Length >> i) & 1) != 0 ? 1 : 0);

        foreach (var b in bytes)
            for (var i = 7; i >= 0; i--)
                bits.Add(((b >> i) & 1) != 0 ? 1 : 0);

        var maxBits = capacityBits;
        var remaining = maxBits - bits.Count;
        var terminatorBits = Math.Min(4, Math.Max(0, remaining));
        for (var i = 0; i < terminatorBits; i++)
            bits.Add(0);

        while (bits.Count % 8 != 0)
            bits.Add(0);

        var codewords = new List<byte>();
        for (var i = 0; i < bits.Count; i += 8)
        {
            byte cw = 0;
            for (var j = 0; j < 8; j++)
                if (bits[i + j] == 1)
                    cw |= (byte)(1 << (7 - j));
            codewords.Add(cw);
        }

        byte[] pad = [0xEC, 0x11];
        var padIndex = 0;
        while (codewords.Count < dataCodewords)
        {
            codewords.Add(pad[padIndex % 2]);
            padIndex++;
        }

        return [.. codewords];
    }

    /// <summary>
    /// Selects the smallest QR version that fits the input using ECC M.
    /// </summary>
    /// <param name="text">The text to encode.</param>
    /// <param name="encoding">The character encoding to use.</param>
    /// <returns>The chosen version between 1 and 40.</returns>
    /// <exception cref="ArgumentException">Thrown if the text cannot fit in Version 40-M.</exception>
    private static int SelectVersion(string text, Encoding encoding)
    {
        var bytes = encoding.GetBytes(text);

        for (var version = MinVersion; version <= MaxVersion; version++)
        {
            var capacityBits = GetNumDataCodewords(version) * 8;
            var charCountBits = GetCharCountBits(version);
            var neededBits = 4 + charCountBits + bytes.Length * 8;

            if (neededBits <= capacityBits)
                return version;
        }

        throw new ArgumentException("Text too long for QR Code Version 40-M.");
    }

    /// <summary>
    /// Adds Reed–Solomon error correction codewords to the data.
    /// </summary>
    /// <param name="version">The QR version.</param>
    /// <returns>Array of all codewords (data + error correction).</returns>
    private static int GetCharCountBits(int version) => version <= 9 ? 8 : 16;

    /// <summary>
    /// Adds Reed–Solomon error correction codewords to the data.
    /// </summary>
    /// <param name="version">The QR version.</param>
    /// <returns>Array of all codewords (data + error correction).</returns>
    private static int GetNumDataCodewords(int version)
    {
        var rawDataModules = GetNumRawDataModules(version);
        return rawDataModules / 8 - EcCodewordsPerBlock[version] * EcBlocks[version];
    }

    /// <summary>
    /// Calculates the number of raw data modules for the given version.
    /// </summary>
    /// <param name="version">The QR version.</param>
    /// <returns>The number of raw data modules.</returns>
    private static int GetNumRawDataModules(int version)
    {
        var result = (16 * version + 128) * version + 64;
        if (version >= 2)
        {
            var numAlign = version / 7 + 2;
            result -= (25 * numAlign - 10) * numAlign - 55;
            if (version >= 7)
                result -= 36;
        }

        return result;
    }
    #endregion

    #region Reed–Solomon (GF(256))
    private static readonly byte[] GfExp;
    private static readonly byte[] GfLog;

    /// <summary>
    /// Static constructor to initialize Galois Field tables.
    /// </summary>
    static QrEncoder()
    {
        GfExp = new byte[512];
        GfLog = new byte[256];

        var x = 1;
        for (var i = 0; i < 255; i++)
        {
            GfExp[i] = (byte)x;
            GfLog[x] = (byte)i;

            x <<= 1;
            if ((x & 0x100) != 0)
                x ^= 0x11D;
        }
        for (var i = 255; i < 512; i++)
            GfExp[i] = GfExp[i - 255];
    }

    /// <summary>
    /// Multiplies two bytes in GF(256).
    /// </summary>
    /// <param name="a">The first byte.</param>
    /// <param name="b">The second byte.</param>
    /// <returns>The product in GF(256).</returns>
    private static byte GfMul(byte a, byte b)
    {
        if (a == 0 || b == 0) return 0;
        int log = GfLog[a] + GfLog[b];
        return GfExp[log];
    }

    /// <summary>
    /// Computes the Reed–Solomon error correction codewords.
    /// </summary>
    /// <param name="data">The data codewords.</param>
    /// <param name="ecCount">Number of error correction codewords to generate.</param>
    /// <returns>Array of error correction codewords.</returns>
    private static byte[] ComputeErrorCorrection(byte[] data, int ecCount)
    {
        var gen = BuildGenerator(ecCount);
        var res = new byte[ecCount];
        foreach (var d in data)
        {
            var factor = (byte)(d ^ res[0]);
            Array.Copy(res, 1, res, 0, ecCount - 1);
            res[ecCount - 1] = 0;

            if (factor != 0)
                for (var i = 0; i < ecCount; i++)
                    res[i] ^= GfMul(gen[i], factor);
        }

        return res;
    }

    /// <summary>
    /// Adds error correction codewords to the data.
    /// </summary>
    /// <param name="data">The data codewords.</param>
    /// <param name="version">The QR version.</param>
    /// <returns>Array of all codewords (data + error correction).</returns>
    private static byte[] AddErrorCorrection(byte[] data, int version)
    {
        var numBlocks = EcBlocks[version];
        var ecPerBlock = EcCodewordsPerBlock[version];
        var rawCodewords = GetNumRawDataModules(version) / 8;
        var numShortBlocks = numBlocks - rawCodewords % numBlocks;
        var shortBlockLength = rawCodewords / numBlocks;

        var blocks = new List<byte[]>();
        var k = 0;

        for (var i = 0; i < numBlocks; i++)
        {
            var dataLength = shortBlockLength - ecPerBlock + (i < numShortBlocks ? 0 : 1);
            var blockData = new byte[dataLength];
            Buffer.BlockCopy(data, k, blockData, 0, dataLength);
            k += dataLength;

            var ecc = ComputeErrorCorrection(blockData, ecPerBlock);
            if (i < numShortBlocks)
                blockData = Combine(blockData, new byte[] { 0 });

            blocks.Add(Combine(blockData, ecc));
        }

        var result = new List<byte>(rawCodewords);
        for (var i = 0; i < blocks[0].Length; i++)
        {
            for (var j = 0; j < blocks.Count; j++)
            {
                if (i == shortBlockLength - ecPerBlock && j < numShortBlocks)
                    continue;

                result.Add(blocks[j][i]);
            }
        }

        return [.. result];
    }

    /// <summary>
    /// Builds the generator polynomial for Reed–Solomon encoding.
    /// </summary>
    /// <param name="degree">The degree of the generator polynomial.</param>
    /// <returns>Array representing the generator polynomial coefficients.</returns>
    private static byte[] BuildGenerator(int degree)
    {
        var gen = new byte[degree];
        gen[degree - 1] = 1;

        for (var i = 0; i < degree; i++)
        {
            var alphaPow = GfExp[i];
            for (var j = 0; j < degree - 1; j++)
                gen[j] = (byte)(gen[j + 1] ^ GfMul(gen[j], alphaPow));
            gen[degree - 1] = GfMul(gen[degree - 1], alphaPow);
        }

        return gen;
    }

    /// <summary>
    /// Combines two byte arrays.
    /// </summary>
    /// <param name="a">The first byte array.</param>
    /// <param name="b">The second byte array.</param>
    /// <returns>The combined byte array.</returns>
    private static byte[] Combine(byte[] a, byte[] b)
    {
        var res = new byte[a.Length + b.Length];
        Buffer.BlockCopy(a, 0, res, 0, a.Length);
        Buffer.BlockCopy(b, 0, res, a.Length, b.Length);
        return res;
    }
    #endregion

    #region Function patterns
    /// <summary>
    /// Places function patterns (finders, timing, alignment, format info) in the QR matrix.
    /// </summary>
    /// <param name="m">The QR code matrix.</param>
    /// <param name="f">The function module tracker.</param>
    /// <param name="version">The QR version.</param>
    private static void PlaceFunctionPatterns(bool[,] m, bool[,] f, int version)
    {
        var size = m.GetLength(0);

        PlaceFinder(m, f, 0, 0);
        PlaceFinder(m, f, size - 7, 0);
        PlaceFinder(m, f, 0, size - 7);

        for (var x = 0; x < size; x++)
        {
            if (f[x, 6]) continue;
            m[x, 6] = (x % 2 == 0);
            f[x, 6] = true;
        }
        for (var y = 0; y < size; y++)
        {
            if (f[6, y]) continue;
            m[6, y] = (y % 2 == 0);
            f[6, y] = true;
        }

        var alignPositions = GetAlignmentPatternPositions(version);
        for (var i = 0; i < alignPositions.Count; i++)
        {
            for (var j = 0; j < alignPositions.Count; j++)
            {
                if ((i == 0 && j == 0) ||
                    (i == 0 && j == alignPositions.Count - 1) ||
                    (i == alignPositions.Count - 1 && j == 0))
                    continue;

                var x = alignPositions[i];
                var y = alignPositions[j];
                if (f[x, y]) continue;
                PlaceAlignment(m, f, x, y);
            }
        }

        m[8, 4 * version + 9] = true;
        f[8, 4 * version + 9] = true;
        ReserveFormatInfo(f, size);

        if (version >= 7)
            ReserveVersionInfo(f, size);
    }

    /// <summary>
    /// Places a finder pattern at the specified position.
    /// </summary>
    /// <param name="m">The QR code matrix.</param>
    /// <param name="f">The function module tracker.</param>
    /// <param name="x0">The x-coordinate of the top-left corner.</param>
    /// <param name="y0">The y-coordinate of the top-left corner.</param>
    private static void PlaceFinder(bool[,] m, bool[,] f, int x0, int y0)
    {
        for (var y = 0; y < 7; y++)
        {
            for (var x = 0; x < 7; x++)
            {
                var dark =
                    x == 0 || x == 6 ||
                    y == 0 || y == 6 ||
                    (x >= 2 && x <= 4 && y >= 2 && y <= 4);

                var xx = x0 + x;
                var yy = y0 + y;

                m[xx, yy] = dark;
                f[xx, yy] = true;
            }
        }

        for (var x = -1; x <= 7; x++)
        {
            SetIfInsideTransparent(m, f, x0 + x, y0 - 1);
            SetIfInsideTransparent(m, f, x0 + x, y0 + 7);
        }
        for (var y = 0; y < 7; y++)
        {
            SetIfInsideTransparent(m, f, x0 - 1, y0 + y);
            SetIfInsideTransparent(m, f, x0 + 7, y0 + y);
        }
    }

    /// <summary>
    /// Places an alignment pattern at the specified center position.
    /// </summary>
    /// <param name="m">The QR code matrix.</param>
    /// <param name="f">The function module tracker.</param>
    /// <param name="cx">The x-coordinate of the center.</param>
    /// <param name="cy">The y-coordinate of the center.</param>
    private static void PlaceAlignment(bool[,] m, bool[,] f, int cx, int cy)
    {
        var size = m.GetLength(0);
        for (var y = -2; y <= 2; y++)
        {
            for (var x = -2; x <= 2; x++)
            {
                var xx = cx + x;
                var yy = cy + y;
                if (xx < 0 || yy < 0 || xx >= size || yy >= size)
                    continue;

                var dark = Math.Max(Math.Abs(x), Math.Abs(y)) == 2 ||
                            (x == 0 && y == 0);

                m[xx, yy] = dark;
                f[xx, yy] = true;
            }
        }
    }

    /// <summary>
    /// Sets the module to transparent if inside bounds.
    /// </summary>
    /// <param name="m">The QR code matrix.</param>
    /// <param name="f">The function module tracker.</param>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    private static void SetIfInsideTransparent(bool[,] m, bool[,] f, int x, int y)
    {
        var size = m.GetLength(0);
        if (x < 0 || y < 0 || x >= size || y >= size)
            return;

        m[x, y] = false;
        f[x, y] = true;
    }

    /// <summary>
    /// Reserves space for format information in the function module tracker.
    /// </summary>
    /// <param name="f">The function module tracker.</param>
    /// <param name="size">The size of the QR code matrix.</param>
    private static void ReserveFormatInfo(bool[,] f, int size)
    {
        for (var i = 0; i <= 8; i++)
        {
            if (i == 6)
                continue;

            f[i, 8] = true;
            f[8, i] = true;
        }

        for (var i = 0; i < 7; i++)
            f[size - 1 - i, 8] = true;

        for (var i = 0; i < 7; i++)
            f[8, size - 1 - i] = true;

        f[8, size - 8] = true;
    }

    /// <summary>
    /// Reserves space for version information in the function module tracker.
    /// </summary>
    /// <param name="f">The function module tracker.</param>
    /// <param name="size">The size of the QR code matrix.</param>
    private static void ReserveVersionInfo(bool[,] f, int size)
    {
        for (var i = 0; i < 6; i++)
        {
            for (var j = 0; j < 3; j++)
            {
                f[i, size - 11 + j] = true;
                f[size - 11 + j, i] = true;
            }
        }
    }

    /// <summary>
    /// Gets the alignment pattern positions for the given version.
    /// </summary>
    /// <param name="version">The QR version.</param>
    /// <returns>List of alignment pattern positions.</returns>
    private static List<int> GetAlignmentPatternPositions(int version)
    {
        if (version == 1)
            return [];

        var size = 4 * version + 17;
        var numAlign = version / 7 + 2;

        var positions = new List<int> { 6 };

        var denom = numAlign * 2 - 2;
        var step = version == 32
            ? 26
            : ((size - 13 + denom - 1) / denom) * 2;

        for (var pos = size - 7; positions.Count < numAlign; pos -= step)
            positions.Insert(1, pos);

        return positions;
    }
    #endregion

    #region Data placement & mask
    /// <summary>
    /// Places data bits into the QR code matrix.
    /// </summary>
    /// <param name="m">The QR code matrix.</param>
    /// <param name="f">The function module tracker.</param>
    /// <param name="codewords">The data and error correction codewords.</param>
    private static void PlaceDataBits(bool[,] m, bool[,] f, byte[] codewords)
    {
        var bitIndex = 0;
        var totalBits = codewords.Length * 8;

        var size = m.GetLength(0);
        var col = size - 1;
        var row = size - 1;
        var goingUp = true;

        while (col > 0)
        {
            if (col == 6) col--;

            for (var i = 0; i < size; i++)
            {
                row = goingUp ? (size - 1 - i) : i;

                for (var c = 0; c < 2; c++)
                {
                    var x = col - c;
                    var y = row;

                    if (f[x, y]) continue;

                    var bit = false;
                    if (bitIndex < totalBits)
                    {
                        var cwIndex = bitIndex / 8;
                        var bitPos = 7 - (bitIndex % 8);
                        bit = ((codewords[cwIndex] >> bitPos) & 1) != 0;
                        bitIndex++;
                    }

                    m[x, y] = bit;
                }
            }

            col -= 2;
            goingUp = !goingUp;
        }
    }

    /// <summary>
    /// Applies the specified mask to the QR code matrix.
    /// </summary>
    /// <param name="m">The QR code matrix.</param>
    /// <param name="f">The QR code matrix.</param>
    /// <param name="maskId">The mask pattern identifier.</param>
    private static void ApplyMask(bool[,] m, bool[,] f, int maskId)
    {
        var size = m.GetLength(0);

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                if (f[x, y])
                    continue;

                var mask = maskId switch
                {
                    0 => ((x + y) % 2) == 0,
                    1 => (y % 2) == 0,
                    2 => (x % 3) == 0,
                    3 => ((x + y) % 3) == 0,
                    4 => ((x / 3 + y / 2) % 2) == 0,
                    5 => (((x * y) % 2) + ((x * y) % 3)) == 0,
                    6 => ((((x * y) % 2) + ((x * y) % 3)) % 2) == 0,
                    7 => ((((x + y) % 2) + ((x * y) % 3)) % 2) == 0,
                    _ => false
                };

                if (mask)
                    m[x, y] = !m[x, y];
            }
        }
    }
    #endregion

    #region Mask penalty calculation
    /// <summary>
    /// Calculates the total penalty score for the given QR code matrix.
    /// </summary>
    /// <param name="m">The QR code matrix.</param>
    /// <returns>The total penalty score.</returns>
    private static int CalculatePenalty(bool[,] m) => PenaltyN1(m) + PenaltyN2(m) + PenaltyN3(m) + PenaltyN4(m);

    /// <summary>
    /// N1: consecutive modules in row/column in same color
    /// </summary>
    /// <param name="m">The QR code matrix.</param>
    /// <returns>The penalty score.</returns>
    private static int PenaltyN1(bool[,] m)
    {
        var size = m.GetLength(0);
        var penalty = 0;

        // Rows
        for (var y = 0; y < size; y++)
        {
            var runColor = m[0, y];
            var runLen = 1;
            for (var x = 1; x < size; x++)
            {
                if (m[x, y] == runColor)
                {
                    runLen++;
                }
                else
                {
                    if (runLen >= 5)
                        penalty += 3 + (runLen - 5);
                    runColor = m[x, y];
                    runLen = 1;
                }
            }
            if (runLen >= 5)
                penalty += 3 + (runLen - 5);
        }

        // Columns
        for (var x = 0; x < size; x++)
        {
            var runColor = m[x, 0];
            var runLen = 1;
            for (var y = 1; y < size; y++)
            {
                if (m[x, y] == runColor)
                {
                    runLen++;
                }
                else
                {
                    if (runLen >= 5)
                        penalty += 3 + (runLen - 5);
                    runColor = m[x, y];
                    runLen = 1;
                }
            }
            if (runLen >= 5)
                penalty += 3 + (runLen - 5);
        }

        return penalty;
    }

    /// <summary>
    /// N2: 2x2 blocks of same-color modules
    /// </summary>
    /// <param name="m">The QR code matrix.</param>
    /// <returns>The penalty score.</returns>
    private static int PenaltyN2(bool[,] m)
    {
        var size = m.GetLength(0);
        var penalty = 0;

        for (var y = 0; y < size - 1; y++)
        {
            for (var x = 0; x < size - 1; x++)
            {
                var c = m[x, y];
                if (m[x + 1, y] == c
                 && m[x, y + 1] == c
                 && m[x + 1, y + 1] == c)
                    penalty += 3;
            }
        }

        return penalty;
    }

    /// <summary>
    /// N3: patterns similar to the finder patterns in rows/columns
    /// </summary>
    /// <param name="m">The QR code matrix.</param>
    /// <returns>The penalty score.</returns>
    private static int PenaltyN3(bool[,] m)
    {
        var size = m.GetLength(0);
        var penalty = 0;

        // Rows
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size - 6; x++)
            {
                bool pattern = m[x + 0, y] &&
                               !m[x + 1, y] &&
                               m[x + 2, y] &&
                               m[x + 3, y] &&
                               m[x + 4, y] &&
                               !m[x + 5, y] &&
                               m[x + 6, y];

                bool patternInv = !m[x + 0, y] &&
                                  m[x + 1, y] &&
                                  !m[x + 2, y] &&
                                  !m[x + 3, y] &&
                                  !m[x + 4, y] &&
                                  m[x + 5, y] &&
                                  !m[x + 6, y];

                if (pattern || patternInv)
                {
                    var leftWhite = x >= 4 &&
                                    !m[x - 1, y] &&
                                    !m[x - 2, y] &&
                                    !m[x - 3, y] &&
                                    !m[x - 4, y];

                    var rightWhite = x + 11 < size &&
                                     !m[x + 7, y] &&
                                     !m[x + 8, y] &&
                                     !m[x + 9, y] &&
                                     !m[x + 10, y];

                    if (leftWhite || rightWhite)
                        penalty += 40;
                }
            }
        }

        // Columns
        for (var x = 0; x < size; x++)
        {
            for (var y = 0; y < size - 6; y++)
            {
                bool pattern = m[x, y + 0] &&
                               !m[x, y + 1] &&
                               m[x, y + 2] &&
                               m[x, y + 3] &&
                               m[x, y + 4] &&
                               !m[x, y + 5] &&
                               m[x, y + 6];

                bool patternInv = !m[x, y + 0] &&
                                  m[x, y + 1] &&
                                  !m[x, y + 2] &&
                                  !m[x, y + 3] &&
                                  !m[x, y + 4] &&
                                  m[x, y + 5] &&
                                  !m[x, y + 6];

                if (pattern || patternInv)
                {
                    var upWhite = y >= 4 &&
                                  !m[x, y - 1] &&
                                  !m[x, y - 2] &&
                                  !m[x, y - 3] &&
                                  !m[x, y - 4];

                    var downWhite = y + 11 < size &&
                                     !m[x, y + 7] &&
                                     !m[x, y + 8] &&
                                     !m[x, y + 9] &&
                                     !m[x, y + 10];

                    if (upWhite || downWhite)
                        penalty += 40;
                }
            }
        }

        return penalty;
    }

    /// <summary>
    /// N4: proportion of dark modules in entire symbol
    /// </summary>
    /// <param name="m">The QR code matrix.</param>
    /// <returns>The penalty score.</returns>
    private static int PenaltyN4(bool[,] m)
    {
        var size = m.GetLength(0);
        var totalModules = size * size;
        var darkCount = 0;

        for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
                if (m[x, y])
                    darkCount++;

        var percent = darkCount * 100.0 / totalModules;
        var k = (int)(Math.Abs(percent - 50) / 5);
        return k * 10;
    }

    #endregion

    #region Format information
    /// <summary>
    /// Adds format information to the QR code matrix.
    /// </summary>
    /// <param name="m">The QR code matrix.</param>
    /// <param name="maskId">The mask pattern identifier.</param>
    /// <param name="version">The QR version.</param>
    private static void AddFormatInformation(bool[,] m, int maskId, int version)
    {
        var size = m.GetLength(0);
        var data = (FormatEcLevel << 3) | (maskId & 0b111);
        var format = ComputeBchFormat(data);
        format ^= 0x5412;

        for (var i = 0; i <= 5; i++)
            m[8, i] = ((format >> i) & 1) != 0;

        m[8, 7] = ((format >> 6) & 1) != 0;
        m[8, 8] = ((format >> 7) & 1) != 0;
        m[7, 8] = ((format >> 8) & 1) != 0;

        for (var i = 9; i <= 14; i++)
        {
            m[14 - i, 8] = ((format >> i) & 1) != 0;
        }

        for (var i = 0; i < 8; i++)
        {
            var bit = ((format >> i) & 1) != 0;
            m[size - 1 - i, 8] = bit;
        }

        for (var i = 8; i < 15; i++)
        {
            var bit = ((format >> i) & 1) != 0;
            m[8, size - 15 + i] = bit;
        }

        m[8, size - 8] = true;

        if (version >= 7)
            AddVersionInformation(m, version);
    }

    /// <summary>
    /// Computes the BCH code for format information.
    /// </summary>
    /// <param name="data">The 5-bit format data.</param>
    /// <returns>The 15-bit format information with BCH code.</returns>
    private static int ComputeBchFormat(int data)
    {
        var g = 0b10100110111;
        var value = data << 10;

        for (var i = 14; i >= 10; i--)
            if (((value >> i) & 1) != 0)
                value ^= g << (i - 10);

        return (data << 10) | (value & 0x3FF);
    }

    /// <summary>
    /// Adds version information to the QR code matrix.
    /// </summary>
    /// <param name="m">The QR code matrix.</param>
    /// <param name="version">The QR version.</param>
    private static void AddVersionInformation(bool[,] m, int version)
    {
        var bits = ComputeBchVersion(version);
        var size = m.GetLength(0);

        for (var i = 0; i < 18; i++)
        {
            var bit = ((bits >> i) & 1) != 0;
            var a = size - 11 + i % 3;
            var b = i / 3;
            m[a, b] = bit;
            m[b, a] = bit;
        }
    }

    /// <summary>
    /// Computes the BCH code for version information.
    /// </summary>
    /// <param name="version">The version number (7 to 40).</param>
    /// <returns>The 18-bit version information with BCH code.</returns>
    private static int ComputeBchVersion(int version)
    {
        var g = 0x1F25;
        var value = version << 12;

        for (var i = 17; i >= 12; i--)
            if (((value >> i) & 1) != 0)
                value ^= g << (i - 12);

        return (version << 12) | (value & 0xFFF);
    }
    #endregion
}
