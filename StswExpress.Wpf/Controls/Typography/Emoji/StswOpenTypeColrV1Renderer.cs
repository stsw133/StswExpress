using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace StswExpress.Wpf;

/// <summary>
/// Lightweight managed renderer for the subset of OpenType COLR v1 used as a fallback by emoji fonts.
/// </summary>
/// <remarks>
/// The normal emoji path remains DirectWrite COLR v0. This renderer is only used when that path reports no
/// color layers. It intentionally ignores variable deltas and, for PaintComposite, renders the backdrop paint.
/// This keeps the fallback dependency-free while still rendering the underlying flag artwork used by Noto COLRv1.
/// </remarks>
internal static class StswOpenTypeColrV1Renderer
{
    private const int MaxPaintDepth = 64;
    private const int MaxContextCacheEntries = 64;

    private static readonly ConcurrentDictionary<ContextCacheKey, Lazy<Context?>> ContextCache = new();

    public static bool TryRender(GlyphRun glyphRun, Brush foreground, out Drawing? drawing)
    {
        drawing = null;

        try
        {
            Context? context = GetOrCreateContext(
                glyphRun.GlyphTypeface,
                foreground,
                glyphRun.FontRenderingEmSize);

            if (context is null)
                return false;

            var result = new DrawingGroup();
            double x = glyphRun.BaselineOrigin.X;
            double direction = (glyphRun.BidiLevel & 1) == 0 ? 1d : -1d;
            bool any = false;

            for (var i = 0; i < glyphRun.GlyphIndices.Count; i++)
            {
                ushort glyphIndex = glyphRun.GlyphIndices[i];
                Point offset = glyphRun.GlyphOffsets is { } offsets && offsets.Count > i
                    ? offsets[i]
                    : default;

                if (context.TryRenderBaseGlyph(glyphIndex, out Drawing? glyphDrawing) && glyphDrawing is not null)
                {
                    var positioned = new DrawingGroup();
                    positioned.Children.Add(glyphDrawing);
                    positioned.Transform = new TranslateTransform(
                        x + offset.X,
                        glyphRun.BaselineOrigin.Y + offset.Y);
                    result.Children.Add(positioned);
                    any = true;
                }

                if (i < glyphRun.AdvanceWidths.Count)
                    x += glyphRun.AdvanceWidths[i] * direction;
            }

            if (!any || result.Bounds.IsEmpty || result.Bounds.Width <= 0 || result.Bounds.Height <= 0)
            {
                return false;
            }

            drawing = result;
            return true;
        }
        catch
        {
            drawing = null;
            return false;
        }
    }

    private static Context? GetOrCreateContext(
        GlyphTypeface glyphTypeface,
        Brush foreground,
        double emSize)
    {
        if (TryCreateContextCacheKey(glyphTypeface, foreground, emSize, out ContextCacheKey key))
        {
            if (ContextCache.Count >= MaxContextCacheEntries)
                ContextCache.Clear();

            Lazy<Context?> lazy = ContextCache.GetOrAdd(
                key,
                _ => new Lazy<Context?>(
                    () => CreateContextFromFont(glyphTypeface, CreateFrozenForeground(foreground), emSize),
                    isThreadSafe: true));

            return lazy.Value;
        }

        return CreateContextFromFont(glyphTypeface, foreground, emSize);
    }

    private static Context? CreateContextFromFont(
        GlyphTypeface glyphTypeface,
        Brush foreground,
        double emSize)
    {
        try
        {
            using Stream stream = glyphTypeface.GetFontStream();
            using var memory = new MemoryStream();
            stream.CopyTo(memory);

            return TryCreateContext(
                memory.ToArray(),
                glyphTypeface,
                foreground,
                emSize,
                out Context? context)
                ? context
                : null;
        }
        catch
        {
            return null;
        }
    }

