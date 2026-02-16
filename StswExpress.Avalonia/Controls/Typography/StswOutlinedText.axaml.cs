using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using System.Globalization;

namespace StswExpress.Avalonia;

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
//[Content(nameof(Text))]
public class StswOutlinedText : TemplatedControl
{
    private FormattedText? _formattedText;
    private Geometry? _textGeometry;
    private IPen? _pen;

    public StswOutlinedText()
    {
        TextDecorations = [];
        UpdatePen();
    }
    static StswOutlinedText()
    {
        AffectsMeasure<StswOutlinedText>(TextProperty, TextWrappingProperty);
        AffectsRender<StswOutlinedText>(
            FillProperty, StrokeProperty, StrokeThicknessProperty,
            TextAlignmentProperty, TextDecorationsProperty, TextTrimmingProperty,
            FontFamilyProperty, FontSizeProperty, FontStyleProperty, FontWeightProperty, FontStretchProperty,
            FlowDirectionProperty);

        // Text -> recreate FormattedText (measure changes)
        TextProperty.Changed.AddClassHandler<StswOutlinedText>((c, _) => c.InvalidateFormattedText());

        // Properties that update existing FormattedText
        TextAlignmentProperty.Changed.AddClassHandler<StswOutlinedText>((c, _) => c.UpdateFormattedTextAndInvalidate());
        TextDecorationsProperty.Changed.AddClassHandler<StswOutlinedText>((c, _) => c.UpdateFormattedTextAndInvalidate());
        TextTrimmingProperty.Changed.AddClassHandler<StswOutlinedText>((c, _) => c.UpdateFormattedTextAndInvalidate());
        TextWrappingProperty.Changed.AddClassHandler<StswOutlinedText>((c, _) => c.UpdateFormattedTextAndInvalidate());

        FontFamilyProperty.Changed.AddClassHandler<StswOutlinedText>((c, _) => c.UpdateFormattedTextAndInvalidate());
        FontSizeProperty.Changed.AddClassHandler<StswOutlinedText>((c, _) => c.UpdateFormattedTextAndInvalidate());
        FontStyleProperty.Changed.AddClassHandler<StswOutlinedText>((c, _) => c.UpdateFormattedTextAndInvalidate());
        FontWeightProperty.Changed.AddClassHandler<StswOutlinedText>((c, _) => c.UpdateFormattedTextAndInvalidate());
        FontStretchProperty.Changed.AddClassHandler<StswOutlinedText>((c, _) => c.UpdateFormattedTextAndInvalidate());
        FlowDirectionProperty.Changed.AddClassHandler<StswOutlinedText>((c, _) => c.UpdateFormattedTextAndInvalidate());

        // Stroke -> pen update
        StrokeProperty.Changed.AddClassHandler<StswOutlinedText>((c, _) => c.UpdatePen());
        StrokeThicknessProperty.Changed.AddClassHandler<StswOutlinedText>((c, _) => c.UpdatePen());
    }

    #region Events & methods
    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        EnsureFormattedText();

        if (_formattedText is not null)
        {
            _formattedText.MaxTextWidth = finalSize.Width;
            _formattedText.MaxTextHeight = Math.Max(0.0001d, finalSize.Height);
        }

        _textGeometry = null;
        return finalSize;
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        EnsureFormattedText();

        if (_formattedText is null)
            return default;

        var maxW = double.IsInfinity(availableSize.Width)
            ? FormattedText.RealInfiniteWidth
            : Math.Min(FormattedText.RealInfiniteWidth, availableSize.Width);

        var maxH = double.IsInfinity(availableSize.Height)
            ? FormattedText.RealInfiniteWidth
            : Math.Max(0.0001d, availableSize.Height);

        _formattedText.MaxTextWidth = maxW;
        _formattedText.MaxTextHeight = maxH;

