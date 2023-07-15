﻿using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;

[ContentProperty(nameof(Text))]
public class StswOutlinedText : FrameworkElement
{
    public StswOutlinedText()
    {
        UpdatePen();
        TextDecorations = new TextDecorationCollection();
    }

    #region Properties
    /// Fill
    public static readonly DependencyProperty FillProperty
        = DependencyProperty.Register(
            nameof(Fill),
            typeof(Brush),
            typeof(StswOutlinedText),
            new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender)
        );
    public Brush Fill
    {
        get => (Brush)GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    /// FontFamily
    public static readonly DependencyProperty FontFamilyProperty
        = TextElement.FontFamilyProperty.AddOwner(
            typeof(StswOutlinedText),
            new FrameworkPropertyMetadata(OnFormattedTextUpdated)
        );
    public FontFamily FontFamily
    {
        get => (FontFamily)GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    /// FontSize
    public static readonly DependencyProperty FontSizeProperty
        = TextElement.FontSizeProperty.AddOwner(
            typeof(StswOutlinedText),
            new FrameworkPropertyMetadata(OnFormattedTextUpdated)
        );
    [TypeConverter(typeof(FontSizeConverter))]
    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    /// FontStretch
    public static readonly DependencyProperty FontStretchProperty
        = TextElement.FontStretchProperty.AddOwner(
            typeof(StswOutlinedText),
            new FrameworkPropertyMetadata(OnFormattedTextUpdated)
        );
    public FontStretch FontStretch
    {
        get => (FontStretch)GetValue(FontStretchProperty);
        set => SetValue(FontStretchProperty, value);
    }

    /// FontStyle
    public static readonly DependencyProperty FontStyleProperty
        = TextElement.FontStyleProperty.AddOwner(
            typeof(StswOutlinedText),
            new FrameworkPropertyMetadata(OnFormattedTextUpdated)
        );
    public FontStyle FontStyle
    {
        get => (FontStyle)GetValue(FontStyleProperty);
        set => SetValue(FontStyleProperty, value);
    }

    /// FontWeight
    public static readonly DependencyProperty FontWeightProperty
        = TextElement.FontWeightProperty.AddOwner(
            typeof(StswOutlinedText),
            new FrameworkPropertyMetadata(OnFormattedTextUpdated)
        );
    public FontWeight FontWeight
    {
        get => (FontWeight)GetValue(FontWeightProperty);
        set => SetValue(FontWeightProperty, value);
    }

    /// Stroke
    public static readonly DependencyProperty StrokeProperty
        = DependencyProperty.Register(
            nameof(Stroke),
            typeof(Brush),
            typeof(StswOutlinedText),
            new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender, StrokePropertyChangedCallback)
        );
    public Brush Stroke
    {
        get => (Brush)GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }
    private static void StrokePropertyChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs e) => (obj as StswOutlinedText)?.UpdatePen();

    /// StrokeThickness
    public static readonly DependencyProperty StrokeThicknessProperty
        = DependencyProperty.Register(
            nameof(StrokeThickness),
            typeof(double),
            typeof(StswOutlinedText),
            new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsRender, StrokePropertyChangedCallback)
        );
    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    /// Text
    public static readonly DependencyProperty TextProperty
        = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(StswOutlinedText),
            new FrameworkPropertyMetadata(OnFormattedTextInvalidated)
        );
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// TextAlignment
    public static readonly DependencyProperty TextAlignmentProperty
        = DependencyProperty.Register(
            nameof(TextAlignment),
            typeof(TextAlignment),
            typeof(StswOutlinedText),
            new FrameworkPropertyMetadata(OnFormattedTextUpdated)
        );
    public TextAlignment TextAlignment
    {
        get => (TextAlignment)GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }

    /// TextDecorations
    public static readonly DependencyProperty TextDecorationsProperty
        = DependencyProperty.Register(
            nameof(TextDecorations),
            typeof(TextDecorationCollection),
            typeof(StswOutlinedText),
            new FrameworkPropertyMetadata(OnFormattedTextUpdated)
        );
    public TextDecorationCollection TextDecorations
    {
        get => (TextDecorationCollection)GetValue(TextDecorationsProperty);
        set => SetValue(TextDecorationsProperty, value);
    }

    /// TextTrimming
    public static readonly DependencyProperty TextTrimmingProperty
        = DependencyProperty.Register(
            nameof(TextTrimming),
            typeof(TextTrimming),
            typeof(StswOutlinedText),
            new FrameworkPropertyMetadata(OnFormattedTextUpdated)
        );
    public TextTrimming TextTrimming
    {
        get => (TextTrimming)GetValue(TextTrimmingProperty);
        set => SetValue(TextTrimmingProperty, value);
    }

    /// TextWrapping
    public static readonly DependencyProperty TextWrappingProperty
        = DependencyProperty.Register(
            nameof(TextWrapping),
            typeof(TextWrapping),
            typeof(StswOutlinedText),
            new FrameworkPropertyMetadata(TextWrapping.NoWrap, OnFormattedTextUpdated)
        );
    public TextWrapping TextWrapping
    {
        get => (TextWrapping)GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }
    #endregion

    private FormattedText? _FormattedText;
    private Geometry? _TextGeometry;
    private Pen? _Pen;

    private void UpdatePen()
    {
        _Pen = new Pen(Stroke, StrokeThickness)
        {
            DashCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round,
            LineJoin = PenLineJoin.Round,
            StartLineCap = PenLineCap.Round
        };

        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        EnsureGeometry();
        drawingContext.DrawGeometry(null, _Pen, _TextGeometry);
        drawingContext.DrawGeometry(Fill, null, _TextGeometry);
    }

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

    private static void OnFormattedTextInvalidated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        var outlinedTextBlock = (StswOutlinedText)dependencyObject;
        outlinedTextBlock._FormattedText = null;
        outlinedTextBlock._TextGeometry = null;

        outlinedTextBlock.InvalidateMeasure();
        outlinedTextBlock.InvalidateVisual();
    }

    private static void OnFormattedTextUpdated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        var outlinedTextBlock = (StswOutlinedText)dependencyObject;
        outlinedTextBlock.UpdateFormattedText();
        outlinedTextBlock._TextGeometry = null;

        outlinedTextBlock.InvalidateMeasure();
        outlinedTextBlock.InvalidateVisual();
    }

    private void EnsureFormattedText()
    {
        if (_FormattedText != null)
            return;

        _FormattedText = new FormattedText(Text ?? string.Empty, CultureInfo.CurrentUICulture, FlowDirection, new Typeface(FontFamily, FontStyle, FontWeight, FontStretch), FontSize, Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip);

        UpdateFormattedText();
    }

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

    private void EnsureGeometry()
    {
        if (_TextGeometry != null)
            return;

        EnsureFormattedText();
        _TextGeometry = _FormattedText?.BuildGeometry(new Point(0, 0));
    }
}