    private static bool TryCreateContextCacheKey(
        GlyphTypeface glyphTypeface,
        Brush foreground,
        double emSize,
        out ContextCacheKey key)
    {
        key = default;

        if (foreground is not SolidColorBrush solid)
            return false;

        string fontIdentity = glyphTypeface.FontUri?.ToString() ?? string.Empty;
        if (string.IsNullOrEmpty(fontIdentity))
            return false;

        key = new ContextCacheKey(
            fontIdentity,
            emSize,
            solid.Color,
            solid.Opacity);

        return true;
    }

    private static Brush CreateFrozenForeground(Brush foreground)
    {
        Brush clone = foreground.CloneCurrentValue();
        if (clone.CanFreeze)
            clone.Freeze();
        return clone;
    }

    private static bool TryCreateContext(
        byte[] data,
        GlyphTypeface glyphTypeface,
        Brush foreground,
        double emSize,
        out Context? context)
    {
        context = null;

        if (data.Length < 12 || ReadTag(data, 0) == "ttcf")
            return false;

        ushort tableCount = ReadUInt16(data, 4);
        if (12L + tableCount * 16L > data.Length)
            return false;

		TableRecord? colr = null;
		TableRecord? cpal = null;
		TableRecord? head = null;

		for (var i = 0; i < tableCount; i++)
        {
            int recordOffset = 12 + i * 16;
            string tag = ReadTag(data, recordOffset);
            uint offset = ReadUInt32(data, recordOffset + 8);
            uint length = ReadUInt32(data, recordOffset + 12);

            if (offset > data.Length || length > data.Length || (long)offset + length > data.Length)
                continue;

            var record = new TableRecord((int)offset, (int)length);
            if (tag == "COLR")
                colr = record;
            else if (tag == "CPAL")
                cpal = record;
            else if (tag == "head")
                head = record;
        }

        if (colr is null || cpal is null || !Contains(colr.Value, 0, 34))
            return false;

        int colrStart = colr.Value.Offset;
        ushort version = ReadUInt16(data, colrStart);
        if (version != 1)
            return false;

        uint baseGlyphListOffset = ReadUInt32(data, colrStart + 14);
        uint layerListOffset = ReadUInt32(data, colrStart + 18);
        if (baseGlyphListOffset == 0 || layerListOffset == 0 ||
            !Contains(colr.Value, baseGlyphListOffset, 4) ||
            !Contains(colr.Value, layerListOffset, 4))
            return false;

        Color[]? palette = ParseFirstPalette(data, cpal.Value);
        if (palette is null)
            return false;

        int baseGlyphListStart = checked(colrStart + (int)baseGlyphListOffset);
        int layerListStart = checked(colrStart + (int)layerListOffset);

        uint baseGlyphCount = ReadUInt32(data, baseGlyphListStart);
        if (baseGlyphCount > int.MaxValue || !Contains(colr.Value, baseGlyphListOffset + 4L, baseGlyphCount * 6L))
            return false;

        var basePaintOffsets = new Dictionary<ushort, int>((int)Math.Min(baseGlyphCount, 4096));
        for (var i = 0; i < (int)baseGlyphCount; i++)
        {
            int recordOffset = baseGlyphListStart + 4 + i * 6;
            ushort glyphId = ReadUInt16(data, recordOffset);
            uint paintOffset = ReadUInt32(data, recordOffset + 2);

            // BaseGlyphPaintRecord's Paint offset is relative to the beginning of BaseGlyphList.
            long absolute = (long)baseGlyphListStart + paintOffset;
            if (paintOffset != 0 && IsInside(colr.Value, absolute))
                basePaintOffsets[glyphId] = (int)absolute;
        }

        uint layerCount = ReadUInt32(data, layerListStart);
        if (layerCount > int.MaxValue || !Contains(colr.Value, layerListOffset + 4L, layerCount * 4L))
            return false;

        var layerPaintOffsets = new int[(int)layerCount];
        for (var i = 0; i < layerPaintOffsets.Length; i++)
        {
            uint paintOffset = ReadUInt32(data, layerListStart + 4 + i * 4);
            long absolute = (long)layerListStart + paintOffset;
            layerPaintOffsets[i] = paintOffset != 0 && IsInside(colr.Value, absolute)
                ? (int)absolute
                : -1;
        }

        ushort unitsPerEm = head is { } headTable && Contains(headTable, 18, 2)
            ? ReadUInt16(data, headTable.Offset + 18)
            : (ushort)0;

        // The OpenType head table is required for normal sfnt fonts, but keep a safe
        // fallback so a malformed/custom emoji font cannot explode COLRv1 transforms.
        if (unitsPerEm == 0)
        {
            unitsPerEm = 2048;
        }

        double emScale = emSize / unitsPerEm;

        context = new Context(
            data,
            colr.Value,
            glyphTypeface,
            foreground,
            palette,
            basePaintOffsets,
            layerPaintOffsets,
            emSize,
            emScale);
        return true;
    }

