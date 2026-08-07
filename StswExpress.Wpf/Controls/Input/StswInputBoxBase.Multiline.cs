using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace StswExpress.Wpf;

public abstract partial class StswInputBoxBase
{
	private const string InternalTextDragFormat = "StswExpress.Wpf.StswInputBoxBase.TextDrag";
	private TextLayoutCache? _multiLineLayout;
	private Size _contentViewportSize;
	private double _verticalOffset;
	private double _preferredCaretX = double.NaN;
	private Point _dragStartPoint;
	private int _dragSelectionStart;
	private int _dragSelectionLength;
	private bool _isDragCandidate;
	private bool _internalDropCompleted;
	private bool _isCaretBlinkEnabled = true;
	private int _desiredVisibleLineCount = 1;

	#region Multiline dependency properties

	public static readonly DependencyProperty AcceptsReturnProperty =
		DependencyProperty.Register(
			nameof(AcceptsReturn),
			typeof(bool),
			typeof(StswInputBoxBase),
			new FrameworkPropertyMetadata(false, OnTextLayoutPropertyChanged));

	/// <summary>Gets or sets whether Enter inserts a line break instead of committing the value.</summary>
	public bool AcceptsReturn
	{
		get => (bool)GetValue(AcceptsReturnProperty);
		set => SetValue(AcceptsReturnProperty, value);
	}

	public static readonly DependencyProperty TextWrappingProperty =
		DependencyProperty.Register(
			nameof(TextWrapping),
			typeof(TextWrapping),
			typeof(StswInputBoxBase),
			new FrameworkPropertyMetadata(TextWrapping.NoWrap, OnTextLayoutPropertyChanged));

	/// <summary>Gets or sets how logical lines are split into visual lines.</summary>
	public TextWrapping TextWrapping
	{
		get => (TextWrapping)GetValue(TextWrappingProperty);
		set => SetValue(TextWrappingProperty, value);
	}

