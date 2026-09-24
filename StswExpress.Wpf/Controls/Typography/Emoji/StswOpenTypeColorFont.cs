using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;

namespace StswExpress.Wpf;

/// <summary>
/// Reads the OpenType COLR/CPAL tables used by color vector fonts.
/// </summary>
/// <remarks>
/// This first implementation intentionally supports the COLR v0 layer model. COLR v1 fonts may still be
/// rendered when they contain the backwards-compatible v0 records, which is the case for Segoe UI Emoji on
/// current Windows versions. Unsupported or malformed font data simply results in a normal monochrome fallback.
/// </remarks>
internal sealed class StswOpenTypeColorFont
{
    private readonly Dictionary<ushort, ColorLayer[]> _layersByGlyph;
    private readonly Color[] _palette;

    private StswOpenTypeColorFont(Dictionary<ushort, ColorLayer[]> layersByGlyph, Color[] palette)
    {
        _layersByGlyph = layersByGlyph;
        _palette = palette;
    }

    public bool TryGetLayers(ushort glyphIndex, out ColorLayer[] layers)
        => _layersByGlyph.TryGetValue(glyphIndex, out layers!);

    public Brush GetBrush(ushort paletteIndex, Brush foreground)
    {
        // 0xFFFF is the OpenType sentinel meaning "use the current foreground color".
        if (paletteIndex == ushort.MaxValue || paletteIndex >= _palette.Length)
            return foreground;

        var brush = new SolidColorBrush(_palette[paletteIndex]);
        brush.Freeze();
        return brush;
    }

    public static StswOpenTypeColorFont? TryCreate(GlyphTypeface glyphTypeface)
    {
        try
        {
            using Stream fontStream = glyphTypeface.GetFontStream();
            using var memory = new MemoryStream();
            fontStream.CopyTo(memory);
            return TryParse(memory.ToArray());
        }
        catch
        {
            return null;
        }
    }

    private static StswOpenTypeColorFont? TryParse(byte[] data)
    {
        if (data.Length < 12)
            return null;

        // A TTC needs face-directory resolution. Segoe UI Emoji is a standalone TTF, so keep v1 focused and
        // gracefully fall back rather than guessing a face in a collection.
        if (ReadTag(data, 0) == "ttcf")
            return null;

        ushort tableCount = ReadUInt16(data, 4);
        var directoryLength = 12L + tableCount * 16L;
        if (directoryLength > data.Length)
            return null;

        TableRecord? colr = null;
        TableRecord? cpal = null;

        for (var i = 0; i < tableCount; i++)
        {
            var recordOffset = 12 + i * 16;
            var tag = ReadTag(data, recordOffset);
            var offset = ReadUInt32(data, recordOffset + 8);
            var length = ReadUInt32(data, recordOffset + 12);

            if ((long)offset > data.Length || (long)length > data.Length || (long)offset + length > data.Length)
                continue;

            var record = new TableRecord((int)offset, (int)length);
            if (tag == "COLR")
                colr = record;
            else if (tag == "CPAL")
                cpal = record;
        }

        if (colr is null || cpal is null)
            return null;

        Color[]? palette = ParseFirstPalette(data, cpal.Value);
        Dictionary<ushort, ColorLayer[]>? layers = ParseColrV0Layers(data, colr.Value);

        if (palette is null || layers is null || layers.Count == 0)
            return null;

        return new StswOpenTypeColorFont(layers, palette);
    }

