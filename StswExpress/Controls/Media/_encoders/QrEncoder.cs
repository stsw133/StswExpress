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
    private const int FormatEcLevel = 0b00; // M

    private readonly struct EcInfo
    {
        public int TotalDataCodewords { get; init; }
        public int EcPerBlock { get; init; }
        public int Group1Blocks { get; init; }
        public int Group1Data { get; init; }
        public int Group2Blocks { get; init; }
        public int Group2Data { get; init; }
    }

    private static readonly EcInfo[] EcTable =
    [
        default,
        new EcInfo { TotalDataCodewords =  16, EcPerBlock = 10, Group1Blocks = 1, Group1Data = 16, Group2Blocks = 0, Group2Data = 0 }, // 1-M
        new EcInfo { TotalDataCodewords =  28, EcPerBlock = 16, Group1Blocks = 1, Group1Data = 28, Group2Blocks = 0, Group2Data = 0 }, // 2-M
        new EcInfo { TotalDataCodewords =  44, EcPerBlock = 26, Group1Blocks = 1, Group1Data = 44, Group2Blocks = 0, Group2Data = 0 }, // 3-M
        new EcInfo { TotalDataCodewords =  64, EcPerBlock = 18, Group1Blocks = 2, Group1Data = 32, Group2Blocks = 0, Group2Data = 0 }, // 4-M
        new EcInfo { TotalDataCodewords =  86, EcPerBlock = 24, Group1Blocks = 2, Group1Data = 43, Group2Blocks = 0, Group2Data = 0 }, // 5-M
        new EcInfo { TotalDataCodewords = 108, EcPerBlock = 16, Group1Blocks = 4, Group1Data = 27, Group2Blocks = 0, Group2Data = 0 }, // 6-M
        new EcInfo { TotalDataCodewords = 124, EcPerBlock = 18, Group1Blocks = 4, Group1Data = 31, Group2Blocks = 0, Group2Data = 0 }, // 7-M
        new EcInfo { TotalDataCodewords = 154, EcPerBlock = 22, Group1Blocks = 2, Group1Data = 38, Group2Blocks = 2, Group2Data = 39 }, // 8-M
        new EcInfo { TotalDataCodewords = 182, EcPerBlock = 22, Group1Blocks = 3, Group1Data = 36, Group2Blocks = 2, Group2Data = 37 }, // 9-M
        new EcInfo { TotalDataCodewords = 216, EcPerBlock = 26, Group1Blocks = 4, Group1Data = 43, Group2Blocks = 1, Group2Data = 44 }, // 10-M
        new EcInfo { TotalDataCodewords = 254, EcPerBlock = 30, Group1Blocks = 1, Group1Data = 50, Group2Blocks = 4, Group2Data = 51 }, // 11-M
        new EcInfo { TotalDataCodewords = 290, EcPerBlock = 22, Group1Blocks = 6, Group1Data = 36, Group2Blocks = 2, Group2Data = 37 }, // 12-M
        new EcInfo { TotalDataCodewords = 334, EcPerBlock = 22, Group1Blocks = 8, Group1Data = 37, Group2Blocks = 1, Group2Data = 38 }, // 13-M
        new EcInfo { TotalDataCodewords = 365, EcPerBlock = 24, Group1Blocks = 4, Group1Data = 40, Group2Blocks = 5, Group2Data = 41 }, // 14-M
        new EcInfo { TotalDataCodewords = 415, EcPerBlock = 24, Group1Blocks = 5, Group1Data = 41, Group2Blocks = 5, Group2Data = 42 }, // 15-M
        new EcInfo { TotalDataCodewords = 453, EcPerBlock = 28, Group1Blocks = 7, Group1Data = 45, Group2Blocks = 3, Group2Data = 46 }, // 16-M
        new EcInfo { TotalDataCodewords = 507, EcPerBlock = 28, Group1Blocks = 10, Group1Data = 46, Group2Blocks = 1, Group2Data = 47 }, // 17-M
        new EcInfo { TotalDataCodewords = 563, EcPerBlock = 26, Group1Blocks = 9, Group1Data = 43, Group2Blocks = 4, Group2Data = 44 }, // 18-M
        new EcInfo { TotalDataCodewords = 627, EcPerBlock = 26, Group1Blocks = 3, Group1Data = 44, Group2Blocks = 11, Group2Data = 45 }, // 19-M
        new EcInfo { TotalDataCodewords = 669, EcPerBlock = 26, Group1Blocks = 3, Group1Data = 41, Group2Blocks = 13, Group2Data = 42 }, // 20-M
        new EcInfo { TotalDataCodewords = 714, EcPerBlock = 26, Group1Blocks = 17, Group1Data = 42, Group2Blocks = 0, Group2Data = 0 }, // 21-M
        new EcInfo { TotalDataCodewords = 782, EcPerBlock = 28, Group1Blocks = 17, Group1Data = 46, Group2Blocks = 0, Group2Data = 0 }, // 22-M
        new EcInfo { TotalDataCodewords = 860, EcPerBlock = 28, Group1Blocks = 4, Group1Data = 47, Group2Blocks = 14, Group2Data = 48 }, // 23-M
        new EcInfo { TotalDataCodewords = 914, EcPerBlock = 28, Group1Blocks = 6, Group1Data = 45, Group2Blocks = 14, Group2Data = 46 }, // 24-M
        new EcInfo { TotalDataCodewords = 1000, EcPerBlock = 28, Group1Blocks = 8, Group1Data = 47, Group2Blocks = 13, Group2Data = 48 }, // 25-M
        new EcInfo { TotalDataCodewords = 1062, EcPerBlock = 28, Group1Blocks = 19, Group1Data = 46, Group2Blocks = 4, Group2Data = 47 }, // 26-M
        new EcInfo { TotalDataCodewords = 1128, EcPerBlock = 28, Group1Blocks = 22, Group1Data = 45, Group2Blocks = 3, Group2Data = 46 }, // 27-M
        new EcInfo { TotalDataCodewords = 1193, EcPerBlock = 28, Group1Blocks = 3, Group1Data = 45, Group2Blocks = 23, Group2Data = 46 }, // 28-M
        new EcInfo { TotalDataCodewords = 1267, EcPerBlock = 28, Group1Blocks = 21, Group1Data = 45, Group2Blocks = 7, Group2Data = 46 }, // 29-M
        new EcInfo { TotalDataCodewords = 1373, EcPerBlock = 28, Group1Blocks = 19, Group1Data = 47, Group2Blocks = 10, Group2Data = 48 }, // 30-M
        new EcInfo { TotalDataCodewords = 1455, EcPerBlock = 28, Group1Blocks = 2, Group1Data = 46, Group2Blocks = 29, Group2Data = 47 }, // 31-M
        new EcInfo { TotalDataCodewords = 1541, EcPerBlock = 28, Group1Blocks = 10, Group1Data = 46, Group2Blocks = 23, Group2Data = 47 }, // 32-M
        new EcInfo { TotalDataCodewords = 1631, EcPerBlock = 28, Group1Blocks = 14, Group1Data = 46, Group2Blocks = 21, Group2Data = 47 }, // 33-M
        new EcInfo { TotalDataCodewords = 1725, EcPerBlock = 28, Group1Blocks = 14, Group1Data = 46, Group2Blocks = 23, Group2Data = 47 }, // 34-M
        new EcInfo { TotalDataCodewords = 1812, EcPerBlock = 28, Group1Blocks = 12, Group1Data = 47, Group2Blocks = 26, Group2Data = 48 }, // 35-M
        new EcInfo { TotalDataCodewords = 1914, EcPerBlock = 28, Group1Blocks = 6,  Group1Data = 47, Group2Blocks = 34, Group2Data = 48 }, // 36-M
        new EcInfo { TotalDataCodewords = 1992, EcPerBlock = 28, Group1Blocks = 29, Group1Data = 46, Group2Blocks = 14, Group2Data = 47 }, // 37-M
        new EcInfo { TotalDataCodewords = 2102, EcPerBlock = 28, Group1Blocks = 13, Group1Data = 46, Group2Blocks = 32, Group2Data = 47 }, // 38-M
        new EcInfo { TotalDataCodewords = 2216, EcPerBlock = 28, Group1Blocks = 40, Group1Data = 47, Group2Blocks = 7,  Group2Data = 48 }, // 39-M
        new EcInfo { TotalDataCodewords = 2334, EcPerBlock = 28, Group1Blocks = 18, Group1Data = 47, Group2Blocks = 31, Group2Data = 48 }, // 40-M
    ];

    /// <summary>
    /// Encodes the given text into a QR code matrix (Model 2, ECC M).
    /// </summary>
    /// <param name="text">Text to encode.</param>
    /// <param name="encoding">Optional text encoding (default: ISO-8859-1).</param>
    /// <returns>2D boolean array representing the QR code modules (true = black, false = white).</returns>
    public static bool[,] Encode(string text, Encoding? encoding = null)
    {
        encoding ??= Encoding.GetEncoding("ISO-8859-1");

        var version = SelectVersion(text, encoding);
        var size = 17 + 4 * version;

        var data = EncodeDataToCodewords(text, version, encoding);
        var allCodewords = AddErrorCorrection(data, version);

        var baseModules = new bool[size, size];
        var functionMask = new bool[size, size];

        PlaceFunctionPatterns(baseModules, functionMask, version);

        bool[,] bestModules = null!;
        var bestMaskId = 0;
        var bestPenalty = int.MaxValue;

        for (var maskId = 0; maskId < 8; maskId++)
        {
            var modules = (bool[,])baseModules.Clone();
            PlaceDataBits(modules, functionMask, allCodewords);
            ApplyMask(modules, functionMask, maskId);
            var penalty = CalculatePenalty(modules);

            if (penalty >= bestPenalty)
                continue;

            bestPenalty = penalty;
            bestMaskId = maskId;
            bestModules = modules;
        }

        AddFormatInformation(bestModules, bestMaskId, version);
        return bestModules;
    }

    #region Data encoding
    /// <summary>
    /// Encodes the text into data codewords for the specified QR version and encoding.
    /// </summary>
    /// <param name="text">Text to encode.</param>
    /// <param name="version">QR code version.</param>
    /// <param name="encoding">Text encoding.</param>
    /// <returns>Array of data codewords.</returns>
    /// <exception cref="ArgumentException">Thrown if the text is too long for the specified version.</exception>
    private static byte[] EncodeDataToCodewords(string text, int version, Encoding encoding)
    {
        var bytes = encoding.GetBytes(text);
        var dataCodewords = GetNumDataCodewords(version);
        var capacityBits = dataCodewords * 8;
        var charCountBits = GetCharCountBits(version);
        var neededBits = 4 + charCountBits + bytes.Length * 8;

        if (neededBits > capacityBits)
            throw new ArgumentException("Text too long for selected QR version at ECC M.", nameof(text));

        var bits = new List<int>();
        bits.AddRange([0, 1, 0, 0]);

        for (var i = charCountBits - 1; i >= 0; i--)
            bits.Add(((bytes.Length >> i) & 1) != 0 ? 1 : 0);

        foreach (var b in bytes)
            for (var i = 7; i >= 0; i--)
                bits.Add(((b >> i) & 1) != 0 ? 1 : 0);

        var remaining = capacityBits - bits.Count;
        var terminator = Math.Min(4, Math.Max(0, remaining));
        for (var i = 0; i < terminator; i++)
            bits.Add(0);

        while (bits.Count % 8 != 0)
            bits.Add(0);

        var codewords = new List<byte>();
        for (var i = 0; i < bits.Count; i += 8)
        {
            byte cw = 0;
            for (var j = 0; j < 8; j++)
                cw = (byte)((cw << 1) | bits[i + j]);

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
    /// Selects the minimum QR code version that can accommodate the given text and encoding.
    /// </summary>
    /// <param name="text">Text to encode.</param>
    /// <param name="encoding">Text encoding.</param>
    /// <returns>Selected QR code version.</returns>
    /// <exception cref="ArgumentException">Thrown if the text is too long for the maximum QR code version.</exception>
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
    /// Gets the number of bits used for the character count indicator based on the QR code version.
    /// </summary>
    /// <param name="version">QR code version.</param>
    /// <returns>Number of bits for character count.</returns>
    private static int GetCharCountBits(int version) => version <= 9 ? 8 : 16;

    /// <summary>
    /// Gets the number of data codewords for the specified QR code version.
    /// </summary>
    /// <param name="version">QR code version.</param>
    /// <returns>Number of data codewords.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the version is out of range.</exception>
    private static int GetNumDataCodewords(int version)
    {
        if (version < MinVersion || version > MaxVersion)
            throw new ArgumentOutOfRangeException(nameof(version));

        return EcTable[version].TotalDataCodewords;
    }
    #endregion

    #region Reed–Solomon (GF(256))
    private static readonly byte[] GfExp;
    private static readonly byte[] GfLog;

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
    /// <param name="a">First byte.</param>
    /// <param name="b">Second byte.</param>
    /// <returns>Product in GF(256).</returns>
    private static byte GfMul(byte a, byte b)
    {
        if (a == 0 || b == 0)
            return 0;

        var log = GfLog[a] + GfLog[b];
        return GfExp[log];
    }

    /// <summary>
    /// Multiplies two polynomials over GF(256).
    /// </summary>
    /// <param name="p">First polynomial coefficients.</param>
    /// <param name="q">Second polynomial coefficients.</param>
    /// <returns>Resulting polynomial coefficients.</returns>
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
    /// Builds the generator polynomial for the given degree.
    /// </summary>
    /// <param name="degree">Degree of the generator polynomial.</param>
    /// <returns>Generator polynomial coefficients.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the degree is out of range.</exception>
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
    /// Computes the error correction codewords for the given data and error correction count.
    /// </summary>
    /// <param name="data">Data codewords.</param>
    /// <param name="ecCount">Number of error correction codewords to generate.</param>
    /// <returns>Error correction codewords.</returns>
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
    /// Adds error correction codewords to the given data for the specified QR version.
    /// </summary>
    /// <param name="data">Data codewords.</param>
    /// <param name="version">QR code version.</param>
    /// <returns>Array of codewords including error correction.</returns>
    /// <exception cref="ArgumentException">Thrown if the data length does not match the expected length for the version.</exception>
    private static byte[] AddErrorCorrection(byte[] data, int version)
    {
        var info = EcTable[version];

        var totalData = info.TotalDataCodewords;
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
            for (var b = 0; b < numBlocks; b++)
                if (i < blocksData[b].Length)
                    result.Add(blocksData[b][i]);

        for (var i = 0; i < ecPerBlock; i++)
            for (var b = 0; b < numBlocks; b++)
                result.Add(blocksEcc[b][i]);

        return [.. result];
    }
    #endregion

    #region Function patterns
    /// <summary>
    /// Places the function patterns (finders, alignments, timing, format/version info) into the QR matrix.
    /// </summary>
    /// <param name="m">QR code matrix.</param>
    /// <param name="f">Function pattern mask.</param>
    /// <param name="version">QR code version.</param>
    private static void PlaceFunctionPatterns(bool[,] m, bool[,] f, int version)
    {
        var size = m.GetLength(0);

        PlaceFinder(m, f, 0, 0);           // TL
        PlaceFinder(m, f, 0, size - 7);    // TR
        PlaceFinder(m, f, size - 7, 0);    // BL

        for (var col = 8; col < size - 8; col++)
        {
            m[6, col] = (col % 2) == 0;
            f[6, col] = true;
        }
        for (var row = 8; row < size - 8; row++)
        {
            m[row, 6] = (row % 2) == 0;
            f[row, 6] = true;
        }

        var alignPositions = GetAlignmentPatternPositions(version);
        for (var i = 0; i < alignPositions.Count; i++)
        {
            for (var j = 0; j < alignPositions.Count; j++)
            {
                var r = alignPositions[i];
                var c = alignPositions[j];

                if ((i == 0 && j == 0) ||
                    (i == 0 && j == alignPositions.Count - 1) ||
                    (i == alignPositions.Count - 1 && j == 0))
                    continue;

                if (f[r, c]) continue;
                PlaceAlignment(m, f, r, c);
            }
        }

        var dmRow = 4 * version + 9;
        if (dmRow < size)
        {
            m[dmRow, 8] = true;
            f[dmRow, 8] = true;
        }

        ReserveFormatInfo(f, size);

        if (version >= 7)
            ReserveVersionInfo(f, size);
    }

    /// <summary>
    /// Places a finder pattern at the specified location in the QR matrix.
    /// </summary>
    /// <param name="m">QR code matrix.</param>
    /// <param name="f">Function pattern mask.</param>
    /// <param name="row0">Top row of the finder pattern.</param>
    /// <param name="col0">Left column of the finder pattern.</param>
    private static void PlaceFinder(bool[,] m, bool[,] f, int row0, int col0)
    {
        var size = m.GetLength(0);

        for (var dy = 0; dy < 7; dy++)
        {
            for (var dx = 0; dx < 7; dx++)
            {
                var r = row0 + dy;
                var c = col0 + dx;
                if (r < 0 || c < 0 || r >= size || c >= size)
                    continue;

                var dark =
                    dx == 0 || dx == 6 ||
                    dy == 0 || dy == 6 ||
                    (dx >= 2 && dx <= 4 && dy >= 2 && dy <= 4);

                m[r, c] = dark;
                f[r, c] = true;
            }
        }

        for (var dx = -1; dx <= 7; dx++)
        {
            SetIfInsideTransparent(m, f, row0 - 1, col0 + dx);
            SetIfInsideTransparent(m, f, row0 + 7, col0 + dx);
        }
        for (var dy = 0; dy < 7; dy++)
        {
            SetIfInsideTransparent(m, f, row0 + dy, col0 - 1);
            SetIfInsideTransparent(m, f, row0 + dy, col0 + 7);
        }
    }

    /// <summary>
    /// Places an alignment pattern centered at the specified location in the QR matrix.
    /// </summary>
    /// <param name="m">QR code matrix.</param>
    /// <param name="f">Function pattern mask.</param>
    /// <param name="centerRow">Center row of the alignment pattern.</param>
    /// <param name="centerCol">Center column of the alignment pattern.</param>
    private static void PlaceAlignment(bool[,] m, bool[,] f, int centerRow, int centerCol)
    {
        var size = m.GetLength(0);

        for (var dy = -2; dy <= 2; dy++)
        {
            for (var dx = -2; dx <= 2; dx++)
            {
                var r = centerRow + dy;
                var c = centerCol + dx;
                if (r < 0 || c < 0 || r >= size || c >= size)
                    continue;

                var dark = Math.Max(Math.Abs(dx), Math.Abs(dy)) == 2 ||
                           (dx == 0 && dy == 0);

                m[r, c] = dark;
                f[r, c] = true;
            }
        }
    }

    /// <summary>
    /// Sets the module at (row, col) to transparent if it's inside the matrix bounds.
    /// </summary>
    /// <param name="m">QR code matrix.</param>
    /// <param name="f">Function pattern mask.</param>
    /// <param name="row">Row index.</param>
    /// <param name="col">Column index.</param>
    private static void SetIfInsideTransparent(bool[,] m, bool[,] f, int row, int col)
    {
        var size = m.GetLength(0);
        if (row < 0 || col < 0 || row >= size || col >= size)
            return;

        m[row, col] = false;
        f[row, col] = true;
    }

    /// <summary>
    /// Reserves space for format information in the function pattern mask.
    /// </summary>
    /// <param name="f">Function pattern mask.</param>
    /// <param name="size">Size of the QR matrix.</param>
    private static void ReserveFormatInfo(bool[,] f, int size)
    {
        for (var i = 0; i <= 8; i++)
        {
            if (i == 6) continue;

            f[i, 8] = true;
            f[8, i] = true;
        }

        for (var i = 0; i < 8; i++)
            f[8, size - 1 - i] = true;

        for (var i = 0; i < 7; i++)
            f[size - 1 - i, 8] = true;
    }

    /// <summary>
    /// Reserves space for version information in the function pattern mask (for versions 7 and above).
    /// </summary>
    /// <param name="f">Function pattern mask.</param>
    /// <param name="size">Size of the QR matrix.</param>
    private static void ReserveVersionInfo(bool[,] f, int size)
    {
        for (var r = 0; r < 6; r++)
        {
            for (var c = 0; c < 3; c++)
            {
                f[size - 11 + r, c] = true;
                f[c, size - 11 + r] = true;
            }
        }
    }

    /// <summary>
    /// Gets the positions of alignment patterns for the specified QR code version.
    /// </summary>
    /// <param name="version">QR code version.</param>
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
    /// Places the data bits into the QR matrix according to the QR code data placement rules.
    /// </summary>
    /// <param name="m">QR code matrix.</param>
    /// <param name="f">Function pattern mask.</param>
    /// <param name="codewords">Data codewords.</param>
    private static void PlaceDataBits(bool[,] m, bool[,] f, byte[] codewords)
    {
        var size = m.GetLength(0);
        var totalBits = codewords.Length * 8;
        var bitIndex = 0;

        var col = size - 1;
        var goingUp = true;

        while (col > 0)
        {
            if (col == 6) col--;

            for (var rowIdx = 0; rowIdx < size; rowIdx++)
            {
                var row = goingUp ? (size - 1 - rowIdx) : rowIdx;

                for (var dx = 0; dx < 2; dx++)
                {
                    var c = col - dx;
                    var r = row;

                    if (f[r, c])
                        continue;

                    var bit = false;
                    if (bitIndex < totalBits)
                    {
                        var cwIndex = bitIndex / 8;
                        var bitPos = 7 - (bitIndex % 8);
                        bit = ((codewords[cwIndex] >> bitPos) & 1) != 0;
                        bitIndex++;
                    }

                    m[r, c] = bit;
                }
            }

            col -= 2;
            goingUp = !goingUp;
        }
    }

    /// <summary>
    /// Applies the specified mask pattern to the QR matrix.
    /// </summary>
    /// <param name="m">QR code matrix.</param>
    /// <param name="f">QR code matrix.</param>
    /// <param name="maskId">Mask pattern ID (0-7).</param>
    private static void ApplyMask(bool[,] m, bool[,] f, int maskId)
    {
        var size = m.GetLength(0);

        for (var r = 0; r < size; r++)
        {
            for (var c = 0; c < size; c++)
            {
                if (f[r, c])
                    continue;

                var row = r;
                var col = c;

                var mask = maskId switch
                {
                    0 => (row + col) % 2 == 0,
                    1 => row % 2 == 0,
                    2 => col % 3 == 0,
                    3 => (row + col) % 3 == 0,
                    4 => ((row / 2) + (col / 3)) % 2 == 0,
                    5 => ((row * col) % 2 + (row * col) % 3) == 0,
                    6 => (((row * col) % 2 + (row * col) % 3) % 2) == 0,
                    7 => (((row + col) % 2 + (row * col) % 3) % 2) == 0,
                    _ => false
                };

                if (mask)
                    m[r, c] = !m[r, c];
            }
        }
    }
    #endregion

    #region Mask penalty
    /// <summary>
    /// Calculates the total penalty score for the given QR matrix based on the four penalty rules.
    /// </summary>
    /// <param name="m">QR code matrix.</param>
    /// <returns>Total penalty score.</returns>
    private static int CalculatePenalty(bool[,] m) => PenaltyN1(m) + PenaltyN2(m) + PenaltyN3(m) + PenaltyN4(m);

    /// <summary>
    /// Calculates penalty for consecutive modules in rows and columns (Rule N1).
    /// </summary>
    /// <param name="m">QR code matrix.</param>
    /// <returns>Penalty score for Rule N1.</returns>
    private static int PenaltyN1(bool[,] m)
    {
        var size = m.GetLength(0);
        var penalty = 0;

        for (var r = 0; r < size; r++)
        {
            var runColor = m[r, 0];
            var runLen = 1;

            for (var c = 1; c < size; c++)
            {
                if (m[r, c] == runColor)
                {
                    runLen++;
                }
                else
                {
                    if (runLen >= 5)
                        penalty += 3 + (runLen - 5);

                    runColor = m[r, c];
                    runLen = 1;
                }
            }

            if (runLen >= 5)
                penalty += 3 + (runLen - 5);
        }

        for (var c = 0; c < size; c++)
        {
            var runColor = m[0, c];
            var runLen = 1;

            for (var r = 1; r < size; r++)
            {
                if (m[r, c] == runColor)
                {
                    runLen++;
                }
                else
                {
                    if (runLen >= 5)
                        penalty += 3 + (runLen - 5);

                    runColor = m[r, c];
                    runLen = 1;
                }
            }

            if (runLen >= 5)
                penalty += 3 + (runLen - 5);
        }

        return penalty;
    }

    /// <summary>
    /// Calculates penalty for 2x2 blocks of same color modules (Rule N2).
    /// </summary>
    /// <param name="m">QR code matrix.</param>
    /// <returns>Penalty score for Rule N2.</returns>
    private static int PenaltyN2(bool[,] m)
    {
        var size = m.GetLength(0);
        var penalty = 0;

        for (var r = 0; r < size - 1; r++)
        {
            for (var c = 0; c < size - 1; c++)
            {
                var v = m[r, c];
                if (m[r, c + 1] == v &&
                    m[r + 1, c] == v &&
                    m[r + 1, c + 1] == v)
                    penalty += 3;
            }
        }

        return penalty;
    }

    /// <summary>
    /// Calculates penalty for patterns similar to the finder patterns (Rule N3).
    /// </summary>
    /// <param name="m">QR code matrix.</param>
    /// <returns>Penalty score for Rule N3.</returns>
    private static int PenaltyN3(bool[,] m)
    {
        var size = m.GetLength(0);
        var penalty = 0;

        static bool EqualsPattern(bool[] arr, int start, bool[] patt)
        {
            for (var i = 0; i < patt.Length; i++)
                if (arr[start + i] != patt[i])
                    return false;
            return true;
        }

        var patt = new[] { true, false, true, true, true, false, true };
        var pattInv = new[] { false, true, false, false, false, true, false };

        for (var r = 0; r < size; r++)
        {
            var row = new bool[size];
            for (var c = 0; c < size; c++)
                row[c] = m[r, c];

            for (var c = 0; c <= size - 7; c++)
            {
                var isPatt = EqualsPattern(row, c, patt) || EqualsPattern(row, c, pattInv);
                if (!isPatt) continue;

                var leftWhite = c >= 4 &&
                                !row[c - 1] && !row[c - 2] && !row[c - 3] && !row[c - 4];

                var rightWhite = c + 11 <= size &&
                                 !row[c + 7] && !row[c + 8] && !row[c + 9] && !row[c + 10];

                if (leftWhite || rightWhite)
                    penalty += 40;
            }
        }

        for (var c = 0; c < size; c++)
        {
            var col = new bool[size];
            for (var r = 0; r < size; r++)
                col[r] = m[r, c];

            for (var r = 0; r <= size - 7; r++)
            {
                var isPatt = EqualsPattern(col, r, patt) || EqualsPattern(col, r, pattInv);
                if (!isPatt) continue;

                var upWhite = r >= 4 &&
                              !col[r - 1] && !col[r - 2] && !col[r - 3] && !col[r - 4];

                var downWhite = r + 11 <= size &&
                                !col[r + 7] && !col[r + 8] && !col[r + 9] && !col[r + 10];

                if (upWhite || downWhite)
                    penalty += 40;
            }
        }

        return penalty;
    }

    /// <summary>
    /// Calculates penalty based on the proportion of dark modules (Rule N4).
    /// </summary>
    /// <param name="m">QR code matrix.</param>
    /// <returns>Penalty score for Rule N4.</returns>
    private static int PenaltyN4(bool[,] m)
    {
        var size = m.GetLength(0);
        var totalModules = size * size;
        var darkCount = 0;

        for (var r = 0; r < size; r++)
            for (var c = 0; c < size; c++)
                if (m[r, c])
                    darkCount++;

        var percent = darkCount * 100.0 / totalModules;
        var k = (int)(Math.Abs(percent - 50) / 5);
        return k * 10;
    }
    #endregion

    #region Format information
    /// <summary>
    /// Adds format information to the QR matrix.
    /// </summary>
    /// <param name="m">QR code matrix.</param>
    /// <param name="maskId">Mask pattern ID (0-7).</param>
    /// <param name="version">QR code version.</param>
    private static void AddFormatInformation(bool[,] m, int maskId, int version)
    {
        var size = m.GetLength(0);
        var data = (FormatEcLevel << 3) | (maskId & 0b111);

        var format = ComputeBchFormat(data) ^ 0x5412; // mask 101010000010010

        for (var i = 0; i <= 5; i++)
            m[i, 8] = ((format >> i) & 1) != 0;

        m[7, 8] = ((format >> 6) & 1) != 0;
        m[8, 8] = ((format >> 7) & 1) != 0;
        m[8, 7] = ((format >> 8) & 1) != 0;

        for (var i = 9; i <= 14; i++)
            m[8, 14 - i] = ((format >> i) & 1) != 0;

        for (var i = 0; i < 8; i++)
        {
            var bit = ((format >> i) & 1) != 0;
            m[8, size - 1 - i] = bit;
        }

        for (var i = 8; i < 15; i++)
        {
            var bit = ((format >> i) & 1) != 0;
            m[size - 15 + i, 8] = bit;
        }

        if (version >= 7)
            AddVersionInformation(m, version);
    }

    /// <summary>
    /// Computes the BCH code for the format information.
    /// </summary>
    /// <param name="data">Format data (5 bits for error correction level and 3 bits for mask ID).</param>
    /// <returns>BCH code (15 bits).</returns>
    private static int ComputeBchFormat(int data)
    {
        const int g = 0b10100110111;
        var value = data << 10;

        for (var i = 14; i >= 10; i--)
            if (((value >> i) & 1) != 0)
                value ^= g << (i - 10);

        return (data << 10) | (value & 0x3FF);
    }

    /// <summary>
    /// Adds version information to the QR matrix for versions 7 and above.
    /// </summary>
    /// <param name="m">QR code matrix.</param>
    /// <param name="version">QR code version.</param>
    private static void AddVersionInformation(bool[,] m, int version)
    {
        var bits = ComputeBchVersion(version);
        var size = m.GetLength(0);

        for (var i = 0; i < 18; i++)
        {
            var bit = ((bits >> i) & 1) != 0;
            var r = size - 11 + i / 3;
            var c = i % 3;
            m[r, c] = bit;
            m[c, r] = bit;
        }
    }

    /// <summary>
    /// Computes the BCH code for the version information.
    /// </summary>
    /// <param name="version">QR code version (7-40).</param>
    /// <returns>BCH code (18 bits).</returns>
    private static int ComputeBchVersion(int version)
    {
        const int g = 0x1F25;
        var value = version << 12;

        for (var i = 17; i >= 12; i--)
            if (((value >> i) & 1) != 0)
                value ^= g << (i - 12);

        return (version << 12) | (value & 0xFFF);
    }
    #endregion
}
