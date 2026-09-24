using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace StswExpress.Wpf;

/// <summary>
/// Displays text with vector color emoji rendered from an installed OpenType color font.
/// </summary>
/// <remarks>
/// Emoji are rendered locally from <see cref="EmojiFontFamily"/> when explicitly overridden on the control,
/// otherwise from <see cref="StswSettingsModel.EmojiFontFamily"/>. No emoji images are downloaded and no disk
/// cache is used. Unsupported glyph formats fall back to normal text rendering.
/// </remarks>
public class StswEmojiText : TextBlock
{
    private static readonly Lazy<Typeface> SystemEmojiTypeface = new(CreateSystemEmojiTypeface);
    private static readonly Lazy<Typeface> RegisteredEmojiTypeface = new(CreateRegisteredEmojiTypeface);
    private static readonly object EmojiTypefaceCacheLock = new();
    private static readonly Dictionary<string, Typeface> EmojiTypefaceCache = new(StringComparer.OrdinalIgnoreCase);

    static StswEmojiText()
    {
        FontFamilyProperty.OverrideMetadata(typeof(StswEmojiText), new FrameworkPropertyMetadata(SystemFonts.MessageFontFamily, OnRelevantPropertyChanged));
        FontSizeProperty.OverrideMetadata(typeof(StswEmojiText), new FrameworkPropertyMetadata(12d, OnRelevantPropertyChanged));
        FontStretchProperty.OverrideMetadata(typeof(StswEmojiText), new FrameworkPropertyMetadata(FontStretches.Normal, OnRelevantPropertyChanged));
        FontStyleProperty.OverrideMetadata(typeof(StswEmojiText), new FrameworkPropertyMetadata(FontStyles.Normal, OnRelevantPropertyChanged));
        FontWeightProperty.OverrideMetadata(typeof(StswEmojiText), new FrameworkPropertyMetadata(FontWeights.Normal, OnRelevantPropertyChanged));
        ForegroundProperty.OverrideMetadata(typeof(StswEmojiText), new FrameworkPropertyMetadata(Brushes.Black, OnRelevantPropertyChanged));
    }

    public StswEmojiText()
    {
        Loaded += (_, _) => RefreshInlines();
    }

    #region Dependency properties
    /// <summary>
    /// Gets or sets a multiplier for the rendered emoji size relative to <see cref="TextBlock.FontSize"/>.
    /// </summary>
    /// <remarks>
    /// The property keeps its historical name for source compatibility. Emoji are now vector drawings rather
    /// than bitmap images.
    /// </remarks>
    public double EmojiImageSizeMultiplier
    {
        get => (double)GetValue(EmojiImageSizeMultiplierProperty);
        set => SetValue(EmojiImageSizeMultiplierProperty, value);
    }
    public static readonly DependencyProperty EmojiImageSizeMultiplierProperty
        = DependencyProperty.Register(
            nameof(EmojiImageSizeMultiplier),
            typeof(double),
            typeof(StswEmojiText),
            new FrameworkPropertyMetadata(1d, OnRelevantPropertyChanged, CoerceEmojiSizeMultiplier)
        );

    /// <summary>
    /// Gets or sets a control-specific font used as the source of color emoji glyphs.
    /// </summary>
    /// <remarks>
    /// When this property is not explicitly set (directly or by a style), the control uses
    /// <see cref="StswSettingsModel.EmojiFontFamily"/>.
    /// </remarks>
    public FontFamily EmojiFontFamily
    {
        get => (FontFamily)GetValue(EmojiFontFamilyProperty);
        set => SetValue(EmojiFontFamilyProperty, value);
    }
    public static readonly DependencyProperty EmojiFontFamilyProperty
        = DependencyProperty.Register(
            nameof(EmojiFontFamily),
            typeof(FontFamily),
            typeof(StswEmojiText),
            new FrameworkPropertyMetadata(new FontFamily("Segoe UI Emoji"), OnRelevantPropertyChanged)
        );

