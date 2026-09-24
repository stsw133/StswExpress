using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace StswExpress.Wpf;

/// <summary>
/// Renders vector color emoji from an installed OpenType color font without network access or external packages.
/// </summary>
internal static class StswColorEmojiRenderer
{
    private const double FormatterWidth = 4096d;
    private const int MaxRenderCacheEntries = 2048;

    private static readonly ConcurrentDictionary<RenderCacheKey, DrawingImage> RenderCache = new();
    private static readonly ConcurrentDictionary<RenderCacheKey, byte> RenderFailureCache = new();
    private static readonly ConcurrentDictionary<string, bool> ColrV1Cache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Shapes the supplied Unicode sequence with WPF's text engine and asks DirectWrite to decompose COLR glyphs
    /// into their colored layers.
    /// </summary>
    public static bool TryRender(
        string text,
        FontFamily emojiFontFamily,
        double fontSize,
        Brush foreground,
        double pixelsPerDip,
        out DrawingImage? image)
        => TryRender(
            text,
            new Typeface(emojiFontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
            fontSize,
            foreground,
            pixelsPerDip,
            out image);

    /// <summary>
    /// Shapes the supplied Unicode sequence using an already resolved physical typeface.
    /// This overload is important for font-file references because it avoids resolving the family name again.
    /// </summary>
    public static bool TryRender(
        string text,
        Typeface emojiTypeface,
        double fontSize,
        Brush foreground,
        double pixelsPerDip,
        out DrawingImage? image)
        => TryRender(
            text,
            emojiTypeface,
            fontSize,
            foreground,
            pixelsPerDip,
            allowModernColorFallback: true,
            out image);

    public static bool TryRender(
        string text,
        Typeface emojiTypeface,
        double fontSize,
        Brush foreground,
        double pixelsPerDip,
        bool allowModernColorFallback,
        out DrawingImage? image)
    {
        image = null;

        if (string.IsNullOrEmpty(text) || fontSize <= 0 || double.IsNaN(fontSize) || double.IsInfinity(fontSize))
            return false;

        bool hasCacheKey = TryCreateCacheKey(
            text,
            emojiTypeface,
            fontSize,
            foreground,
            pixelsPerDip,
            allowModernColorFallback,
            out RenderCacheKey cacheKey);

        if (hasCacheKey)
        {
            if (RenderCache.TryGetValue(cacheKey, out DrawingImage? cached))
            {
                image = cached;
                return true;
            }

            if (RenderFailureCache.ContainsKey(cacheKey))
                return false;
        }

        Brush renderForeground = hasCacheKey && foreground is SolidColorBrush solidForeground
            ? CreateFrozenBrush(solidForeground.Color, solidForeground.Opacity)
            : foreground;

        try
        {
            var culture = CultureInfo.CurrentUICulture;
            var runProperties = new EmojiTextRunProperties(emojiTypeface, fontSize, renderForeground, culture, pixelsPerDip);
            var paragraphProperties = new EmojiTextParagraphProperties(runProperties);
            var textSource = new EmojiTextSource(text, runProperties, culture)
            {
                PixelsPerDip = pixelsPerDip,
            };

            using TextFormatter formatter = TextFormatter.Create(TextFormattingMode.Ideal);
            using TextLine line = formatter.FormatLine(textSource, 0, FormatterWidth, paragraphProperties, null);

            var drawing = new DrawingGroup();
            var hasColorDrawing = false;

            using (DrawingContext drawingContext = drawing.Open())
            {
                foreach (IndexedGlyphRun indexedRun in line.GetIndexedGlyphRuns())
                {
                    GlyphRun glyphRun = indexedRun.GlyphRun;

                    if (allowModernColorFallback &&
                        IsColrV1OrHigher(glyphRun.GlyphTypeface) &&
                        StswOpenTypeColrV1Renderer.TryRender(
                            glyphRun,
                            renderForeground,
                            out Drawing? colrV1Drawing) &&
                        colrV1Drawing is not null)
                    {
                        drawingContext.DrawDrawing(colrV1Drawing);
                        hasColorDrawing = true;
                        continue;
                    }

                    if (StswDirectWriteColorGlyphs.TryTranslate(glyphRun, out var layers))
                    {
                        foreach (StswDirectWriteColorGlyphs.ColorGlyphLayer layer in layers)
                        {
                            Geometry geometry = BuildLayerGeometry(
                                glyphRun.GlyphTypeface,
                                layer,
                                pixelsPerDip);

                            if (!geometry.IsEmpty())
                            {
                                Brush brush = layer.UseForegroundColor
                                    ? renderForeground
                                    : CreateFrozenBrush(layer.Color);

                                drawingContext.DrawGeometry(brush, null, geometry);
                                hasColorDrawing = true;
                            }
                        }
                    }
                    else if (hasColorDrawing)
                    {
                        drawingContext.DrawGlyphRun(renderForeground, glyphRun);
                    }
                }
            }

            if (!hasColorDrawing || drawing.Bounds.IsEmpty || drawing.Bounds.Width <= 0 || drawing.Bounds.Height <= 0)
            {
                if (hasCacheKey)
                    CacheFailure(cacheKey);

                return false;
            }

            if (drawing.CanFreeze)
                drawing.Freeze();

            image = new DrawingImage(drawing);
            if (image.CanFreeze)
                image.Freeze();

            if (hasCacheKey && image.IsFrozen)
                CacheSuccess(cacheKey, image);

            return true;
        }
        catch
        {
            image = null;
            return false;
        }
    }

    private static bool TryCreateCacheKey(
        string text,
        Typeface typeface,
        double fontSize,
        Brush foreground,
        double pixelsPerDip,
        bool allowModernColorFallback,
        out RenderCacheKey key)
    {
        key = default;

        if (foreground is not SolidColorBrush solid)
            return false;

        string fontIdentity = typeface.TryGetGlyphTypeface(out GlyphTypeface? glyphTypeface)
            ? glyphTypeface.FontUri?.ToString() ?? typeface.FontFamily.Source
            : typeface.FontFamily.Source;

        key = new RenderCacheKey(
            text,
            fontIdentity,
            fontSize,
            pixelsPerDip,
            solid.Color,
            solid.Opacity,
            allowModernColorFallback);

        return true;
    }

    private static void CacheSuccess(RenderCacheKey key, DrawingImage image)
    {
        TrimRenderCachesIfNeeded();
        RenderFailureCache.TryRemove(key, out _);
        RenderCache[key] = image;
    }

    private static void CacheFailure(RenderCacheKey key)
    {
        TrimRenderCachesIfNeeded();
        RenderFailureCache[key] = 0;
    }

    private static void TrimRenderCachesIfNeeded()
    {
        if (RenderCache.Count + RenderFailureCache.Count < MaxRenderCacheEntries)
            return;

        RenderCache.Clear();
        RenderFailureCache.Clear();
    }

    private static Geometry BuildLayerGeometry(
        GlyphTypeface glyphTypeface,
        StswDirectWriteColorGlyphs.ColorGlyphLayer layer,
        double pixelsPerDip)
    {
        var glyphRun = new GlyphRun(
            glyphTypeface,
            layer.BidiLevel,
            layer.IsSideways,
            layer.FontRenderingEmSize,
            (float)pixelsPerDip,
            layer.GlyphIndices,
            layer.BaselineOrigin,
            layer.AdvanceWidths,
            layer.GlyphOffsets,
            null,
            null,
            null,
            null,
            null);

        return glyphRun.BuildGeometry();
    }

    private static Brush CreateFrozenBrush(Color color, double opacity = 1d)
    {
        var brush = new SolidColorBrush(color)
        {
            Opacity = opacity,
        };

        if (brush.CanFreeze)
            brush.Freeze();

        return brush;
    }

    /// <summary>
    /// Checks the COLR table version directly from the font file.
    /// IDWriteFactory2 handles COLR v0; COLR v1 is handled by the managed fallback renderer.
    /// </summary>
    private static bool IsColrV1OrHigher(GlyphTypeface glyphTypeface)
    {
        string key = glyphTypeface.FontUri?.ToString() ?? string.Empty;
        if (!string.IsNullOrEmpty(key) && ColrV1Cache.TryGetValue(key, out bool cached))
            return cached;

        bool result = false;
        try
        {
            using Stream stream = glyphTypeface.GetFontStream();
            using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: false);

            if (stream.Length >= 12)
            {
                stream.Position = 4;
                ushort tableCount = ReadUInt16BigEndian(reader);
                stream.Position = 12;

                const uint ColrTag = 0x434F4C52; // 'COLR'

                for (var i = 0; i < tableCount; i++)
                {
                    if (stream.Position + 16 > stream.Length)
                        break;

                    uint tag = ReadUInt32BigEndian(reader);
                    _ = ReadUInt32BigEndian(reader);
                    uint offset = ReadUInt32BigEndian(reader);
                    uint length = ReadUInt32BigEndian(reader);

                    if (tag != ColrTag)
                        continue;

                    if (length >= 2 && offset <= stream.Length - 2)
                    {
                        stream.Position = offset;
                        result = ReadUInt16BigEndian(reader) >= 1;
                    }

                    break;
                }
            }
        }
        catch
        {
            result = false;
        }

        if (!string.IsNullOrEmpty(key))
            ColrV1Cache[key] = result;

        return result;
    }