	public static readonly DependencyProperty MinLinesProperty =
		DependencyProperty.Register(
			nameof(MinLines),
			typeof(int),
			typeof(StswInputBoxBase),
			new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsMeasure, OnMinLinesChanged, CoerceMinLines));

	/// <summary>Gets or sets the minimum number of visible text lines used to determine the control's natural height.</summary>
	public int MinLines
	{
		get => (int)GetValue(MinLinesProperty);
		set => SetValue(MinLinesProperty, value);
	}

	private static object CoerceMinLines(DependencyObject d, object baseValue)
	{
		var input = (StswInputBoxBase)d;
		var value = Math.Max(1, (int)baseValue);
		return input.MaxLines > 0 ? Math.Min(value, input.MaxLines) : value;
	}

	private static void OnMinLinesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is not StswInputBoxBase input)
			return;

		input.CoerceValue(MaxLinesProperty);
		input._contentView?.InvalidateMeasure();
		input.InvalidateMeasure();
	}

	public static readonly DependencyProperty MaxLinesProperty =
		DependencyProperty.Register(
			nameof(MaxLines),
			typeof(int),
			typeof(StswInputBoxBase),
			new FrameworkPropertyMetadata(int.MaxValue, FrameworkPropertyMetadataOptions.AffectsMeasure, OnMaxLinesChanged, CoerceMaxLines));

	/// <summary>
	/// Gets or sets the maximum number of visible text lines used to determine the control's natural height.
	/// A value of <c>0</c> means that the number of visible lines is not limited by this property.
	/// </summary>
	public int MaxLines
	{
		get => (int)GetValue(MaxLinesProperty);
		set => SetValue(MaxLinesProperty, value);
	}

	private static object CoerceMaxLines(DependencyObject d, object baseValue)
	{
		var input = (StswInputBoxBase)d;
		var value = Math.Max(0, (int)baseValue);
		return value == 0 ? 0 : Math.Max(input.MinLines, value);
	}

	private static void OnMaxLinesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is not StswInputBoxBase input)
			return;

		input.CoerceValue(MinLinesProperty);
		input._contentView?.InvalidateMeasure();
		input.InvalidateMeasure();
	}

	private static readonly DependencyPropertyKey LineCountPropertyKey =
		DependencyProperty.RegisterReadOnly(
			nameof(LineCount),
			typeof(int),
			typeof(StswInputBoxBase),
			new FrameworkPropertyMetadata(1));

	public static readonly DependencyProperty LineCountProperty = LineCountPropertyKey.DependencyProperty;

	/// <summary>Gets the number of logical lines separated by <c>\n</c>.</summary>
	public int LineCount => (int)GetValue(LineCountProperty);

	public static readonly DependencyProperty IsTextDragDropEnabledProperty =
		DependencyProperty.Register(
			nameof(IsTextDragDropEnabled),
			typeof(bool),
			typeof(StswInputBoxBase),
			new FrameworkPropertyMetadata(true, OnIsTextDragDropEnabledChanged));

	public bool IsTextDragDropEnabled
	{
		get => (bool)GetValue(IsTextDragDropEnabledProperty);
		set => SetValue(IsTextDragDropEnabledProperty, value);
	}

	private static void OnIsTextDragDropEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is StswInputBoxBase input)
			input.SetCurrentValue(AllowDropProperty, (bool)e.NewValue);
	}

	public static readonly DependencyProperty CaretWidthProperty =
		DependencyProperty.Register(
			nameof(CaretWidth),
			typeof(double),
			typeof(StswInputBoxBase),
			new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsRender, null, CoerceCaretWidth));

	/// <summary>Gets or sets the caret width. NaN uses the system caret width.</summary>
	public double CaretWidth
	{
		get => (double)GetValue(CaretWidthProperty);
		set => SetValue(CaretWidthProperty, value);
	}

	private static object CoerceCaretWidth(DependencyObject d, object baseValue)
	{
		var value = (double)baseValue;
		return double.IsNaN(value) ? value : Math.Max(0.5, value);
	}

	public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty =
		ScrollViewer.HorizontalScrollBarVisibilityProperty.AddOwner(
			typeof(StswInputBoxBase),
			new FrameworkPropertyMetadata(ScrollBarVisibility.Hidden));

	/// <summary>Gets or sets the visibility policy for the horizontal scrollbar in <c>PART_ContentHost</c>.</summary>
	public ScrollBarVisibility HorizontalScrollBarVisibility
	{
		get => (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty);
		set => SetValue(HorizontalScrollBarVisibilityProperty, value);
	}

	public static readonly DependencyProperty VerticalScrollBarVisibilityProperty =
		ScrollViewer.VerticalScrollBarVisibilityProperty.AddOwner(
			typeof(StswInputBoxBase),
			new FrameworkPropertyMetadata(ScrollBarVisibility.Hidden));

	/// <summary>Gets or sets the visibility policy for the vertical scrollbar in <c>PART_ContentHost</c>.</summary>
	public ScrollBarVisibility VerticalScrollBarVisibility
	{
		get => (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty);
		set => SetValue(VerticalScrollBarVisibilityProperty, value);
	}

	#endregion

	#region Scroll information

	private static readonly DependencyPropertyKey HorizontalOffsetPropertyKey = RegisterReadOnlyDouble(nameof(HorizontalOffset));
	public static readonly DependencyProperty HorizontalOffsetProperty = HorizontalOffsetPropertyKey.DependencyProperty;
	public double HorizontalOffset => (double)GetValue(HorizontalOffsetProperty);

	private static readonly DependencyPropertyKey VerticalOffsetPropertyKey = RegisterReadOnlyDouble(nameof(VerticalOffset));
	public static readonly DependencyProperty VerticalOffsetProperty = VerticalOffsetPropertyKey.DependencyProperty;
	public double VerticalOffset => (double)GetValue(VerticalOffsetProperty);

	private static readonly DependencyPropertyKey ViewportWidthPropertyKey = RegisterReadOnlyDouble(nameof(ViewportWidth));
	public static readonly DependencyProperty ViewportWidthProperty = ViewportWidthPropertyKey.DependencyProperty;
	public double ViewportWidth => (double)GetValue(ViewportWidthProperty);

	private static readonly DependencyPropertyKey ViewportHeightPropertyKey = RegisterReadOnlyDouble(nameof(ViewportHeight));
	public static readonly DependencyProperty ViewportHeightProperty = ViewportHeightPropertyKey.DependencyProperty;
	public double ViewportHeight => (double)GetValue(ViewportHeightProperty);

	private static readonly DependencyPropertyKey ExtentWidthPropertyKey = RegisterReadOnlyDouble(nameof(ExtentWidth));
	public static readonly DependencyProperty ExtentWidthProperty = ExtentWidthPropertyKey.DependencyProperty;
	public double ExtentWidth => (double)GetValue(ExtentWidthProperty);

	private static readonly DependencyPropertyKey ExtentHeightPropertyKey = RegisterReadOnlyDouble(nameof(ExtentHeight));
	public static readonly DependencyProperty ExtentHeightProperty = ExtentHeightPropertyKey.DependencyProperty;
	public double ExtentHeight => (double)GetValue(ExtentHeightProperty);

	private static readonly DependencyPropertyKey ScrollableWidthPropertyKey = RegisterReadOnlyDouble(nameof(ScrollableWidth));
	public static readonly DependencyProperty ScrollableWidthProperty = ScrollableWidthPropertyKey.DependencyProperty;
	public double ScrollableWidth => (double)GetValue(ScrollableWidthProperty);

	private static readonly DependencyPropertyKey ScrollableHeightPropertyKey = RegisterReadOnlyDouble(nameof(ScrollableHeight));
	public static readonly DependencyProperty ScrollableHeightProperty = ScrollableHeightPropertyKey.DependencyProperty;
	public double ScrollableHeight => (double)GetValue(ScrollableHeightProperty);

	private static DependencyPropertyKey RegisterReadOnlyDouble(string name)
		=> DependencyProperty.RegisterReadOnly(name, typeof(double), typeof(StswInputBoxBase), new FrameworkPropertyMetadata(0d));

	public bool CanHorizontallyScroll { get; set; } = true;
	public bool CanVerticallyScroll { get; set; } = true;

	public void LineUp() => SetVerticalOffset(VerticalOffset - GetLineHeight());
	public void LineDown() => SetVerticalOffset(VerticalOffset + GetLineHeight());
	public void LineLeft() => SetHorizontalOffset(HorizontalOffset - Math.Max(1, FontSize / 2));
	public void LineRight() => SetHorizontalOffset(HorizontalOffset + Math.Max(1, FontSize / 2));
	public void PageUp() => SetVerticalOffset(VerticalOffset - ViewportHeight);
	public void PageDown() => SetVerticalOffset(VerticalOffset + ViewportHeight);
	public void PageLeft() => SetHorizontalOffset(HorizontalOffset - ViewportWidth);
	public void PageRight() => SetHorizontalOffset(HorizontalOffset + ViewportWidth);
	public void MouseWheelUp()
	{
		if (SystemParameters.WheelScrollLines < 0) PageUp(); else SetVerticalOffset(VerticalOffset - (GetLineHeight() * SystemParameters.WheelScrollLines));
	}
	public void MouseWheelDown()
	{
		if (SystemParameters.WheelScrollLines < 0) PageDown(); else SetVerticalOffset(VerticalOffset + (GetLineHeight() * SystemParameters.WheelScrollLines));
	}
	public void MouseWheelLeft() => SetHorizontalOffset(HorizontalOffset - (Math.Max(1, FontSize) * 3));
	public void MouseWheelRight() => SetHorizontalOffset(HorizontalOffset + (Math.Max(1, FontSize) * 3));

	public void SetHorizontalOffset(double offset)
	{
		if (double.IsNaN(offset) || double.IsInfinity(offset))
			throw new ArgumentOutOfRangeException(nameof(offset));

		offset = TextWrapping == TextWrapping.NoWrap && CanHorizontallyScroll
			? Math.Clamp(offset, 0, ScrollableWidth)
			: 0;

		if (_horizontalOffset.Equals(offset))
			return;

		_horizontalOffset = offset;
		SetValue(HorizontalOffsetPropertyKey, offset);
		InvalidateEditorVisual();
		_contentView?.InvalidateScrollInfo();
	}

	public void SetVerticalOffset(double offset)
	{
		if (double.IsNaN(offset) || double.IsInfinity(offset))
			throw new ArgumentOutOfRangeException(nameof(offset));

		offset = CanVerticallyScroll ? Math.Clamp(offset, 0, ScrollableHeight) : 0;
		if (_verticalOffset.Equals(offset))
			return;

		_verticalOffset = offset;
		SetValue(VerticalOffsetPropertyKey, offset);
		InvalidateEditorVisual();
		_contentView?.InvalidateScrollInfo();
	}

	public Rect MakeVisible(Visual visual, Rect rectangle)
	{
		if (!ReferenceEquals(visual, this) && IsAncestorOf(visual))
			rectangle = visual.TransformToAncestor(this).TransformBounds(rectangle);

		var textRect = GetTextRect();
		return MakeContentRectangleVisible(new Rect(
			rectangle.X - textRect.X,
			rectangle.Y - textRect.Y,
			rectangle.Width,
			rectangle.Height));
	}

	private Rect MakeContentRectangleVisible(Rect viewportRectangle)
	{
		var contentRectangle = new Rect(
			viewportRectangle.X + HorizontalOffset,
			viewportRectangle.Y + VerticalOffset,
			viewportRectangle.Width,
			viewportRectangle.Height);

		if (contentRectangle.Left < HorizontalOffset)
			SetHorizontalOffset(contentRectangle.Left);
		else if (contentRectangle.Right > HorizontalOffset + ViewportWidth)
			SetHorizontalOffset(contentRectangle.Right - ViewportWidth);

		if (contentRectangle.Top < VerticalOffset)
			SetVerticalOffset(contentRectangle.Top);
		else if (contentRectangle.Bottom > VerticalOffset + ViewportHeight)
			SetVerticalOffset(contentRectangle.Bottom - ViewportHeight);

		return new Rect(
			contentRectangle.X - HorizontalOffset,
			contentRectangle.Y - VerticalOffset,
			contentRectangle.Width,
			contentRectangle.Height);
	}

	public void ScrollToHorizontalOffset(double offset) => SetHorizontalOffset(offset);
	public void ScrollToVerticalOffset(double offset) => SetVerticalOffset(offset);

	/// <summary>Scrolls the viewport so that the specified logical line is visible.</summary>
	public void ScrollToLine(int lineIndex)
	{
		var layout = GetMultilineLayout(GetTextRect());
		if (lineIndex < 0 || lineIndex >= layout.LogicalLines.Count)
			throw new ArgumentOutOfRangeException(nameof(lineIndex));

		var visualLine = layout.VisualLines.First(x => x.LogicalLineIndex == lineIndex);
		SetVerticalOffset(visualLine.Y);
	}

	private void UpdateScrollInfo(TextLayoutCache layout, Rect viewport)
	{
		var viewportWidth = Math.Max(0, viewport.Width);
		var viewportHeight = Math.Max(0, viewport.Height);
		var extentWidth = TextWrapping == TextWrapping.NoWrap ? Math.Max(0, layout.ExtentWidth) : viewportWidth;
		var extentHeight = Math.Max(0, layout.ExtentHeight);
		var scrollableWidth = Math.Max(0, extentWidth - viewportWidth);
		var scrollableHeight = Math.Max(0, extentHeight - viewportHeight);
		var horizontalOffset = TextWrapping == TextWrapping.NoWrap && CanHorizontallyScroll
			? Math.Clamp(_horizontalOffset, 0, scrollableWidth)
			: 0;
		var verticalOffset = CanVerticallyScroll ? Math.Clamp(_verticalOffset, 0, scrollableHeight) : 0;

		var metricsChanged = !ViewportWidth.Equals(viewportWidth)
			|| !ViewportHeight.Equals(viewportHeight)
			|| !ExtentWidth.Equals(extentWidth)
			|| !ExtentHeight.Equals(extentHeight)
			|| !ScrollableWidth.Equals(scrollableWidth)
			|| !ScrollableHeight.Equals(scrollableHeight)
			|| !_horizontalOffset.Equals(horizontalOffset)
			|| !_verticalOffset.Equals(verticalOffset);

		if (!metricsChanged)
			return;

		SetValue(ViewportWidthPropertyKey, viewportWidth);
		SetValue(ViewportHeightPropertyKey, viewportHeight);
		SetValue(ExtentWidthPropertyKey, extentWidth);
		SetValue(ExtentHeightPropertyKey, extentHeight);
		SetValue(ScrollableWidthPropertyKey, scrollableWidth);
		SetValue(ScrollableHeightPropertyKey, scrollableHeight);
		_horizontalOffset = horizontalOffset;
		_verticalOffset = verticalOffset;
		SetValue(HorizontalOffsetPropertyKey, horizontalOffset);
		SetValue(VerticalOffsetPropertyKey, verticalOffset);

		InvalidateEditorVisual();
		_contentView?.InvalidateScrollInfo();
	}

	#endregion

	#region Logical line API

	/// <summary>Returns the first character index of a logical (newline-delimited) line.</summary>
	public int GetCharacterIndexFromLineIndex(int lineIndex)
	{
		var lines = GetLogicalLines(Text);
		return lineIndex >= 0 && lineIndex < lines.Count ? lines[lineIndex].Start : -1;
	}

	/// <summary>Returns the logical line containing the first visible visual line.</summary>
	public int GetFirstVisibleLineIndex()
	{
		var layout = GetMultilineLayout(GetTextRect());
		var (first, _) = GetVisibleVisualLineRange(layout);
		return layout.VisualLines[first].LogicalLineIndex;
	}

	/// <summary>Returns the logical line containing the last visible visual line.</summary>
	public int GetLastVisibleLineIndex()
	{
		var layout = GetMultilineLayout(GetTextRect());
		var (_, lastExclusive) = GetVisibleVisualLineRange(layout);
		return layout.VisualLines[Math.Max(0, lastExclusive - 1)].LogicalLineIndex;
	}

	/// <summary>Returns the logical (newline-delimited) line containing a character index.</summary>
	public int GetLineIndexFromCharacterIndex(int charIndex)
	{
		charIndex = Math.Clamp(charIndex, 0, Text.Length);
		var lines = GetLogicalLines(Text);
		var low = 0;
		var high = lines.Count - 1;
		while (low <= high)
		{
			var middle = low + ((high - low) / 2);
			if (lines[middle].Start <= charIndex)
				low = middle + 1;
			else
				high = middle - 1;
		}

		return Math.Clamp(high, 0, lines.Count - 1);
	}

	/// <summary>Returns the text of a logical line, excluding its newline delimiter.</summary>
	public string GetLineText(int lineIndex)
	{
		var lines = GetLogicalLines(Text);
		if (lineIndex < 0 || lineIndex >= lines.Count)
			throw new ArgumentOutOfRangeException(nameof(lineIndex));
		return Text.Substring(lines[lineIndex].Start, lines[lineIndex].Length);
	}

	/// <summary>Returns the UTF-16 length of a logical line, excluding its newline delimiter.</summary>
	public int GetLineLength(int lineIndex)
	{
		var lines = GetLogicalLines(Text);
		if (lineIndex < 0 || lineIndex >= lines.Count)
			throw new ArgumentOutOfRangeException(nameof(lineIndex));
		return lines[lineIndex].Length;
	}

	public Rect GetRectFromCharacterIndex(int charIndex, bool trailingEdge)
	{
		var textRect = GetTextRect();
		var layout = GetMultilineLayout(textRect);
		charIndex = SnapToTextElementBoundary(Text, Math.Clamp(charIndex, 0, Text.Length), trailingEdge);
		var line = FindVisualLine(layout, charIndex);
		var boundaryIndex = trailingEdge && charIndex < Text.Length ? GetNextTextElementIndex(Text, charIndex) : charIndex;
		boundaryIndex = Math.Min(line.End, boundaryIndex);
		var x = GetVisualLineOriginX(textRect, line) + GetBoundaryX(line, boundaryIndex);
		var y = GetContentOriginY(textRect, layout) + line.Y - VerticalOffset;
		return new Rect(x, y, GetEffectiveCaretWidth(), line.Height);
	}

	public int GetCharacterIndexFromPoint(Point point, bool snapToText)
	{
		var textRect = GetTextRect();
		if (!snapToText && !textRect.Contains(point))
			return -1;
		return HitTestTextPosition(point, textRect);
	}

	#endregion

	private static void OnTextLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is StswInputBoxBase input)
		{
			if (e.Property == TextWrappingProperty && (TextWrapping)e.NewValue != TextWrapping.NoWrap)
				input.SetHorizontalOffset(0);
			input.InvalidateTextLayout();
		}
	}

	private void InitializeMultilineSupport()
	{
		SetCurrentValue(AllowDropProperty, IsTextDragDropEnabled);
		Loaded += OnInputLoaded;
		Unloaded += OnInputUnloaded;
	}

	private Size MeasureContentView(Size availableSize)
	{
		var lineHeight = Math.Max(1, GetLineHeight());
		_desiredVisibleLineCount = GetDesiredVisibleLineCount(availableSize.Width);
		var desiredHeight = _desiredVisibleLineCount * lineHeight;
		var desiredWidth = GetEffectiveCaretWidth();

		if (!string.IsNullOrEmpty(Placeholder))
		{
			desiredWidth += CreateFormattedText(Placeholder, PlaceholderBrush ?? Foreground ?? Brushes.Black, isPlaceholder: true)
				.WidthIncludingTrailingWhitespace;
		}

		if (!double.IsInfinity(availableSize.Width))
			desiredWidth = Math.Min(desiredWidth, Math.Max(0, availableSize.Width));
		if (!double.IsInfinity(availableSize.Height))
			desiredHeight = Math.Min(desiredHeight, Math.Max(0, availableSize.Height));

		return new Size(Math.Max(0, desiredWidth), Math.Max(0, desiredHeight));
	}

	private int GetDesiredVisibleLineCount(double availableWidth)
	{
		// NoWrap is independent of the viewport width. For wrapping, use the width
		// supplied by the content presenter. If WPF performs an unconstrained/zero
		// probe measure first, fall back to the last real viewport width; otherwise
		// measure as unwrapped until a usable width becomes available.
		var measureWidth = availableWidth;
		if (TextWrapping != TextWrapping.NoWrap && (double.IsInfinity(measureWidth) || measureWidth <= 0))
		{
			measureWidth = _contentViewportSize.Width > 0
				? _contentViewportSize.Width
				: double.PositiveInfinity;
		}

		var layout = GetMultilineLayout(
			new Rect(0, 0, Math.Max(1, measureWidth), double.MaxValue),
			updateScrollInfo: false);

		var visibleLines = Math.Max(1, layout.VisualLines.Count);
		var desiredLines = Math.Max(Math.Max(1, MinLines), visibleLines);

		if (MaxLines > 0)
			desiredLines = Math.Min(desiredLines, MaxLines);

		return desiredLines;
	}

	private void ArrangeContentView(Size finalSize)
	{
		if (finalSize.Width <= 0 || finalSize.Height <= 0)
			return;

		if (!_contentViewportSize.Equals(finalSize))
		{
			_contentViewportSize = finalSize;
			_multiLineLayout = null;
		}

		var viewport = new Rect(new Point(), finalSize);
		var layout = GetMultilineLayout(viewport, updateScrollInfo: false);
		UpdateScrollInfo(layout, viewport);
	}

	private void OnContentScrollOwnerChanged()
	{
		if (_contentViewportSize.Width > 0 && _contentViewportSize.Height > 0)
			ArrangeContentView(_contentViewportSize);
	}

	private void OnInputLoaded(object sender, RoutedEventArgs e)
	{
		ConfigureCaretTimer();
		if (IsKeyboardFocused)
			ResetCaretBlink();
	}

	private void OnInputUnloaded(object sender, RoutedEventArgs e)
	{
		_caretTimer.Stop();
		_selectionAutoScrollTimer.Stop();
		CancelImeComposition();
	}

	private void ConfigureCaretTimer()
	{
		var blinkTime = GetCaretBlinkTime();
		if (blinkTime == 0 || blinkTime == uint.MaxValue)
		{
			_isCaretBlinkEnabled = false;
			_caretTimer.Stop();
			_isCaretVisible = true;
			return;
		}

		_isCaretBlinkEnabled = true;
		_caretTimer.Interval = TimeSpan.FromMilliseconds(Math.Max(100, blinkTime));
	}

	protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
	{
		base.OnPreviewMouseWheel(e);
		if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
			return;

		var oldOffset = HorizontalOffset;
		if (e.Delta > 0)
			MouseWheelLeft();
		else
			MouseWheelRight();
		e.Handled = !HorizontalOffset.Equals(oldOffset);
	}

	protected override void OnMouseWheel(MouseWheelEventArgs e)
	{
		base.OnMouseWheel(e);
		var oldOffset = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? HorizontalOffset : VerticalOffset;
		if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
		{
			if (e.Delta > 0) MouseWheelLeft(); else MouseWheelRight();
			e.Handled = !HorizontalOffset.Equals(oldOffset);
		}
		else
		{
			if (e.Delta > 0) MouseWheelUp(); else MouseWheelDown();
			e.Handled = !VerticalOffset.Equals(oldOffset);
		}
	}

	protected override void OnTouchDown(TouchEventArgs e)
	{
		base.OnTouchDown(e);
		Focus();
		// Leave the event unhandled so WPF can promote it to the existing mouse-selection path.
	}

	protected override void OnStylusDown(StylusDownEventArgs e)
	{
		base.OnStylusDown(e);
		Focus();
		// Stylus events are deliberately allowed to promote to mouse events exactly once.
	}

	protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
	{
		base.OnDpiChanged(oldDpi, newDpi);
		InvalidateTextLayout();
		ConfigureCaretTimer();
	}

	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (e.Property == FontFamilyProperty
			|| e.Property == FontStyleProperty
			|| e.Property == FontWeightProperty
			|| e.Property == FontStretchProperty
			|| e.Property == FontSizeProperty
			|| e.Property == FlowDirectionProperty)
		{
			InvalidateTextLayout();
		}
		else if (e.Property == HorizontalContentAlignmentProperty || e.Property == VerticalContentAlignmentProperty)
		{
			InvalidateEditorVisual();
		}
		else if (e.Property == IsReadOnlyProperty && IsReadOnly)
		{
			TryCompleteImeComposition();
			if (IsImeCompositionActive)
				CommitImeComposition(_imeCompositionText);
			CancelImeComposition();
		}
	}

	private double GetEffectiveCaretWidth()
	{
		if (!double.IsNaN(CaretWidth))
			return CaretWidth;

		return Math.Max(1, SystemParameters.CaretWidth);
	}

	private static string NormalizeLineEndings(string text)
		=> string.IsNullOrEmpty(text) ? string.Empty : text.Replace("\r\n", "\n").Replace('\r', '\n');

	private void InvalidateMultilineLayout()
	{
		_multiLineLayout = null;
		_preferredCaretX = double.NaN;
		SetValue(LineCountPropertyKey, CountLogicalLines(Text));
	}

	private static int CountLogicalLines(string text)
	{
		var count = 1;
		foreach (var character in text)
		{
			if (character == '\n')
				count++;
		}
		return count;
	}

	private TextLayoutCache GetMultilineLayout(Rect textRect, bool updateScrollInfo = true)
	{
		var sourceText = GetRenderedText();
		var wrapWidth = TextWrapping == TextWrapping.NoWrap ? double.PositiveInfinity : Math.Max(1, textRect.Width);
		var pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
		var key = new TextLayoutKey(
			sourceText,
			wrapWidth,
			TextWrapping,
			FontFamily.Source,
			FontStyle,
			FontWeight,
			FontStretch,
			FontSize,
			FlowDirection,
			pixelsPerDip,
			CultureInfo.CurrentUICulture.Name);

		if (_multiLineLayout?.Key.Equals(key) == true)
		{
			if (updateScrollInfo)
				UpdateScrollInfo(_multiLineLayout, textRect);
			return _multiLineLayout;
		}

		var lineHeight = Math.Max(1, GetLineHeight());
		var logicalLines = GetLogicalLines(sourceText);
		var visualLines = new List<VisualTextLine>();
		var extentWidth = 0d;

		for (var logicalIndex = 0; logicalIndex < logicalLines.Count; logicalIndex++)
		{
			var logical = logicalLines[logicalIndex];
			var logicalText = sourceText.Substring(logical.Start, logical.Length);
			var relativeBoundaries = GetTextElementBoundaryMap(logicalText);

			if (logical.Length == 0)
			{
				visualLines.Add(new VisualTextLine(logicalIndex, logical.Start, 0, logical.HasLineBreak, visualLines.Count * lineHeight, lineHeight, 0, string.Empty, [logical.Start], [0d]));
				continue;
			}

			if (TextWrapping == TextWrapping.NoWrap)
			{
				var width = MeasureDisplayTextWidth(GetDisplayText(logicalText) ?? string.Empty, pixelsPerDip);
				var absoluteBoundaries = new int[relativeBoundaries.Length];
				for (var i = 0; i < relativeBoundaries.Length; i++)
					absoluteBoundaries[i] = logical.Start + relativeBoundaries[i];

				var positions = new double[relativeBoundaries.Length];
				Array.Fill(positions, double.NaN);
				positions[0] = 0;
				positions[^1] = width;

				visualLines.Add(new VisualTextLine(
					logicalIndex,
					logical.Start,
					logical.Length,
					logical.HasLineBreak,
					visualLines.Count * lineHeight,
					lineHeight,
					width,
					logicalText,
					absoluteBoundaries,
					positions));
				extentWidth = Math.Max(extentWidth, width);
				continue;
			}

			var boundaryStart = 0;
			while (boundaryStart < relativeBoundaries.Length - 1)
			{
				var boundaryEnd = FindWrappedBoundaryEnd(
					logicalText,
					relativeBoundaries,
					boundaryStart,
					wrapWidth,
					pixelsPerDip);

				if (boundaryEnd < relativeBoundaries.Length - 1)
				{
					var whitespaceBreak = FindPreviousWhitespaceBoundary(logicalText, relativeBoundaries, boundaryStart, boundaryEnd);
					if (whitespaceBreak > boundaryStart)
					{
						boundaryEnd = whitespaceBreak;
					}
					else if (TextWrapping == TextWrapping.WrapWithOverflow)
					{
						boundaryEnd = FindNextWhitespaceBoundary(logicalText, relativeBoundaries, boundaryEnd);
					}
				}

				boundaryEnd = Math.Max(boundaryStart + 1, boundaryEnd);
				var relativeStart = relativeBoundaries[boundaryStart];
				var relativeEnd = relativeBoundaries[boundaryEnd];
				var absoluteStart = logical.Start + relativeStart;
				var absoluteBoundaries = new int[boundaryEnd - boundaryStart + 1];
				for (var i = 0; i < absoluteBoundaries.Length; i++)
					absoluteBoundaries[i] = logical.Start + relativeBoundaries[boundaryStart + i];

				var width = MeasureTextSegmentWidth(logicalText, relativeStart, relativeEnd, pixelsPerDip);
				var localPositions = new double[absoluteBoundaries.Length];
				Array.Fill(localPositions, double.NaN);
				localPositions[0] = 0;
				localPositions[^1] = width;
				var isLast = boundaryEnd == relativeBoundaries.Length - 1;
				visualLines.Add(new VisualTextLine(
					logicalIndex,
					absoluteStart,
					relativeEnd - relativeStart,
					isLast && logical.HasLineBreak,
					visualLines.Count * lineHeight,
					lineHeight,
					width,
					logicalText.Substring(relativeStart, relativeEnd - relativeStart),
					absoluteBoundaries,
					localPositions));
				extentWidth = Math.Max(extentWidth, width);
				boundaryStart = boundaryEnd;
			}
		}

		_multiLineLayout = new TextLayoutCache(key, logicalLines, visualLines, extentWidth, visualLines.Count * lineHeight, lineHeight);
		if (updateScrollInfo)
			UpdateScrollInfo(_multiLineLayout, textRect);
		return _multiLineLayout;
	}

	private int FindWrappedBoundaryEnd(string text, int[] boundaries, int boundaryStart, double wrapWidth, double pixelsPerDip)
	{
		var lastBoundary = boundaries.Length - 1;
		if (boundaryStart >= lastBoundary)
			return lastBoundary;

		var segmentStart = boundaries[boundaryStart];
		var low = boundaryStart + 1;
		var high = lastBoundary;
		var best = boundaryStart;

		while (low <= high)
		{
			var middle = low + ((high - low) / 2);
			var width = MeasureTextSegmentWidth(text, segmentStart, boundaries[middle], pixelsPerDip);
			if (width <= wrapWidth)
			{
				best = middle;
				low = middle + 1;
			}
			else
			{
				high = middle - 1;
			}
		}

		return best > boundaryStart ? best : boundaryStart + 1;
	}

	private static int FindPreviousWhitespaceBoundary(string text, int[] boundaries, int boundaryStart, int boundaryEnd)
	{
		for (var boundary = boundaryEnd; boundary > boundaryStart; boundary--)
		{
			var characterIndex = boundaries[boundary - 1];
			if (characterIndex < text.Length && char.IsWhiteSpace(text, characterIndex))
				return boundary;
		}

		return -1;
	}

	private static int FindNextWhitespaceBoundary(string text, int[] boundaries, int boundaryStart)
	{
		var lastBoundary = boundaries.Length - 1;
		for (var boundary = boundaryStart + 1; boundary <= lastBoundary; boundary++)
		{
			var characterIndex = boundaries[boundary - 1];
			if (characterIndex < text.Length && char.IsWhiteSpace(text, characterIndex))
				return boundary;
		}

		return lastBoundary;
	}

	private double MeasureTextSegmentWidth(string text, int start, int end, double pixelsPerDip)
	{
		var length = Math.Max(0, end - start);
		if (length == 0)
			return 0;

		var segment = start == 0 && length == text.Length
			? text
			: text.Substring(start, length);
		return MeasureDisplayTextWidth(GetDisplayText(segment) ?? string.Empty, pixelsPerDip);
	}

	private static List<LogicalTextLine> GetLogicalLines(string text)
	{
		var lines = new List<LogicalTextLine>();
		var start = 0;
		for (var i = 0; i < text.Length; i++)
		{
			if (text[i] != '\n')
				continue;
			lines.Add(new LogicalTextLine(start, i - start, true));
			start = i + 1;
		}
		lines.Add(new LogicalTextLine(start, text.Length - start, false));
		return lines;
	}

	private double GetContentOriginY(Rect textRect, TextLayoutCache layout)
	{
		if (layout.ExtentHeight >= textRect.Height || VerticalOffset > 0)
			return textRect.Top;

		return VerticalContentAlignment switch
		{
			VerticalAlignment.Bottom => textRect.Bottom - layout.ExtentHeight,
			VerticalAlignment.Center => textRect.Top + ((textRect.Height - layout.ExtentHeight) / 2),
			VerticalAlignment.Stretch when !AcceptsReturn && layout.VisualLines.Count == 1 => textRect.Top + ((textRect.Height - layout.ExtentHeight) / 2),
			_ => textRect.Top
		};
	}

	private double GetVisualLineOriginX(Rect textRect, VisualTextLine line)
	{
		if (TextWrapping == TextWrapping.NoWrap && (line.Width > textRect.Width || HorizontalOffset > 0))
			return textRect.Left - HorizontalOffset;

		return HorizontalContentAlignment switch
		{
			HorizontalAlignment.Center => textRect.Left + Math.Max(0, (textRect.Width - line.Width) / 2),
			HorizontalAlignment.Right => textRect.Right - Math.Min(textRect.Width, line.Width),
			HorizontalAlignment.Stretch when FlowDirection == FlowDirection.RightToLeft => textRect.Right - Math.Min(textRect.Width, line.Width),
			_ => textRect.Left
		};
	}

	private double GetTextDrawingOriginX(Rect textRect, VisualTextLine line)
	{
		var left = GetVisualLineOriginX(textRect, line);
		return FlowDirection == FlowDirection.RightToLeft ? left + line.Width : left;
	}

	private double GetBoundaryX(VisualTextLine line, int sourceIndex)
	{
		sourceIndex = Math.Clamp(sourceIndex, line.Start, line.End);
		var position = Array.BinarySearch(line.Boundaries, sourceIndex);
		if (position < 0)
			position = Math.Clamp(~position - 1, 0, line.Positions.Length - 1);

		var logicalX = GetBoundaryPosition(line, position);
		return FlowDirection == FlowDirection.RightToLeft ? line.Width - logicalX : logicalX;
	}

	private double GetBoundaryPosition(VisualTextLine line, int boundaryPosition)
	{
		boundaryPosition = Math.Clamp(boundaryPosition, 0, line.Positions.Length - 1);
		var cached = line.Positions[boundaryPosition];
		if (!double.IsNaN(cached))
			return cached;

		var sourceLength = Math.Clamp(line.Boundaries[boundaryPosition] - line.Start, 0, line.Text.Length);
		var pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
		var measured = MeasureTextSegmentWidth(line.Text, 0, sourceLength, pixelsPerDip);
		line.Positions[boundaryPosition] = measured;
		return measured;
	}

	private static VisualTextLine FindVisualLine(TextLayoutCache layout, int index)
		=> layout.VisualLines[FindVisualLineIndex(layout, index)];

	private static int FindVisualLineIndex(TextLayoutCache layout, int index)
	{
		index = Math.Clamp(index, 0, layout.Key.Text.Length);
		var lines = layout.VisualLines;
		var low = 0;
		var high = lines.Count - 1;

		while (low <= high)
		{
			var middle = low + ((high - low) / 2);
			var line = lines[middle];

			if (index < line.Start)
			{
				high = middle - 1;
				continue;
			}

			if (index < line.End || index == line.End && (line.HasLineBreak || middle == lines.Count - 1))
				return middle;

			low = middle + 1;
		}

		return Math.Clamp(low, 0, lines.Count - 1);
	}

	private int HitTestTextPosition(Point point, Rect textRect)
	{
		var layout = GetMultilineLayout(textRect);
		var contentY = point.Y - GetContentOriginY(textRect, layout) + VerticalOffset;
		var visualIndex = Math.Clamp((int)Math.Floor(contentY / layout.LineHeight), 0, layout.VisualLines.Count - 1);
		var line = layout.VisualLines[visualIndex];
		var localX = point.X - GetVisualLineOriginX(textRect, line);
		if (FlowDirection == FlowDirection.RightToLeft)
			localX = line.Width - localX;

		if (localX <= 0 || line.Boundaries.Length <= 1)
			return line.Start;
		if (localX >= line.Width)
			return line.End;

		var low = 1;
		var high = line.Positions.Length - 1;
		while (low <= high)
		{
			var middle = low + ((high - low) / 2);
			var previousX = GetBoundaryPosition(line, middle - 1);
			var currentX = GetBoundaryPosition(line, middle);
			var threshold = previousX + ((currentX - previousX) / 2);
			if (localX < threshold)
				high = middle - 1;
			else
				low = middle + 1;
		}
		return line.Boundaries[Math.Clamp(low - 1, 0, line.Boundaries.Length - 1)];
	}

	private void EnsureMultilineCaretVisible()
	{
		if (ActualWidth <= 0)
			return;
		var textRect = GetTextRect();
		if (textRect.Width <= 0 || textRect.Height <= 0)
			return;

		var layout = GetMultilineLayout(textRect);
		var caretIndex = IsImeCompositionActive ? GetImeCompositionEndIndex() : CaretIndex;
		var line = FindVisualLine(layout, caretIndex);
		var caretX = GetBoundaryX(line, Math.Min(caretIndex, line.End));
		if (caretX < HorizontalOffset)
			SetHorizontalOffset(caretX);
		else if (caretX + GetEffectiveCaretWidth() > HorizontalOffset + textRect.Width)
			SetHorizontalOffset(caretX + GetEffectiveCaretWidth() - textRect.Width);

		if (line.Y < VerticalOffset)
			SetVerticalOffset(line.Y);
		else if (line.Y + line.Height > VerticalOffset + textRect.Height)
			SetVerticalOffset(line.Y + line.Height - textRect.Height);

		UpdateImeCandidateWindow();
	}

	private void MoveCaretVertically(int visualLineDelta, bool extendSelection)
	{
		var textRect = GetTextRect();
		var layout = GetMultilineLayout(textRect);
		var currentLineIndex = FindVisualLineIndex(layout, CaretIndex);
		var current = layout.VisualLines[currentLineIndex];
		if (double.IsNaN(_preferredCaretX))
			_preferredCaretX = GetBoundaryX(current, Math.Min(CaretIndex, current.End));

		var target = layout.VisualLines[Math.Clamp(currentLineIndex + visualLineDelta, 0, layout.VisualLines.Count - 1)];
		var point = new Point(GetVisualLineOriginX(textRect, target) + _preferredCaretX, GetContentOriginY(textRect, layout) + target.Y - VerticalOffset + (target.Height / 2));
		var preferred = _preferredCaretX;
		MoveCaret(HitTestTextPosition(point, textRect), extendSelection);
		_preferredCaretX = preferred;
	}

	private void MoveCaretByPage(int direction, bool extendSelection)
	{
		var linesPerPage = Math.Max(1, (int)Math.Floor(Math.Max(GetLineHeight(), ViewportHeight) / GetLineHeight()));
		MoveCaretVertically(direction * linesPerPage, extendSelection);
	}

	private void MoveCaretToVisualLineBoundary(bool end, bool extendSelection)
	{
		var layout = GetMultilineLayout(GetTextRect());
		var line = FindVisualLine(layout, CaretIndex);
		MoveCaret(end ? line.End : line.Start, extendSelection);
	}

	private void DrawMultilineText(DrawingContext drawingContext, Rect textRect, Brush brush, bool usePlaceholder)
	{
		if (usePlaceholder && !IsImeCompositionActive && string.IsNullOrEmpty(Text) && !string.IsNullOrEmpty(Placeholder))
		{
			var placeholder = CreateFormattedText(Placeholder, PlaceholderBrush ?? brush, isPlaceholder: true);
			var placeholderLeft = HorizontalContentAlignment switch
			{
				HorizontalAlignment.Center => textRect.Left + Math.Max(0, (textRect.Width - placeholder.WidthIncludingTrailingWhitespace) / 2),
				HorizontalAlignment.Right => textRect.Right - Math.Min(textRect.Width, placeholder.WidthIncludingTrailingWhitespace),
				HorizontalAlignment.Stretch when FlowDirection == FlowDirection.RightToLeft => textRect.Right - Math.Min(textRect.Width, placeholder.WidthIncludingTrailingWhitespace),
				_ => textRect.Left
			};
			var placeholderX = FlowDirection == FlowDirection.RightToLeft
				? placeholderLeft + placeholder.WidthIncludingTrailingWhitespace
				: placeholderLeft;
			drawingContext.DrawText(placeholder, new Point(placeholderX, GetContentOriginY(textRect, GetMultilineLayout(textRect))));
			return;
		}

		var layout = GetMultilineLayout(textRect);
		var originY = GetContentOriginY(textRect, layout);
		var (firstVisible, lastVisibleExclusive) = GetVisibleVisualLineRange(layout);
		for (var i = firstVisible; i < lastVisibleExclusive; i++)
		{
			var line = layout.VisualLines[i];
			if (line.Text.Length == 0)
				continue;
			var formattedText = GetCachedFormattedText(line, brush);
			drawingContext.DrawText(formattedText, new Point(GetTextDrawingOriginX(textRect, line), originY + line.Y - VerticalOffset));
		}
	}

	private FormattedText GetCachedFormattedText(VisualTextLine line, Brush brush)
	{
		if (ReferenceEquals(line.PrimaryBrush, brush) && line.PrimaryFormattedText is not null)
			return line.PrimaryFormattedText;

		if (ReferenceEquals(line.SecondaryBrush, brush) && line.SecondaryFormattedText is not null)
			return line.SecondaryFormattedText;

		var formattedText = CreateFormattedText(line.Text, brush);
		if (line.PrimaryFormattedText is null)
		{
			line.PrimaryBrush = brush;
			line.PrimaryFormattedText = formattedText;
		}
		else
		{
			line.SecondaryBrush = brush;
			line.SecondaryFormattedText = formattedText;
		}

		return formattedText;
	}

	private (int First, int LastExclusive) GetVisibleVisualLineRange(TextLayoutCache layout)
	{
		if (layout.VisualLines.Count == 0)
			return (0, 0);

		var lineHeight = Math.Max(1, layout.LineHeight);
		var first = Math.Clamp((int)Math.Floor(VerticalOffset / lineHeight), 0, layout.VisualLines.Count - 1);
		var visibleHeight = Math.Max(lineHeight, ViewportHeight);
		var lastExclusive = Math.Clamp(
			(int)Math.Ceiling((VerticalOffset + visibleHeight) / lineHeight),
			first + 1,
			layout.VisualLines.Count);

		return (first, lastExclusive);
	}

	private IEnumerable<Rect> GetRangeRectangles(TextLayoutCache layout, Rect textRect, int start, int end, bool includeLineBreaks)
	{
		var originY = GetContentOriginY(textRect, layout);
		var (firstVisible, lastVisibleExclusive) = GetVisibleVisualLineRange(layout);
		for (var i = firstVisible; i < lastVisibleExclusive; i++)
		{
			var line = layout.VisualLines[i];
			var intersectionStart = Math.Max(start, line.Start);
			var intersectionEnd = Math.Min(end, line.End);
			var selectsBreak = includeLineBreaks && line.HasLineBreak && start <= line.End && end > line.End;
			if (intersectionEnd <= intersectionStart && !selectsBreak)
				continue;

			var x1 = GetVisualLineOriginX(textRect, line) + GetBoundaryX(line, intersectionStart);
			var x2 = GetVisualLineOriginX(textRect, line) + GetBoundaryX(line, intersectionEnd);
			var left = Math.Min(x1, x2);
			var right = Math.Max(x1, x2);
			if (selectsBreak)
			{
				if (FlowDirection == FlowDirection.RightToLeft)
					left -= Math.Max(4, GetEffectiveCaretWidth());
				else
					right += Math.Max(4, GetEffectiveCaretWidth());
			}
			var rect = new Rect(left, originY + line.Y - VerticalOffset, Math.Max(GetEffectiveCaretWidth(), right - left), line.Height);
			rect.Intersect(textRect);
			if (!rect.IsEmpty)
				yield return rect;
		}
	}

	private void DrawMultilineSelection(DrawingContext drawingContext, Rect textRect)
	{
		if (IsImeCompositionActive || !ShouldRenderSelection())
			return;
		var layout = GetMultilineLayout(textRect);
		var brush = IsKeyboardFocusWithin ? SelectionBrush : InactiveSelectionBrush;
		foreach (var rect in GetRangeRectangles(layout, textRect, SelectionStart, SelectionStart + SelectionLength, includeLineBreaks: true))
			drawingContext.DrawRectangle(brush, null, rect);
	}

	private void DrawMultilineSelectedText(DrawingContext drawingContext, Rect textRect)
	{
		if (IsImeCompositionActive || !ShouldRenderSelection())
			return;
		var brush = IsKeyboardFocusWithin ? SelectionTextBrush ?? Foreground ?? Brushes.Black : InactiveSelectionTextBrush ?? Foreground ?? Brushes.Black;
		var layout = GetMultilineLayout(textRect);
		var originY = GetContentOriginY(textRect, layout);
		var selectionEnd = SelectionStart + SelectionLength;
		var (firstVisible, lastVisibleExclusive) = GetVisibleVisualLineRange(layout);
		for (var i = firstVisible; i < lastVisibleExclusive; i++)
		{
			var line = layout.VisualLines[i];
			var start = Math.Max(SelectionStart, line.Start);
			var end = Math.Min(selectionEnd, line.End);
			if (end <= start || line.Text.Length == 0)
				continue;
			var x1 = GetVisualLineOriginX(textRect, line) + GetBoundaryX(line, start);
			var x2 = GetVisualLineOriginX(textRect, line) + GetBoundaryX(line, end);
			var clip = new Rect(Math.Min(x1, x2), originY + line.Y - VerticalOffset, Math.Abs(x2 - x1), line.Height);
			clip.Intersect(textRect);
			if (clip.IsEmpty)
				continue;
			drawingContext.PushClip(new RectangleGeometry(clip));
			drawingContext.DrawText(GetCachedFormattedText(line, brush), new Point(GetTextDrawingOriginX(textRect, line), originY + line.Y - VerticalOffset));
			drawingContext.Pop();
		}
	}

	private void DrawMultilineImeComposition(DrawingContext drawingContext, Rect textRect)
	{
		if (!IsImeCompositionActive || ImeCompositionUnderlineThickness <= 0 || _imeCompositionText.Length == 0)
			return;
		var layout = GetMultilineLayout(textRect);
		var brush = ImeCompositionUnderlineBrush ?? CaretBrush ?? Foreground ?? Brushes.Black;
		var pen = new Pen(brush, ImeCompositionUnderlineThickness);
		pen.Freeze();
		foreach (var rect in GetRangeRectangles(layout, textRect, _imeCompositionStart, GetImeCompositionEndIndex(), includeLineBreaks: false))
			drawingContext.DrawLine(pen, new Point(rect.Left, rect.Bottom - (ImeCompositionUnderlineThickness / 2)), new Point(rect.Right, rect.Bottom - (ImeCompositionUnderlineThickness / 2)));
	}

	private void DrawMultilineCaret(DrawingContext drawingContext, Rect textRect)
	{
		if (!IsKeyboardFocused || (!IsImeCompositionActive && HasSelection) || !_isCaretVisible)
			return;
		var caretIndex = IsImeCompositionActive ? GetImeCompositionEndIndex() : CaretIndex;
		var rect = GetRectFromRenderedCharacterIndex(caretIndex, textRect);
		if (rect.IntersectsWith(textRect))
			drawingContext.DrawRectangle(CaretBrush ?? Foreground, null, rect);
	}

	private Rect GetRectFromRenderedCharacterIndex(int index, Rect textRect)
	{
		var layout = GetMultilineLayout(textRect);
		var line = FindVisualLine(layout, index);
		return new Rect(
			GetVisualLineOriginX(textRect, line) + GetBoundaryX(line, Math.Min(index, line.End)),
			GetContentOriginY(textRect, layout) + line.Y - VerticalOffset,
			GetEffectiveCaretWidth(),
			line.Height);
	}

	private Size MeasureMultilineContent(Size constraint, Size templateSize)
	{
		var lineHeight = Math.Max(1, GetLineHeight());
		var verticalChrome = BorderThickness.Top + BorderThickness.Bottom + Padding.Top + Padding.Bottom;
		var desiredHeight = (Math.Max(1, _desiredVisibleLineCount) * lineHeight) + verticalChrome;

		// Width may come from the template (icon/sub-controls/placeholder), but editable
		// document width never participates in the control's DesiredSize. Height is the
		// opposite: it follows the current number of visual lines, clamped by MinLines /
		// MaxLines. Scrollbars themselves still must not add to the natural size.
		var desiredWidth = templateSize.Width;

		if (!double.IsInfinity(constraint.Width))
			desiredWidth = Math.Min(desiredWidth, Math.Max(0, constraint.Width));
		if (!double.IsInfinity(constraint.Height))
			desiredHeight = Math.Min(desiredHeight, Math.Max(0, constraint.Height));

		return new Size(Math.Max(0, desiredWidth), Math.Max(0, desiredHeight));
	}

	private void UpdateImeCandidateWindow()
	{
		if (!IsImeCompositionActive || PresentationSource.FromVisual(this) is not HwndSource source)
			return;
		try
		{
			var caret = GetRectFromRenderedCharacterIndex(GetImeCompositionEndIndex(), GetTextRect());
			var screenPoint = PointToScreen(new Point(caret.Left, caret.Bottom));
			var point = new NativePoint((int)Math.Round(screenPoint.X), (int)Math.Round(screenPoint.Y));
			if (!ScreenToClient(source.Handle, ref point))
				return;
			var context = ImmGetContext(source.Handle);
			if (context == IntPtr.Zero)
				return;
			try
			{
				var candidate = new CandidateForm { Style = 0x0040, CurrentPosition = point };
				ImmSetCandidateWindow(context, ref candidate);
				var composition = new CompositionForm { Style = 0x0002, CurrentPosition = point };
				ImmSetCompositionWindow(context, ref composition);
			}
			finally
			{
				ImmReleaseContext(source.Handle, context);
			}
		}
		catch (InvalidOperationException)
		{
			// The visual may be disconnected while a composition is being cancelled.
		}
	}

	protected override AutomationPeer OnCreateAutomationPeer()
		=> CreateAutomationPeerCore();

	protected virtual AutomationPeer CreateAutomationPeerCore()
		=> new StswInputBoxBaseAutomationPeer(this);

	protected internal virtual string GetAutomationValue() => Text;
	protected internal virtual bool IsAutomationValueProtected => false;

	internal void SetAutomationValue(string value)
	{
		if (!IsEnabled || IsReadOnly || IsAutomationValueProtected)
			throw new InvalidOperationException("The value cannot be changed through automation.");
		var normalized = NormalizeLineEndings(value ?? string.Empty);
		ApplyTextChange(normalized, normalized.Length, normalized.Length, 0, TextChangeKind.Programmatic, allowUndoMerge: false);
	}

	private void RaiseAutomationTextChanged(string oldText, string newText)
	{
		if (UIElementAutomationPeer.FromElement(this) is StswInputBoxBaseAutomationPeer peer)
			peer.RaiseValueChanged(oldText, newText);
	}

	private void RaiseAutomationSelectionChanged()
	{
		if (UIElementAutomationPeer.FromElement(this) is StswInputBoxBaseAutomationPeer peer)
			peer.RaiseSelectionChanged();
	}

	private void BeginTextDrag(Point point)
	{
		_dragStartPoint = point;
		_dragSelectionStart = SelectionStart;
		_dragSelectionLength = SelectionLength;
		_isDragCandidate = true;
	}

	private bool TryStartTextDrag(Point currentPoint)
	{
		if (!_isDragCandidate || !IsTextDragDropEnabled || _dragSelectionLength <= 0)
			return false;
		if (Math.Abs(currentPoint.X - _dragStartPoint.X) < SystemParameters.MinimumHorizontalDragDistance
			&& Math.Abs(currentPoint.Y - _dragStartPoint.Y) < SystemParameters.MinimumVerticalDragDistance)
			return false;

		_isDragCandidate = false;
		if (IsMouseCaptured)
			ReleaseMouseCapture();
		var selectedText = Text.Substring(_dragSelectionStart, _dragSelectionLength);
		var data = new DataObject();
		data.SetData(DataFormats.UnicodeText, selectedText);
		data.SetData(InternalTextDragFormat, this);
		_internalDropCompleted = false;
		var effect = DragDrop.DoDragDrop(this, data, DragDropEffects.Copy | DragDropEffects.Move);
		if (effect == DragDropEffects.Move && !_internalDropCompleted && !IsReadOnly)
		{
			Select(_dragSelectionStart, Math.Min(_dragSelectionLength, Math.Max(0, Text.Length - _dragSelectionStart)));
			DeleteSelection(TextChangeKind.DragDrop);
		}
		return true;
	}

	protected override void OnDragOver(DragEventArgs e)
	{
		base.OnDragOver(e);
		if (!CanAcceptTextDrop(e))
		{
			e.Effects = DragDropEffects.None;
			return;
		}
		e.Effects = Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && e.AllowedEffects.HasFlag(DragDropEffects.Copy)
			? DragDropEffects.Copy
			: e.AllowedEffects.HasFlag(DragDropEffects.Move) ? DragDropEffects.Move : DragDropEffects.Copy;
		var point = e.GetPosition(this);
		AutoScrollForPoint(point);
		SetCaretAndClearSelection(HitTestTextPosition(point, GetTextRect()));
		e.Handled = true;
	}

	protected override void OnDrop(DragEventArgs e)
	{
		base.OnDrop(e);
		if (!CanAcceptTextDrop(e) || e.Data.GetData(DataFormats.UnicodeText) is not string droppedText)
			return;

		var dropIndex = HitTestTextPosition(e.GetPosition(this), GetTextRect());
		var source = e.Data.GetData(InternalTextDragFormat) as StswInputBoxBase;
		var copy = Keyboard.Modifiers.HasFlag(ModifierKeys.Control) || e.Effects == DragDropEffects.Copy;
		droppedText = NormalizeInsertedText(droppedText, isPaste: true);
		if (ReferenceEquals(source, this) && !copy)
		{
			if (dropIndex >= _dragSelectionStart && dropIndex <= _dragSelectionStart + _dragSelectionLength)
			{
				_internalDropCompleted = true;
				e.Effects = DragDropEffects.None;
				e.Handled = true;
				return;
			}
			var text = Text.Remove(_dragSelectionStart, _dragSelectionLength);
			if (dropIndex > _dragSelectionStart)
				dropIndex -= _dragSelectionLength;
			text = text.Insert(dropIndex, droppedText);
			ApplyTextChange(text, dropIndex + droppedText.Length, dropIndex, droppedText.Length, TextChangeKind.DragDrop, allowUndoMerge: false);
			_internalDropCompleted = true;
		}
		else
		{
			SetCaretAndClearSelection(dropIndex);
			InsertText(droppedText, TextChangeKind.DragDrop, allowUndoMerge: false);
		}
		e.Effects = copy ? DragDropEffects.Copy : DragDropEffects.Move;
		OnTextDropCompleted(droppedText, dropIndex, e.Effects, ReferenceEquals(source, this));
		e.Handled = true;
	}

	/// <summary>Determines whether a drag operation can supply editable text to this control.</summary>
	protected virtual bool CanAcceptTextDrop(DragEventArgs e)
		=> IsTextDragDropEnabled && !IsReadOnly && IsEnabled && e.Data.GetDataPresent(DataFormats.UnicodeText);

	/// <summary>Called after dropped text has been applied successfully.</summary>
	protected virtual void OnTextDropCompleted(string text, int dropIndex, DragDropEffects effect, bool isInternal)
	{
	}

	private void AutoScrollForPoint(Point point)
	{
		var rect = GetTextRect();
		if (point.Y < rect.Top + 8)
			LineUp();
		else if (point.Y > rect.Bottom - 8)
			LineDown();
		if (point.X < rect.Left + 8)
			LineLeft();
		else if (point.X > rect.Right - 8)
			LineRight();
	}

	[DllImport("user32.dll")]
	private static extern uint GetCaretBlinkTime();
	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool ScreenToClient(IntPtr window, ref NativePoint point);
	[DllImport("imm32.dll")]
	private static extern IntPtr ImmGetContext(IntPtr window);
	[DllImport("imm32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool ImmReleaseContext(IntPtr window, IntPtr context);
	[DllImport("imm32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool ImmSetCandidateWindow(IntPtr context, ref CandidateForm form);
	[DllImport("imm32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool ImmSetCompositionWindow(IntPtr context, ref CompositionForm form);

	[StructLayout(LayoutKind.Sequential)]
	private struct NativePoint(int x, int y)
	{
		public int X = x;
		public int Y = y;
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct CandidateForm
	{
		public int Index;
		public int Style;
		public NativePoint CurrentPosition;
		public NativeRect Area;
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct CompositionForm
	{
		public int Style;
		public NativePoint CurrentPosition;
		public NativeRect Area;
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct NativeRect
	{
		public int Left;
		public int Top;
		public int Right;
		public int Bottom;
	}

	private sealed record LogicalTextLine(int Start, int Length, bool HasLineBreak)
	{
		public int End => Start + Length;
	}

	private sealed record VisualTextLine(
		int LogicalLineIndex,
		int Start,
		int Length,
		bool HasLineBreak,
		double Y,
		double Height,
		double Width,
		string Text,
		int[] Boundaries,
		double[] Positions)
	{
		public int End => Start + Length;
		public Brush? PrimaryBrush { get; set; }
		public FormattedText? PrimaryFormattedText { get; set; }
		public Brush? SecondaryBrush { get; set; }
		public FormattedText? SecondaryFormattedText { get; set; }
	}

	private sealed record TextLayoutKey(
		string Text,
		double WrapWidth,
		TextWrapping Wrapping,
		string FontFamily,
		FontStyle FontStyle,
		FontWeight FontWeight,
		FontStretch FontStretch,
		double FontSize,
		FlowDirection FlowDirection,
		double PixelsPerDip,
		string CultureName);

	private sealed record TextLayoutCache(
		TextLayoutKey Key,
		List<LogicalTextLine> LogicalLines,
		List<VisualTextLine> VisualLines,
		double ExtentWidth,
		double ExtentHeight,
		double LineHeight);
}