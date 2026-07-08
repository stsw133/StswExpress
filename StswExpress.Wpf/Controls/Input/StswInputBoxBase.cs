using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace StswExpress.Wpf;

/// <summary>
/// Base class for custom text input controls rendered without using the native WPF TextBox.
/// </summary>
public abstract class StswInputBoxBase : Control
{
	private const double CaretWidth = 1.0;
	private const int CaretBlinkMilliseconds = 530;

	private readonly DispatcherTimer _caretTimer;
	private readonly Stack<TextState> _undoStack = [];
	private readonly Stack<TextState> _redoStack = [];
	private ContextMenu? _defaultContextMenu;
	private TextChangeKind _activeUndoUnitKind;
	private bool _isCaretVisible;
	private bool _isInternalTextChange;
	private bool _isRestoringHistory;
	private int _selectionAnchor;
	private double _horizontalOffset;

	static StswInputBoxBase()
	{
		FocusableProperty.OverrideMetadata(typeof(StswInputBoxBase), new FrameworkPropertyMetadata(true));
		CursorProperty.OverrideMetadata(typeof(StswInputBoxBase), new FrameworkPropertyMetadata(Cursors.IBeam));
		HorizontalContentAlignmentProperty.OverrideMetadata(typeof(StswInputBoxBase), new FrameworkPropertyMetadata(HorizontalAlignment.Left, FrameworkPropertyMetadataOptions.AffectsRender));
		VerticalContentAlignmentProperty.OverrideMetadata(typeof(StswInputBoxBase), new FrameworkPropertyMetadata(VerticalAlignment.Center, FrameworkPropertyMetadataOptions.AffectsRender));
	}