    private static Dictionary<ushort, ColorLayer[]>? ParseColrV0Layers(byte[] data, TableRecord table)
    {
        // COLR v0 header (also the backwards-compatible prefix of COLR v1):
        // version, numBaseGlyphRecords, baseGlyphRecordsOffset, layerRecordsOffset, numLayerRecords.
        if (!Contains(table, 0, 14))
            return null;

        var tableStart = table.Offset;
        ushort version = ReadUInt16(data, tableStart);
        if (version > 1)
            return null;

        ushort baseGlyphCount = ReadUInt16(data, tableStart + 2);
        uint baseGlyphRecordsOffset = ReadUInt32(data, tableStart + 4);
        uint layerRecordsOffset = ReadUInt32(data, tableStart + 8);
        ushort layerCount = ReadUInt16(data, tableStart + 12);

        if (!Contains(table, baseGlyphRecordsOffset, baseGlyphCount * 6L) ||
            !Contains(table, layerRecordsOffset, layerCount * 4L))
            return null;

        var layerRecords = new ColorLayer[layerCount];
        for (var i = 0; i < layerCount; i++)
        {
            var offset = checked(tableStart + (int)layerRecordsOffset + i * 4);
            layerRecords[i] = new ColorLayer(
                ReadUInt16(data, offset),
                ReadUInt16(data, offset + 2));
        }

        var result = new Dictionary<ushort, ColorLayer[]>(baseGlyphCount);
        for (var i = 0; i < baseGlyphCount; i++)
        {
            var offset = checked(tableStart + (int)baseGlyphRecordsOffset + i * 6);
            ushort glyphIndex = ReadUInt16(data, offset);
            ushort firstLayerIndex = ReadUInt16(data, offset + 2);
            ushort glyphLayerCount = ReadUInt16(data, offset + 4);

            if ((long)firstLayerIndex + glyphLayerCount > layerRecords.Length)
                continue;

            var glyphLayers = new ColorLayer[glyphLayerCount];
            Array.Copy(layerRecords, firstLayerIndex, glyphLayers, 0, glyphLayerCount);
            result[glyphIndex] = glyphLayers;
        }

        return result;
    }

    private static Color[]? ParseFirstPalette(byte[] data, TableRecord table)
    {
        // CPAL v0 header. CPAL v1 preserves this prefix and appends optional offsets afterwards.
        if (!Contains(table, 0, 12))
            return null;

        var tableStart = table.Offset;
        ushort version = ReadUInt16(data, tableStart);
        if (version > 1)
            return null;

        ushort entriesPerPalette = ReadUInt16(data, tableStart + 2);
        ushort paletteCount = ReadUInt16(data, tableStart + 4);
        ushort colorRecordCount = ReadUInt16(data, tableStart + 6);
        uint colorRecordsOffset = ReadUInt32(data, tableStart + 8);

        if (paletteCount == 0 || entriesPerPalette == 0 || colorRecordCount == 0)
            return null;

        if (!Contains(table, 12, paletteCount * 2L) ||
            !Contains(table, colorRecordsOffset, colorRecordCount * 4L))
            return null;

        ushort firstColorRecord = ReadUInt16(data, tableStart + 12);
        if ((long)firstColorRecord + entriesPerPalette > colorRecordCount)
            return null;

        var palette = new Color[entriesPerPalette];
        for (var i = 0; i < entriesPerPalette; i++)
        {
            var recordIndex = firstColorRecord + i;
            var offset = checked(tableStart + (int)colorRecordsOffset + recordIndex * 4);

            // OpenType CPAL ColorRecord is BGRA.
            byte blue = data[offset];
            byte green = data[offset + 1];
            byte red = data[offset + 2];
            byte alpha = data[offset + 3];
            palette[i] = Color.FromArgb(alpha, red, green, blue);
        }

        return palette;
    }

    private static bool Contains(TableRecord table, long relativeOffset, long length)
        => relativeOffset >= 0 && length >= 0 && relativeOffset + length <= table.Length;

    private static ushort ReadUInt16(byte[] data, int offset)
    {
        if (offset < 0 || offset > data.Length - 2)
            throw new InvalidDataException("Unexpected end of OpenType data.");

        return (ushort)((data[offset] << 8) | data[offset + 1]);
    }

    private static uint ReadUInt32(byte[] data, int offset)
    {
        if (offset < 0 || offset > data.Length - 4)
            throw new InvalidDataException("Unexpected end of OpenType data.");

        return ((uint)data[offset] << 24) |
               ((uint)data[offset + 1] << 16) |
               ((uint)data[offset + 2] << 8) |
               data[offset + 3];
    }

    private static string ReadTag(byte[] data, int offset)
    {
        if (offset < 0 || offset > data.Length - 4)
            return string.Empty;

        return string.Create(4, (data, offset), static (span, state) =>
        {
            for (var i = 0; i < 4; i++)
                span[i] = (char)state.data[state.offset + i];
        });
    }

    internal readonly record struct ColorLayer(ushort GlyphIndex, ushort PaletteIndex);
    private readonly record struct TableRecord(int Offset, int Length);
}
