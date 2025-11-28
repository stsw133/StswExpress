using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StswExpress;

/// <summary>
/// Simple QR Code encoder for Model 2 QR codes (ECC M) with automatic version selection.
/// </summary>
public static class QrEncoder
{
    private const int MinVersion = 1;
    private const int MaxVersion = 40;
    private const int FormatEcLevel = 0b00;

    private readonly struct EcInfo
    {
        public int EcPerBlock { get; init; }
        public int Group1Blocks { get; init; }
        public int Group1Data { get; init; }
        public int Group2Blocks { get; init; }
        public int Group2Data { get; init; }

        public int TotalDataBytes =>
            Group1Blocks * Group1Data +
            Group2Blocks * Group2Data;
    }

    private static readonly EcInfo[] EcTable =
    [
        default,
        new EcInfo { EcPerBlock = 10, Group1Blocks = 1, Group1Data = 16, Group2Blocks = 0, Group2Data = 0 }, // version 1
        new EcInfo { EcPerBlock = 16, Group1Blocks = 1, Group1Data = 28, Group2Blocks = 0, Group2Data = 0 }, // 2
        new EcInfo { EcPerBlock = 26, Group1Blocks = 1, Group1Data = 44, Group2Blocks = 0, Group2Data = 0 }, // 3
        new EcInfo { EcPerBlock = 18, Group1Blocks = 2, Group1Data = 32, Group2Blocks = 0, Group2Data = 0 }, // 4
        new EcInfo { EcPerBlock = 24, Group1Blocks = 2, Group1Data = 43, Group2Blocks = 0, Group2Data = 0 }, // 5
        new EcInfo { EcPerBlock = 16, Group1Blocks = 4, Group1Data = 27, Group2Blocks = 0, Group2Data = 0 }, // 6
        new EcInfo { EcPerBlock = 18, Group1Blocks = 4, Group1Data = 31, Group2Blocks = 0, Group2Data = 0 }, // 7
        new EcInfo { EcPerBlock = 22, Group1Blocks = 2, Group1Data = 38, Group2Blocks = 2, Group2Data = 39 }, // 8
        new EcInfo { EcPerBlock = 22, Group1Blocks = 3, Group1Data = 36, Group2Blocks = 2, Group2Data = 37 }, // 9
        new EcInfo { EcPerBlock = 26, Group1Blocks = 4, Group1Data = 43, Group2Blocks = 1, Group2Data = 44 }, // 10
        new EcInfo { EcPerBlock = 30, Group1Blocks = 1, Group1Data = 50, Group2Blocks = 4, Group2Data = 51 }, // 11
        new EcInfo { EcPerBlock = 22, Group1Blocks = 6, Group1Data = 36, Group2Blocks = 2, Group2Data = 37 }, // 12
        new EcInfo { EcPerBlock = 22, Group1Blocks = 8, Group1Data = 37, Group2Blocks = 1, Group2Data = 38 }, // 13
        new EcInfo { EcPerBlock = 24, Group1Blocks = 4, Group1Data = 40, Group2Blocks = 5, Group2Data = 41 }, // 14
        new EcInfo { EcPerBlock = 24, Group1Blocks = 5, Group1Data = 41, Group2Blocks = 5, Group2Data = 42 }, // 15
        new EcInfo { EcPerBlock = 28, Group1Blocks = 7, Group1Data = 45, Group2Blocks = 3, Group2Data = 46 }, // 16
        new EcInfo { EcPerBlock = 28, Group1Blocks = 10, Group1Data = 46, Group2Blocks = 1, Group2Data = 47 }, // 17
        new EcInfo { EcPerBlock = 26, Group1Blocks = 9, Group1Data = 43, Group2Blocks = 4, Group2Data = 44 }, // 18
        new EcInfo { EcPerBlock = 26, Group1Blocks = 3, Group1Data = 44, Group2Blocks = 11, Group2Data = 45 }, // 19
        new EcInfo { EcPerBlock = 26, Group1Blocks = 3, Group1Data = 41, Group2Blocks = 13, Group2Data = 42 }, // 20
        new EcInfo { EcPerBlock = 26, Group1Blocks = 17, Group1Data = 42, Group2Blocks = 0, Group2Data = 0 }, // 21
        new EcInfo { EcPerBlock = 28, Group1Blocks = 17, Group1Data = 46, Group2Blocks = 0, Group2Data = 0 }, // 22
        new EcInfo { EcPerBlock = 28, Group1Blocks = 4, Group1Data = 47, Group2Blocks = 14, Group2Data = 48 }, // 23
        new EcInfo { EcPerBlock = 28, Group1Blocks = 6, Group1Data = 45, Group2Blocks = 14, Group2Data = 46 }, // 24
        new EcInfo { EcPerBlock = 28, Group1Blocks = 8, Group1Data = 47, Group2Blocks = 13, Group2Data = 48 }, // 25
        new EcInfo { EcPerBlock = 28, Group1Blocks = 19, Group1Data = 46, Group2Blocks = 4, Group2Data = 47 }, // 26
        new EcInfo { EcPerBlock = 28, Group1Blocks = 22, Group1Data = 45, Group2Blocks = 3, Group2Data = 46 }, // 27
        new EcInfo { EcPerBlock = 28, Group1Blocks = 3, Group1Data = 45, Group2Blocks = 23, Group2Data = 46 }, // 28
        new EcInfo { EcPerBlock = 28, Group1Blocks = 21, Group1Data = 45, Group2Blocks = 7, Group2Data = 46 }, // 29
        new EcInfo { EcPerBlock = 28, Group1Blocks = 19, Group1Data = 47, Group2Blocks = 10, Group2Data = 48 }, // 30
        new EcInfo { EcPerBlock = 28, Group1Blocks = 2, Group1Data = 46, Group2Blocks = 29, Group2Data = 47 }, // 31
        new EcInfo { EcPerBlock = 28, Group1Blocks = 10, Group1Data = 46, Group2Blocks = 23, Group2Data = 47 }, // 32
        new EcInfo { EcPerBlock = 28, Group1Blocks = 14, Group1Data = 46, Group2Blocks = 21, Group2Data = 47 }, // 33
        new EcInfo { EcPerBlock = 28, Group1Blocks = 14, Group1Data = 46, Group2Blocks = 23, Group2Data = 47 }, // 34
        new EcInfo { EcPerBlock = 28, Group1Blocks = 12, Group1Data = 47, Group2Blocks = 26, Group2Data = 48 }, // 35
        new EcInfo { EcPerBlock = 28, Group1Blocks = 6,  Group1Data = 47, Group2Blocks = 34, Group2Data = 48 }, // 36
        new EcInfo { EcPerBlock = 28, Group1Blocks = 29, Group1Data = 46, Group2Blocks = 14, Group2Data = 47 }, // 37
        new EcInfo { EcPerBlock = 28, Group1Blocks = 13, Group1Data = 46, Group2Blocks = 32, Group2Data = 47 }, // 38
        new EcInfo { EcPerBlock = 28, Group1Blocks = 40, Group1Data = 47, Group2Blocks = 7,  Group2Data = 48 }, // 39
        new EcInfo { EcPerBlock = 28, Group1Blocks = 18, Group1Data = 47, Group2Blocks = 31, Group2Data = 48 }, // 40
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
        var dataCodewords = EcTable[version].TotalDataBytes;
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
        if (version < MinVersion || version > MaxVersion)
            throw new ArgumentOutOfRangeException(nameof(version));

        return EcTable[version].TotalDataBytes;
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
        if (a == 0 || b == 0)
            return 0;

        var log = GfLog[a] + GfLog[b];
        return GfExp[log];
    }

