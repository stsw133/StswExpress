using System;
using System.Collections.Generic;
using System.Text;

namespace StswExpress;

/// <summary>
/// Simple QR Code encoder for fixed Version 2-L.
/// </summary>
public static class QrEncoder
{
    private const int Version = 2;
    private const int Size = 25;
    private const int DataCodewords = 34;
    private const int EcCodewords = 10;
    private const int TotalCodewords = DataCodewords + EcCodewords;

    /// <summary>
    /// Encodes the given text into a QR code matrix (Version 2-L).
    /// </summary>
    /// <param name="text">The text to encode (max ~32 bytes).</param>
    /// <returns>2D boolean array representing the QR code modules (true = black, false = white).</returns>
    public static bool[,] Encode(string text)
    {
        var data = EncodeDataToCodewords(text);
        var ec = ComputeErrorCorrection(data, EcCodewords);
        var allCodewords = new byte[TotalCodewords];
        Buffer.BlockCopy(data, 0, allCodewords, 0, data.Length);
        Buffer.BlockCopy(ec, 0, allCodewords, data.Length, ec.Length);

        var modules = new bool[Size, Size];
        var isFunction = new bool[Size, Size];

        PlaceFunctionPatterns(modules, isFunction);
        PlaceDataBits(modules, isFunction, allCodewords);

        var maskId = 0;
        ApplyMask(modules, isFunction, maskId);
        AddFormatInformation(modules, maskId);

        return modules;
    }

    #region Data encoding
    /// <summary>
    /// Encodes the input text into data codewords using Byte mode.
    /// </summary>
    /// <param name="text">The input text to encode.</param>
    /// <returns>Array of data codewords.</returns>
    /// <exception cref="ArgumentException">Thrown if the text is too long for Version 2-L.</exception>
    private static byte[] EncodeDataToCodewords(string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        if (bytes.Length > (DataCodewords - 2))
            throw new ArgumentException("Text too long for fixed Version 2-L encoder.");

        var bits = new List<int>();
        bits.AddRange([0, 1, 0, 0]);

        for (var i = 7; i >= 0; i--)
            bits.Add(((bytes.Length >> i) & 1) != 0 ? 1 : 0);

        foreach (var b in bytes)
            for (var i = 7; i >= 0; i--)
                bits.Add(((b >> i) & 1) != 0 ? 1 : 0);

        var maxBits = DataCodewords * 8;
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
        while (codewords.Count < DataCodewords)
        {
            codewords.Add(pad[padIndex % 2]);
            padIndex++;
        }

        return [.. codewords];
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
    #endregion

    #region Function patterns
    /// <summary>
    /// Places function patterns (finders, timing, alignment, format info) in the QR matrix.
    /// </summary>
    /// <param name="m">The QR code matrix.</param>
    /// <param name="f">The function module tracker.</param>
    private static void PlaceFunctionPatterns(bool[,] m, bool[,] f)
    {
        PlaceFinder(m, f, 0, 0);
        PlaceFinder(m, f, Size - 7, 0);
        PlaceFinder(m, f, 0, Size - 7);

        for (var x = 0; x < Size; x++)
        {
            if (f[x, 6]) continue;
            m[x, 6] = (x % 2 == 0);
            f[x, 6] = true;
        }
        for (var y = 0; y < Size; y++)
        {
            if (f[6, y]) continue;
            m[6, y] = (y % 2 == 0);
            f[6, y] = true;
        }

        PlaceAlignment(m, f, 18, 18);
        m[8, 4 * Version + 9] = true;
        f[8, 4 * Version + 9] = true;
        ReserveFormatInfo(f);
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
        for (var y = -2; y <= 2; y++)
        {
            for (var x = -2; x <= 2; x++)
            {
                var xx = cx + x;
                var yy = cy + y;
                if (xx < 0 || yy < 0 || xx >= Size || yy >= Size)
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
        if (x < 0 || y < 0 || x >= Size || y >= Size)
            return;
        m[x, y] = false;
        f[x, y] = true;
    }

    /// <summary>
    /// Reserves space for format information in the function module tracker.
    /// </summary>
    /// <param name="f">The function module tracker.</param>
    private static void ReserveFormatInfo(bool[,] f)
    {
        for (var i = 0; i <= 8; i++)
        {
            if (i == 6)
                continue;

            f[i, 8] = true;
            f[8, i] = true;
        }

        for (var i = 0; i < 7; i++)
            f[Size - 1 - i, 8] = true;

        for (var i = 0; i < 7; i++)
            f[8, Size - 1 - i] = true;

        f[8, Size - 8] = true;
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
        int bitIndex = 0;
        int totalBits = codewords.Length * 8;

        var col = Size - 1;
        var row = Size - 1;
        var goingUp = true;

        while (col > 0)
        {
            if (col == 6) col--;

            for (var i = 0; i < Size; i++)
            {
                row = goingUp ? (Size - 1 - i) : i;

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
        for (var y = 0; y < Size; y++)
        {
            for (var x = 0; x < Size; x++)
            {
                if (f[x, y])
                    continue;

                var mask = maskId switch
                {
                    0 => ((x + y) % 2) == 0,
                    _ => false
                };

                if (mask)
                    m[x, y] = !m[x, y];
            }
        }
    }
    #endregion

    #region Format information
    /// <summary>
    /// Adds format information to the QR code matrix.
    /// </summary>
    /// <param name="m">The QR code matrix.</param>
    /// <param name="maskId">The mask pattern identifier.</param>
    private static void AddFormatInformation(bool[,] m, int maskId)
    {
        var ecLevel = 0b01;
        var data = (ecLevel << 3) | (maskId & 0b111);
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
            m[Size - 1 - i, 8] = bit;
        }

        for (var i = 8; i < 15; i++)
        {
            var bit = ((format >> i) & 1) != 0;
            m[8, Size - 15 + i] = bit;
        }

        m[8, Size - 8] = true;
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
    #endregion
}
