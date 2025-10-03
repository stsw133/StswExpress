using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// A custom text control that displays outlined text with adjustable stroke thickness and fill.
/// Supports text alignment, decorations, font customization, and wrapping options.
/// </summary>
/// <remarks>
/// The text is rendered with a configurable outline and fill, making it ideal for stylized UI elements.
/// </remarks>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswOutlinedText Text="Outlined Text" FontSize="24" FontWeight="Bold" Stroke="Red" Fill="Blue"/&gt;
/// </code>
/// </example>
[ContentProperty(nameof(Text))]
public class StswOutlinedText : FrameworkElement
{
    private FormattedText? _formattedText;
    private Geometry? _textGeometry;
    private Pen? _pen;

    public StswOutlinedText()
    {
        UpdatePen();
        TextDecorations = [];
    }
    static StswOutlinedText()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswOutlinedText), new FrameworkPropertyMetadata(typeof(StswOutlinedText)));
    }

    #region Events & methods
    /// <summary>
    /// Updates the pen used for drawing the text outline.
    /// Adjusts stroke properties such as thickness and line caps.
    /// </summary>
    private void UpdatePen()
    {
        var newPen = new Pen(Stroke, StrokeThickness)
        {
            DashCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round,
            LineJoin = PenLineJoin.Round,
            StartLineCap = PenLineCap.Round
        };

        if (!newPen.Equals(_pen))
        {
            _pen = newPen;
            InvalidateVisual();
        }
    }

    /// <inheritdoc/>
    protected override void OnRender(DrawingContext drawingContext)
    {
        EnsureGeometry();

        if (_textGeometry != null)
        {
            if (_pen != null)
                drawingContext.DrawGeometry(null, _pen, _textGeometry);
            if (Fill != null)
                drawingContext.DrawGeometry(Fill, null, _textGeometry);
        }
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        EnsureFormattedText();
        if (_formattedText != null)
        {
            _formattedText.MaxTextWidth = Math.Min(3579139, availableSize.Width);
            _formattedText.MaxTextHeight = Math.Max(0.0001d, availableSize.Height);
            return new Size(Math.Ceiling(_formattedText.Width), Math.Ceiling(_formattedText.Height));
        }
        return new Size();
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        EnsureFormattedText();
        if (_formattedText != null)
        {
            _formattedText.MaxTextWidth = finalSize.Width;
            _formattedText.MaxTextHeight = Math.Max(0.0001d, finalSize.Height);
        }
        _textGeometry = null;

        return finalSize;
    }

    /// <summary>
    /// Handles changes that require invalidating the formatted text and updating the visual display.
    /// </summary>
    /// <param name="dependencyObject">The dependency object that triggered the update.</param>
    /// <param name="e">Event data containing the changed property.</param>
    private static void OnFormattedTextInvalidated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        var outlinedTextBlock = (StswOutlinedText)dependencyObject;
        outlinedTextBlock._formattedText = null;
        outlinedTextBlock._textGeometry = null;

        outlinedTextBlock.InvalidateMeasure();
        outlinedTextBlock.InvalidateVisual();
    }

    /// <summary>
    /// Updates the formatted text properties when certain attributes such as font or text alignment change.
    /// </summary>
    /// <param name="dependencyObject">The dependency object that triggered the update.</param>
    /// <param name="e">Event data containing the changed property.</param>
    private static void OnFormattedTextUpdated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        var outlinedTextBlock = (StswOutlinedText)dependencyObject;
        outlinedTextBlock.UpdateFormattedText();
        outlinedTextBlock._textGeometry = null;

        outlinedTextBlock.InvalidateMeasure();
        outlinedTextBlock.InvalidateVisual();
    }

    /// <summary>
    /// Ensures that the formatted text object is initialized.
    /// Creates a new formatted text instance if necessary.
    /// </summary>
    private void EnsureFormattedText()
    {
        if (_formattedText != null)
            return;

        _formattedText = new FormattedText(Text ?? string.Empty, CultureInfo.CurrentUICulture, FlowDirection, new Typeface(FontFamily, FontStyle, FontWeight, FontStretch), FontSize, Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip);

        UpdateFormattedText();
    }

    /// <summary>
    /// Updates the properties of the formatted text object, such as font size, weight, alignment, and decorations.
    /// </summary>
    private void UpdateFormattedText()
    {
        if (_formattedText == null)
            return;

        _formattedText.MaxLineCount = TextWrapping == TextWrapping.NoWrap ? 1 : int.MaxValue;
        _formattedText.TextAlignment = TextAlignment;
        _formattedText.Trimming = TextTrimming;

        _formattedText.SetFontSize(FontSize);
        _formattedText.SetFontStyle(FontStyle);
        _formattedText.SetFontWeight(FontWeight);
        _formattedText.SetFontFamily(FontFamily);
        _formattedText.SetFontStretch(FontStretch);
        _formattedText.SetTextDecorations(TextDecorations);
    }

    /// <summary>
    /// Ensures that the text geometry is generated for rendering.
    /// </summary>
    private void EnsureGeometry()
    {
        if (_textGeometry != null)
            return;

        EnsureFormattedText();
        _textGeometry = _formattedText?.BuildGeometry(new Point(0, 0));
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the fill color of the text.
    /// The fill applies to the interior of the text glyphs.
    /// </summary>
    public Brush Fill
    {
        get => (Brush)GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }
    public static readonly DependencyProperty FillProperty
        = DependencyProperty.Register(
            nameof(Fill),
            typeof(Brush),
            typeof(StswOutlinedText),
            new FrameworkPropertyMetadata(Brushes.Black,
                FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the font family of the text.
    /// </summary>
    public FontFamily FontFamily
    {
        get => (FontFamily)GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }
    public static readonly DependencyProperty FontFamilyProperty
        = TextElement.FontFamilyProperty.AddOwner(
            typeof(StswOutlinedText),
            new FrameworkPropertyMetadata(OnFormattedTextUpdated)
        );

    /// <summary>
    /// Gets or sets the font size of the text.
    /// </summary>
    [TypeConverter(typeof(FontSizeConverter))]
    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }
    public static readonly DependencyProperty FontSizeProperty
        = TextElement.FontSizeProperty.AddOwner(
            typeof(StswOutlinedText),
            new FrameworkPropertyMetadata(OnFormattedTextUpdated)
        );

    /// <summary>
    /// Gets or sets the font stretch of the text.
    /// Controls how the font is expanded or condensed.
    /// </summary>
    public FontStretch FontStretch
    {
        get => (FontStretch)GetValue(FontStretchProperty);
        set => SetValue(FontStretchProperty, value);
    }
    public static readonly DependencyProperty FontStretchProperty
        = TextElement.FontStretchProperty.AddOwner(
            typeof(StswOutlinedText),
            new FrameworkPropertyMetadata(OnFormattedTextUpdated)
        );

    /// <summary>
    /// Gets or sets the font style of the text.
    /// Defines whether the text is normal, italic, or oblique.
    /// </summary>
    public FontStyle FontStyle
    {
        get => (FontStyle)GetValue(FontStyleProperty);
        set => SetValue(FontStyleProperty, value);
    }
    public static readonly DependencyProperty FontStyleProperty
        = TextElement.FontStyleProperty.AddOwner(
            typeof(StswOutlinedText),
            new FrameworkPropertyMetadata(OnFormattedTextUpdated)
        );

    /// <summary>
    /// Gets or sets the font weight of the text.
    /// Determines how bold the text appears.
    /// </summary>
    public FontWeight FontWeight
    {
        get => (FontWeight)GetValue(FontWeightProperty);
        set => SetValue(FontWeightProperty, value);
    }
    public static readonly DependencyProperty FontWeightProperty
        = TextElement.FontWeightProperty.AddOwner(
            typeof(StswOutlinedText),
            new FrameworkPropertyMetadata(OnFormattedTextUpdated)
        );

    /// <summary>
    /// Gets or sets the stroke (outline) color of the text.
    /// </summary>
    public Brush Stroke
    {
        get => (Brush)GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }
    public static readonly DependencyProperty StrokeProperty
        = DependencyProperty.Register(
            nameof(Stroke),
            typeof(Brush),
            typeof(StswOutlinedText),
            new FrameworkPropertyMetadata(Brushes.Black,
                FrameworkPropertyMetadataOptions.AffectsRender,
                StrokePropertyChangedCallback)
        );
    private static void StrokePropertyChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs e) => (obj as StswOutlinedText)?.UpdatePen();

    /// <summary>
    /// Gets or sets the thickness of the text outline.
    /// A larger value increases the stroke width.
    /// </summary>
    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }
    public static readonly DependencyProperty StrokeThicknessProperty
        = DependencyProperty.Register(
            nameof(StrokeThickness),
            typeof(double),
            typeof(StswOutlinedText),
            new FrameworkPropertyMetadata(1.0,
                FrameworkPropertyMetadataOptions.AffectsRender,
                StrokePropertyChangedCallback)
        );

    /// <summary>
    /// Gets or sets the text to be displayed.
    /// </summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    public static readonly DependencyProperty TextProperty
        = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(StswOutlinedText),
            new FrameworkPropertyMetadata(OnFormattedTextInvalidated)
        );

    /// <summary>
    /// Gets or sets the alignment of the text within the control.
    /// </summary>
    public TextAlignment TextAlignment
    {
        get => (TextAlignment)GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }
    public static readonly DependencyProperty TextAlignmentProperty
        = DependencyProperty.Register(
            nameof(TextAlignment),
            typeof(TextAlignment),
            typeof(StswOutlinedText),
            new FrameworkPropertyMetadata(OnFormattedTextUpdated)
        );

    /// <summary>
    /// Gets or sets the text decorations applied to the text, such as underline or strikethrough.
    /// </summary>
    public TextDecorationCollection TextDecorations
    {
        get => (TextDecorationCollection)GetValue(TextDecorationsProperty);
        set => SetValue(TextDecorationsProperty, value);
    }
    public static readonly DependencyProperty TextDecorationsProperty
        = DependencyProperty.Register(
            nameof(TextDecorations),
            typeof(TextDecorationCollection),
            typeof(StswOutlinedText),
            new FrameworkPropertyMetadata(OnFormattedTextUpdated)
        );

    /// <summary>
    /// Gets or sets the text trimming behavior for overflowing text.
    /// </summary>
    public TextTrimming TextTrimming
    {
        get => (TextTrimming)GetValue(TextTrimmingProperty);
        set => SetValue(TextTrimmingProperty, value);
    }
    public static readonly DependencyProperty TextTrimmingProperty
        = DependencyProperty.Register(
            nameof(TextTrimming),
            typeof(TextTrimming),
            typeof(StswOutlinedText),
            new FrameworkPropertyMetadata(OnFormattedTextUpdated)
        );

    /// <summary>
    /// Gets or sets the text wrapping behavior for multiline text rendering.
    /// </summary>
    public TextWrapping TextWrapping
    {
        get => (TextWrapping)GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }
    public static readonly DependencyProperty TextWrappingProperty
        = DependencyProperty.Register(
            nameof(TextWrapping),
            typeof(TextWrapping),
            typeof(StswOutlinedText),
            new FrameworkPropertyMetadata(TextWrapping.NoWrap, OnFormattedTextUpdated)
        );
    #endregion
}