        return new Size(
            Math.Ceiling(_formattedText.Width),
            Math.Ceiling(_formattedText.Height));
    }

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        base.Render(context);

        EnsureGeometry();

        if (_textGeometry is null)
            return;

        if (_pen is not null)
            context.DrawGeometry(null, _pen, _textGeometry);

        if (Fill is not null)
            context.DrawGeometry(Fill, null, _textGeometry);
    }

    /// <summary>
    /// Updates the formatted text and invalidates the measure and visual display.
    /// </summary>
    private void InvalidateFormattedText()
    {
        _formattedText = null;
        _textGeometry = null;
        InvalidateMeasure();
        InvalidateVisual();
    }

    /// <summary>
    /// Ensures that the formatted text object is initialized.
    /// Creates a new formatted text instance if necessary.
    /// </summary>
    private void EnsureFormattedText()
    {
        if (_formattedText is not null)
            return;

        _formattedText = new FormattedText(
            Text ?? string.Empty,
            CultureInfo.CurrentUICulture,
            FlowDirection,
            new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
            FontSize,
            Brushes.Black);

        UpdateFormattedText();
    }

    /// <summary>
    /// Ensures that the text geometry is generated for rendering.
    /// </summary>
    private void EnsureGeometry()
    {
        if (_textGeometry is not null)
            return;

        EnsureFormattedText();

        // BuildGeometry(Point) is supported in Avalonia :contentReference[oaicite:3]{index=3}
        _textGeometry = _formattedText?.BuildGeometry(new Point(0, 0));
    }

    /// <summary>
    /// Updates the properties of the formatted text object, such as font size, weight, alignment, and decorations.
    /// </summary>
    private void UpdateFormattedText()
    {
        if (_formattedText is null)
            return;

        _formattedText.TextAlignment = TextAlignment;
        _formattedText.Trimming = TextTrimming;
        _formattedText.MaxLineCount = TextWrapping == TextWrapping.NoWrap ? 1 : int.MaxValue;

        _formattedText.SetFontTypeface(new Typeface(FontFamily, FontStyle, FontWeight, FontStretch));
        _formattedText.SetFontSize(FontSize);
        _formattedText.SetTextDecorations(TextDecorations);
        _formattedText.SetCulture(CultureInfo.CurrentUICulture);
    }

    /// <summary>
    /// Updates the formatted text and invalidates the measure and visual display.
    /// </summary>
    private void UpdateFormattedTextAndInvalidate()
    {
        EnsureFormattedText();
        UpdateFormattedText();
        _textGeometry = null;
        InvalidateMeasure();
        InvalidateVisual();
    }

    /// <summary>
    /// Updates the pen used for drawing the text outline.
    /// Adjusts stroke properties such as thickness and line caps.
    /// </summary>
    private void UpdatePen()
    {
        // Avalonia Pen has a common LineCap for both ends :contentReference[oaicite:2]{index=2}
        var newPen = new Pen(Stroke, StrokeThickness)
        {
            LineCap = PenLineCap.Round,
            LineJoin = PenLineJoin.Round
        };

        _pen = newPen;
        InvalidateVisual();
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the fill color of the text.
    /// The fill applies to the interior of the text glyphs.
    /// </summary>
    public IBrush? Fill
    {
        get => GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }
    public static readonly StyledProperty<IBrush?> FillProperty = AvaloniaProperty.Register<StswOutlinedText, IBrush?>(nameof(Fill), Brushes.Black);

    /// <summary>
    /// Gets or sets the stroke (outline) color of the text.
    /// </summary>
    public IBrush? Stroke
    {
        get => GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }
    public static readonly StyledProperty<IBrush?> StrokeProperty = AvaloniaProperty.Register<StswOutlinedText, IBrush?>(nameof(Stroke), Brushes.Black);

    /// <summary>
    /// Gets or sets the thickness of the text outline.
    /// A larger value increases the stroke width.
    /// </summary>
    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }
    public static readonly StyledProperty<double> StrokeThicknessProperty = AvaloniaProperty.Register<StswOutlinedText, double>(nameof(StrokeThickness), 1.0);

    /// <summary>
    /// Gets or sets the text to be displayed.
    /// </summary>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    public static readonly StyledProperty<string?> TextProperty = AvaloniaProperty.Register<StswOutlinedText, string?>(nameof(Text), string.Empty);

    /// <summary>
    /// Gets or sets the alignment of the text within the control.
    /// </summary>
    public TextAlignment TextAlignment
    {
        get => GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }
    public static readonly StyledProperty<TextAlignment> TextAlignmentProperty = AvaloniaProperty.Register<StswOutlinedText, TextAlignment>(nameof(TextAlignment), TextAlignment.Left);

    /// <summary>
    /// Gets or sets the text decorations applied to the text, such as underline or strikethrough.
    /// </summary>
    public TextDecorationCollection TextDecorations
    {
        get => GetValue(TextDecorationsProperty);
        set => SetValue(TextDecorationsProperty, value);
    }
    public static readonly StyledProperty<TextDecorationCollection> TextDecorationsProperty = AvaloniaProperty.Register<StswOutlinedText, TextDecorationCollection>(nameof(TextDecorations), []);

    /// <summary>
    /// Gets or sets the text trimming behavior for overflowing text.
    /// </summary>
    public TextTrimming TextTrimming
    {
        get => GetValue(TextTrimmingProperty);
        set => SetValue(TextTrimmingProperty, value);
    }
    public static readonly StyledProperty<TextTrimming> TextTrimmingProperty = AvaloniaProperty.Register<StswOutlinedText, TextTrimming>(nameof(TextTrimming), TextTrimming.None);

    /// <summary>
    /// Gets or sets the text wrapping behavior for multiline text rendering.
    /// </summary>
    public TextWrapping TextWrapping
    {
        get => GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }
    public static readonly StyledProperty<TextWrapping> TextWrappingProperty = AvaloniaProperty.Register<StswOutlinedText, TextWrapping>(nameof(TextWrapping), TextWrapping.NoWrap);
    #endregion
}
