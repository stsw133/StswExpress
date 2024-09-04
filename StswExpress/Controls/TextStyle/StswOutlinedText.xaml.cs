using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// Represents a custom outlined text control with various text formatting options.
/// </summary>
[ContentProperty(nameof(Text))]
public class StswOutlinedText : FrameworkElement
{
    public StswOutlinedText()
    {
        UpdatePen();
        TextDecorations = new TextDecorationCollection();
    }
    static StswOutlinedText()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswOutlinedText), new FrameworkPropertyMetadata(typeof(StswOutlinedText)));
    }

    #region Events & methods
    private FormattedText? _FormattedText;
    private Geometry? _TextGeometry;
    private Pen? _Pen;

    /// <summary>
    /// Updates the pen used for text outline.
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

        if (!newPen.Equals(_Pen))
        {
            _Pen = newPen;
            InvalidateVisual();
        }
    }

    /// <summary>
    /// Renders the outlined text.
    /// </summary>
    protected override void OnRender(DrawingContext drawingContext)
    {
        EnsureGeometry();

        if (_TextGeometry != null)
        {
            if (_Pen != null)
                drawingContext.DrawGeometry(null, _Pen, _TextGeometry);
            if (Fill != null)
                drawingContext.DrawGeometry(Fill, null, _TextGeometry);
        }
    }

    /// <summary>
    /// Measures the size of the outlined text.
    /// </summary>
    protected override Size MeasureOverride(Size availableSize)
    {
        EnsureFormattedText();
        if (_FormattedText != null)
        {
            _FormattedText.MaxTextWidth = Math.Min(3579139, availableSize.Width);
            _FormattedText.MaxTextHeight = Math.Max(0.0001d, availableSize.Height);
            return new Size(Math.Ceiling(_FormattedText.Width), Math.Ceiling(_FormattedText.Height));
        }
        return new Size();
    }

    /// <summary>
    /// Arranges the outlined text.
    /// </summary>
    protected override Size ArrangeOverride(Size finalSize)
    {
        EnsureFormattedText();
        if (_FormattedText != null)
        {
            _FormattedText.MaxTextWidth = finalSize.Width;
            _FormattedText.MaxTextHeight = Math.Max(0.0001d, finalSize.Height);
        }
        _TextGeometry = null;

        return finalSize;
    }

    /// <summary>
    /// Event handler for the invalidation of the formatted text.
    /// </summary>
    private static void OnFormattedTextInvalidated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        var outlinedTextBlock = (StswOutlinedText)dependencyObject;
        outlinedTextBlock._FormattedText = null;
        outlinedTextBlock._TextGeometry = null;

        outlinedTextBlock.InvalidateMeasure();
        outlinedTextBlock.InvalidateVisual();
    }

    /// <summary>
    /// Event handler for the updated formatted text.
    /// </summary>
    private static void OnFormattedTextUpdated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        var outlinedTextBlock = (StswOutlinedText)dependencyObject;
        outlinedTextBlock.UpdateFormattedText();
        outlinedTextBlock._TextGeometry = null;

        outlinedTextBlock.InvalidateMeasure();
        outlinedTextBlock.InvalidateVisual();
    }

    /// <summary>
    /// Ensures the availability of the formatted text.
    /// </summary>
    private void EnsureFormattedText()
    {
        if (_FormattedText != null)
            return;

        _FormattedText = new FormattedText(Text ?? string.Empty, CultureInfo.CurrentUICulture, FlowDirection, new Typeface(FontFamily, FontStyle, FontWeight, FontStretch), FontSize, Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip);

        UpdateFormattedText();
    }

    /// <summary>
    /// Updates the properties of the formatted text.
    /// </summary>
    private void UpdateFormattedText()
    {
        if (_FormattedText == null)
            return;

        _FormattedText.MaxLineCount = TextWrapping == TextWrapping.NoWrap ? 1 : int.MaxValue;
        _FormattedText.TextAlignment = TextAlignment;
        _FormattedText.Trimming = TextTrimming;

        _FormattedText.SetFontSize(FontSize);
        _FormattedText.SetFontStyle(FontStyle);
        _FormattedText.SetFontWeight(FontWeight);
        _FormattedText.SetFontFamily(FontFamily);
        _FormattedText.SetFontStretch(FontStretch);
        _FormattedText.SetTextDecorations(TextDecorations);
    }

    /// <summary>
    /// Ensures the availability of the text geometry.
    /// </summary>
    private void EnsureGeometry()
    {
        if (_TextGeometry != null)
            return;

        EnsureFormattedText();
        _TextGeometry = _FormattedText?.BuildGeometry(new Point(0, 0));
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the fill color of the text.
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
    /// Gets or sets the outline color of the text.
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
    /// Gets or sets the text to display.
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
    /// Gets or sets the text alignment of the text.
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
    /// Gets or sets the text decorations of the text.
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
    /// Gets or sets the text trimming behavior of the text.
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
    /// Gets or sets the text wrapping behavior of the text.
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