	protected StswInputBoxBase()
	{
		SnapsToDevicePixels = true;

		_caretTimer = new DispatcherTimer
		{
			Interval = TimeSpan.FromMilliseconds(CaretBlinkMilliseconds)
		};
		_caretTimer.Tick += (_, _) =>
		{
			_isCaretVisible = !_isCaretVisible;
			InvalidateVisual();
		};

		CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, OnCopyCommand, CanCopyCommand));
		CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, OnCutCommand, CanCutCommand));
		CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, OnPasteCommand, CanPasteCommand));
		CommandBindings.Add(new CommandBinding(ApplicationCommands.SelectAll, OnSelectAllCommand, CanSelectAllCommand));
		CommandBindings.Add(new CommandBinding(ApplicationCommands.Undo, OnUndoCommand, CanUndoCommand));
		CommandBindings.Add(new CommandBinding(ApplicationCommands.Redo, OnRedoCommand, CanRedoCommand));

		ContextMenuOpening += OnContextMenuOpening;
	}

	#region Text

	public static readonly DependencyProperty TextProperty =
		DependencyProperty.Register(
			nameof(Text),
			typeof(string),
			typeof(StswInputBoxBase),
			CreateTextMetadata());

	public string Text
	{
		get => (string)GetValue(TextProperty);
		set => SetValue(TextProperty, value);
	}

	private static FrameworkPropertyMetadata CreateTextMetadata()
	{
		var metadata = new FrameworkPropertyMetadata(
			string.Empty,
			FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
			OnTextChanged,
			CoerceText);

		metadata.DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
		return metadata;
	}

	private static object CoerceText(DependencyObject d, object baseValue)
		=> baseValue as string ?? string.Empty;

	private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is not StswInputBoxBase input)
			return;

		if (!input._isInternalTextChange)
			input.ClearHistory();

		input.CoerceCaretAndSelection();
		input.EnsureCaretVisible();
		input.InvalidateMeasure();
		input.InvalidateVisual();
	}

	#endregion

	#region CaretIndex

	public static readonly DependencyProperty CaretIndexProperty =
		DependencyProperty.Register(
			nameof(CaretIndex),
			typeof(int),
			typeof(StswInputBoxBase),
			new FrameworkPropertyMetadata(0, OnCaretIndexChanged, CoerceCaretIndex));

	public int CaretIndex
	{
		get => (int)GetValue(CaretIndexProperty);
		set => SetValue(CaretIndexProperty, value);
	}

	private static object CoerceCaretIndex(DependencyObject d, object baseValue)
	{
		var input = (StswInputBoxBase)d;
		var value = (int)baseValue;
		return Math.Clamp(value, 0, input.Text.Length);
	}

	private static void OnCaretIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is not StswInputBoxBase input)
			return;

		input.EnsureCaretVisible();
		input.ResetCaretBlink();
		input.InvalidateVisual();
	}

	#endregion

	#region SelectionStart

	public static readonly DependencyProperty SelectionStartProperty =
		DependencyProperty.Register(
			nameof(SelectionStart),
			typeof(int),
			typeof(StswInputBoxBase),
			new FrameworkPropertyMetadata(0, OnSelectionChanged, CoerceSelectionStart));

	public int SelectionStart
	{
		get => (int)GetValue(SelectionStartProperty);
		set => SetValue(SelectionStartProperty, value);
	}

	private static object CoerceSelectionStart(DependencyObject d, object baseValue)
	{
		var input = (StswInputBoxBase)d;
		var value = (int)baseValue;
		return Math.Clamp(value, 0, input.Text.Length);
	}

	#endregion

	#region SelectionLength

	public static readonly DependencyProperty SelectionLengthProperty =
		DependencyProperty.Register(
			nameof(SelectionLength),
			typeof(int),
			typeof(StswInputBoxBase),
			new FrameworkPropertyMetadata(0, OnSelectionChanged, CoerceSelectionLength));

	public int SelectionLength
	{
		get => (int)GetValue(SelectionLengthProperty);
		set => SetValue(SelectionLengthProperty, value);
	}

	private static object CoerceSelectionLength(DependencyObject d, object baseValue)
	{
		var input = (StswInputBoxBase)d;
		var value = (int)baseValue;

		if (value < 0)
			return 0;

		return Math.Min(value, input.Text.Length - input.SelectionStart);
	}

	private static void OnSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is not StswInputBoxBase input)
			return;

		input.ResetCaretBlink();
		input.InvalidateVisual();
	}

	#endregion

	#region IsReadOnly

	public static readonly DependencyProperty IsReadOnlyProperty =
		DependencyProperty.Register(
			nameof(IsReadOnly),
			typeof(bool),
			typeof(StswInputBoxBase),
			new FrameworkPropertyMetadata(false));

	public bool IsReadOnly
	{
		get => (bool)GetValue(IsReadOnlyProperty);
		set => SetValue(IsReadOnlyProperty, value);
	}

	#endregion

	#region AcceptsTab

	public static readonly DependencyProperty AcceptsTabProperty =
		DependencyProperty.Register(
			nameof(AcceptsTab),
			typeof(bool),
			typeof(StswInputBoxBase),
			new FrameworkPropertyMetadata(false));

	public bool AcceptsTab
	{
		get => (bool)GetValue(AcceptsTabProperty);
		set => SetValue(AcceptsTabProperty, value);
	}

	#endregion

	#region MaxLength

	public static readonly DependencyProperty MaxLengthProperty =
		DependencyProperty.Register(
			nameof(MaxLength),
			typeof(int),
			typeof(StswInputBoxBase),
			new FrameworkPropertyMetadata(0, null, CoerceMaxLength));

	public int MaxLength
	{
		get => (int)GetValue(MaxLengthProperty);
		set => SetValue(MaxLengthProperty, value);
	}

	private static object CoerceMaxLength(DependencyObject d, object baseValue)
		=> Math.Max(0, (int)baseValue);

	#endregion

	#region Placeholder

	public static readonly DependencyProperty PlaceholderProperty =
		DependencyProperty.Register(
			nameof(Placeholder),
			typeof(string),
			typeof(StswInputBoxBase),
			new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender, null, CoercePlaceholder));

	public string Placeholder
	{
		get => (string)GetValue(PlaceholderProperty);
		set => SetValue(PlaceholderProperty, value);
	}

	private static object CoercePlaceholder(DependencyObject d, object baseValue)
		=> baseValue as string ?? string.Empty;

	#endregion

	#region SelectionBrush

	public static readonly DependencyProperty SelectionBrushProperty =
		DependencyProperty.Register(
			nameof(SelectionBrush),
			typeof(Brush),
			typeof(StswInputBoxBase),
			new FrameworkPropertyMetadata(SystemColors.HighlightBrush, FrameworkPropertyMetadataOptions.AffectsRender));

	public Brush? SelectionBrush
	{
		get => (Brush?)GetValue(SelectionBrushProperty);
		set => SetValue(SelectionBrushProperty, value);
	}

	#endregion

	#region SelectionTextBrush

	public static readonly DependencyProperty SelectionTextBrushProperty =
		DependencyProperty.Register(
			nameof(SelectionTextBrush),
			typeof(Brush),
			typeof(StswInputBoxBase),
			new FrameworkPropertyMetadata(SystemColors.HighlightTextBrush, FrameworkPropertyMetadataOptions.AffectsRender));

	public Brush? SelectionTextBrush
	{
		get => (Brush?)GetValue(SelectionTextBrushProperty);
		set => SetValue(SelectionTextBrushProperty, value);
	}

	#endregion

	#region PlaceholderBrush

	public static readonly DependencyProperty PlaceholderBrushProperty =
		DependencyProperty.Register(
			nameof(PlaceholderBrush),
			typeof(Brush),
			typeof(StswInputBoxBase),
			new FrameworkPropertyMetadata(SystemColors.GrayTextBrush, FrameworkPropertyMetadataOptions.AffectsRender));

	public Brush? PlaceholderBrush
	{
		get => (Brush?)GetValue(PlaceholderBrushProperty);
		set => SetValue(PlaceholderBrushProperty, value);
	}

	#endregion

	#region CaretBrush

	public static readonly DependencyProperty CaretBrushProperty =
		DependencyProperty.Register(
			nameof(CaretBrush),
			typeof(Brush),
			typeof(StswInputBoxBase),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

	public Brush? CaretBrush
	{
		get => (Brush?)GetValue(CaretBrushProperty);
		set => SetValue(CaretBrushProperty, value);
	}

	#endregion

	#region UseDefaultContextMenu

	public static readonly DependencyProperty UseDefaultContextMenuProperty =
		DependencyProperty.Register(
			nameof(UseDefaultContextMenu),
			typeof(bool),
			typeof(StswInputBoxBase),
			new FrameworkPropertyMetadata(true, OnUseDefaultContextMenuChanged));

	public bool UseDefaultContextMenu
	{
		get => (bool)GetValue(UseDefaultContextMenuProperty);
		set => SetValue(UseDefaultContextMenuProperty, value);
	}

	private static void OnUseDefaultContextMenuChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is not StswInputBoxBase input)
			return;

		if (e.NewValue is true)
		{
			input.EnsureDefaultContextMenu();
		}
		else if (ReferenceEquals(input.ContextMenu, input._defaultContextMenu))
		{
			input.SetCurrentValue(ContextMenuProperty, null);
			input._defaultContextMenu = null;
		}
	}

	#endregion

	#region IsUndoEnabled

	public static readonly DependencyProperty IsUndoEnabledProperty =
		DependencyProperty.Register(
			nameof(IsUndoEnabled),
			typeof(bool),
			typeof(StswInputBoxBase),
			new FrameworkPropertyMetadata(true, OnIsUndoEnabledChanged));

	public bool IsUndoEnabled
	{
		get => (bool)GetValue(IsUndoEnabledProperty);
		set => SetValue(IsUndoEnabledProperty, value);
	}

	private static void OnIsUndoEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is not StswInputBoxBase input)
			return;

		if (e.NewValue is bool isEnabled && !isEnabled)
			input.ClearHistory();
	}

	#endregion

	#region UndoLimit

	public static readonly DependencyProperty UndoLimitProperty =
		DependencyProperty.Register(
			nameof(UndoLimit),
			typeof(int),
			typeof(StswInputBoxBase),
			new FrameworkPropertyMetadata(100, OnUndoLimitChanged, CoerceUndoLimit));

	public int UndoLimit
	{
		get => (int)GetValue(UndoLimitProperty);
		set => SetValue(UndoLimitProperty, value);
	}

	private static object CoerceUndoLimit(DependencyObject d, object baseValue)
		=> Math.Max(0, (int)baseValue);

	private static void OnUndoLimitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is not StswInputBoxBase input)
			return;

		input.TrimUndoStack();
		input.UpdateHistoryState();
	}

	#endregion

	#region CanUndo / CanRedo

	private static readonly DependencyPropertyKey CanUndoPropertyKey =
		DependencyProperty.RegisterReadOnly(
			nameof(CanUndo),
			typeof(bool),
			typeof(StswInputBoxBase),
			new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty CanUndoProperty = CanUndoPropertyKey.DependencyProperty;

	public bool CanUndo => (bool)GetValue(CanUndoProperty);

	private static readonly DependencyPropertyKey CanRedoPropertyKey =
		DependencyProperty.RegisterReadOnly(
			nameof(CanRedo),
			typeof(bool),
			typeof(StswInputBoxBase),
			new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty CanRedoProperty = CanRedoPropertyKey.DependencyProperty;

	public bool CanRedo => (bool)GetValue(CanRedoProperty);

	#endregion

	public string SelectedText
	{
		get
		{
			if (SelectionLength <= 0)
				return string.Empty;

			return Text.Substring(SelectionStart, SelectionLength);
		}
	}

	protected bool HasSelection => SelectionLength > 0;

	protected override void OnInitialized(EventArgs e)
	{
		base.OnInitialized(e);
		EnsureDefaultContextMenu();
	}

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		EnsureDefaultContextMenu();
	}

	protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
	{
		var point = hitTestParameters.HitPoint;
		if (point.X >= 0 && point.X <= ActualWidth && point.Y >= 0 && point.Y <= ActualHeight)
			return new PointHitTestResult(this, point);

		return base.HitTestCore(hitTestParameters);
	}

	protected override Size MeasureOverride(Size constraint)
	{
		var text = string.IsNullOrEmpty(Text) ? Placeholder : Text;
		var formattedText = CreateFormattedText(string.IsNullOrEmpty(text) ? " " : text, Foreground);

		var width = formattedText.WidthIncludingTrailingWhitespace + BorderThickness.Left + BorderThickness.Right + Padding.Left + Padding.Right + 8;
		var height = formattedText.Height + BorderThickness.Top + BorderThickness.Bottom + Padding.Top + Padding.Bottom + 6;

		if (double.IsInfinity(constraint.Width))
			width = Math.Max(120, width);
		else
			width = Math.Min(constraint.Width, Math.Max(40, width));

		if (!double.IsInfinity(constraint.Height))
			height = Math.Min(constraint.Height, height);

		return new Size(width, height);
	}

	protected override void OnRender(DrawingContext drawingContext)
	{
		base.OnRender(drawingContext);

		var outerRect = new Rect(0, 0, ActualWidth, ActualHeight);
		DrawBackgroundAndBorder(drawingContext, outerRect);

		var textRect = GetTextRect();
		if (textRect.Width <= 0 || textRect.Height <= 0)
			return;

		drawingContext.PushClip(new RectangleGeometry(textRect));

		DrawSelection(drawingContext, textRect);
		DrawText(drawingContext, textRect);
		DrawSelectedText(drawingContext, textRect);
		DrawCaret(drawingContext, textRect);

		drawingContext.Pop();
	}

	protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
	{
		base.OnRenderSizeChanged(sizeInfo);
		EnsureCaretVisible();
		InvalidateVisual();
	}

	protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
	{
		base.OnGotKeyboardFocus(e);
		ResetCaretBlink();
		_caretTimer.Start();
	}

	protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
	{
		base.OnLostKeyboardFocus(e);
		CloseUndoUnit();
		_caretTimer.Stop();
		_isCaretVisible = false;
		InvalidateVisual();
	}

	protected override void OnTextInput(TextCompositionEventArgs e)
	{
		base.OnTextInput(e);

		if (IsReadOnly)
			return;

		var text = NormalizeInputText(e.Text, replaceLineBreaksWithSpaces: false, acceptsTab: AcceptsTab, replaceTabsWithSpaces: false);
		if (string.IsNullOrEmpty(text))
			return;

		InsertText(text, TextChangeKind.Typing, allowUndoMerge: true);
		e.Handled = true;
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);

		var ctrl = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
		var shift = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);

		switch (e.Key)
		{
			case Key.Left:
				MoveCaret(GetPreviousCaretIndex(ctrl), shift);
				e.Handled = true;
				break;

			case Key.Right:
				MoveCaret(GetNextCaretIndex(ctrl), shift);
				e.Handled = true;
				break;

			case Key.Home:
				MoveCaret(0, shift);
				e.Handled = true;
				break;

			case Key.End:
				MoveCaret(Text.Length, shift);
				e.Handled = true;
				break;

			case Key.Return:
				CloseUndoUnit();
				e.Handled = true;
				break;

			case Key.Tab when AcceptsTab && !IsReadOnly:
				InsertText("\t", TextChangeKind.Typing, allowUndoMerge: true);
				e.Handled = true;
				break;

			case Key.Tab:
				CloseUndoUnit();
				break;

			case Key.Back:
				if (!IsReadOnly)
					Backspace(ctrl);
				e.Handled = true;
				break;

			case Key.Delete:
				if (!IsReadOnly)
					Delete(ctrl);
				e.Handled = true;
				break;

			case Key.A when ctrl:
				SelectAll();
				e.Handled = true;
				break;

			case Key.C when ctrl:
				Copy();
				e.Handled = true;
				break;

			case Key.X when ctrl:
				if (!IsReadOnly)
					Cut();
				e.Handled = true;
				break;

			case Key.V when ctrl:
				if (!IsReadOnly)
					Paste();
				e.Handled = true;
				break;

			case Key.Z when ctrl && shift:
				if (!IsReadOnly)
					Redo();
				e.Handled = true;
				break;

			case Key.Z when ctrl:
				if (!IsReadOnly)
					Undo();
				e.Handled = true;
				break;

			case Key.Y when ctrl:
				if (!IsReadOnly)
					Redo();
				e.Handled = true;
				break;
		}
	}

	protected override void OnMouseDown(MouseButtonEventArgs e)
	{
		base.OnMouseDown(e);

		if (e.ChangedButton != MouseButton.Left)
			return;

		CloseUndoUnit();
		Focus();

		var index = GetCaretIndexFromPoint(e.GetPosition(this));

		if (e.ClickCount >= 3)
		{
			SelectAll();
			e.Handled = true;
			return;
		}

		if (e.ClickCount == 2)
		{
			SelectWordAt(index);
			e.Handled = true;
			return;
		}

		CaptureMouse();

		if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
		{
			SetSelectionFromAnchor(index);
		}
		else
		{
			_selectionAnchor = index;
			SetCaretAndClearSelection(index);
		}

		e.Handled = true;
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);

		if (!IsMouseCaptured || e.LeftButton != MouseButtonState.Pressed)
			return;

		var index = GetCaretIndexFromPoint(e.GetPosition(this));
		SetSelectionFromAnchor(index);

		e.Handled = true;
	}

	protected override void OnMouseUp(MouseButtonEventArgs e)
	{
		base.OnMouseUp(e);

		if (IsMouseCaptured)
			ReleaseMouseCapture();

		e.Handled = true;
	}

	protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
	{
		base.OnMouseRightButtonDown(e);
		CloseUndoUnit();
		Focus();
		EnsureDefaultContextMenu();
	}

	protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
	{
		base.OnMouseRightButtonUp(e);
		EnsureDefaultContextMenu();

		if (ContextMenu is null || !ContextMenuService.GetIsEnabled(this))
			return;

		ContextMenu.PlacementTarget = this;
		ContextMenu.IsOpen = true;
		e.Handled = true;
	}

	public void SelectAll()
	{
		CloseUndoUnit();
		_selectionAnchor = 0;

		SetCurrentValue(CaretIndexProperty, Text.Length);
		SetCurrentValue(SelectionStartProperty, 0);
		SetCurrentValue(SelectionLengthProperty, Text.Length);

		ResetCaretBlink();
		InvalidateVisual();
	}

	public void Copy()
	{
		if (!HasSelection)
			return;

		TrySetClipboardText(SelectedText);
	}

	public void Cut()
	{
		if (IsReadOnly || !HasSelection)
			return;

		TrySetClipboardText(SelectedText);
		DeleteSelection(TextChangeKind.Cut);
	}

	public void Paste()
	{
		if (IsReadOnly)
			return;

		var text = TryGetClipboardText();
		if (string.IsNullOrEmpty(text))
			return;

		InsertText(NormalizeInputText(text, replaceLineBreaksWithSpaces: true, acceptsTab: AcceptsTab, replaceTabsWithSpaces: !AcceptsTab), TextChangeKind.Paste, allowUndoMerge: false);
	}

	public void Undo()
	{
		CloseUndoUnit();
		if (IsReadOnly || !IsUndoEnabled || _undoStack.Count == 0)
			return;

		var previousState = _undoStack.Pop();
		_redoStack.Push(CaptureState());
		RestoreState(previousState);
		UpdateHistoryState();
	}

	public void Redo()
	{
		CloseUndoUnit();
		if (IsReadOnly || !IsUndoEnabled || _redoStack.Count == 0)
			return;

		var nextState = _redoStack.Pop();
		_undoStack.Push(CaptureState());
		TrimUndoStack();
		RestoreState(nextState);
		UpdateHistoryState();
	}

	private void InsertText(string insertedText, TextChangeKind changeKind, bool allowUndoMerge)
	{
		if (string.IsNullOrEmpty(insertedText))
			return;

		var text = Text;
		var start = SelectionStart;
		var length = SelectionLength;
		var hadSelection = HasSelection;

		if (hadSelection)
			text = text.Remove(start, length);
		else
			start = CaretIndex;

		if (MaxLength > 0)
		{
			var available = MaxLength - text.Length;
			if (available <= 0)
				return;

			if (insertedText.Length > available)
				insertedText = insertedText.Substring(0, available);
		}

		if (insertedText.Length == 0)
			return;

		text = text.Insert(start, insertedText);

		ApplyTextChange(text, start + insertedText.Length, start + insertedText.Length, 0, changeKind, allowUndoMerge && !hadSelection);
	}

	private void Backspace(bool wordMode)
	{
		if (HasSelection)
		{
			DeleteSelection(TextChangeKind.DeleteSelection);
			return;
		}

		if (CaretIndex <= 0)
			return;

		var removeStart = wordMode ? GetPreviousWordIndex(CaretIndex) : CaretIndex - 1;
		var removeLength = CaretIndex - removeStart;

		if (removeLength <= 0)
			return;

		var text = Text.Remove(removeStart, removeLength);
		ApplyTextChange(text, removeStart, removeStart, 0, TextChangeKind.Backspace, allowUndoMerge: !wordMode);
	}

	private void Delete(bool wordMode)
	{
		if (HasSelection)
		{
			DeleteSelection(TextChangeKind.DeleteSelection);
			return;
		}

		if (CaretIndex >= Text.Length)
			return;

		var removeEnd = wordMode ? GetNextWordIndex(CaretIndex) : CaretIndex + 1;
		var removeLength = removeEnd - CaretIndex;

		if (removeLength <= 0)
			return;

		var text = Text.Remove(CaretIndex, removeLength);
		ApplyTextChange(text, CaretIndex, CaretIndex, 0, TextChangeKind.Delete, allowUndoMerge: !wordMode);
	}

	private void DeleteSelection(TextChangeKind changeKind)
	{
		if (!HasSelection)
			return;

		var start = SelectionStart;
		var text = Text.Remove(SelectionStart, SelectionLength);

		ApplyTextChange(text, start, start, 0, changeKind, allowUndoMerge: false);
	}

	private void MoveCaret(int index, bool extendSelection)
	{
		CloseUndoUnit();
		index = Math.Clamp(index, 0, Text.Length);

		if (extendSelection)
		{
			if (!HasSelection)
				_selectionAnchor = CaretIndex;

			SetSelectionFromAnchor(index);
		}
		else if (HasSelection)
		{
			if (index < CaretIndex)
				SetCaretAndClearSelection(SelectionStart);
			else if (index > CaretIndex)
				SetCaretAndClearSelection(SelectionStart + SelectionLength);
			else
				SetCaretAndClearSelection(index);
		}
		else
		{
			SetCaretAndClearSelection(index);
		}
	}

	private void SetCaretAndClearSelection(int index)
	{
		index = Math.Clamp(index, 0, Text.Length);

		SetCurrentValue(CaretIndexProperty, index);
		SetCurrentValue(SelectionStartProperty, index);
		SetCurrentValue(SelectionLengthProperty, 0);

		_selectionAnchor = index;

		EnsureCaretVisible();
		ResetCaretBlink();
		InvalidateVisual();
	}

	private void SetSelectionFromAnchor(int caretIndex)
	{
		caretIndex = Math.Clamp(caretIndex, 0, Text.Length);

		var start = Math.Min(_selectionAnchor, caretIndex);
		var length = Math.Abs(caretIndex - _selectionAnchor);

		SetCurrentValue(CaretIndexProperty, caretIndex);
		SetCurrentValue(SelectionStartProperty, start);
		SetCurrentValue(SelectionLengthProperty, length);

		EnsureCaretVisible();
		ResetCaretBlink();
		InvalidateVisual();
	}

	private void SelectWordAt(int caretIndex)
	{
		if (Text.Length == 0)
		{
			SetCaretAndClearSelection(0);
			return;
		}

		var range = GetWordRange(caretIndex);
		_selectionAnchor = range.Start;

		SetCurrentValue(CaretIndexProperty, range.End);
		SetCurrentValue(SelectionStartProperty, range.Start);
		SetCurrentValue(SelectionLengthProperty, range.End - range.Start);

		EnsureCaretVisible();
		ResetCaretBlink();
		InvalidateVisual();
	}

	private int GetPreviousCaretIndex(bool wordMode)
	{
		if (CaretIndex <= 0)
			return 0;

		return wordMode ? GetPreviousWordIndex(CaretIndex) : CaretIndex - 1;
	}

	private int GetNextCaretIndex(bool wordMode)
	{
		if (CaretIndex >= Text.Length)
			return Text.Length;

		return wordMode ? GetNextWordIndex(CaretIndex) : CaretIndex + 1;
	}

	private int GetPreviousWordIndex(int index)
	{
		var text = Text;
		index = Math.Clamp(index, 0, text.Length);

		while (index > 0 && char.IsWhiteSpace(text[index - 1]))
			index--;

		if (index <= 0)
			return 0;

		var kind = GetCharacterKind(text[index - 1]);
		while (index > 0 && GetCharacterKind(text[index - 1]) == kind)
			index--;

		return index;
	}

	private int GetNextWordIndex(int index)
	{
		var text = Text;
		index = Math.Clamp(index, 0, text.Length);

		if (index >= text.Length)
			return text.Length;

		if (char.IsWhiteSpace(text[index]))
		{
			while (index < text.Length && char.IsWhiteSpace(text[index]))
				index++;

			return index;
		}

		var kind = GetCharacterKind(text[index]);
		while (index < text.Length && GetCharacterKind(text[index]) == kind)
			index++;

		while (index < text.Length && char.IsWhiteSpace(text[index]))
			index++;

		return index;
	}

	private TextRange GetWordRange(int caretIndex)
	{
		var text = Text;
		if (text.Length == 0)
			return new TextRange(0, 0);

		var charIndex = Math.Clamp(caretIndex, 0, text.Length - 1);
		if (caretIndex == text.Length)
			charIndex = text.Length - 1;

		var kind = GetCharacterKind(text[charIndex]);
		var start = charIndex;
		var end = charIndex + 1;

		while (start > 0 && GetCharacterKind(text[start - 1]) == kind)
			start--;

		while (end < text.Length && GetCharacterKind(text[end]) == kind)
			end++;

		return new TextRange(start, end);
	}

	private int GetCaretIndexFromPoint(Point point)
	{
		var text = Text;
		var textRect = GetTextRect();
		var textWidth = GetTextWidth(text);
		var textOriginX = GetTextOriginX(textRect, textWidth, allowHorizontalScrolling: true);
		var x = point.X - textOriginX;

		if (x <= 0)
			return 0;

		for (var i = 1; i <= text.Length; i++)
		{
			var previousWidth = GetTextWidth(text.Substring(0, i - 1));
			var currentWidth = GetTextWidth(text.Substring(0, i));
			var middle = previousWidth + ((currentWidth - previousWidth) / 2);

			if (x < middle)
				return i - 1;
		}

		return text.Length;
	}

	private void EnsureCaretVisible()
	{
		if (ActualWidth <= 0)
			return;

		var textRect = GetTextRect();
		if (textRect.Width <= 0)
			return;

		var textWidth = GetTextWidth(Text);
		if (textWidth <= textRect.Width)
		{
			_horizontalOffset = 0;
			return;
		}

		var caretX = GetTextWidth(Text.Substring(0, CaretIndex));
		var rightEdge = _horizontalOffset + textRect.Width - 2;

		if (caretX < _horizontalOffset)
			_horizontalOffset = caretX;
		else if (caretX > rightEdge)
			_horizontalOffset = caretX - textRect.Width + 2;

		if (_horizontalOffset < 0)
			_horizontalOffset = 0;
	}

	private void ApplyTextChange(string text, int caretIndex, int selectionStart, int selectionLength, TextChangeKind changeKind, bool allowUndoMerge)
	{
		if (Text == text && CaretIndex == caretIndex && SelectionStart == selectionStart && SelectionLength == selectionLength)
			return;

		PushUndoState(changeKind, allowUndoMerge);
		_redoStack.Clear();

		SetTextState(new TextState(text, caretIndex, selectionStart, selectionLength));
		UpdateHistoryState();
	}

	private TextState CaptureState()
		=> new(Text, CaretIndex, SelectionStart, SelectionLength);

	private void RestoreState(TextState state)
	{
		_isRestoringHistory = true;
		try
		{
			SetTextState(state);
		}
		finally
		{
			_isRestoringHistory = false;
		}
	}

	private void SetTextState(TextState state)
	{
		var text = state.Text ?? string.Empty;
		var caretIndex = Math.Clamp(state.CaretIndex, 0, text.Length);
		var selectionStart = Math.Clamp(state.SelectionStart, 0, text.Length);
		var selectionLength = Math.Clamp(state.SelectionLength, 0, text.Length - selectionStart);

		_isInternalTextChange = true;
		try
		{
			SetCurrentValue(TextProperty, text);
		}
		finally
		{
			_isInternalTextChange = false;
		}

		SetCurrentValue(CaretIndexProperty, caretIndex);
		SetCurrentValue(SelectionStartProperty, selectionStart);
		SetCurrentValue(SelectionLengthProperty, selectionLength);

		_selectionAnchor = selectionLength > 0 && caretIndex == selectionStart
			? selectionStart + selectionLength
			: selectionStart;

		EnsureCaretVisible();
		ResetCaretBlink();
		InvalidateMeasure();
		InvalidateVisual();
	}

	private void PushUndoState(TextChangeKind changeKind, bool allowUndoMerge)
	{
		if (_isRestoringHistory || !IsUndoEnabled || UndoLimit <= 0)
			return;

		if (allowUndoMerge && _activeUndoUnitKind == changeKind && _undoStack.Count > 0)
			return;

		var state = CaptureState();
		if (_undoStack.Count > 0 && _undoStack.Peek().Equals(state))
			return;

		_undoStack.Push(state);
		_activeUndoUnitKind = changeKind;
		TrimUndoStack();
	}

	private void CloseUndoUnit()
		=> _activeUndoUnitKind = TextChangeKind.None;

	private void TrimUndoStack()
	{
		if (UndoLimit <= 0)
		{
			_undoStack.Clear();
			_redoStack.Clear();
			return;
		}

		if (_undoStack.Count <= UndoLimit)
			return;

		var states = _undoStack.ToArray();
		Array.Resize(ref states, UndoLimit);
		Array.Reverse(states);

		_undoStack.Clear();
		foreach (var state in states)
			_undoStack.Push(state);
	}

	private void ClearHistory()
	{
		CloseUndoUnit();
		_undoStack.Clear();
		_redoStack.Clear();
		UpdateHistoryState();
	}

	private void UpdateHistoryState()
	{
		SetValue(CanUndoPropertyKey, IsUndoEnabled && _undoStack.Count > 0);
		SetValue(CanRedoPropertyKey, IsUndoEnabled && _redoStack.Count > 0);
		CommandManager.InvalidateRequerySuggested();
	}

	protected virtual void DrawBackgroundAndBorder(DrawingContext drawingContext, Rect outerRect)
	{
		drawingContext.DrawRectangle(Background ?? Brushes.Transparent, null, outerRect);

		if (BorderBrush is null)
			return;

		var thickness = BorderThickness;

		if (thickness.Left > 0)
			drawingContext.DrawRectangle(BorderBrush, null, new Rect(0, 0, thickness.Left, outerRect.Height));

		if (thickness.Top > 0)
			drawingContext.DrawRectangle(BorderBrush, null, new Rect(0, 0, outerRect.Width, thickness.Top));

		if (thickness.Right > 0)
			drawingContext.DrawRectangle(BorderBrush, null, new Rect(outerRect.Width - thickness.Right, 0, thickness.Right, outerRect.Height));

		if (thickness.Bottom > 0)
			drawingContext.DrawRectangle(BorderBrush, null, new Rect(0, outerRect.Height - thickness.Bottom, outerRect.Width, thickness.Bottom));
	}

	protected virtual void DrawSelection(DrawingContext drawingContext, Rect textRect)
	{
		var selectionRect = GetSelectionRect(textRect);
		if (selectionRect is null)
			return;

		drawingContext.DrawRectangle(SelectionBrush, null, selectionRect.Value);
	}

	protected virtual void DrawText(DrawingContext drawingContext, Rect textRect)
	{
		var layout = CreateTextLayout(textRect, Foreground ?? Brushes.Black, usePlaceholder: true);
		if (layout is null)
			return;

		drawingContext.DrawText(layout.Value.FormattedText, layout.Value.TextPoint);
	}

	protected virtual void DrawSelectedText(DrawingContext drawingContext, Rect textRect)
	{
		if (!HasSelection)
			return;

		var selectionRect = GetSelectionRect(textRect);
		if (selectionRect is null)
			return;

		var brush = SelectionTextBrush ?? Foreground ?? Brushes.Black;
		var layout = CreateTextLayout(textRect, brush, usePlaceholder: false);
		if (layout is null)
			return;

		drawingContext.PushClip(new RectangleGeometry(selectionRect.Value));
		drawingContext.DrawText(layout.Value.FormattedText, layout.Value.TextPoint);
		drawingContext.Pop();
	}

	protected virtual void DrawCaret(DrawingContext drawingContext, Rect textRect)
	{
		if (!IsKeyboardFocused || HasSelection || !_isCaretVisible)
			return;

		var lineRect = GetLineRect(textRect);
		if (lineRect.Width <= 0 || lineRect.Height <= 0)
			return;

		var textWidth = GetTextWidth(Text);
		var caretX = GetTextOriginX(textRect, textWidth, allowHorizontalScrolling: true) + GetTextWidth(Text.Substring(0, CaretIndex));

		if (caretX < textRect.Left || caretX > textRect.Right)
			return;

		var caretHeight = Math.Max(1, Math.Min(lineRect.Height, GetLineHeight()));
		var caretRect = new Rect(
			caretX,
			lineRect.Top + Math.Max(0, (lineRect.Height - caretHeight) / 2),
			CaretWidth,
			caretHeight);

		drawingContext.DrawRectangle(CaretBrush ?? Foreground, null, caretRect);
	}

	protected virtual Rect GetTextRect()
	{
		var rect = new Rect(0, 0, ActualWidth, ActualHeight);
		rect = Deflate(rect, BorderThickness);
		rect = Deflate(rect, Padding);
		return rect;
	}

	protected virtual Rect GetLineRect(Rect textRect)
	{
		var lineHeight = Math.Min(textRect.Height, GetLineHeight());
		var top = VerticalContentAlignment switch
		{
			VerticalAlignment.Top => textRect.Top,
			VerticalAlignment.Bottom => textRect.Bottom - lineHeight,
			VerticalAlignment.Stretch => textRect.Top + Math.Max(0, (textRect.Height - lineHeight) / 2),
			_ => textRect.Top + Math.Max(0, (textRect.Height - lineHeight) / 2)
		};

		return new Rect(textRect.Left, top, textRect.Width, lineHeight);
	}

	private double GetTextOriginX(Rect textRect, double textWidth, bool allowHorizontalScrolling)
	{
		if (allowHorizontalScrolling && (textWidth > textRect.Width || _horizontalOffset > 0))
			return textRect.Left - _horizontalOffset;

		return HorizontalContentAlignment switch
		{
			HorizontalAlignment.Center => textRect.Left + Math.Max(0, (textRect.Width - textWidth) / 2),
			HorizontalAlignment.Right => textRect.Right - Math.Min(textRect.Width, textWidth),
			HorizontalAlignment.Stretch => textRect.Left,
			_ => textRect.Left
		};
	}

	private Rect? GetSelectionRect(Rect textRect)
	{
		if (!HasSelection)
			return null;

		var lineRect = GetLineRect(textRect);
		if (lineRect.Width <= 0 || lineRect.Height <= 0)
			return null;

		var textWidth = GetTextWidth(Text);
		var textOriginX = GetTextOriginX(textRect, textWidth, allowHorizontalScrolling: true);
		var selectionStartX = textOriginX + GetTextWidth(Text.Substring(0, SelectionStart));
		var selectionEndX = textOriginX + GetTextWidth(Text.Substring(0, SelectionStart + SelectionLength));

		var selectionRect = new Rect(
			selectionStartX,
			lineRect.Top,
			selectionEndX - selectionStartX,
			lineRect.Height);

		selectionRect.Intersect(textRect);
		return selectionRect.IsEmpty ? null : selectionRect;
	}

	private TextLayout? CreateTextLayout(Rect textRect, Brush brush, bool usePlaceholder)
	{
		var text = Text;
		var isPlaceholder = usePlaceholder && string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(Placeholder);

		if (isPlaceholder)
		{
			text = Placeholder;
			brush = PlaceholderBrush ?? brush;
		}

		if (!usePlaceholder && string.IsNullOrEmpty(text))
			return null;

		var formattedText = CreateFormattedText(string.IsNullOrEmpty(text) ? " " : text, brush);
		var lineRect = GetLineRect(textRect);
		var textWidth = formattedText.WidthIncludingTrailingWhitespace;
		var textPoint = new Point(
			GetTextOriginX(textRect, textWidth, allowHorizontalScrolling: !isPlaceholder),
			lineRect.Top + Math.Max(0, (lineRect.Height - formattedText.Height) / 2));

		return new TextLayout(formattedText, textPoint);
	}

	private double GetLineHeight()
		=> CreateFormattedText(" ", Foreground ?? Brushes.Black).Height;

	private static Rect Deflate(Rect rect, Thickness thickness)
	{
		var width = Math.Max(0, rect.Width - thickness.Left - thickness.Right);
		var height = Math.Max(0, rect.Height - thickness.Top - thickness.Bottom);

		return new Rect(
			rect.Left + thickness.Left,
			rect.Top + thickness.Top,
			width,
			height);
	}

	private FormattedText CreateFormattedText(string text, Brush brush)
	{
		var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
		var pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;

		return new FormattedText(
			CreateDisplayText(text),
			CultureInfo.CurrentUICulture,
			FlowDirection,
			typeface,
			FontSize,
			brush,
			pixelsPerDip);
	}

	private static string CreateDisplayText(string text)
		=> string.IsNullOrEmpty(text) ? string.Empty : text.Replace("\t", "    ");

	private double GetTextWidth(string text)
	{
		if (string.IsNullOrEmpty(text))
			return 0;

		return CreateFormattedText(text, Foreground ?? Brushes.Black).WidthIncludingTrailingWhitespace;
	}

	private void CoerceCaretAndSelection()
	{
		CoerceValue(CaretIndexProperty);
		CoerceValue(SelectionStartProperty);
		CoerceValue(SelectionLengthProperty);

		if (SelectionStart + SelectionLength > Text.Length)
			SetCurrentValue(SelectionLengthProperty, Math.Max(0, Text.Length - SelectionStart));
	}

	private void ResetCaretBlink()
	{
		_isCaretVisible = true;

		if (IsKeyboardFocused)
		{
			_caretTimer.Stop();
			_caretTimer.Start();
		}
	}

	private static CharacterKind GetCharacterKind(char character)
	{
		if (char.IsWhiteSpace(character))
			return CharacterKind.WhiteSpace;

		return char.IsLetterOrDigit(character) || character == '_'
			? CharacterKind.Word
			: CharacterKind.Symbol;
	}

	private static string NormalizeInputText(string text, bool replaceLineBreaksWithSpaces, bool acceptsTab, bool replaceTabsWithSpaces)
	{
		if (string.IsNullOrEmpty(text))
			return string.Empty;

		var result = replaceLineBreaksWithSpaces
			? text
				.Replace("\r\n", " ")
				.Replace('\r', ' ')
				.Replace('\n', ' ')
			: text
				.Replace("\r\n", string.Empty)
				.Replace("\r", string.Empty)
				.Replace("\n", string.Empty);

		if (!acceptsTab)
			result = replaceTabsWithSpaces
				? result.Replace('\t', ' ')
				: result.Replace("\t", string.Empty);

		return result;
	}

	private static bool TryContainsClipboardText()
	{
		try
		{
			return Clipboard.ContainsText(TextDataFormat.UnicodeText);
		}
		catch
		{
			return false;
		}
	}

	private static string? TryGetClipboardText()
	{
		try
		{
			return Clipboard.ContainsText(TextDataFormat.UnicodeText)
				? Clipboard.GetText(TextDataFormat.UnicodeText)
				: null;
		}
		catch
		{
			return null;
		}
	}

	private static void TrySetClipboardText(string text)
	{
		try
		{
			Clipboard.SetText(text ?? string.Empty, TextDataFormat.UnicodeText);
		}
		catch
		{
			// Clipboard can be temporarily unavailable when another process owns it.
		}
	}

	private void EnsureDefaultContextMenu()
	{
		if (!UseDefaultContextMenu || ContextMenu is not null)
			return;

		_defaultContextMenu = CreateDefaultContextMenu();
		SetCurrentValue(ContextMenuProperty, _defaultContextMenu);
	}

	private void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
		=> CommandManager.InvalidateRequerySuggested();

	protected virtual ContextMenu CreateDefaultContextMenu()
	{
		var menu = new ContextMenu
		{
			PlacementTarget = this
		};

		menu.Items.Add(CreateDefaultContextMenuItem("_Undo", ApplicationCommands.Undo));
		menu.Items.Add(CreateDefaultContextMenuItem("_Redo", ApplicationCommands.Redo));
		menu.Items.Add(new Separator());
		menu.Items.Add(CreateDefaultContextMenuItem("Cu_t", ApplicationCommands.Cut));
		menu.Items.Add(CreateDefaultContextMenuItem("_Copy", ApplicationCommands.Copy));
		menu.Items.Add(CreateDefaultContextMenuItem("_Paste", ApplicationCommands.Paste));
		menu.Items.Add(new Separator());
		menu.Items.Add(CreateDefaultContextMenuItem("Select _All", ApplicationCommands.SelectAll));

		menu.Opened += (_, _) => CommandManager.InvalidateRequerySuggested();
		return menu;
	}

	private MenuItem CreateDefaultContextMenuItem(string header, ICommand command)
		=> new()
		{
			Header = header,
			Command = command,
			CommandTarget = this
		};

	private void CanCopyCommand(object sender, CanExecuteRoutedEventArgs e)
		=> e.CanExecute = HasSelection;

	private void CanCutCommand(object sender, CanExecuteRoutedEventArgs e)
		=> e.CanExecute = HasSelection && !IsReadOnly;

	private void CanPasteCommand(object sender, CanExecuteRoutedEventArgs e)
		=> e.CanExecute = !IsReadOnly && TryContainsClipboardText();

	private void CanSelectAllCommand(object sender, CanExecuteRoutedEventArgs e)
		=> e.CanExecute = !string.IsNullOrEmpty(Text);

	private void CanUndoCommand(object sender, CanExecuteRoutedEventArgs e)
		=> e.CanExecute = !IsReadOnly && CanUndo;

	private void CanRedoCommand(object sender, CanExecuteRoutedEventArgs e)
		=> e.CanExecute = !IsReadOnly && CanRedo;

	private void OnCopyCommand(object sender, ExecutedRoutedEventArgs e)
		=> Copy();

	private void OnCutCommand(object sender, ExecutedRoutedEventArgs e)
		=> Cut();

	private void OnPasteCommand(object sender, ExecutedRoutedEventArgs e)
		=> Paste();

	private void OnSelectAllCommand(object sender, ExecutedRoutedEventArgs e)
		=> SelectAll();

	private void OnUndoCommand(object sender, ExecutedRoutedEventArgs e)
		=> Undo();

	private void OnRedoCommand(object sender, ExecutedRoutedEventArgs e)
		=> Redo();

	private enum TextChangeKind
	{
		None,
		Typing,
		Paste,
		Cut,
		Backspace,
		Delete,
		DeleteSelection
	}

	private enum CharacterKind
	{
		WhiteSpace,
		Word,
		Symbol
	}

	private readonly record struct TextRange(int Start, int End);

	private readonly record struct TextLayout(FormattedText FormattedText, Point TextPoint);

	private readonly record struct TextState(string Text, int CaretIndex, int SelectionStart, int SelectionLength);
}