    private sealed class Context
    {
        private readonly byte[] _data;
        private readonly TableRecord _colr;
        private readonly GlyphTypeface _glyphTypeface;
        private readonly Brush _foreground;
        private readonly Color[] _palette;
        private readonly Dictionary<ushort, int> _basePaintOffsets;
        private readonly int[] _layerPaintOffsets;
        private readonly double _emSize;
        private readonly double _emScale;

        public Context(
            byte[] data,
            TableRecord colr,
            GlyphTypeface glyphTypeface,
            Brush foreground,
            Color[] palette,
            Dictionary<ushort, int> basePaintOffsets,
            int[] layerPaintOffsets,
            double emSize,
            double emScale)
        {
            _data = data;
            _colr = colr;
            _glyphTypeface = glyphTypeface;
            _foreground = foreground;
            _palette = palette;
            _basePaintOffsets = basePaintOffsets;
            _layerPaintOffsets = layerPaintOffsets;
            _emSize = emSize;
            _emScale = emScale;
        }

        public bool TryRenderBaseGlyph(ushort glyphId, out Drawing? drawing)
        {
            drawing = null;
            if (!_basePaintOffsets.TryGetValue(glyphId, out int paintOffset))
                return false;

            var stack = new HashSet<int>();
            PaintResult result = RenderPaint(paintOffset, 0, stack);
            drawing = result.Drawing;
            return drawing is not null && !drawing.Bounds.IsEmpty;
        }

        private PaintResult RenderPaint(int paintOffset, int depth, HashSet<int> stack)
        {
            if (depth > MaxPaintDepth || !IsInside(_colr, paintOffset) || !stack.Add(paintOffset))
                return PaintResult.Empty;

            try
            {
                byte format = _data[paintOffset];
                return format switch
                {
                    1 => RenderLayers(paintOffset, depth, stack),
                    2 => RenderSolid(paintOffset, variable: false),
                    3 => RenderSolid(paintOffset, variable: true),
                    10 => RenderGlyph(paintOffset, depth, stack),
                    11 => RenderColrGlyph(paintOffset, depth, stack),
                    12 => RenderAffineTransform(paintOffset, depth, stack, variable: false),
                    13 => RenderAffineTransform(paintOffset, depth, stack, variable: true),
                    14 => RenderTranslate(paintOffset, depth, stack, variable: false),
                    15 => RenderTranslate(paintOffset, depth, stack, variable: true),
                    16 => RenderScale(paintOffset, depth, stack, aroundCenter: false, uniform: false, variable: false),
                    17 => RenderScale(paintOffset, depth, stack, aroundCenter: false, uniform: false, variable: true),
                    18 => RenderScale(paintOffset, depth, stack, aroundCenter: true, uniform: false, variable: false),
                    19 => RenderScale(paintOffset, depth, stack, aroundCenter: true, uniform: false, variable: true),
                    20 => RenderScale(paintOffset, depth, stack, aroundCenter: false, uniform: true, variable: false),
                    21 => RenderScale(paintOffset, depth, stack, aroundCenter: false, uniform: true, variable: true),
                    22 => RenderScale(paintOffset, depth, stack, aroundCenter: true, uniform: true, variable: false),
                    23 => RenderScale(paintOffset, depth, stack, aroundCenter: true, uniform: true, variable: true),
                    24 => RenderRotate(paintOffset, depth, stack, aroundCenter: false, variable: false),
                    25 => RenderRotate(paintOffset, depth, stack, aroundCenter: false, variable: true),
                    26 => RenderRotate(paintOffset, depth, stack, aroundCenter: true, variable: false),
                    27 => RenderRotate(paintOffset, depth, stack, aroundCenter: true, variable: true),
                    28 => RenderSkew(paintOffset, depth, stack, aroundCenter: false, variable: false),
                    29 => RenderSkew(paintOffset, depth, stack, aroundCenter: false, variable: true),
                    30 => RenderSkew(paintOffset, depth, stack, aroundCenter: true, variable: false),
                    31 => RenderSkew(paintOffset, depth, stack, aroundCenter: true, variable: true),
                    32 => RenderCompositeBackdrop(paintOffset, depth, stack),
                    _ => PaintResult.Empty,
                };
            }
            finally
            {
                stack.Remove(paintOffset);
            }
        }