    /// <summary>
    /// Multiplies two polynomials in GF(256).
    /// <param name="p">The first polynomial coefficients.</param>
    /// <param name="q">The second polynomial coefficients.</param>
    /// </summary>
    private static byte[] PolyMultiply(byte[] p, byte[] q)
    {
        var res = new byte[p.Length + q.Length - 1];

        for (var i = 0; i < p.Length; i++)
        {
            var a = p[i];
            if (a == 0) continue;

            for (var j = 0; j < q.Length; j++)
            {
                var b = q[j];
                if (b == 0) continue;

                res[i + j] ^= GfMul(a, b);
            }
        }

        return res;
    }

    /// <summary>
    /// Builds the generator polynomial for Reed–Solomon encoding.
    /// </summary>
    /// <param name="degree">The degree of the generator polynomial.</param>
    /// <returns>Array representing the generator polynomial coefficients.</returns>
    private static byte[] BuildGenerator(int degree)
    {
        if (degree < 1 || degree > 255)
            throw new ArgumentOutOfRangeException(nameof(degree));

        var gen = new byte[] { 1 };
        for (var i = 0; i < degree; i++)
        {
            var factor = new[] { (byte)1, GfExp[i] };
            gen = PolyMultiply(gen, factor);
        }

        return gen;
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

        var msg = new byte[data.Length + ecCount];
        Buffer.BlockCopy(data, 0, msg, 0, data.Length);

        for (var i = 0; i < data.Length; i++)
        {
            var coef = msg[i];
            if (coef == 0) continue;

            for (var j = 0; j < gen.Length; j++)
                msg[i + j] ^= GfMul(coef, gen[j]);
        }

        var ecc = new byte[ecCount];
        Buffer.BlockCopy(msg, data.Length, ecc, 0, ecCount);
        return ecc;
    }

