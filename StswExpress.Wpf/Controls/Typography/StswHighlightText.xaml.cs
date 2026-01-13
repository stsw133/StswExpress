using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace StswExpress.Wpf;
/// <summary>
/// A TextBlock that highlights occurrences of <see cref="HighlightText"/> inside its content (Inlines or Text).
/// </summary>
public class StswHighlightText : TextBlock
{
    private bool _isUpdating;
    private bool _sourceCaptured;
    private readonly List<Inline> _sourceInlines = [];

    static StswHighlightText()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswHighlightText), new FrameworkPropertyMetadata(typeof(StswHighlightText)));
        TextProperty.OverrideMetadata(typeof(StswHighlightText), new FrameworkPropertyMetadata(OnTextChanged));
    }
    public StswHighlightText()
    {
        Loaded += (_, __) =>
        {
            CaptureSourceIfNeeded();
            UpdateHighlighting();
        };
    }

    #region Dependency properties
    /// <summary>
    /// Gets or sets the background brush used for highlighted text.
    /// </summary>
    public Brush? HighlightBackground
    {
        get => (Brush?)GetValue(HighlightBackgroundProperty);
        set => SetValue(HighlightBackgroundProperty, value);
    }
    public static readonly DependencyProperty HighlightBackgroundProperty
        = DependencyProperty.Register(
            nameof(HighlightBackground),
            typeof(Brush),
            typeof(StswHighlightText),
            new FrameworkPropertyMetadata(Brushes.Yellow, OnAnyHighlightSettingChanged)
        );

    /// <summary>
    /// Gets or sets the string comparison type used for highlighting.
    /// </summary>
    public StringComparison HighlightComparison
    {
        get => (StringComparison)GetValue(HighlightComparisonProperty);
        set => SetValue(HighlightComparisonProperty, value);
    }
    public static readonly DependencyProperty HighlightComparisonProperty
        = DependencyProperty.Register(
            nameof(HighlightComparison),
            typeof(StringComparison),
            typeof(StswHighlightText),
            new FrameworkPropertyMetadata(StringComparison.CurrentCultureIgnoreCase, OnAnyHighlightSettingChanged)
        );

    /// <summary>
    /// Gets or sets the foreground brush used for highlighted text.
    /// </summary>
    public Brush? HighlightForeground
    {
        get => (Brush?)GetValue(HighlightForegroundProperty);
        set => SetValue(HighlightForegroundProperty, value);
    }
    public static readonly DependencyProperty HighlightForegroundProperty
        = DependencyProperty.Register(
            nameof(HighlightForeground),
            typeof(Brush),
            typeof(StswHighlightText),
            new FrameworkPropertyMetadata(Brushes.Red, OnAnyHighlightSettingChanged)
        );

    /// <summary>
    /// Gets or sets the text to highlight within the control.
    /// </summary>
    public string? HighlightText
    {
        get => (string?)GetValue(HighlightTextProperty);
        set => SetValue(HighlightTextProperty, value);
    }
    public static readonly DependencyProperty HighlightTextProperty
        = DependencyProperty.Register(
            nameof(HighlightText),
            typeof(string),
            typeof(StswHighlightText),
            new FrameworkPropertyMetadata(default(string), OnAnyHighlightSettingChanged)
        );
    private static void OnAnyHighlightSettingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var stsw = (StswHighlightText)d;
        stsw.CaptureSourceIfNeeded();
        stsw.UpdateHighlighting();
    }
    #endregion

    #region Overrides
    /// <summary>
    /// Called when the Text property changes.
    /// </summary>
    /// <param name="d">The dependency object where the property changed.</param>
    /// <param name="e">The event data.</param>
    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var stsw = (StswHighlightText)d;
        stsw.CaptureSourceIfNeeded(forceFromText: true);
        stsw.UpdateHighlighting();
    }
    #endregion

    #region Logic
    /// <summary>
    /// Appends the given Inline content to this TextBlock, applying highlighting for occurrences of the specified text.
    /// </summary>
    /// <param name="src">The source Inline content to process.</param>
    /// <param name="highlight">The text to highlight within the Inline content.</param>
    private void AppendHighlighted(Inline src, string highlight)
    {
        if (src is Run run)
        {
            AppendHighlightedRun(run, highlight);
            return;
        }

        if (src is Span span)
        {
            var newSpan = (Span)CloneInlineShallow(span);
            foreach (var child in span.Inlines)
            {
                // Add into *this* TextBlock, but keep nesting by adding into newSpan.
                // We'll build newSpan content, then add newSpan.
            }

            newSpan.Inlines.Clear();
            foreach (var child in span.Inlines)
                AppendHighlightedInto(spanChild: child, highlight: highlight, target: newSpan.Inlines);

            Inlines.Add(newSpan);
            return;
        }

        Inlines.Add(CloneInline(src));
    }

    /// <summary>
    /// Appends the given Inline child into the target InlineCollection, applying highlighting for occurrences of the specified text.
    /// </summary>
    /// <param name="spanChild">The Inline child to process.</param>
    /// <param name="highlight">The text to highlight within the Inline child.</param>
    /// <param name="target">The target InlineCollection to append to.</param>
    private void AppendHighlightedInto(Inline spanChild, string highlight, InlineCollection target)
    {
        if (spanChild is Run run)
        {
            AppendHighlightedRun(run, highlight, target);
            return;
        }

        if (spanChild is Span span)
        {
            var newSpan = (Span)CloneInlineShallow(span);
            newSpan.Inlines.Clear();
            foreach (var child in span.Inlines)
                AppendHighlightedInto(child, highlight, newSpan.Inlines);

            target.Add(newSpan);
            return;
        }

        target.Add(CloneInline(spanChild));
    }

    /// <summary>
    /// Appends the given Run to this TextBlock, applying highlighting for occurrences of the specified text.
    /// </summary>
    /// <param name="run">The Run to process.</param>
    /// <param name="highlight">The text to highlight within the Run.</param>
    private void AppendHighlightedRun(Run run, string highlight) => AppendHighlightedRun(run, highlight, Inlines);

    /// <summary>
    /// Appends the given Run into the target InlineCollection, applying highlighting for occurrences of the specified text.
    /// </summary>
    /// <param name="run">The Run to process.</param>
    /// <param name="highlight">The text to highlight within the Run.</param>
    /// <param name="target">The target InlineCollection to append to.</param>
    private void AppendHighlightedRun(Run run, string highlight, InlineCollection target)
    {
        var text = run.Text ?? string.Empty;
        if (text.Length == 0)
            return;

        int index = 0;
        while (index < text.Length)
        {
            int matchIndex = text.IndexOf(highlight, index, HighlightComparison);
            if (matchIndex < 0)
            {
                target.Add(CloneRunWithText(run, text.Substring(index)));
                break;
            }

            if (matchIndex > index)
                target.Add(CloneRunWithText(run, text.Substring(index, matchIndex - index)));

            var matched = CloneRunWithText(run, text.Substring(matchIndex, highlight.Length));

            if (HighlightForeground != null)
                matched.Foreground = HighlightForeground;
            if (HighlightBackground != null)
                matched.Background = HighlightBackground;

            target.Add(matched);

            index = matchIndex + highlight.Length;
        }
    }

    /// <summary>
    /// Captures original content (Inlines if present; otherwise Text as a single Run).
    /// Captured only once by default, so authored XAML content is preserved.
    /// </summary>
    /// <param name="forceFromText">If set to <see langword="true"/>, captures from Text property even if Inlines are present.</param>
    private void CaptureSourceIfNeeded(bool forceFromText = false)
    {
        if (_sourceCaptured)
            return;

        if (Inlines.Count > 0 && !forceFromText)
        {
            _sourceInlines.Clear();
            foreach (var inline in Inlines)
                _sourceInlines.Add(CloneInline(inline));

            _sourceCaptured = true;
            return;
        }

        var text = Text;
        if (!string.IsNullOrEmpty(text))
        {
            _sourceInlines.Clear();
            _sourceInlines.Add(new Run(text));
            _sourceCaptured = true;
        }
    }

    /// <summary>
    /// Clones an Inline element, including its formatting and child Inlines if applicable.
    /// </summary>
    /// <param name="inline">The Inline element to clone.</param>
    /// <returns>The cloned Inline element.</returns>
    private static Inline CloneInline(Inline inline)
    {
        if (inline is Run run)
            return CloneRunWithText(run, run.Text ?? string.Empty);

        if (inline is Span span)
        {
            var s = (Span)CloneInlineShallow(span);
            s.Inlines.Clear();
            foreach (var child in span.Inlines)
                s.Inlines.Add(CloneInline(child));
            return s;
        }

        return CloneInlineShallow(inline);
    }

    /// <summary>
    /// Clones an Inline element shallowly, copying only its formatting without child Inlines.
    /// </summary>
    /// <param name="inline">The Inline element to clone.</param>
    /// <returns>The cloned Inline element.</returns>
    private static Inline CloneInlineShallow(Inline inline)
    {
        Inline clone;

        if (inline is Bold) clone = new Bold();
        else if (inline is Italic) clone = new Italic();
        else if (inline is Underline) clone = new Underline();
        else if (inline is Span) clone = new Span();
        else if (inline is Run) clone = new Run();
        else clone = new Run();

        CopyTextElementFormatting(inline, clone);
        return clone;
    }

    /// <summary>
    /// Clones a Run with the specified text, copying formatting from the source Run.
    /// </summary>
    /// <param name="source">The source Run to copy formatting from.</param>
    /// <param name="text">The text for the new Run.</param>
    /// <returns>The cloned Run with the specified text.</returns>
    private static Run CloneRunWithText(Run source, string text)
    {
        var r = new Run(text);
        CopyTextElementFormatting(source, r);
        return r;
    }

    /// <summary>
    /// Copies formatting from one TextElement to another.
    /// </summary>
    /// <param name="source">The source TextElement to copy formatting from.</param>
    /// <param name="target">The target TextElement to copy formatting to.</param>
    private static void CopyTextElementFormatting(TextElement source, TextElement target)
    {
        target.FontFamily = source.FontFamily;
        target.FontSize = source.FontSize;
        target.FontStyle = source.FontStyle;
        target.FontWeight = source.FontWeight;
        target.FontStretch = source.FontStretch;

        target.Foreground = source.Foreground;
        target.Background = source.Background;

        target.TextEffects = source.TextEffects;

        if (source is Run sourceRun && target is Run targetRun)
            targetRun.TextDecorations = sourceRun.TextDecorations;
        else if (source is Span sourceSpan && target is Span targetSpan)
            targetSpan.TextDecorations = sourceSpan.TextDecorations;
    }

    /// <summary>
    /// Manually re-capture current content as the new "source".
    /// Call this if you modify Inlines in code-behind after load and want highlighting to use the new content.
    /// </summary>
    public void RecaptureSource()
    {
        if (_isUpdating)
            return;

        _sourceInlines.Clear();
        foreach (var inline in Inlines)
            _sourceInlines.Add(CloneInline(inline));

        _sourceCaptured = true;
        UpdateHighlighting();
    }

    /// <summary>
    /// Updates the highlighting based on the current source content and highlight settings.
    /// </summary>
    private void UpdateHighlighting()
    {
        if (_isUpdating)
            return;

        _isUpdating = true;
        try
        {
            if (!_sourceCaptured)
                CaptureSourceIfNeeded();

            Inlines.Clear();

            if (_sourceInlines.Count == 0)
                return;

            var highlight = HighlightText;
            if (string.IsNullOrEmpty(highlight))
            {
                foreach (var src in _sourceInlines)
                    Inlines.Add(CloneInline(src));
                return;
            }

            foreach (var src in _sourceInlines)
            {
                AppendHighlighted(src, highlight);
            }
        }
        finally
        {
            _isUpdating = false;
        }
    }
    #endregion
}
