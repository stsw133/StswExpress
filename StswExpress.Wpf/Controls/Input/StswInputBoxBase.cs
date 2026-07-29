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
/// <remarks>
/// A control template may expose an empty <c>PART_ContentHost</c> based on <see cref="Decorator"/>
/// or <see cref="ContentControl"/>. The control inserts its internal rendering view into that host,
/// whose bounds then define the editable viewport.
/// </remarks>
[TemplatePart(Name = PartContentHost, Type = typeof(FrameworkElement))]
public abstract class StswInputBoxBase : Control
{
	protected const string PartContentHost = "PART_ContentHost";

	private const double CaretWidth = 1.0;
	private const int CaretBlinkMilliseconds = 530;
	private const int SelectionAutoScrollMilliseconds = 30;
	private const double SelectionAutoScrollStep = 8.0;

	private readonly DispatcherTimer _caretTimer;
	private readonly DispatcherTimer _selectionAutoScrollTimer;
	private readonly Stack<TextState> _undoStack = [];
	private readonly Stack<TextState> _redoStack = [];
	private ContextMenu? _defaultContextMenu;
	private FrameworkElement? _contentHost;
	private TextComposition? _activeTextComposition;
	private TextState _imeBaseState;
	private string _imeCompositionText = string.Empty;
	private SingleLineTextMetrics? _textMetrics;
	private InputBoxView? _contentView;
	private bool _isContentViewAttached;
	private bool _isImeCompositionPending;
	private int _imeCompositionStart;
	private int _imeCompositionReplacementLength;
	private TextChangeKind _activeUndoUnitKind;
	private bool _isCaretVisible;
	private bool _isInternalTextChange;
	private bool _isRestoringHistory;
	private bool _selectionChangedPending;
	private int _selectionAnchor;
	private int _selectionUpdateDepth;
	private Point _lastSelectionMousePosition;
	private double _horizontalOffset;

	static StswInputBoxBase()
	{
		FocusableProperty.OverrideMetadata(typeof(StswInputBoxBase), new FrameworkPropertyMetadata(true));
		CursorProperty.OverrideMetadata(typeof(StswInputBoxBase), new FrameworkPropertyMetadata(Cursors.IBeam));
		FocusVisualStyleProperty.OverrideMetadata(typeof(StswInputBoxBase), new FrameworkPropertyMetadata(null));
		HorizontalContentAlignmentProperty.OverrideMetadata(typeof(StswInputBoxBase), new FrameworkPropertyMetadata(HorizontalAlignment.Left, FrameworkPropertyMetadataOptions.AffectsRender));
		VerticalContentAlignmentProperty.OverrideMetadata(typeof(StswInputBoxBase), new FrameworkPropertyMetadata(VerticalAlignment.Center, FrameworkPropertyMetadataOptions.AffectsRender));
	}