        private PaintResult RenderLayers(int paintOffset, int depth, HashSet<int> stack)
        {
            if (!HasBytes(paintOffset, 6))
                return PaintResult.Empty;

            int count = _data[paintOffset + 1];
            uint firstLayer = ReadUInt32(_data, paintOffset + 2);
            if (firstLayer > _layerPaintOffsets.Length || (long)firstLayer + count > _layerPaintOffsets.Length)
                return PaintResult.Empty;

            var group = new DrawingGroup();
            for (var i = 0; i < count; i++)
            {
                int childOffset = _layerPaintOffsets[(int)firstLayer + i];
                if (childOffset < 0)
                    continue;

                PaintResult child = RenderPaint(childOffset, depth + 1, stack);
                if (child.Drawing is not null)
                    group.Children.Add(child.Drawing);
            }

            return group.Children.Count > 0
                ? PaintResult.FromDrawing(group)
                : PaintResult.Empty;
        }

        private PaintResult RenderSolid(int paintOffset, bool variable)
        {
            int required = variable ? 9 : 5;
            if (!HasBytes(paintOffset, required))
                return PaintResult.Empty;

            ushort paletteIndex = ReadUInt16(_data, paintOffset + 1);
            double alpha = Math.Clamp(ReadF2Dot14(_data, paintOffset + 3), 0d, 1d);
            Brush brush = CreateBrush(paletteIndex, alpha);
            return PaintResult.FromBrush(brush);
        }

        private PaintResult RenderGlyph(int paintOffset, int depth, HashSet<int> stack)
        {
            if (!HasBytes(paintOffset, 6))
                return PaintResult.Empty;

            uint childRelative = ReadUInt24(_data, paintOffset + 1);
            ushort glyphId = ReadUInt16(_data, paintOffset + 4);
            int childOffset = RelativePaintOffset(paintOffset, childRelative);
            if (childOffset < 0)
                return PaintResult.Empty;

            PaintResult fill = RenderPaint(childOffset, depth + 1, stack);
            if (fill.Brush is null)
            {
                return PaintResult.Empty;
            }

            Geometry geometry = _glyphTypeface.GetGlyphOutline(glyphId, _emSize, _emSize);
            if (geometry.IsEmpty())
                return PaintResult.Empty;

            return PaintResult.FromDrawing(new GeometryDrawing(fill.Brush, null, geometry));
        }

        private PaintResult RenderColrGlyph(int paintOffset, int depth, HashSet<int> stack)
        {
            if (!HasBytes(paintOffset, 3))
                return PaintResult.Empty;

            ushort glyphId = ReadUInt16(_data, paintOffset + 1);
            if (!_basePaintOffsets.TryGetValue(glyphId, out int referencedPaint))
                return PaintResult.Empty;

            return RenderPaint(referencedPaint, depth + 1, stack);
        }

