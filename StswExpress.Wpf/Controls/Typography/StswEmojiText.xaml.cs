using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StswExpress.Wpf;

public class StswEmojiText : TextBlock
{
    private static readonly string DiskCacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), nameof(StswExpress), "EmojiCache");
    private static readonly ConcurrentDictionary<string, Task<BitmapSource?>> EmojiCache = new();
    private static readonly HttpClient HttpClient = new();
    private int _renderVersion;

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
        Loaded += async (_, _) => await RefreshInlinesAsync();
    }

    #region Dependency properties
    /// <summary>
    /// Gets or sets a multiplier for the size of the emoji images relative to the current font size. The actual size of the emoji images will be calculated as FontSize multiplied by this multiplier. For example, if FontSize is 12 and EmojiImageSizeMultiplier is 1.5, the emoji images will be rendered at a size of 18. This allows you to adjust the size of the emojis independently from the text, ensuring that they are visually balanced and appropriately scaled within the control.
    /// </summary>
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
            new FrameworkPropertyMetadata(1d, OnRelevantPropertyChanged)
        );

    /// <summary>
    /// Gets or sets a URL template for loading emoji images. The template should contain a "{0}" placeholder, which will be replaced with the emoji key derived from the text. For example, the default template "https://cdnjs.cloudflare.com/ajax/libs/twemoji/14.0.2/72x72/{0}.png" will load emoji images from the Twemoji CDN, where "{0}" is replaced by the hexadecimal code points of the emoji. You can customize this template to use a different source for emoji images if desired.
    /// </summary>
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
            new FrameworkPropertyMetadata("https://cdnjs.cloudflare.com/ajax/libs/twemoji/14.0.2/72x72/{0}.png", OnRelevantPropertyChanged)
        );

    /// <summary>
    /// Gets or sets the text to display, which may include emojis. The control will attempt to detect emojis in the text and replace them with images based on the provided template.
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
    /// Gets or sets a value indicating whether to preserve the variation selector (U+FE0F) in the emoji key generation. The variation selector is used in Unicode to specify that a character should be displayed as an emoji rather than as text. By default, this property is set to false, which means that the variation selector will be ignored when generating the emoji key. Setting it to true will include the variation selector in the key, which may be necessary for certain emojis that have different appearances based on its presence.
    /// </summary>
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
            new FrameworkPropertyMetadata(false, OnRelevantPropertyChanged)
        );
    private static void OnRelevantPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var stsw = (StswEmojiText)d;
        if (stsw.IsLoaded)
            _ = stsw.RefreshInlinesAsync();
    }
    #endregion

    #region Logic
    /// <summary>
    /// Refreshes the collection of inlines by parsing the current text and replacing emoji sequences with their corresponding inline images asynchronously.
    /// </summary>
    /// <remarks>This method updates the inlines in a thread-safe manner by using a versioning system to ensure that only the most recent refresh operation applies its changes to the UI. It first splits the input text into segments, identifies which segments are emojis, and then loads the corresponding images for those emojis asynchronously. Once all images are loaded, it updates the Inlines collection on the UI thread, ensuring that any intermediate changes from older refresh operations are discarded if a newer refresh has been initiated.</remarks>
    /// <returns>A task that represents the asynchronous refresh operation.</returns>
    private async Task RefreshInlinesAsync()
    {
        var version = Interlocked.Increment(ref _renderVersion);
        var text = EmojiText ?? string.Empty;
        var emojiSourceTemplate = EmojiSourceTemplate;
        var preserveVariationSelector = PreserveVariationSelector;

        if (string.IsNullOrEmpty(text))
        {
            await Dispatcher.InvokeAsync(() => Inlines.Clear());
            return;
        }

        var preparedSegments = new List<PreparedSegment>();
        var tasks = new List<Task>();

        foreach (string segment in EnumerateTextElements(text))
        {
            if (!LooksLikeEmoji(segment))
            {
                preparedSegments.Add(new PreparedSegment(segment, false, null));
                continue;
            }

            string? emojiKey = BuildEmojiKey(segment, preserveVariationSelector);
            if (string.IsNullOrWhiteSpace(emojiKey))
            {
                preparedSegments.Add(new PreparedSegment(segment, false, null));
                continue;
            }

            var prepared = new PreparedSegment(segment, true, null);
            preparedSegments.Add(prepared);
            tasks.Add(LoadBitmapIntoSegmentAsync(prepared, emojiSourceTemplate, emojiKey));
        }

        if (tasks.Count > 0)
            await Task.WhenAll(tasks).ConfigureAwait(false);

        if (version != _renderVersion)
            return;

        await Dispatcher.InvokeAsync(() =>
        {
            if (version != _renderVersion)
                return;

            Inlines.Clear();

            foreach (PreparedSegment segment in preparedSegments)
            {
                if (segment.IsEmoji && segment.Bitmap is not null)
                    Inlines.Add(CreateEmojiInline(segment.Bitmap));
                else
                    Inlines.Add(CreateTextRun(segment.Text));
            }
        });
    }

    /// <summary>
    /// Asynchronously loads a bitmap image for the specified emoji and assigns it to the provided segment.
    /// </summary>
    /// <remarks>If the emoji image cannot be loaded, the segment's bitmap will be set to <see langword="null"/>.</remarks>
    /// <param name="segment">The segment to which the loaded bitmap image will be assigned. Cannot be <see langword="null"/>.</param>
    /// <param name="emojiSourceTemplate">A template string used to locate or generate the source URI for the emoji image. Cannot be <see langword="null"/> or empty.</param>
    /// <param name="emojiKey">The key identifying the specific emoji to load. Cannot be <see langword="null"/> or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task completes when the bitmap has been loaded and assigned to the segment.</returns>
    private static async Task LoadBitmapIntoSegmentAsync(PreparedSegment segment, string emojiSourceTemplate, string emojiKey)
    {
        try
        {
            segment.Bitmap = await GetOrDownloadEmojiAsync(emojiSourceTemplate, emojiKey).ConfigureAwait(false);
        }
        catch
        {
            segment.Bitmap = null;
        }
    }

    /// <summary>
    /// Creates a new <see cref="Run"/> instance containing the specified text and applies the current font and foreground settings.
    /// </summary>
    /// <remarks>The returned <see cref="Run"/> uses the same font family, size, stretch, style, weight, and foreground brush as the <see cref="StswEmojiText"/> control. This ensures that the text segments are visually consistent with the overall styling of the control, while allowing emojis to be rendered as images in line with the text.</remarks>
    /// <param name="text">The text to display in the created <see cref="Run"/>.</param>
    /// <returns>A <see cref="Run"/> object initialized with the specified text and the current font and foreground properties.</returns>
    private Run CreateTextRun(string text) => new Run(text)
    {
        FontFamily = FontFamily,
        FontSize = FontSize,
        FontStretch = FontStretch,
        FontStyle = FontStyle,
        FontWeight = FontWeight,
        Foreground = Foreground,
    };

    /// <summary>
    /// Creates an InlineUIContainer element containing an Image control to display the emoji. The size of the image is determined by multiplying the current font size by the EmojiImageSizeMultiplier property, ensuring that the emoji is appropriately scaled relative to the text. The image is set to stretch uniformly and is aligned to the center of the text baseline for proper vertical alignment. Additionally, bitmap scaling mode is set to HighQuality to ensure that the emoji images are rendered with good visual quality, even when resized.
    /// </summary>
    /// <param name="bitmap">The BitmapSource containing the emoji image to be displayed. This bitmap is typically loaded from a cache or downloaded based on the emoji key derived from the text. The method will create an Image control using this bitmap and wrap it in an InlineUIContainer for display within the TextBlock.</param>
    /// <returns>An InlineUIContainer element that contains an Image control displaying the emoji. This InlineUIContainer can be added to the Inlines collection of the TextBlock to render the emoji in line with the text.</returns>
    private InlineUIContainer CreateEmojiInline(BitmapSource bitmap)
    {
        var size = Math.Max(8d, FontSize * EmojiImageSizeMultiplier);
        var image = new Image
        {
            Source = bitmap,
            Width = size,
            Height = size,
            Stretch = Stretch.Uniform,
            SnapsToDevicePixels = true,
            UseLayoutRounding = true,
        };

        RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);
        return new InlineUIContainer(image)
        {
            BaselineAlignment = BaselineAlignment.Center,
        };
    }

    /// <summary>
    /// Retrieves the emoji image from the cache if it exists; otherwise, it attempts to download the image using the provided source template and emoji key. The method constructs a cache key by combining the emoji source template and the emoji key, then checks if a Task for that key already exists in the cache. If it does, it awaits the existing Task; if not, it creates a new Task to download the emoji and adds it to the cache. If the download is successful, the resulting BitmapSource is returned. If any error occurs during the download or if the source template is invalid, the method returns <see langword="null"/>. This caching mechanism ensures that each unique emoji is downloaded only once, improving performance when rendering multiple instances of the same emoji.
    /// </summary>
    /// <param name="emojiSourceTemplate">A string template for the emoji image source URL, which must contain a "{0}" placeholder that will be replaced with the emoji key. For example, "https://cdnjs.cloudflare.com/ajax/libs/twemoji/14.0.2/72x72/{0}.png".</param>
    /// <param name="emojiKey">The key representing the emoji, typically a string of hexadecimal code points separated by hyphens. This key is used to replace the "{0}" placeholder in the source template to form the complete URI for downloading the emoji image.</param>
    /// <returns>A Task that represents the asynchronous operation of retrieving the emoji image. The result of the Task is a BitmapSource containing the loaded emoji image if the download and caching were successful; otherwise, it returns <see langword="null"/>.</returns>
    private static async Task<BitmapSource?> GetOrDownloadEmojiAsync(string emojiSourceTemplate, string emojiKey)
    {
        if (string.IsNullOrWhiteSpace(emojiSourceTemplate) || !emojiSourceTemplate.Contains("{0}", StringComparison.Ordinal))
            return null;

        string cacheKey = $"{emojiSourceTemplate}|{emojiKey}";
        Task<BitmapSource?> task = EmojiCache.GetOrAdd(cacheKey, _ => DownloadEmojiAsync(emojiSourceTemplate, emojiKey));

        try
        {
            return await task.ConfigureAwait(false);
        }
        catch
        {
            EmojiCache.TryRemove(cacheKey, out _);
            return null;
        }
    }

    /// <summary>
    /// Downloads the emoji image based on the provided source template and emoji key. The method constructs the URI by replacing the "{0}" placeholder in the template with the emoji key, then attempts to download the image as a byte array. If the download is successful, it creates a BitmapImage from the byte array and returns it. If any error occurs during the download or image creation process, the method returns null, indicating that the emoji could not be loaded. This method is designed to be used in conjunction with caching to avoid redundant downloads of the same emoji.
    /// </summary>
    /// <param name="emojiSourceTemplate">A string template for the emoji image source URL, which must contain a "{0}" placeholder that will be replaced with the emoji key. For example, "https://cdnjs.cloudflare.com/ajax/libs/twemoji/14.0.2/72x72/{0}.png".</param>
    /// <param name="emojiKey">The key representing the emoji, typically a string of hexadecimal code points separated by hyphens. This key is used to replace the "{0}" placeholder in the source template to form the complete URI for downloading the emoji image.</param>
    /// <returns>A Task that represents the asynchronous operation of downloading the emoji image. The result of the Task is a BitmapSource containing the loaded emoji image if the download and image creation were successful; otherwise, it returns <see langword="null"/>.</returns>
    private static async Task<BitmapSource?> DownloadEmojiAsync(string emojiSourceTemplate, string emojiKey)
    {
        try
        {
            Directory.CreateDirectory(DiskCacheFolder);

            var filePath = Path.Combine(DiskCacheFolder, emojiKey + ".png");
            if (File.Exists(filePath))
            {
                await using var localStream = File.OpenRead(filePath);
                return LoadBitmap(localStream);
            }

            var uri = string.Format(CultureInfo.InvariantCulture, emojiSourceTemplate, emojiKey);
            var bytes = await HttpClient.GetByteArrayAsync(uri).ConfigureAwait(false);
            await File.WriteAllBytesAsync(filePath, bytes).ConfigureAwait(false);

            await using var stream = new MemoryStream(bytes);
            return LoadBitmap(stream);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Creates a bitmap image from the specified stream, loading the image data into memory and preserving the original pixel format.
    /// </summary>
    /// <remarks>The method loads the entire image into memory and freezes the resulting BitmapSource for thread safety. The caller is responsible for disposing the stream after use.</remarks>
    /// <param name="stream">The stream containing the image data to load. The stream must be readable and positioned at the start of the image data.</param>
    /// <returns>A frozen BitmapSource representing the loaded image. The returned object is immutable and can be safely shared across threads.</returns>
    private static BitmapImage LoadBitmap(Stream stream)
    {
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
        image.StreamSource = stream;
        image.EndInit();
        image.Freeze();
        return image;
    }

    /// <summary>
    /// Determines whether the given text element looks like an emoji. This is done by checking if the text element contains any Unicode code points that are commonly associated with emojis, such as those in the ranges U+1F000 to U+1FAFF, U+2600 to U+27BF, U+2300 to U+23FF, or U+2B00 to U+2BFF. Additionally, it checks for the presence of the zero-width joiner (U+200D) and variation selector (U+FE0F), which are often used in emoji sequences. If any of these code points are found in the text element, it is considered to look like an emoji.
    /// </summary>
    /// <param name="textElement">The text element to check, which may consist of one or more Unicode code points. This is typically a string that has been extracted from the input text and is suspected to be an emoji based on its content. The method will analyze the code points in this string to determine if it resembles an emoji.</param>
    /// <returns><see langword="true"/> if the text element looks like an emoji based on the presence of certain Unicode code points; otherwise, <see langword="false"/>.</returns>
    private static bool LooksLikeEmoji(string textElement)
    {
        if (string.IsNullOrEmpty(textElement))
            return false;

        foreach (int codePoint in EnumerateCodePoints(textElement))
        {
            if (codePoint is 0x200D or 0xFE0F)
                return true;

            if ((codePoint >= 0x1F000 && codePoint <= 0x1FAFF) ||
                (codePoint >= 0x2600 && codePoint <= 0x27BF) ||
                (codePoint >= 0x2300 && codePoint <= 0x23FF) ||
                (codePoint >= 0x2B00 && codePoint <= 0x2BFF))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Builds a key for the emoji based on its Unicode code points. The key is generated by enumerating the code points in the text element, optionally skipping the variation selector (U+FE0F) if the preserveVariationSelector parameter is false. The remaining code points are then converted to their hexadecimal representation and concatenated with hyphens to form the final key. This key can be used to look up the corresponding emoji image in a source that uses this naming convention, such as the Twemoji CDN.
    /// </summary>
    /// <param name="textElement">The text element representing the emoji, which may consist of one or more Unicode code points. This is typically a string that has been extracted from the input text and is suspected to be an emoji based on its content. The method will process this string to generate a key that can be used to retrieve the appropriate emoji image.</param>
    /// <param name="preserveVariationSelector">A boolean flag indicating whether to include the variation selector (U+FE0F) in the emoji key generation. If set to false, the variation selector will be ignored, which may be appropriate for certain emojis that do not require it for correct rendering. If set to true, the variation selector will be included in the key, which may be necessary for emojis that have different appearances based on its presence.</param>
    /// <returns>A string representing the emoji key, which is a concatenation of the hexadecimal code points of the emoji, separated by hyphens. This key can be used to look up the corresponding emoji image in a source that uses this naming convention. If the input text element does not contain any valid code points after processing, the method returns <see langword="null"/>.</returns>
    private static string? BuildEmojiKey(string textElement, bool preserveVariationSelector)
    {
        var codePoints = new List<int>();

        foreach (int cp in EnumerateCodePoints(textElement))
        {
            if (!preserveVariationSelector && cp == 0xFE0F)
                continue;

            codePoints.Add(cp);
        }

        if (codePoints.Count == 0)
            return null;

        var sb = new StringBuilder();

        for (var i = 0; i < codePoints.Count; i++)
        {
            if (i > 0)
                sb.Append('-');

            sb.Append(codePoints[i].ToString("x", CultureInfo.InvariantCulture));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Enumerates the Unicode code points contained in the specified string.
    /// </summary>
    /// <remarks>The enumeration correctly handles surrogate pairs, ensuring that each Unicode code point is represented as a single integer, even if it is encoded as a surrogate pair in UTF-16. This method is essential for accurately processing text that may contain emojis or other characters outside the Basic Multilingual Plane (BMP).</remarks>
    /// <param name="value">The string to enumerate code points from. Cannot be <see langword="null"/>.</param>
    /// <returns>An enumerable collection of integers, where each integer represents a Unicode code point from the input string.</returns>
    private static IEnumerable<int> EnumerateCodePoints(string value)
    {
        for (var i = 0; i < value.Length; i++)
        {
            var codePoint = char.ConvertToUtf32(value, i);

            if (char.IsHighSurrogate(value[i]))
                i++;

            yield return codePoint;
        }
    }

    /// <summary>
    /// Enumerates the text elements of the specified string, returning each as a separate element.
    /// </summary>
    /// <remarks>A text element may consist of a single character or a sequence of characters that together form a single visual unit, such as an emoji composed of multiple code points. This method uses the StringInfo.GetTextElementEnumerator to ensure that complex characters are correctly identified and returned as individual elements.</remarks>
    /// <param name="text">The string to be parsed into text elements. Can be <see langword="null"/> or empty; in such cases, the returned sequence will be empty.</param>
    /// <returns>An enumerable collection of strings, each representing a text element in the input string. The collection will be empty if the input string is <see langword="null"/> or contains no text elements.</returns>
    private static IEnumerable<string> EnumerateTextElements(string text)
    {
        var enumerator = StringInfo.GetTextElementEnumerator(text);
        while (enumerator.MoveNext())
            yield return enumerator.GetTextElement();
    }
    #endregion

    /// <summary>
    /// Represents a segment of text that has been prepared for rendering, containing the original text, a flag indicating whether it is an emoji, and an optional bitmap if it is an emoji that has been successfully loaded. This class is used internally to store the results of processing the input text and preparing it for display in the control.
    /// </summary>
    /// <param name="text">The original text of the segment, which may be a single character, an emoji, or a sequence of characters. This is the raw text that was extracted from the input string before any processing to determine if it is an emoji.</param>
    /// <param name="isEmoji">A boolean flag indicating whether this segment is identified as an emoji. This is determined based on the presence of certain Unicode code points that are commonly used in emojis.</param>
    /// <param name="bitmap">An optional BitmapSource containing the loaded image for the emoji, if applicable. This will be <see langword="null"/> for non-emoji segments or if the emoji image failed to load.</param>
    private sealed class PreparedSegment(string text, bool isEmoji, BitmapSource? bitmap)
    {
        public string Text { get; } = text;
        public bool IsEmoji { get; } = isEmoji;
        public BitmapSource? Bitmap { get; set; } = bitmap;
    }
}