    private static ushort ReadUInt16BigEndian(BinaryReader reader)
    {
        byte a = reader.ReadByte();
        byte b = reader.ReadByte();
        return (ushort)((a << 8) | b);
    }

    private static uint ReadUInt32BigEndian(BinaryReader reader)
    {
        uint a = reader.ReadByte();
        uint b = reader.ReadByte();
        uint c = reader.ReadByte();
        uint d = reader.ReadByte();
        return (a << 24) | (b << 16) | (c << 8) | d;
    }


    private readonly record struct RenderCacheKey(
        string Text,
        string FontIdentity,
        double FontSize,
        double PixelsPerDip,
        Color ForegroundColor,
        double ForegroundOpacity,
        bool AllowModernColorFallback);

    private sealed class EmojiTextSource : TextSource
    {
        private readonly string _text;
        private readonly TextRunProperties _properties;
        private readonly CultureInfo _culture;

        public EmojiTextSource(string text, TextRunProperties properties, CultureInfo culture)
        {
            _text = text;
            _properties = properties;
            _culture = culture;
        }

        public override TextRun GetTextRun(int textSourceCharacterIndex)
        {
            if (textSourceCharacterIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(textSourceCharacterIndex));

            if (textSourceCharacterIndex >= _text.Length)
                return new TextEndOfParagraph(1);

            return new TextCharacters(
                _text,
                textSourceCharacterIndex,
                _text.Length - textSourceCharacterIndex,
                _properties);
        }