        private PaintResult RenderAffineTransform(int paintOffset, int depth, HashSet<int> stack, bool variable)
        {
            if (!HasBytes(paintOffset, 7))
                return PaintResult.Empty;

            uint childRelative = ReadUInt24(_data, paintOffset + 1);
            uint transformRelative = ReadUInt24(_data, paintOffset + 4);
            int childOffset = RelativePaintOffset(paintOffset, childRelative);
            long transformLong = (long)paintOffset + transformRelative;
            int transformSize = variable ? 28 : 24;
            if (childOffset < 0 || !HasBytes(transformLong, transformSize))
                return PaintResult.Empty;

            int t = (int)transformLong;
            double xx = ReadFixed(_data, t);
            double yx = ReadFixed(_data, t + 4);
            double xy = ReadFixed(_data, t + 8);
            double yy = ReadFixed(_data, t + 12);
            double dx = ReadFixed(_data, t + 16) * _emScale;
            double dy = -ReadFixed(_data, t + 20) * _emScale;

            // Convert the OpenType y-up affine transform to WPF's y-down coordinate space.
            var matrix = new Matrix(xx, -yx, -xy, yy, dx, dy);
            return TransformResult(RenderPaint(childOffset, depth + 1, stack), new MatrixTransform(matrix));
        }

        private PaintResult RenderTranslate(int paintOffset, int depth, HashSet<int> stack, bool variable)
        {
            int required = variable ? 12 : 8;
            if (!HasBytes(paintOffset, required))
                return PaintResult.Empty;

            int childOffset = RelativePaintOffset(paintOffset, ReadUInt24(_data, paintOffset + 1));
            if (childOffset < 0)
                return PaintResult.Empty;

            double dx = ReadInt16(_data, paintOffset + 4) * _emScale;
            double dy = -ReadInt16(_data, paintOffset + 6) * _emScale;
            return TransformResult(RenderPaint(childOffset, depth + 1, stack), new TranslateTransform(dx, dy));
        }

        private PaintResult RenderScale(
            int paintOffset,
            int depth,
            HashSet<int> stack,
            bool aroundCenter,
            bool uniform,
            bool variable)
        {
            int cursor = paintOffset + 4;
            int minimum = 4 + (uniform ? 2 : 4) + (aroundCenter ? 4 : 0) + (variable ? 4 : 0);
            if (!HasBytes(paintOffset, minimum))
                return PaintResult.Empty;

            int childOffset = RelativePaintOffset(paintOffset, ReadUInt24(_data, paintOffset + 1));
            if (childOffset < 0)
                return PaintResult.Empty;

            double scaleX;
            double scaleY;
            if (uniform)
            {
                scaleX = scaleY = ReadF2Dot14(_data, cursor);
                cursor += 2;
            }
            else
            {
                scaleX = ReadF2Dot14(_data, cursor);
                scaleY = ReadF2Dot14(_data, cursor + 2);
                cursor += 4;
            }

            double centerX = 0d;
            double centerY = 0d;
            if (aroundCenter)
            {
                centerX = ReadInt16(_data, cursor) * _emScale;
                centerY = -ReadInt16(_data, cursor + 2) * _emScale;
            }

            return TransformResult(
                RenderPaint(childOffset, depth + 1, stack),
                new ScaleTransform(scaleX, scaleY, centerX, centerY));
        }