    /// <summary>
    /// Gets or sets the text to display, which may contain emoji sequences.
    /// </summary>
    public string EmojiText
    {
        get => (string)GetValue(EmojiTextProperty);
        set => SetValue(EmojiTextProperty, value);
    }
    public static readonly DependencyProperty EmojiTextProperty
        = DependencyProperty.Register(
            nameof(EmojiText),
            typeof(string),
            typeof(StswEmojiText),
            new FrameworkPropertyMetadata(string.Empty, OnRelevantPropertyChanged)
        );

    /// <summary>
    /// Legacy property retained for binary/source compatibility. Network emoji sources are no longer used.
    /// </summary>
    [Obsolete("StswEmojiText now renders emoji locally from EmojiFontFamily. EmojiSourceTemplate is ignored.")]
    public string EmojiSourceTemplate
    {
        get => (string)GetValue(EmojiSourceTemplateProperty);
        set => SetValue(EmojiSourceTemplateProperty, value);
    }
    public static readonly DependencyProperty EmojiSourceTemplateProperty
        = DependencyProperty.Register(
            nameof(EmojiSourceTemplate),
            typeof(string),
            typeof(StswEmojiText),
            new FrameworkPropertyMetadata(string.Empty)
        );

    /// <summary>
    /// Legacy property retained for binary/source compatibility. Variation selectors are now passed directly to
    /// WPF's text shaping engine and are no longer used to build image URLs.
    /// </summary>
    [Obsolete("Variation selectors are handled by the local font shaping engine. PreserveVariationSelector is ignored.")]
    public bool PreserveVariationSelector
    {
        get => (bool)GetValue(PreserveVariationSelectorProperty);
        set => SetValue(PreserveVariationSelectorProperty, value);
    }
    public static readonly DependencyProperty PreserveVariationSelectorProperty
        = DependencyProperty.Register(
            nameof(PreserveVariationSelector),
            typeof(bool),
            typeof(StswEmojiText),
            new FrameworkPropertyMetadata(false)
        );

    private static object CoerceEmojiSizeMultiplier(DependencyObject d, object baseValue)
    {
        var value = (double)baseValue;
        return double.IsNaN(value) || double.IsInfinity(value) || value <= 0d ? 1d : value;
    }