    /// <summary>
    /// Adds error correction codewords to the data.
    /// </summary>
    /// <param name="data">The data codewords.</param>
    /// <param name="version">The QR version.</param>
    /// <returns>Array of all codewords (data + error correction).</returns>
    private static byte[] AddErrorCorrection(byte[] data, int version)
    {
        var info = EcTable[version];

        var totalData = info.TotalDataBytes;
        var ecPerBlock = info.EcPerBlock;
        var numBlocks = info.Group1Blocks + info.Group2Blocks;

        if (data.Length != totalData)
            throw new ArgumentException(
                $"Expected {totalData} data codewords for version {version}-M, got {data.Length}.",
                nameof(data));

        var blocksData = new List<byte[]>(numBlocks);
        var blocksEcc = new List<byte[]>(numBlocks);

        var offset = 0;

        for (var i = 0; i < info.Group1Blocks; i++)
        {
            var len = info.Group1Data;
            var block = new byte[len];
            Buffer.BlockCopy(data, offset, block, 0, len);
            offset += len;

            blocksData.Add(block);
            blocksEcc.Add(ComputeErrorCorrection(block, ecPerBlock));
        }

        for (var i = 0; i < info.Group2Blocks; i++)
        {
            var len = info.Group2Data;
            var block = new byte[len];
            Buffer.BlockCopy(data, offset, block, 0, len);
            offset += len;

            blocksData.Add(block);
            blocksEcc.Add(ComputeErrorCorrection(block, ecPerBlock));
        }

        var maxDataLen = 0;
        foreach (var b in blocksData)
            if (b.Length > maxDataLen)
                maxDataLen = b.Length;

        var result = new List<byte>(totalData + numBlocks * ecPerBlock);

        for (var i = 0; i < maxDataLen; i++)
        {
            for (var b = 0; b < numBlocks; b++)
            {
                var block = blocksData[b];
                if (i < block.Length)
                    result.Add(block[i]);
            }
        }

        for (var i = 0; i < ecPerBlock; i++)
        {
            for (var b = 0; b < numBlocks; b++)
            {
                result.Add(blocksEcc[b][i]);
            }
        }

        return [.. result];
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

#if DEBUG
    /// <summary>
    /// Generates a detailed debug output of the QR code encoding process.
    /// </summary>
    public static string DebugEncode(string text, Encoding? encoding = null)
    {
        encoding ??= Encoding.GetEncoding("ISO-8859-1");
        var bytes = encoding.GetBytes(text);

        var sb = new StringBuilder();
        sb.AppendLine($"TEXT: \"{text}\"");
        sb.AppendLine($"BYTES ({bytes.Length}): {string.Join(" ", bytes.Select(b => b.ToString("X2")))}");

        var version = SelectVersion(text, encoding);
        var size = 17 + 4 * version;
        sb.AppendLine($"VERSION: {version}");
        sb.AppendLine($"SIZE: {size}x{size}");

        var ecInfo = EcTable[version];
        var totalDataBytes = ecInfo.Group1Blocks * ecInfo.Group1Data + ecInfo.Group2Blocks * ecInfo.Group2Data;
        sb.AppendLine($"EC INFO: EcPerBlock={ecInfo.EcPerBlock}, " +
                      $"G1Blocks={ecInfo.Group1Blocks}, G1Data={ecInfo.Group1Data}, " +
                      $"G2Blocks={ecInfo.Group2Blocks}, G2Data={ecInfo.Group2Data}");
        sb.AppendLine($"TOTAL DATA BYTES (wg tabeli EC): {totalDataBytes}");

        // 1) data bytes
        var dataBytes = EncodeDataToCodewords(text, version, encoding);
        sb.AppendLine($"ENCODED DATA BYTES (Length={dataBytes.Length}):");
        sb.AppendLine(string.Join(" ", dataBytes.Select(b => b.ToString("X2"))));

        // 2) error correction bytes
        var eccBytes = ComputeErrorCorrection(dataBytes, ecInfo.EcPerBlock);
        sb.AppendLine($"ECC BYTES (Length={eccBytes.Length}):");
        sb.AppendLine(string.Join(" ", eccBytes.Select(b => b.ToString("X2"))));

        // 3) all codewords (data + ecc)
        var allCodewords = AddErrorCorrection(dataBytes, version);
        sb.AppendLine($"ALL CODEWORDS (data+ecc, Length={allCodewords.Length}):");
        sb.AppendLine(string.Join(" ", allCodewords.Select(b => b.ToString("X2"))));

        // 4) place data into matrix
        var modules = new bool[size, size];
        var isFunction = new bool[size, size];
        PlaceFunctionPatterns(modules, isFunction, version);
        PlaceDataBits(modules, isFunction, allCodewords);

        sb.AppendLine();
        sb.AppendLine("MATRIX BEFORE MASK (D = dark, . = light):");
        sb.AppendLine(DumpMatrix(modules));

        // 5) try all masks and select the best one
        var baseMatrix = (bool[,])modules.Clone();
        var bestMask = 0;
        var bestPenalty = int.MaxValue;

        for (var maskId = 0; maskId < 8; maskId++)
        {
            var candidate = (bool[,])baseMatrix.Clone();
            ApplyMask(candidate, isFunction, maskId);
            var penalty = CalculatePenalty(candidate);
            sb.AppendLine($"MASK {maskId}: penalty = {penalty}");
            if (penalty < bestPenalty)
            {
                bestPenalty = penalty;
                bestMask = maskId;
            }
        }

        sb.AppendLine($"BEST MASK ID: {bestMask}");
        var bestMatrix = (bool[,])baseMatrix.Clone();
        ApplyMask(bestMatrix, isFunction, bestMask);
        AddFormatInformation(bestMatrix, bestMask, version);

        sb.AppendLine();
        sb.AppendLine("FINAL MATRIX (after mask + format info):");
        sb.AppendLine(DumpMatrix(bestMatrix));

        return sb.ToString();
    }

    /// <summary>
    /// Prosty tekstowy dump macierzy QR: 'D' = ciemny, '.' = jasny.
    /// </summary>
    private static string DumpMatrix(bool[,] m)
    {
        var size = m.GetLength(0);
        var sb = new StringBuilder(size * (size + 2));

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
                sb.Append(m[x, y] ? 'D' : '.');
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public static void SaveTestHello()
    {
        var matrix = QrEncoder.Encode("HELLO");

        int modules = matrix.GetLength(0);
        int moduleSize = 10;
        int quiet = 4;
        int size = (modules + 2 * quiet) * moduleSize;

        var bmp = new WriteableBitmap(size, size, 96, 96, PixelFormats.Pbgra32, null);
        var dark = Colors.Black;
        var light = Colors.White;

        bmp.Lock();
        unsafe
        {
            byte* buffer = (byte*)bmp.BackBuffer;
            int stride = bmp.BackBufferStride;

            for (int y = 0; y < size; y++)
            {
                byte* row = buffer + y * stride;
                int my = y / moduleSize - quiet;

                for (int x = 0; x < size; x++)
                {
                    int mx = x / moduleSize - quiet;
                    bool isDark =
                        mx >= 0 && mx < modules &&
                        my >= 0 && my < modules &&
                        matrix[mx, my]; // znów: [mx, my] !!!

                    var c = isDark ? dark : light;
                    int idx = x * 4;
                    row[idx + 0] = c.B;
                    row[idx + 1] = c.G;
                    row[idx + 2] = c.R;
                    row[idx + 3] = c.A;
                }
            }
        }
        bmp.AddDirtyRect(new Int32Rect(0, 0, size, size));
        bmp.Unlock();

        using var fs = File.Create("qr_hello.png");
        var enc = new PngBitmapEncoder();
        enc.Frames.Add(BitmapFrame.Create(bmp));
        enc.Save(fs);
    }
#endif
}