        private PaintResult RenderRotate(
            int paintOffset,
            int depth,
            HashSet<int> stack,
            bool aroundCenter,
            bool variable)
        {
            int minimum = 6 + (aroundCenter ? 4 : 0) + (variable ? 4 : 0);
            if (!HasBytes(paintOffset, minimum))
                return PaintResult.Empty;

            int childOffset = RelativePaintOffset(paintOffset, ReadUInt24(_data, paintOffset + 1));
            if (childOffset < 0)
                return PaintResult.Empty;

            double angleDegrees = -ReadF2Dot14(_data, paintOffset + 4) * 180d;
            double centerX = 0d;
            double centerY = 0d;
            if (aroundCenter)
            {
                centerX = ReadInt16(_data, paintOffset + 6) * _emScale;
                centerY = -ReadInt16(_data, paintOffset + 8) * _emScale;
            }

            return TransformResult(
                RenderPaint(childOffset, depth + 1, stack),
                new RotateTransform(angleDegrees, centerX, centerY));
        }

        private PaintResult RenderSkew(
            int paintOffset,
            int depth,
            HashSet<int> stack,
            bool aroundCenter,
            bool variable)
        {
            int minimum = 8 + (aroundCenter ? 4 : 0) + (variable ? 4 : 0);
            if (!HasBytes(paintOffset, minimum))
                return PaintResult.Empty;

            int childOffset = RelativePaintOffset(paintOffset, ReadUInt24(_data, paintOffset + 1));
            if (childOffset < 0)
                return PaintResult.Empty;

            double xAngle = -ReadF2Dot14(_data, paintOffset + 4) * 180d;
            double yAngle = -ReadF2Dot14(_data, paintOffset + 6) * 180d;
            double centerX = 0d;
            double centerY = 0d;
            if (aroundCenter)
            {
                centerX = ReadInt16(_data, paintOffset + 8) * _emScale;
                centerY = -ReadInt16(_data, paintOffset + 10) * _emScale;
            }

            return TransformResult(
                RenderPaint(childOffset, depth + 1, stack),
                new SkewTransform(xAngle, yAngle, centerX, centerY));
        }

        private PaintResult RenderCompositeBackdrop(int paintOffset, int depth, HashSet<int> stack)
        {
            if (!HasBytes(paintOffset, 8))
                return PaintResult.Empty;

            uint backdropRelative = ReadUInt24(_data, paintOffset + 5);
            int backdropOffset = RelativePaintOffset(paintOffset, backdropRelative);
            if (backdropOffset < 0)
                return PaintResult.Empty;

            // Noto applies a cosmetic soft-light gradient to flags using nested PaintComposite nodes.
            // Rendering the backdrop gives us the complete underlying flag without requiring a full blend engine.
            return RenderPaint(backdropOffset, depth + 1, stack);
        }

        private PaintResult TransformResult(PaintResult child, Transform transform)
        {
            if (child.Drawing is null)
            {
                // A transform around a uniform solid fill has no observable effect until a glyph clips it.
                return child;
            }

            var group = new DrawingGroup
            {
                Transform = transform,
            };
            group.Children.Add(child.Drawing);
            return PaintResult.FromDrawing(group);
        }

        private Brush CreateBrush(ushort paletteIndex, double alphaMultiplier)
        {
            if (paletteIndex == ushort.MaxValue || paletteIndex >= _palette.Length)
            {
                if (alphaMultiplier >= 0.999999)
                    return _foreground;

                Brush clone = _foreground.Clone();
                clone.Opacity *= alphaMultiplier;
                if (clone.CanFreeze)
                    clone.Freeze();
                return clone;
            }

            Color color = _palette[paletteIndex];
            byte alpha = (byte)Math.Clamp(
                (int)Math.Round(color.A * alphaMultiplier, MidpointRounding.AwayFromZero),
                0,
                255);
            color = Color.FromArgb(alpha, color.R, color.G, color.B);

            var brush = new SolidColorBrush(color);
            if (brush.CanFreeze)
                brush.Freeze();
            return brush;
        }

        private int RelativePaintOffset(int paintOffset, uint relative)
        {
            long absolute = (long)paintOffset + relative;
            return relative != 0 && IsInside(_colr, absolute)
                ? (int)absolute
                : -1;
        }

        private bool HasBytes(long absoluteOffset, int length)
            => absoluteOffset >= _colr.Offset &&
               length >= 0 &&
               absoluteOffset + length <= (long)_colr.Offset + _colr.Length &&
               absoluteOffset + length <= _data.Length;
    }