	protected StswInputBoxBase()
	{
		SetCurrentValue(FocusVisualStyleProperty, null);
		SnapsToDevicePixels = true;

		_caretTimer = new DispatcherTimer
		{
			Interval = TimeSpan.FromMilliseconds(CaretBlinkMilliseconds)
		};
		_caretTimer.Tick += (_, _) =>
		{
			_isCaretVisible = !_isCaretVisible;
			InvalidateEditorVisual();
		};

		_selectionAutoScrollTimer = new DispatcherTimer
		{
			Interval = TimeSpan.FromMilliseconds(SelectionAutoScrollMilliseconds)
		};
		_selectionAutoScrollTimer.Tick += (_, _) => UpdateSelectionAutoScroll();

		CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, OnCopyCommand, CanCopyCommand));
		CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, OnCutCommand, CanCutCommand));
		CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, OnPasteCommand, CanPasteCommand));
		CommandBindings.Add(new CommandBinding(ApplicationCommands.SelectAll, OnSelectAllCommand, CanSelectAllCommand));
		CommandBindings.Add(new CommandBinding(ApplicationCommands.Undo, OnUndoCommand, CanUndoCommand));
		CommandBindings.Add(new CommandBinding(ApplicationCommands.Redo, OnRedoCommand, CanRedoCommand));

		ContextMenuOpening += OnContextMenuOpening;

		TextCompositionManager.AddPreviewTextInputStartHandler(this, OnPreviewTextInputStart);
		TextCompositionManager.AddPreviewTextInputUpdateHandler(this, OnPreviewTextInputUpdate);
	}

	#region Routed events

	public static readonly RoutedEvent TextChangedEvent =
		EventManager.RegisterRoutedEvent(
			nameof(TextChanged),
			RoutingStrategy.Bubble,
			typeof(RoutedEventHandler),
			typeof(StswInputBoxBase));

	public event RoutedEventHandler TextChanged
	{
		add => AddHandler(TextChangedEvent, value);
		remove => RemoveHandler(TextChangedEvent, value);
	}

	public static readonly RoutedEvent SelectionChangedEvent =
		EventManager.RegisterRoutedEvent(
			nameof(SelectionChanged),
			RoutingStrategy.Bubble,
			typeof(RoutedEventHandler),
			typeof(StswInputBoxBase));

	public event RoutedEventHandler SelectionChanged
	{
		add => AddHandler(SelectionChangedEvent, value);
		remove => RemoveHandler(SelectionChangedEvent, value);
	}

	#endregion

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
			OnTextPropertyChanged,
			CoerceText);

		metadata.DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
		return metadata;
	}

	private static object CoerceText(DependencyObject d, object baseValue)
	{
		var input = (StswInputBoxBase)d;
		var text = baseValue as string ?? string.Empty;

		return input._isInternalTextChange
			? text
			: input.CoerceTextCore(text) ?? string.Empty;
	}

	private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is not StswInputBoxBase input)
			return;

		if (!input._isInternalTextChange)
		{
			input.CancelImeComposition();
			input.ClearHistory();
		}

		input.InvalidateTextLayout();
		input.CoerceCaretAndSelection();
		input.EnsureCaretVisible();
		input.InvalidateMeasure();
		input.InvalidateEditorVisual();
		input.RaiseEvent(new RoutedEventArgs(TextChangedEvent, input));
		input.OnTextChanged((string?)e.OldValue ?? string.Empty, (string?)e.NewValue ?? string.Empty);
	}

	/// <summary>
	/// Allows a derived input control to normalize any value assigned to <see cref="Text"/>.
	/// </summary>
	protected virtual string CoerceTextCore(string text)
		=> text;

	/// <summary>
	/// Called after <see cref="Text"/> changes, including changes received from a binding.
	/// </summary>
	protected virtual void OnTextChanged(string oldText, string newText)
	{
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
		var value = Math.Clamp((int)baseValue, 0, input.Text.Length);
		return SnapToTextElementBoundary(input.Text, value, preferNext: false);
	}

	private static void OnCaretIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is not StswInputBoxBase input)
			return;

		input.EnsureCaretVisible();
		input.ResetCaretBlink();
		input.InvalidateEditorVisual();
		input.OnCaretIndexChanged((int)e.OldValue, (int)e.NewValue);
	}

	/// <summary>
	/// Called after the caret position changes.
	/// </summary>
	protected virtual void OnCaretIndexChanged(int oldIndex, int newIndex)
	{
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
		var value = Math.Clamp((int)baseValue, 0, input.Text.Length);
		return SnapToTextElementBoundary(input.Text, value, preferNext: false);
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
		var value = Math.Max(0, (int)baseValue);
		var requestedEnd = Math.Min(input.Text.Length, input.SelectionStart + value);
		var selectionEnd = SnapToTextElementBoundary(input.Text, requestedEnd, preferNext: true);
		return Math.Max(0, selectionEnd - input.SelectionStart);
	}

	private static void OnSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is not StswInputBoxBase input)
			return;

		if (input._selectionUpdateDepth > 0)
		{
			input._selectionChangedPending = true;
			return;
		}

		input.NotifySelectionChanged();
	}

	private void NotifySelectionChanged()
	{
		ResetCaretBlink();
		InvalidateEditorVisual();
		RaiseEvent(new RoutedEventArgs(SelectionChangedEvent, this));
		OnSelectionChanged();
	}

	private void BeginSelectionUpdate()
		=> _selectionUpdateDepth++;

	private void EndSelectionUpdate()
	{
		if (_selectionUpdateDepth <= 0)
			return;

		_selectionUpdateDepth--;
		if (_selectionUpdateDepth == 0 && _selectionChangedPending)
		{
			_selectionChangedPending = false;
			NotifySelectionChanged();
		}
	}

	/// <summary>
	/// Called after the selection range changes.
	/// </summary>
	protected virtual void OnSelectionChanged()
	{
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

	#region CharacterCasing

	public static readonly DependencyProperty CharacterCasingProperty =
		DependencyProperty.Register(
			nameof(CharacterCasing),
			typeof(CharacterCasing),
			typeof(StswInputBoxBase),
			new FrameworkPropertyMetadata(System.Windows.Controls.CharacterCasing.Normal));

	public CharacterCasing CharacterCasing
	{
		get => (CharacterCasing)GetValue(CharacterCasingProperty);
		set => SetValue(CharacterCasingProperty, value);
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

	#region Inactive selection

	public static readonly DependencyProperty IsInactiveSelectionHighlightEnabledProperty =
		DependencyProperty.Register(
			nameof(IsInactiveSelectionHighlightEnabled),
			typeof(bool),
			typeof(StswInputBoxBase),
			new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

	public bool IsInactiveSelectionHighlightEnabled
	{
		get => (bool)GetValue(IsInactiveSelectionHighlightEnabledProperty);
		set => SetValue(IsInactiveSelectionHighlightEnabledProperty, value);
	}

	public static readonly DependencyProperty InactiveSelectionBrushProperty =
		DependencyProperty.Register(
			nameof(InactiveSelectionBrush),
			typeof(Brush),
			typeof(StswInputBoxBase),
			new FrameworkPropertyMetadata(SystemColors.InactiveSelectionHighlightBrush, FrameworkPropertyMetadataOptions.AffectsRender));

	public Brush? InactiveSelectionBrush
	{
		get => (Brush?)GetValue(InactiveSelectionBrushProperty);
		set => SetValue(InactiveSelectionBrushProperty, value);
	}

	public static readonly DependencyProperty InactiveSelectionTextBrushProperty =
		DependencyProperty.Register(
			nameof(InactiveSelectionTextBrush),
			typeof(Brush),
			typeof(StswInputBoxBase),
			new FrameworkPropertyMetadata(SystemColors.InactiveSelectionHighlightTextBrush, FrameworkPropertyMetadataOptions.AffectsRender));

	public Brush? InactiveSelectionTextBrush
	{
		get => (Brush?)GetValue(InactiveSelectionTextBrushProperty);
		set => SetValue(InactiveSelectionTextBrushProperty, value);
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


	#region IME composition

	private static readonly DependencyPropertyKey IsImeCompositionActivePropertyKey =
		DependencyProperty.RegisterReadOnly(
			nameof(IsImeCompositionActive),
			typeof(bool),
			typeof(StswInputBoxBase),
			new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty IsImeCompositionActiveProperty =
		IsImeCompositionActivePropertyKey.DependencyProperty;

	/// <summary>
	/// Gets whether an input method editor currently has an active text composition.
	/// </summary>
	public bool IsImeCompositionActive
		=> (bool)GetValue(IsImeCompositionActiveProperty);

	private static readonly DependencyPropertyKey ImeCompositionTextPropertyKey =
		DependencyProperty.RegisterReadOnly(
			nameof(ImeCompositionText),
			typeof(string),
			typeof(StswInputBoxBase),
			new FrameworkPropertyMetadata(string.Empty));

	public static readonly DependencyProperty ImeCompositionTextProperty =
		ImeCompositionTextPropertyKey.DependencyProperty;

	/// <summary>
	/// Gets the temporary text currently supplied by an input method editor.
	/// The value is not committed to <see cref="Text"/> until the composition completes.
	/// </summary>
	public string ImeCompositionText
		=> (string)GetValue(ImeCompositionTextProperty);

	public static readonly DependencyProperty ImeCompositionUnderlineBrushProperty =
		DependencyProperty.Register(
			nameof(ImeCompositionUnderlineBrush),
			typeof(Brush),
			typeof(StswInputBoxBase),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

	/// <summary>
	/// Gets or sets the brush used to underline temporary IME composition text.
	/// When unset, <see cref="CaretBrush"/> or <see cref="Control.Foreground"/> is used.
	/// </summary>
	public Brush? ImeCompositionUnderlineBrush
	{
		get => (Brush?)GetValue(ImeCompositionUnderlineBrushProperty);
		set => SetValue(ImeCompositionUnderlineBrushProperty, value);
	}

	public static readonly DependencyProperty ImeCompositionUnderlineThicknessProperty =
		DependencyProperty.Register(
			nameof(ImeCompositionUnderlineThickness),
			typeof(double),
			typeof(StswInputBoxBase),
			new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender, null, CoerceImeCompositionUnderlineThickness));

	/// <summary>
	/// Gets or sets the thickness of the line drawn below temporary IME composition text.
	/// </summary>
	public double ImeCompositionUnderlineThickness
	{
		get => (double)GetValue(ImeCompositionUnderlineThicknessProperty);
		set => SetValue(ImeCompositionUnderlineThicknessProperty, value);
	}

	private static object CoerceImeCompositionUnderlineThickness(DependencyObject d, object baseValue)
		=> Math.Max(0, (double)baseValue);

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
		set
		{
			var insertedText = NormalizeInsertedText(value ?? string.Empty, isPaste: true);
			ReplaceSelection(insertedText, TextChangeKind.Programmatic);
		}
	}

	protected bool HasSelection => SelectionLength > 0;

	/// <summary>
	/// Gets the template element that defines the editable viewport, when supplied by the active control template.
	/// </summary>
	protected FrameworkElement? ContentHost => _contentHost;

	/// <summary>
	/// Invalidates cached single-line text measurements. Derived controls should call this method
	/// whenever their display-text transformation changes without changing <see cref="Text"/>.
	/// </summary>
	protected void InvalidateTextLayout()
	{
		_textMetrics = null;
		InvalidateMeasure();
		InvalidateEditorVisual();
	}

	protected override void OnInitialized(EventArgs e)
	{
		base.OnInitialized(e);
		EnsureDefaultContextMenu();
	}

	public override void OnApplyTemplate()
	{
		DetachContentView();

		base.OnApplyTemplate();

		_contentHost = GetTemplateChild(PartContentHost) as FrameworkElement;
		if (_contentHost is not null)
		{
			_contentView ??= new InputBoxView(this);
			_isContentViewAttached = AttachContentView(_contentHost, _contentView);
		}

		EnsureDefaultContextMenu();
		EnsureCaretVisible();
		InvalidateMeasure();
		InvalidateEditorVisual();
	}

	private static bool AttachContentView(FrameworkElement host, InputBoxView view)
	{
		switch (host)
		{
			case Decorator decorator:
				decorator.Child = view;
				return true;

			case ContentControl contentControl:
				contentControl.SetCurrentValue(HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
				contentControl.SetCurrentValue(VerticalContentAlignmentProperty, VerticalAlignment.Stretch);
				contentControl.Content = view;
				return true;

			default:
				return false;
		}
	}

	private void DetachContentView()
	{
		if (_contentHost is null || _contentView is null)
		{
			_isContentViewAttached = false;
			return;
		}

		switch (_contentHost)
		{
			case Decorator decorator when ReferenceEquals(decorator.Child, _contentView):
				decorator.Child = null;
				break;

			case ContentControl contentControl when ReferenceEquals(contentControl.Content, _contentView):
				contentControl.Content = null;
				break;
		}

		_isContentViewAttached = false;
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
		var templateSize = base.MeasureOverride(constraint);
		var text = string.IsNullOrEmpty(Text) ? Placeholder : Text;
		var formattedText = CreateFormattedText(string.IsNullOrEmpty(text) ? " " : text, Foreground, isPlaceholder: string.IsNullOrEmpty(Text));

		var width = formattedText.WidthIncludingTrailingWhitespace + BorderThickness.Left + BorderThickness.Right + Padding.Left + Padding.Right + 8;
		var height = formattedText.Height + BorderThickness.Top + BorderThickness.Bottom + Padding.Top + Padding.Bottom + 6;

		if (double.IsInfinity(constraint.Width))
			width = Math.Max(120, width);
		else
			width = Math.Min(constraint.Width, Math.Max(40, width));

		if (!double.IsInfinity(constraint.Height))
			height = Math.Min(constraint.Height, height);

		return new Size(Math.Max(templateSize.Width, width), Math.Max(templateSize.Height, height));
	}

	protected override void OnRender(DrawingContext drawingContext)
	{
		base.OnRender(drawingContext);

		if (_isContentViewAttached && _contentView is not null)
		{
			_contentView.InvalidateVisual();
			return;
		}

		var outerRect = new Rect(0, 0, ActualWidth, ActualHeight);
		DrawBackgroundAndBorder(drawingContext, outerRect);
		RenderEditableContent(drawingContext);
	}

	private void RenderEditableContent(DrawingContext drawingContext)
	{
		var textRect = GetTextRect();
		if (textRect.Width <= 0 || textRect.Height <= 0)
			return;

		drawingContext.PushClip(new RectangleGeometry(textRect));

		DrawSelection(drawingContext, textRect);
		DrawText(drawingContext, textRect);
		DrawSelectedText(drawingContext, textRect);
		DrawImeComposition(drawingContext, textRect);
		DrawCaret(drawingContext, textRect);

		drawingContext.Pop();
	}

	protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
	{
		base.OnRenderSizeChanged(sizeInfo);
		EnsureCaretVisible();
		InvalidateEditorVisual();
	}

	protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
	{
		base.OnGotKeyboardFocus(e);
		ResetCaretBlink();
		_caretTimer.Start();
		InvalidateEditorVisual();
	}

	protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
	{
		base.OnLostKeyboardFocus(e);
		TryCompleteImeComposition();
		CancelImeComposition();
		CloseUndoUnit();
		StopSelectionAutoScroll();
		_caretTimer.Stop();
		_isCaretVisible = false;
		InvalidateEditorVisual();
	}

	private void OnPreviewTextInputStart(object sender, TextCompositionEventArgs e)
	{
		if (IsReadOnly)
			return;

		_activeTextComposition = e.TextComposition;
		_isImeCompositionPending = true;
	}

	private void OnPreviewTextInputUpdate(object sender, TextCompositionEventArgs e)
	{
		if (IsReadOnly)
		{
			CancelImeComposition();
			return;
		}

		if (!IsImeCompositionActive)
			BeginImeComposition(e.TextComposition);

		UpdateImeCompositionText(GetUpdatingCompositionText(e));
	}

	protected override void OnTextInput(TextCompositionEventArgs e)
	{
		base.OnTextInput(e);

		if (IsReadOnly)
		{
			CancelImeComposition();
			return;
		}

		if (IsImeCompositionActive)
		{
			var committedText = GetCommittedCompositionText(e);
			CommitImeComposition(committedText);
			e.Handled = true;
			return;
		}

		_isImeCompositionPending = false;
		_activeTextComposition = null;

		var text = NormalizeInsertedText(e.Text, isPaste: false);
		if (string.IsNullOrEmpty(text))
			return;

		InsertText(text, TextChangeKind.Typing, allowUndoMerge: true);
		e.Handled = true;
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);

		if (IsImeCompositionActive)
		{
			if (e.Key == Key.Escape)
			{
				CancelImeComposition();
				e.Handled = true;
			}

			return;
		}

		var modifiers = Keyboard.Modifiers;
		var ctrl = modifiers.HasFlag(ModifierKeys.Control);
		var alt = modifiers.HasFlag(ModifierKeys.Alt);
		var shift = modifiers.HasFlag(ModifierKeys.Shift);
		var commandModifier = ctrl && !alt;

		switch (e.Key)
		{
			case Key.Left:
				MoveCaret(GetPreviousCaretIndex(commandModifier), shift);
				e.Handled = true;
				break;

			case Key.Right:
				MoveCaret(GetNextCaretIndex(commandModifier), shift);
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
				OnCommit();
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
					Backspace(commandModifier);
				e.Handled = true;
				break;

			case Key.Insert when commandModifier:
				Copy();
				e.Handled = true;
				break;

			case Key.Insert when shift && !ctrl && !alt:
				if (!IsReadOnly)
					Paste();
				e.Handled = true;
				break;

			case Key.Delete when shift && !ctrl && !alt:
				if (!IsReadOnly)
					Cut();
				e.Handled = true;
				break;

			case Key.Delete:
				if (!IsReadOnly)
					Delete(commandModifier);
				e.Handled = true;
				break;

			case Key.A when commandModifier:
				SelectAll();
				e.Handled = true;
				break;

			case Key.C when commandModifier:
				Copy();
				e.Handled = true;
				break;

			case Key.X when commandModifier:
				if (!IsReadOnly)
					Cut();
				e.Handled = true;
				break;

			case Key.V when commandModifier:
				if (!IsReadOnly)
					Paste();
				e.Handled = true;
				break;

			case Key.Z when commandModifier && shift:
				if (!IsReadOnly)
					Redo();
				e.Handled = true;
				break;

			case Key.Z when commandModifier:
				if (!IsReadOnly)
					Undo();
				e.Handled = true;
				break;

			case Key.Y when commandModifier:
				if (!IsReadOnly)
					Redo();
				e.Handled = true;
				break;
		}
	}

	/// <summary>
	/// Called when the user presses Enter in the single-line editor.
	/// </summary>
	protected virtual void OnCommit()
	{
	}

	/// <summary>
	/// Normalizes text received from keyboard input or the clipboard before insertion.
	/// </summary>
	protected virtual string NormalizeInsertedText(string text, bool isPaste)
	{
		var normalizedText = NormalizeInputText(
			text,
			replaceLineBreaksWithSpaces: isPaste,
			acceptsTab: AcceptsTab,
			replaceTabsWithSpaces: isPaste && !AcceptsTab);

		return CharacterCasing switch
		{
			System.Windows.Controls.CharacterCasing.Lower => normalizedText.ToLower(CultureInfo.CurrentCulture),
			System.Windows.Controls.CharacterCasing.Upper => normalizedText.ToUpper(CultureInfo.CurrentCulture),
			_ => normalizedText
		};
	}

	/// <summary>
	/// Determines whether a proposed complete text value may be applied.
	/// </summary>
	protected virtual bool CanApplyTextChange(string oldText, string newText)
		=> true;

	/// <summary>
	/// Called when <see cref="CanApplyTextChange"/> rejects a proposed text value.
	/// </summary>
	protected virtual void OnTextChangeRejected(string proposedText)
	{
	}

	protected override void OnMouseDown(MouseButtonEventArgs e)
	{
		base.OnMouseDown(e);

		if (e.ChangedButton != MouseButton.Left)
			return;

		TryCompleteImeComposition();
		CancelImeComposition();
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
		_lastSelectionMousePosition = e.GetPosition(this);

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

		_lastSelectionMousePosition = e.GetPosition(this);
		UpdateSelectionFromMousePosition(_lastSelectionMousePosition);
		UpdateSelectionAutoScrollState();

		e.Handled = true;
	}

	protected override void OnMouseUp(MouseButtonEventArgs e)
	{
		base.OnMouseUp(e);

		if (e.ChangedButton != MouseButton.Left)
			return;

		StopSelectionAutoScroll();

		if (IsMouseCaptured)
			ReleaseMouseCapture();

		e.Handled = true;
	}

	protected override void OnLostMouseCapture(MouseEventArgs e)
	{
		base.OnLostMouseCapture(e);
		StopSelectionAutoScroll();
	}

	protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
	{
		base.OnMouseRightButtonDown(e);
		TryCompleteImeComposition();
		CancelImeComposition();
		CloseUndoUnit();
		Focus();
		EnsureDefaultContextMenu();

		var point = e.GetPosition(this);
		if (!IsPointInsideSelection(point))
			SetCaretAndClearSelection(GetCaretIndexFromPoint(point));
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

	public void Select(int start, int length)
	{
		CloseUndoUnit();

		var selectionStart = SnapToTextElementBoundary(Text, Math.Clamp(start, 0, Text.Length), preferNext: false);
		var requestedEnd = Math.Min(Text.Length, selectionStart + Math.Max(0, length));
		var selectionEnd = SnapToTextElementBoundary(Text, requestedEnd, preferNext: true);

		BeginSelectionUpdate();
		try
		{
			SetCurrentValue(CaretIndexProperty, selectionEnd);
			SetCurrentValue(SelectionStartProperty, selectionStart);
			SetCurrentValue(SelectionLengthProperty, selectionEnd - selectionStart);
		}
		finally
		{
			EndSelectionUpdate();
		}

		_selectionAnchor = selectionStart;
		EnsureCaretVisible();
		ResetCaretBlink();
		InvalidateEditorVisual();
	}

	public void SelectAll()
		=> Select(0, Text.Length);

	public void Clear()
	{
		if (Text.Length == 0)
			return;

		ApplyTextChange(string.Empty, 0, 0, 0, TextChangeKind.Programmatic, allowUndoMerge: false);
	}

	public void AppendText(string text)
	{
		var appendedText = NormalizeInsertedText(text ?? string.Empty, isPaste: true);
		if (appendedText.Length == 0)
			return;

		var available = MaxLength > 0 ? MaxLength - Text.Length : int.MaxValue;
		if (available <= 0)
			return;

		if (appendedText.Length > available)
			appendedText = TruncateToTextElementBoundary(appendedText, available);

		var newText = Text + appendedText;
		ApplyTextChange(newText, newText.Length, newText.Length, 0, TextChangeKind.Programmatic, allowUndoMerge: false);
		ScrollToEnd();
	}

	public void ScrollToHome()
	{
		_horizontalOffset = 0;
		InvalidateEditorVisual();
	}

	public void ScrollToEnd()
	{
		var textRect = GetTextRect();
		_horizontalOffset = Math.Max(0, GetTextMetrics().TotalWidth - Math.Max(0, textRect.Width));
		InvalidateEditorVisual();
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

		InsertText(NormalizeInsertedText(text, isPaste: true), TextChangeKind.Paste, allowUndoMerge: false);
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

	private void ReplaceSelection(string insertedText, TextChangeKind changeKind)
	{
		var text = Text;
		var start = HasSelection ? SelectionStart : CaretIndex;
		var length = HasSelection ? SelectionLength : 0;

		if (length > 0)
			text = text.Remove(start, length);

		if (MaxLength > 0)
		{
			var available = MaxLength - text.Length;
			if (available <= 0)
				insertedText = string.Empty;
			else if (insertedText.Length > available)
				insertedText = TruncateToTextElementBoundary(insertedText, available);
		}

		text = text.Insert(start, insertedText);
		var caretIndex = start + insertedText.Length;
		ApplyTextChange(text, caretIndex, caretIndex, 0, changeKind, allowUndoMerge: false);
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
				insertedText = TruncateToTextElementBoundary(insertedText, available);
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

		var removeStart = wordMode ? GetPreviousWordIndex(CaretIndex) : GetPreviousTextElementIndex(CaretIndex);
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

		var removeEnd = wordMode ? GetNextWordIndex(CaretIndex) : GetNextTextElementIndex(CaretIndex);
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
		index = SnapToTextElementBoundary(Text, Math.Clamp(index, 0, Text.Length), preferNext: false);

		BeginSelectionUpdate();
		try
		{
			SetCurrentValue(CaretIndexProperty, index);
			SetCurrentValue(SelectionStartProperty, index);
			SetCurrentValue(SelectionLengthProperty, 0);
		}
		finally
		{
			EndSelectionUpdate();
		}

		_selectionAnchor = index;

		EnsureCaretVisible();
		ResetCaretBlink();
		InvalidateEditorVisual();
	}

	private void SetSelectionFromAnchor(int caretIndex)
	{
		caretIndex = SnapToTextElementBoundary(Text, Math.Clamp(caretIndex, 0, Text.Length), preferNext: caretIndex >= _selectionAnchor);

		var start = Math.Min(_selectionAnchor, caretIndex);
		var length = Math.Abs(caretIndex - _selectionAnchor);

		BeginSelectionUpdate();
		try
		{
			SetCurrentValue(CaretIndexProperty, caretIndex);
			SetCurrentValue(SelectionStartProperty, start);
			SetCurrentValue(SelectionLengthProperty, length);
		}
		finally
		{
			EndSelectionUpdate();
		}

		EnsureCaretVisible();
		ResetCaretBlink();
		InvalidateEditorVisual();
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

		BeginSelectionUpdate();
		try
		{
			SetCurrentValue(CaretIndexProperty, range.End);
			SetCurrentValue(SelectionStartProperty, range.Start);
			SetCurrentValue(SelectionLengthProperty, range.End - range.Start);
		}
		finally
		{
			EndSelectionUpdate();
		}

		EnsureCaretVisible();
		ResetCaretBlink();
		InvalidateEditorVisual();
	}

	private int GetPreviousCaretIndex(bool wordMode)
	{
		if (CaretIndex <= 0)
			return 0;

		return wordMode
			? GetPreviousWordIndex(CaretIndex)
			: GetPreviousTextElementIndex(CaretIndex);
	}

	private int GetNextCaretIndex(bool wordMode)
	{
		if (CaretIndex >= Text.Length)
			return Text.Length;

		return wordMode
			? GetNextWordIndex(CaretIndex)
			: GetNextTextElementIndex(CaretIndex);
	}

	private int GetPreviousWordIndex(int index)
	{
		var text = Text;
		index = SnapToTextElementBoundary(text, Math.Clamp(index, 0, text.Length), preferNext: false);

		while (index > 0)
		{
			var previous = GetPreviousTextElementIndex(index);
			if (GetTextElementKind(text, previous) != CharacterKind.WhiteSpace)
				break;

			index = previous;
		}

		if (index <= 0)
			return 0;

		var elementIndex = GetPreviousTextElementIndex(index);
		var kind = GetTextElementKind(text, elementIndex);

		while (index > 0)
		{
			var previous = GetPreviousTextElementIndex(index);
			if (GetTextElementKind(text, previous) != kind)
				break;

			index = previous;
		}

		return index;
	}

	private int GetNextWordIndex(int index)
	{
		var text = Text;
		index = SnapToTextElementBoundary(text, Math.Clamp(index, 0, text.Length), preferNext: true);

		if (index >= text.Length)
			return text.Length;

		var kind = GetTextElementKind(text, index);
		while (index < text.Length && GetTextElementKind(text, index) == kind)
			index = GetNextTextElementIndex(index);

		while (index < text.Length && GetTextElementKind(text, index) == CharacterKind.WhiteSpace)
			index = GetNextTextElementIndex(index);

		return index;
	}

	private TextRange GetWordRange(int caretIndex)
	{
		var text = Text;
		if (text.Length == 0)
			return new TextRange(0, 0);

		var elementIndex = caretIndex >= text.Length
			? GetPreviousTextElementIndex(text.Length)
			: SnapToTextElementBoundary(text, Math.Max(0, caretIndex), preferNext: false);

		var kind = GetTextElementKind(text, elementIndex);
		var start = elementIndex;
		var end = GetNextTextElementIndex(text, elementIndex);

		while (start > 0)
		{
			var previous = GetPreviousTextElementIndex(text, start);
			if (GetTextElementKind(text, previous) != kind)
				break;

			start = previous;
		}

		while (end < text.Length && GetTextElementKind(text, end) == kind)
			end = GetNextTextElementIndex(text, end);

		return new TextRange(start, end);
	}

	private int GetCaretIndexFromPoint(Point point)
	{
		var textRect = GetTextRect();
		var metrics = GetTextMetrics();
		var textOriginX = GetTextOriginX(textRect, metrics.TotalWidth, allowHorizontalScrolling: true);
		var x = point.X - textOriginX;

		if (x <= 0 || metrics.Boundaries.Length <= 1)
			return 0;

		if (x >= metrics.TotalWidth)
			return Text.Length;

		var low = 1;
		var high = metrics.Boundaries.Length - 1;
		while (low <= high)
		{
			var middleIndex = low + ((high - low) / 2);
			var previousWidth = metrics.Positions[middleIndex - 1];
			var currentWidth = metrics.Positions[middleIndex];
			var threshold = previousWidth + ((currentWidth - previousWidth) / 2);

			if (x < threshold)
				high = middleIndex - 1;
			else
				low = middleIndex + 1;
		}

		var boundaryIndex = Math.Clamp(low - 1, 0, metrics.Boundaries.Length - 1);
		return metrics.Boundaries[boundaryIndex];
	}

	private void EnsureCaretVisible()
	{
		if (ActualWidth <= 0)
			return;

		var textRect = GetTextRect();
		if (textRect.Width <= 0)
			return;

		double totalWidth;
		double caretX;

		if (IsImeCompositionActive)
		{
			var renderedText = GetRenderedText();
			totalWidth = MeasureSourceTextWidth(renderedText);
			caretX = MeasureSourceTextWidth(renderedText.Substring(0, GetImeCompositionEndIndex()));
		}
		else
		{
			var metrics = GetTextMetrics();
			totalWidth = metrics.TotalWidth;
			caretX = GetTextPosition(CaretIndex);
		}

		if (totalWidth <= textRect.Width)
		{
			_horizontalOffset = 0;
			return;
		}

		var rightEdge = _horizontalOffset + textRect.Width - 2;

		if (caretX < _horizontalOffset)
			_horizontalOffset = caretX;
		else if (caretX > rightEdge)
			_horizontalOffset = caretX - textRect.Width + 2;

		_horizontalOffset = Math.Clamp(_horizontalOffset, 0, Math.Max(0, totalWidth - textRect.Width));
	}

	private void UpdateSelectionFromMousePosition(Point point)
	{
		var textRect = GetTextRect();
		var clampedPoint = new Point(
			Math.Clamp(point.X, textRect.Left, textRect.Right),
			Math.Clamp(point.Y, textRect.Top, textRect.Bottom));

		SetSelectionFromAnchor(GetCaretIndexFromPoint(clampedPoint));
	}

	private void UpdateSelectionAutoScrollState()
	{
		var textRect = GetTextRect();
		var shouldScroll = IsMouseCaptured
			&& Mouse.LeftButton == MouseButtonState.Pressed
			&& GetTextMetrics().TotalWidth > textRect.Width
			&& (_lastSelectionMousePosition.X < textRect.Left || _lastSelectionMousePosition.X > textRect.Right);

		if (shouldScroll)
		{
			if (!_selectionAutoScrollTimer.IsEnabled)
				_selectionAutoScrollTimer.Start();
		}
		else
		{
			StopSelectionAutoScroll();
		}
	}

	private void UpdateSelectionAutoScroll()
	{
		if (!IsMouseCaptured || Mouse.LeftButton != MouseButtonState.Pressed)
		{
			StopSelectionAutoScroll();
			return;
		}

		var textRect = GetTextRect();
		var maxOffset = Math.Max(0, GetTextMetrics().TotalWidth - Math.Max(0, textRect.Width));
		if (maxOffset <= 0)
		{
			StopSelectionAutoScroll();
			return;
		}

		if (_lastSelectionMousePosition.X < textRect.Left)
			_horizontalOffset = Math.Max(0, _horizontalOffset - SelectionAutoScrollStep);
		else if (_lastSelectionMousePosition.X > textRect.Right)
			_horizontalOffset = Math.Min(maxOffset, _horizontalOffset + SelectionAutoScrollStep);
		else
		{
			StopSelectionAutoScroll();
			return;
		}

		UpdateSelectionFromMousePosition(_lastSelectionMousePosition);
		InvalidateEditorVisual();
	}

	private void StopSelectionAutoScroll()
	{
		if (_selectionAutoScrollTimer.IsEnabled)
			_selectionAutoScrollTimer.Stop();
	}

	private void BeginImeComposition(TextComposition composition)
	{
		CloseUndoUnit();

		_activeTextComposition = composition;
		_isImeCompositionPending = true;
		_imeBaseState = CaptureState();
		_imeCompositionStart = HasSelection ? SelectionStart : CaretIndex;
		_imeCompositionReplacementLength = HasSelection ? SelectionLength : 0;

		SetValue(IsImeCompositionActivePropertyKey, true);
		UpdateImeCompositionText(string.Empty);
	}

	private void UpdateImeCompositionText(string text)
	{
		if (!IsImeCompositionActive)
			return;

		var normalizedText = NormalizeInsertedText(text ?? string.Empty, isPaste: false);
		if (MaxLength > 0)
		{
			var available = MaxLength - (_imeBaseState.Text.Length - _imeCompositionReplacementLength);
			normalizedText = available <= 0
				? string.Empty
				: TruncateToTextElementBoundary(normalizedText, available);
		}

		_imeCompositionText = normalizedText;
		SetValue(ImeCompositionTextPropertyKey, normalizedText);

		ResetCaretBlink();
		EnsureCaretVisible();
		InvalidateEditorVisual();
	}

	private void CommitImeComposition(string text)
	{
		if (!IsImeCompositionActive)
			return;

		var baseState = _imeBaseState;
		var start = _imeCompositionStart;
		var replacementLength = _imeCompositionReplacementLength;
		var committedText = NormalizeInsertedText(text ?? string.Empty, isPaste: false);

		if (MaxLength > 0)
		{
			var available = MaxLength - (baseState.Text.Length - replacementLength);
			committedText = available <= 0
				? string.Empty
				: TruncateToTextElementBoundary(committedText, available);
		}

		var newText = baseState.Text.Remove(start, replacementLength).Insert(start, committedText);
		var caretIndex = start + committedText.Length;

		ClearImeCompositionState();
		ApplyTextChange(newText, caretIndex, caretIndex, 0, TextChangeKind.Composition, allowUndoMerge: false);
	}

	private void CancelImeComposition()
	{
		if (!IsImeCompositionActive && !_isImeCompositionPending)
			return;

		ClearImeCompositionState();
		EnsureCaretVisible();
		InvalidateEditorVisual();
	}

	private void ClearImeCompositionState()
	{
		_activeTextComposition = null;
		_isImeCompositionPending = false;
		_imeCompositionText = string.Empty;
		_imeCompositionStart = 0;
		_imeCompositionReplacementLength = 0;

		SetValue(IsImeCompositionActivePropertyKey, false);
		SetValue(ImeCompositionTextPropertyKey, string.Empty);
	}

	private void TryCompleteImeComposition()
	{
		if (!IsImeCompositionActive)
			return;

		try
		{
			_activeTextComposition?.Complete();
		}
		catch (InvalidOperationException)
		{
			// The text services framework may already be completing or disposing the composition.
		}
	}

	private string GetRenderedText()
	{
		if (!IsImeCompositionActive)
			return Text;

		return _imeBaseState.Text
			.Remove(_imeCompositionStart, _imeCompositionReplacementLength)
			.Insert(_imeCompositionStart, _imeCompositionText);
	}

	private int GetImeCompositionEndIndex()
		=> IsImeCompositionActive
			? _imeCompositionStart + _imeCompositionText.Length
			: CaretIndex;

	private ImeCompositionLayout? GetImeCompositionLayout(Rect textRect)
	{
		if (!IsImeCompositionActive)
			return null;

		var renderedText = GetRenderedText();
		var totalWidth = MeasureSourceTextWidth(renderedText);
		var prefixWidth = MeasureSourceTextWidth(renderedText.Substring(0, _imeCompositionStart));
		var compositionEndWidth = MeasureSourceTextWidth(renderedText.Substring(0, GetImeCompositionEndIndex()));
		var textOriginX = GetTextOriginX(textRect, totalWidth, allowHorizontalScrolling: true);

		return new ImeCompositionLayout(
			textOriginX + prefixWidth,
			textOriginX + compositionEndWidth);
	}

	private double MeasureSourceTextWidth(string sourceText)
	{
		if (string.IsNullOrEmpty(sourceText))
			return 0;

		var displayText = GetDisplayText(sourceText) ?? string.Empty;
		return MeasureDisplayTextWidth(displayText, VisualTreeHelper.GetDpi(this).PixelsPerDip);
	}

	private static string GetUpdatingCompositionText(TextCompositionEventArgs e)
	{
		var composition = e.TextComposition;
		if (!string.IsNullOrEmpty(composition.CompositionText))
			return composition.CompositionText;

		if (!string.IsNullOrEmpty(composition.SystemCompositionText))
			return composition.SystemCompositionText;

		return string.Empty;
	}

	private static string GetCommittedCompositionText(TextCompositionEventArgs e)
	{
		var composition = e.TextComposition;
		return FirstNotEmpty(
			e.Text,
			composition.Text,
			composition.SystemText,
			composition.CompositionText,
			composition.SystemCompositionText);
	}

	private static string FirstNotEmpty(params string?[] values)
	{
		foreach (var value in values)
		{
			if (!string.IsNullOrEmpty(value))
				return value;
		}

		return string.Empty;
	}

	private void ApplyTextChange(string text, int caretIndex, int selectionStart, int selectionLength, TextChangeKind changeKind, bool allowUndoMerge)
	{
		text = CoerceTextCore(text) ?? string.Empty;
		if (!CanApplyTextChange(Text, text))
		{
			CloseUndoUnit();
			OnTextChangeRejected(text);
			return;
		}

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
		var caretIndex = SnapToTextElementBoundary(text, Math.Clamp(state.CaretIndex, 0, text.Length), preferNext: false);
		var selectionStart = SnapToTextElementBoundary(text, Math.Clamp(state.SelectionStart, 0, text.Length), preferNext: false);
		var requestedSelectionEnd = Math.Min(text.Length, selectionStart + Math.Max(0, state.SelectionLength));
		var selectionEnd = SnapToTextElementBoundary(text, requestedSelectionEnd, preferNext: true);
		var selectionLength = Math.Max(0, selectionEnd - selectionStart);

		BeginSelectionUpdate();
		try
		{
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
		}
		finally
		{
			EndSelectionUpdate();
		}

		_selectionAnchor = selectionLength > 0 && caretIndex == selectionStart
			? selectionStart + selectionLength
			: selectionStart;

		EnsureCaretVisible();
		ResetCaretBlink();
		InvalidateMeasure();
		InvalidateEditorVisual();
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

	private void InvalidateEditorVisual()
	{
		if (_isContentViewAttached)
			_contentView?.InvalidateVisual();

		InvalidateVisual();
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
		if (IsImeCompositionActive || !ShouldRenderSelection())
			return;

		var selectionRect = GetSelectionRect(textRect);
		if (selectionRect is null)
			return;

		var brush = IsKeyboardFocusWithin ? SelectionBrush : InactiveSelectionBrush;
		drawingContext.DrawRectangle(brush, null, selectionRect.Value);
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
		if (IsImeCompositionActive || !ShouldRenderSelection())
			return;

		var selectionRect = GetSelectionRect(textRect);
		if (selectionRect is null)
			return;

		var brush = IsKeyboardFocusWithin
			? SelectionTextBrush ?? Foreground ?? Brushes.Black
			: InactiveSelectionTextBrush ?? Foreground ?? Brushes.Black;
		var layout = CreateTextLayout(textRect, brush, usePlaceholder: false);
		if (layout is null)
			return;

		drawingContext.PushClip(new RectangleGeometry(selectionRect.Value));
		drawingContext.DrawText(layout.Value.FormattedText, layout.Value.TextPoint);
		drawingContext.Pop();
	}
	private bool ShouldRenderSelection()
		=> HasSelection && (IsKeyboardFocusWithin || IsInactiveSelectionHighlightEnabled);


	protected virtual void DrawImeComposition(DrawingContext drawingContext, Rect textRect)
	{
		if (!IsImeCompositionActive || ImeCompositionUnderlineThickness <= 0)
			return;

		var layout = GetImeCompositionLayout(textRect);
		if (layout is null)
			return;

		var lineRect = GetLineRect(textRect);
		var y = Math.Min(textRect.Bottom - (ImeCompositionUnderlineThickness / 2), lineRect.Bottom - (ImeCompositionUnderlineThickness / 2));
		var startX = Math.Clamp(layout.Value.CompositionStartX, textRect.Left, textRect.Right);
		var endX = Math.Clamp(layout.Value.CompositionEndX, textRect.Left, textRect.Right);

		if (endX <= startX)
			return;

		var brush = ImeCompositionUnderlineBrush ?? CaretBrush ?? Foreground ?? Brushes.Black;
		var pen = new Pen(brush, ImeCompositionUnderlineThickness);
		pen.Freeze();

		drawingContext.DrawLine(pen, new Point(startX, y), new Point(endX, y));
	}

	protected virtual void DrawCaret(DrawingContext drawingContext, Rect textRect)
	{
		if (!IsKeyboardFocused || (!IsImeCompositionActive && HasSelection) || !_isCaretVisible)
			return;

		var lineRect = GetLineRect(textRect);
		if (lineRect.Width <= 0 || lineRect.Height <= 0)
			return;

		double caretX;
		if (IsImeCompositionActive)
		{
			var compositionLayout = GetImeCompositionLayout(textRect);
			if (compositionLayout is null)
				return;

			caretX = compositionLayout.Value.CompositionEndX;
		}
		else
		{
			var metrics = GetTextMetrics();
			caretX = GetTextOriginX(textRect, metrics.TotalWidth, allowHorizontalScrolling: true) + GetTextPosition(CaretIndex);
		}

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
		if (_isContentViewAttached && _contentView is not null && _contentView.IsVisible && _contentView.ActualWidth > 0 && _contentView.ActualHeight > 0)
		{
			try
			{
				var origin = _contentView.TranslatePoint(new Point(0, 0), this);
				return new Rect(origin, _contentView.RenderSize);
			}
			catch (InvalidOperationException)
			{
				// The template view is not connected to this visual tree yet.
			}
		}

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

		var metrics = GetTextMetrics();
		var textOriginX = GetTextOriginX(textRect, metrics.TotalWidth, allowHorizontalScrolling: true);
		var selectionStartX = textOriginX + GetTextPosition(SelectionStart);
		var selectionEndX = textOriginX + GetTextPosition(SelectionStart + SelectionLength);

		var selectionRect = new Rect(
			selectionStartX,
			lineRect.Top,
			selectionEndX - selectionStartX,
			lineRect.Height);

		selectionRect.Intersect(textRect);
		return selectionRect.IsEmpty ? null : selectionRect;
	}

	private bool IsPointInsideSelection(Point point)
	{
		if (!HasSelection)
			return false;

		var selectionRect = GetSelectionRect(GetTextRect());
		return selectionRect?.Contains(point) == true;
	}

	private TextLayout? CreateTextLayout(Rect textRect, Brush brush, bool usePlaceholder)
	{
		var text = GetRenderedText();
		var isPlaceholder = usePlaceholder && !IsImeCompositionActive && string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(Placeholder);

		if (isPlaceholder)
		{
			text = Placeholder;
			brush = PlaceholderBrush ?? brush;
		}

		if (!usePlaceholder && string.IsNullOrEmpty(text))
			return null;

		var formattedText = CreateFormattedText(string.IsNullOrEmpty(text) ? " " : text, brush, isPlaceholder);
		var lineRect = GetLineRect(textRect);
		var textWidth = formattedText.WidthIncludingTrailingWhitespace;
		var textPoint = new Point(
			GetTextOriginX(textRect, textWidth, allowHorizontalScrolling: !isPlaceholder),
			lineRect.Top + Math.Max(0, (lineRect.Height - formattedText.Height) / 2));

		return new TextLayout(formattedText, textPoint);
	}

	private double GetLineHeight()
		=> CreateFormattedText(" ", Foreground ?? Brushes.Black, isPlaceholder: false).Height;

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

	private FormattedText CreateFormattedText(string text, Brush brush, bool isPlaceholder = false)
	{
		var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
		var pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
		var displayText = isPlaceholder
			? GetDisplayPlaceholderText(text)
			: GetDisplayText(text);

		return new FormattedText(
			displayText,
			CultureInfo.CurrentUICulture,
			FlowDirection,
			typeface,
			FontSize,
			brush,
			pixelsPerDip);
	}

	/// <summary>
	/// Converts source text into the text rendered by the control.
	/// The returned string should preserve a predictable positional mapping to the source text.
	/// </summary>
	protected virtual string GetDisplayText(string text)
		=> string.IsNullOrEmpty(text) ? string.Empty : text.Replace("\t", "    ");

	/// <summary>
	/// Converts placeholder text into the text rendered by the control.
	/// </summary>
	protected virtual string GetDisplayPlaceholderText(string text)
		=> text ?? string.Empty;

	private double GetTextWidth(string text)
	{
		if (string.IsNullOrEmpty(text))
			return 0;

		if (text.Length <= Text.Length && Text.StartsWith(text, StringComparison.Ordinal))
			return GetTextPosition(text.Length);

		return CreateFormattedText(text, Foreground ?? Brushes.Black, isPlaceholder: false).WidthIncludingTrailingWhitespace;
	}

	private SingleLineTextMetrics GetTextMetrics()
	{
		var sourceText = Text ?? string.Empty;
		var displayText = GetDisplayText(sourceText) ?? string.Empty;
		var pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
		var cultureName = CultureInfo.CurrentUICulture.Name;

		if (_textMetrics is not null
			&& _textMetrics.Matches(
				sourceText,
				displayText,
				FontFamily,
				FontStyle,
				FontWeight,
				FontStretch,
				FontSize,
				FlowDirection,
				pixelsPerDip,
				cultureName))
		{
			return _textMetrics;
		}

		var boundaries = GetTextElementBoundaryMap(sourceText);
		var positions = new double[boundaries.Length];
		for (var i = 1; i < boundaries.Length; i++)
		{
			var prefix = sourceText.Substring(0, boundaries[i]);
			var displayPrefix = GetDisplayText(prefix) ?? string.Empty;
			positions[i] = MeasureDisplayTextWidth(displayPrefix, pixelsPerDip);
		}

		_textMetrics = new SingleLineTextMetrics(
			sourceText,
			displayText,
			FontFamily,
			FontStyle,
			FontWeight,
			FontStretch,
			FontSize,
			FlowDirection,
			pixelsPerDip,
			cultureName,
			boundaries,
			positions);

		return _textMetrics;
	}

	private double GetTextPosition(int sourceIndex)
	{
		var metrics = GetTextMetrics();
		var index = SnapToBoundary(metrics.Boundaries, Math.Clamp(sourceIndex, 0, Text.Length), preferNext: false);
		var position = Array.BinarySearch(metrics.Boundaries, index);
		return position >= 0 ? metrics.Positions[position] : 0;
	}

	private double MeasureDisplayTextWidth(string displayText, double pixelsPerDip)
	{
		if (string.IsNullOrEmpty(displayText))
			return 0;

		var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
		var formattedText = new FormattedText(
			displayText,
			CultureInfo.CurrentUICulture,
			FlowDirection,
			typeface,
			FontSize,
			Foreground ?? Brushes.Black,
			pixelsPerDip);

		return formattedText.WidthIncludingTrailingWhitespace;
	}

	private void CoerceCaretAndSelection()
	{
		BeginSelectionUpdate();
		try
		{
			CoerceValue(CaretIndexProperty);
			CoerceValue(SelectionStartProperty);
			CoerceValue(SelectionLengthProperty);

			if (SelectionStart + SelectionLength > Text.Length)
				SetCurrentValue(SelectionLengthProperty, Math.Max(0, Text.Length - SelectionStart));
		}
		finally
		{
			EndSelectionUpdate();
		}
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

	private static CharacterKind GetTextElementKind(string text, int index)
	{
		if (index < 0 || index >= text.Length)
			return CharacterKind.Symbol;

		var element = StringInfo.GetNextTextElement(text, index);
		if (string.IsNullOrWhiteSpace(element))
			return CharacterKind.WhiteSpace;

		var firstCharacter = element[0];
		return char.IsLetterOrDigit(firstCharacter) || firstCharacter == '_'
			? CharacterKind.Word
			: CharacterKind.Symbol;
	}

	private int GetPreviousTextElementIndex(int index)
	{
		var boundaries = GetTextMetrics().Boundaries;
		index = Math.Clamp(index, 0, Text.Length);
		var position = Array.BinarySearch(boundaries, index);

		if (position >= 0)
			return position == 0 ? 0 : boundaries[position - 1];

		var insertionIndex = ~position;
		return insertionIndex > 0 ? boundaries[insertionIndex - 1] : 0;
	}

	private int GetNextTextElementIndex(int index)
	{
		var boundaries = GetTextMetrics().Boundaries;
		index = Math.Clamp(index, 0, Text.Length);
		var position = Array.BinarySearch(boundaries, index);

		if (position >= 0)
			return position + 1 < boundaries.Length ? boundaries[position + 1] : Text.Length;

		var insertionIndex = ~position;
		return insertionIndex < boundaries.Length ? boundaries[insertionIndex] : Text.Length;
	}

	private static int[] GetTextElementBoundaryMap(string text)
	{
		if (string.IsNullOrEmpty(text))
			return [0];

		var starts = StringInfo.ParseCombiningCharacters(text);
		var boundaries = new int[starts.Length + 1];
		Array.Copy(starts, boundaries, starts.Length);
		boundaries[^1] = text.Length;
		return boundaries;
	}

	private static int SnapToBoundary(int[] boundaries, int index, bool preferNext)
	{
		var position = Array.BinarySearch(boundaries, index);
		if (position >= 0)
			return boundaries[position];

		var insertionIndex = ~position;
		if (preferNext && insertionIndex < boundaries.Length)
			return boundaries[insertionIndex];

		return insertionIndex > 0 ? boundaries[insertionIndex - 1] : 0;
	}

	private static int[] GetTextElementBoundaries(string text)
		=> string.IsNullOrEmpty(text) ? [] : StringInfo.ParseCombiningCharacters(text);

	private static int GetPreviousTextElementIndex(string text, int index)
	{
		index = Math.Clamp(index, 0, text.Length);
		if (index <= 0 || text.Length == 0)
			return 0;

		var boundaries = GetTextElementBoundaries(text);
		var position = Array.BinarySearch(boundaries, index);

		if (position >= 0)
			return position == 0 ? 0 : boundaries[position - 1];

		var insertionIndex = ~position;
		return insertionIndex > 0 ? boundaries[insertionIndex - 1] : 0;
	}

	private static int GetNextTextElementIndex(string text, int index)
	{
		index = Math.Clamp(index, 0, text.Length);
		if (index >= text.Length || text.Length == 0)
			return text.Length;

		var boundaries = GetTextElementBoundaries(text);
		var position = Array.BinarySearch(boundaries, index);

		if (position >= 0)
			return position + 1 < boundaries.Length ? boundaries[position + 1] : text.Length;

		var insertionIndex = ~position;
		return insertionIndex < boundaries.Length ? boundaries[insertionIndex] : text.Length;
	}

	private static int SnapToTextElementBoundary(string text, int index, bool preferNext)
	{
		index = Math.Clamp(index, 0, text.Length);
		if (index == 0 || index == text.Length || text.Length == 0)
			return index;

		var boundaries = GetTextElementBoundaries(text);
		var position = Array.BinarySearch(boundaries, index);
		if (position >= 0)
			return index;

		var insertionIndex = ~position;
		if (preferNext && insertionIndex < boundaries.Length)
			return boundaries[insertionIndex];

		return insertionIndex > 0 ? boundaries[insertionIndex - 1] : 0;
	}

	private static string TruncateToTextElementBoundary(string text, int maximumLength)
	{
		if (string.IsNullOrEmpty(text) || maximumLength <= 0)
			return string.Empty;

		if (text.Length <= maximumLength)
			return text;

		var length = SnapToTextElementBoundary(text, maximumLength, preferNext: false);
		return length <= 0 ? string.Empty : text.Substring(0, length);
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

	private sealed class SingleLineTextMetrics
	{
		public SingleLineTextMetrics(
			string sourceText,
			string displayText,
			FontFamily fontFamily,
			FontStyle fontStyle,
			FontWeight fontWeight,
			FontStretch fontStretch,
			double fontSize,
			FlowDirection flowDirection,
			double pixelsPerDip,
			string cultureName,
			int[] boundaries,
			double[] positions)
		{
			SourceText = sourceText;
			DisplayText = displayText;
			FontFamily = fontFamily;
			FontStyle = fontStyle;
			FontWeight = fontWeight;
			FontStretch = fontStretch;
			FontSize = fontSize;
			FlowDirection = flowDirection;
			PixelsPerDip = pixelsPerDip;
			CultureName = cultureName;
			Boundaries = boundaries;
			Positions = positions;
		}

		public string SourceText { get; }
		public string DisplayText { get; }
		public FontFamily FontFamily { get; }
		public FontStyle FontStyle { get; }
		public FontWeight FontWeight { get; }
		public FontStretch FontStretch { get; }
		public double FontSize { get; }
		public FlowDirection FlowDirection { get; }
		public double PixelsPerDip { get; }
		public string CultureName { get; }
		public int[] Boundaries { get; }
		public double[] Positions { get; }
		public double TotalWidth => Positions.Length == 0 ? 0 : Positions[^1];

		public bool Matches(
			string sourceText,
			string displayText,
			FontFamily fontFamily,
			FontStyle fontStyle,
			FontWeight fontWeight,
			FontStretch fontStretch,
			double fontSize,
			FlowDirection flowDirection,
			double pixelsPerDip,
			string cultureName)
			=> SourceText == sourceText
				&& DisplayText == displayText
				&& Equals(FontFamily, fontFamily)
				&& FontStyle == fontStyle
				&& FontWeight == fontWeight
				&& FontStretch == fontStretch
				&& FontSize.Equals(fontSize)
				&& FlowDirection == flowDirection
				&& PixelsPerDip.Equals(pixelsPerDip)
				&& CultureName == cultureName;
	}

	private sealed class InputBoxView : FrameworkElement
	{
		private readonly StswInputBoxBase _owner;

		public InputBoxView(StswInputBoxBase owner)
		{
			_owner = owner;
			ClipToBounds = true;
			Focusable = false;
			IsHitTestVisible = false;
			SnapsToDevicePixels = true;
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);

			Point origin;
			try
			{
				origin = TranslatePoint(new Point(0, 0), _owner);
			}
			catch (InvalidOperationException)
			{
				origin = new Point();
			}

			drawingContext.PushTransform(new TranslateTransform(-origin.X, -origin.Y));
			_owner.RenderEditableContent(drawingContext);
			drawingContext.Pop();
		}
	}

	private enum TextChangeKind
	{
		None,
		Typing,
		Paste,
		Cut,
		Backspace,
		Delete,
		DeleteSelection,
		Programmatic,
		Composition
	}

	private enum CharacterKind
	{
		WhiteSpace,
		Word,
		Symbol
	}

	private readonly record struct TextRange(int Start, int End);

	private readonly record struct TextLayout(FormattedText FormattedText, Point TextPoint);

	private readonly record struct ImeCompositionLayout(double CompositionStartX, double CompositionEndX);

	private readonly record struct TextState(string Text, int CaretIndex, int SelectionStart, int SelectionLength);
}