    private static void OnRelevantPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (StswEmojiText)d;
        if (control.IsLoaded)
            control.RefreshInlines();
    }
    #endregion

    #region Logic
    /// <summary>
    /// Rebuilds the inline collection, replacing supported emoji grapheme clusters with local vector drawings.
    /// </summary>
    private void RefreshInlines()
    {
        var text = EmojiText ?? string.Empty;
        Inlines.Clear();

        if (string.IsNullOrEmpty(text))
            return;

        var dpi = VisualTreeHelper.GetDpi(this);
        var emojiSize = Math.Max(1d, FontSize * EmojiImageSizeMultiplier);
        var emojiTypeface = ResolveEmojiTypeface();

        foreach (string segment in EnumerateTextElements(text))
        {
            if (LooksLikeEmoji(segment) &&
                TryRenderEmojiWithFallback(
                    segment,
                    emojiTypeface,
                    emojiSize,
                    dpi.PixelsPerDip,
                    out DrawingImage? drawing))
            {
                Inlines.Add(CreateEmojiInline(drawing, emojiSize));
            }
            else
            {
                Inlines.Add(CreateTextRun(segment));
            }
        }
    }

    /// <summary>
    /// Attempts to render an emoji using the configured physical typeface and then the original Windows
    /// Segoe UI Emoji font file. The fallback is resolved by physical font URI, not only by family name.
    /// </summary>
    private bool TryRenderEmojiWithFallback(
        string text,
        Typeface primaryTypeface,
        double emojiSize,
        double pixelsPerDip,
        out DrawingImage? drawing)
    {
        Typeface systemTypeface = SystemEmojiTypeface.Value;
        bool primaryIsSystemTypeface = AreSamePhysicalTypeface(primaryTypeface, systemTypeface);

        // The physical Windows Segoe UI Emoji font does not contain country flags.
        // If another font is registered under "Segoe UI Emoji", try it first for regional-indicator flags.
        if (primaryIsSystemTypeface && IsCountryFlag(text))
        {
            Typeface registeredTypeface = RegisteredEmojiTypeface.Value;
            if (!AreSamePhysicalTypeface(systemTypeface, registeredTypeface) &&
                StswColorEmojiRenderer.TryRender(
                    text,
                    registeredTypeface,
                    emojiSize,
                    Foreground,
                    pixelsPerDip,
                    allowModernColorFallback: true,
                    out drawing))
            {
                return true;
            }
        }

        if (StswColorEmojiRenderer.TryRender(
            text,
            primaryTypeface,
            emojiSize,
            Foreground,
            pixelsPerDip,
            allowModernColorFallback: !primaryIsSystemTypeface,
            out drawing))
        {
            return true;
        }

        if (!primaryIsSystemTypeface &&
            StswColorEmojiRenderer.TryRender(
                text,
                systemTypeface,
                emojiSize,
                Foreground,
                pixelsPerDip,
                allowModernColorFallback: false,
                out drawing))
        {
            return true;
        }

        Typeface fallbackTypeface = RegisteredEmojiTypeface.Value;
        if (!AreSamePhysicalTypeface(primaryTypeface, fallbackTypeface) &&
            !AreSamePhysicalTypeface(systemTypeface, fallbackTypeface) &&
            StswColorEmojiRenderer.TryRender(
                text,
                fallbackTypeface,
                emojiSize,
                Foreground,
                pixelsPerDip,
                allowModernColorFallback: true,
                out drawing))
        {
            return true;
        }

        drawing = null;
        return false;
    }

    /// <summary>
    /// Resolves the effective emoji typeface. Font-file references such as
    /// <c>seguiemj.ttf#Segoe UI Emoji</c> are resolved by enumerating the physical file's directory and selecting
    /// the typeface whose <see cref="GlyphTypeface.FontUri"/> points to that exact file. This bypasses installed
    /// per-user font-family overrides.
    /// </summary>
    private Typeface ResolveEmojiTypeface()
    {
        var valueSource = DependencyPropertyHelper.GetValueSource(this, EmojiFontFamilyProperty);
        if (valueSource.BaseValueSource != BaseValueSource.Default)
        {
            string localSource = EmojiFontFamily.Source;
            return ResolveTypefaceSource(localSource);
        }

        string configuredSource = StswApp.Settings?.EmojiFontFamily ?? string.Empty;
        if (string.IsNullOrWhiteSpace(configuredSource))
            configuredSource = "seguiemj.ttf#Segoe UI Emoji";

        return ResolveTypefaceSource(configuredSource);
    }

    private static Typeface ResolveTypefaceSource(string source)
    {
        source = string.IsNullOrWhiteSpace(source)
            ? "seguiemj.ttf#Segoe UI Emoji"
            : source.Trim();

        lock (EmojiTypefaceCacheLock)
        {
            if (EmojiTypefaceCache.TryGetValue(source, out Typeface? cached))
                return cached;
        }

        Typeface resolved;
        if (TryResolveFontFileReference(source, out string? fontPath, out string? familyName) &&
            TryCreateTypefaceFromFile(fontPath, familyName, out Typeface? fileTypeface))
        {
            resolved = fileTypeface;
        }
        else
        {
            try
            {
                resolved = new Typeface(
                    new FontFamily(source),
                    FontStyles.Normal,
                    FontWeights.Normal,
                    FontStretches.Normal);
            }
            catch (ArgumentException)
            {
                resolved = new Typeface(
                    new FontFamily("Segoe UI Emoji"),
                    FontStyles.Normal,
                    FontWeights.Normal,
                    FontStretches.Normal);
            }
        }

        lock (EmojiTypefaceCacheLock)
            EmojiTypefaceCache[source] = resolved;

        return resolved;
    }

    private static Typeface CreateSystemEmojiTypeface()
    {
        string fontsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
        string fontPath = Path.Combine(fontsDirectory, "seguiemj.ttf");

        if (TryCreateTypefaceFromFile(fontPath, "Segoe UI Emoji", out Typeface? typeface))
        {
            return typeface;
        }

        var fallback = new Typeface(
            new FontFamily("Segoe UI Emoji"),
            FontStyles.Normal,
            FontWeights.Normal,
            FontStretches.Normal);
        return fallback;
    }

    private static Typeface CreateRegisteredEmojiTypeface()
    {
        var typeface = new Typeface(
            new FontFamily("Segoe UI Emoji"),
            FontStyles.Normal,
            FontWeights.Normal,
            FontStretches.Normal);
        return typeface;
    }

    private static bool TryResolveFontFileReference(
        string source,
        out string fontPath,
        out string? familyName)
    {
        fontPath = string.Empty;
        familyName = null;

        int separatorIndex = source.LastIndexOf('#');
        if (separatorIndex <= 0)
            return false;

        string location = source[..separatorIndex].Trim();
        familyName = source[(separatorIndex + 1)..].Trim();
        if (string.IsNullOrWhiteSpace(location))
            return false;

        string extension;
        try
        {
            if (Uri.TryCreate(location, UriKind.Absolute, out Uri? uri) && uri.IsFile)
            {
                fontPath = uri.LocalPath;
            }
            else if (Path.IsPathRooted(location))
            {
                fontPath = location;
            }
            else
            {
                extension = Path.GetExtension(location);
                if (!IsSupportedFontExtension(extension))
                    return false;

                string fontsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
                fontPath = Path.Combine(fontsDirectory, location);
            }

            extension = Path.GetExtension(fontPath);
            if (!IsSupportedFontExtension(extension))
                return false;

            fontPath = Path.GetFullPath(fontPath);
            return File.Exists(fontPath);
        }
        catch
        {
            fontPath = string.Empty;
            return false;
        }
    }

    private static bool TryCreateTypefaceFromFile(
        string fontPath,
        string? requestedFamilyName,
        out Typeface? typeface)
    {
        typeface = null;

        try
        {
            if (string.IsNullOrWhiteSpace(fontPath) || !File.Exists(fontPath))
                return false;

            string normalizedPath = Path.GetFullPath(fontPath);
            var fontUri = new Uri(normalizedPath, UriKind.Absolute);

            Typeface? firstPhysicalMatch = null;
            foreach (Typeface candidate in Fonts.GetTypefaces(fontUri.AbsoluteUri))
            {
                if (!candidate.TryGetGlyphTypeface(out GlyphTypeface? glyphTypeface) ||
                    glyphTypeface.FontUri is null ||
                    !glyphTypeface.FontUri.IsFile)
                {
                    continue;
                }

                string candidatePath;
                try
                {
                    candidatePath = Path.GetFullPath(glyphTypeface.FontUri.LocalPath);
                }
                catch
                {
                    continue;
                }

                if (!string.Equals(candidatePath, normalizedPath, StringComparison.OrdinalIgnoreCase))
                    continue;

                firstPhysicalMatch ??= candidate;

                if (string.IsNullOrWhiteSpace(requestedFamilyName) ||
                    TypefaceMatchesFamily(candidate, requestedFamilyName))
                {
                    typeface = candidate;
                    return true;
                }
            }

            typeface = firstPhysicalMatch;
            return typeface is not null;
        }
        catch
        {
            typeface = null;
            return false;
        }
    }

    private static bool TypefaceMatchesFamily(Typeface typeface, string requestedFamilyName)
    {
        if (typeface.FontFamily.FamilyNames.Values.Any(
            x => string.Equals(x, requestedFamilyName, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        string source = typeface.FontFamily.Source;
        int separatorIndex = source.LastIndexOf('#');
        string familyName = separatorIndex >= 0 ? source[(separatorIndex + 1)..] : source;
        return string.Equals(familyName, requestedFamilyName, StringComparison.OrdinalIgnoreCase);
    }

    private static bool AreSamePhysicalTypeface(Typeface left, Typeface right)
    {
        if (!left.TryGetGlyphTypeface(out GlyphTypeface? leftGlyph) ||
            !right.TryGetGlyphTypeface(out GlyphTypeface? rightGlyph))
        {
            return false;
        }

        return Equals(leftGlyph.FontUri, rightGlyph.FontUri);
    }

    private static bool IsSupportedFontExtension(string extension)
        => extension.Equals(".ttf", StringComparison.OrdinalIgnoreCase) ||
           extension.Equals(".otf", StringComparison.OrdinalIgnoreCase) ||
           extension.Equals(".ttc", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Creates an ordinary text run matching the control's text appearance.
    /// </summary>
    private Run CreateTextRun(string text) => new(text)
    {
        FontFamily = FontFamily,
        FontSize = FontSize,
        FontStretch = FontStretch,
        FontStyle = FontStyle,
        FontWeight = FontWeight,
        Foreground = Foreground,
    };

    /// <summary>
    /// Wraps the vector emoji drawing in an inline image so TextBlock keeps its native wrapping/trimming behavior.
    /// </summary>
    private static InlineUIContainer CreateEmojiInline(DrawingImage drawing, double size)
    {
        var image = new Image
        {
            Source = drawing,
            Width = size,
            Height = size,
            Stretch = Stretch.Uniform,
            SnapsToDevicePixels = true,
            UseLayoutRounding = true,
        };

        return new InlineUIContainer(image)
        {
            BaselineAlignment = BaselineAlignment.Center,
        };
    }

    /// <summary>
    /// Performs a cheap pre-filter before invoking the text shaper. Final support is determined by the color font.
    /// </summary>
    private static bool IsCountryFlag(string textElement)
    {
        int regionalIndicators = 0;

        foreach (int codePoint in EnumerateCodePoints(textElement))
        {
            if (codePoint >= 0x1F1E6 && codePoint <= 0x1F1FF)
            {
                regionalIndicators++;
                continue;
            }

            if (codePoint == 0xFE0F)
                continue;

            return false;
        }

        return regionalIndicators == 2;
    }

    private static bool LooksLikeEmoji(string textElement)
    {
        if (string.IsNullOrEmpty(textElement))
            return false;

        foreach (int codePoint in EnumerateCodePoints(textElement))
        {
            if (codePoint is 0x200D or 0xFE0F or 0x20E3)
                return true;

            if ((codePoint >= 0x1F000 && codePoint <= 0x1FAFF) ||
                (codePoint >= 0x2600 && codePoint <= 0x27BF) ||
                (codePoint >= 0x2300 && codePoint <= 0x23FF) ||
                (codePoint >= 0x2B00 && codePoint <= 0x2BFF))
                return true;
        }

        return false;
    }

    private static IEnumerable<int> EnumerateCodePoints(string value)
    {
        for (var i = 0; i < value.Length; i++)
        {
            int codePoint = char.ConvertToUtf32(value, i);
            if (char.IsHighSurrogate(value[i]))
                i++;

            yield return codePoint;
        }
    }

    private static IEnumerable<string> EnumerateTextElements(string text)
    {
        TextElementEnumerator enumerator = StringInfo.GetTextElementEnumerator(text);
        while (enumerator.MoveNext())
            yield return enumerator.GetTextElement();
    }
    #endregion
}