    private readonly struct PaintResult
    {
        private PaintResult(Drawing? drawing, Brush? brush)
        {
            Drawing = drawing;
            Brush = brush;
        }

        public Drawing? Drawing { get; }
        public Brush? Brush { get; }

        public static PaintResult Empty => default;
        public static PaintResult FromDrawing(Drawing drawing) => new(drawing, null);
        public static PaintResult FromBrush(Brush brush) => new(null, brush);
    }

    private static Color[]? ParseFirstPalette(byte[] data, TableRecord table)
    {
        if (!Contains(table, 0, 12))
            return null;

        int start = table.Offset;
        ushort version = ReadUInt16(data, start);
        if (version > 1)
            return null;

        ushort entriesPerPalette = ReadUInt16(data, start + 2);
        ushort paletteCount = ReadUInt16(data, start + 4);
        ushort colorRecordCount = ReadUInt16(data, start + 6);
        uint colorRecordsOffset = ReadUInt32(data, start + 8);

        if (paletteCount == 0 || entriesPerPalette == 0 || colorRecordCount == 0 ||
            !Contains(table, 12, paletteCount * 2L) ||
            !Contains(table, colorRecordsOffset, colorRecordCount * 4L))
            return null;

        ushort firstColorRecord = ReadUInt16(data, start + 12);
        if ((long)firstColorRecord + entriesPerPalette > colorRecordCount)
            return null;

        var palette = new Color[entriesPerPalette];
        for (var i = 0; i < palette.Length; i++)
        {
            int recordIndex = firstColorRecord + i;
            int offset = checked(start + (int)colorRecordsOffset + recordIndex * 4);
            byte blue = data[offset];
            byte green = data[offset + 1];
            byte red = data[offset + 2];
            byte alpha = data[offset + 3];
            palette[i] = Color.FromArgb(alpha, red, green, blue);
        }

        return palette;
    }

    private static bool Contains(TableRecord table, long relativeOffset, long length)
        => relativeOffset >= 0 &&
           length >= 0 &&
           relativeOffset + length <= table.Length;

    private static bool IsInside(TableRecord table, long absoluteOffset)
        => absoluteOffset >= table.Offset && absoluteOffset < (long)table.Offset + table.Length;

    private static ushort ReadUInt16(byte[] data, int offset)
    {
        Ensure(data, offset, 2);
        return (ushort)((data[offset] << 8) | data[offset + 1]);
    }

    private static short ReadInt16(byte[] data, int offset)
        => unchecked((short)ReadUInt16(data, offset));

    private static uint ReadUInt24(byte[] data, int offset)
    {
        Ensure(data, offset, 3);
        return ((uint)data[offset] << 16) |
               ((uint)data[offset + 1] << 8) |
               data[offset + 2];
    }

    private static uint ReadUInt32(byte[] data, int offset)
    {
        Ensure(data, offset, 4);
        return ((uint)data[offset] << 24) |
               ((uint)data[offset + 1] << 16) |
               ((uint)data[offset + 2] << 8) |
               data[offset + 3];
    }

    private static int ReadInt32(byte[] data, int offset)
        => unchecked((int)ReadUInt32(data, offset));

    private static double ReadF2Dot14(byte[] data, int offset)
        => ReadInt16(data, offset) / 16384d;

    private static double ReadFixed(byte[] data, int offset)
        => ReadInt32(data, offset) / 65536d;

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

    private static void Ensure(byte[] data, int offset, int length)
    {
        if (offset < 0 || length < 0 || offset > data.Length - length)
            throw new InvalidDataException("Unexpected end of OpenType data.");
    }

    private readonly record struct ContextCacheKey(
        string FontIdentity,
        double EmSize,
        Color ForegroundColor,
        double ForegroundOpacity);

    private readonly record struct TableRecord(int Offset, int Length);
}