        public override TextSpan<CultureSpecificCharacterBufferRange> GetPrecedingText(int textSourceCharacterIndexLimit)
        {
            var length = Math.Clamp(textSourceCharacterIndexLimit, 0, _text.Length);
            var range = new CharacterBufferRange(_text, 0, length);
            return new TextSpan<CultureSpecificCharacterBufferRange>(
                length,
                new CultureSpecificCharacterBufferRange(_culture, range));
        }

        public override int GetTextEffectCharacterIndexFromTextSourceCharacterIndex(int textSourceCharacterIndex)
            => textSourceCharacterIndex;
    }

    private sealed class EmojiTextRunProperties : TextRunProperties
    {
        private readonly Typeface _typeface;
        private readonly double _fontSize;
        private readonly Brush _foreground;
        private readonly CultureInfo _culture;

        public EmojiTextRunProperties(Typeface typeface, double fontSize, Brush foreground, CultureInfo culture, double pixelsPerDip)
        {
            _typeface = typeface;
            _fontSize = fontSize;
            _foreground = foreground;
            _culture = culture;
            PixelsPerDip = pixelsPerDip;
        }

        public override Typeface Typeface => _typeface;
        public override double FontRenderingEmSize => _fontSize;
        public override double FontHintingEmSize => _fontSize;
        public override TextDecorationCollection TextDecorations => null!;
        public override Brush ForegroundBrush => _foreground;
        public override Brush BackgroundBrush => null!;
        public override BaselineAlignment BaselineAlignment => BaselineAlignment.Baseline;
        public override CultureInfo CultureInfo => _culture;
        public override TextRunTypographyProperties TypographyProperties => null!;
        public override TextEffectCollection TextEffects => null!;
        public override NumberSubstitution NumberSubstitution => null!;
    }

    private sealed class EmojiTextParagraphProperties : TextParagraphProperties
    {
        private readonly TextRunProperties _defaultRunProperties;

        public EmojiTextParagraphProperties(TextRunProperties defaultRunProperties)
            => _defaultRunProperties = defaultRunProperties;

        public override FlowDirection FlowDirection => FlowDirection.LeftToRight;
        public override TextAlignment TextAlignment => TextAlignment.Left;
        public override double LineHeight => 0d;
        public override bool FirstLineInParagraph => true;
        public override bool AlwaysCollapsible => false;
        public override TextRunProperties DefaultTextRunProperties => _defaultRunProperties;
        public override TextWrapping TextWrapping => TextWrapping.NoWrap;
        public override TextMarkerProperties TextMarkerProperties => null!;
        public override double Indent => 0d;
    }
}
