using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Automation.Text;

namespace StswExpress.Wpf;

/// <summary>Provides value and basic text automation for <see cref="StswInputBoxBase"/>.</summary>
public class StswInputBoxBaseAutomationPeer : FrameworkElementAutomationPeer, IValueProvider, ITextProvider
{
	public StswInputBoxBaseAutomationPeer(StswInputBoxBase owner) : base(owner)
	{
	}

	private StswInputBoxBase InputOwner => (StswInputBoxBase)Owner;

	protected override string GetClassNameCore() => nameof(StswInputBoxBase);
	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Edit;
	protected override bool IsPasswordCore() => InputOwner.IsAutomationValueProtected;
	protected override string GetNameCore()
	{
		var name = base.GetNameCore();
		return string.IsNullOrWhiteSpace(name) ? InputOwner.Placeholder : name;
	}

	public override object? GetPattern(PatternInterface patternInterface)
		=> patternInterface is PatternInterface.Value or PatternInterface.Text ? this : base.GetPattern(patternInterface);

	bool IValueProvider.IsReadOnly => InputOwner.IsReadOnly || !InputOwner.IsEnabled || InputOwner.IsAutomationValueProtected;
	string IValueProvider.Value => InputOwner.IsAutomationValueProtected ? string.Empty : InputOwner.GetAutomationValue();
	void IValueProvider.SetValue(string value) => InputOwner.SetAutomationValue(value);

	ITextRangeProvider ITextProvider.DocumentRange => new InputTextRange(this, 0, InputOwner.Text.Length);
	SupportedTextSelection ITextProvider.SupportedTextSelection => SupportedTextSelection.Single;

	ITextRangeProvider[] ITextProvider.GetSelection()
		=> [new InputTextRange(this, InputOwner.SelectionStart, InputOwner.SelectionStart + InputOwner.SelectionLength)];

	ITextRangeProvider[] ITextProvider.GetVisibleRanges()
	{
		var firstLine = InputOwner.GetFirstVisibleLineIndex();
		var lastLine = InputOwner.GetLastVisibleLineIndex();
		var start = Math.Max(0, InputOwner.GetCharacterIndexFromLineIndex(firstLine));
		var lastStart = Math.Max(start, InputOwner.GetCharacterIndexFromLineIndex(lastLine));
		var end = Math.Min(InputOwner.Text.Length, lastStart + InputOwner.GetLineLength(lastLine));
		return [new InputTextRange(this, start, end)];
	}

	ITextRangeProvider? ITextProvider.RangeFromChild(IRawElementProviderSimple childElement) => null;

	ITextRangeProvider ITextProvider.RangeFromPoint(Point screenLocation)
	{
		var point = InputOwner.PointFromScreen(screenLocation);
		var index = InputOwner.GetCharacterIndexFromPoint(point, snapToText: true);
		return new InputTextRange(this, index, index);
	}

	internal IRawElementProviderSimple Provider => ProviderFromPeer(this);

	internal void RaiseValueChanged(string oldValue, string newValue)
	{
		if (!InputOwner.IsAutomationValueProtected)
			RaisePropertyChangedEvent(ValuePatternIdentifiers.ValueProperty, oldValue, newValue);
		RaiseAutomationEvent(AutomationEvents.TextPatternOnTextChanged);
	}

	internal void RaiseSelectionChanged()
		=> RaiseAutomationEvent(AutomationEvents.TextPatternOnTextSelectionChanged);

	private sealed class InputTextRange : ITextRangeProvider
	{
		private readonly StswInputBoxBaseAutomationPeer _peer;
		private int _start;
		private int _end;

		public InputTextRange(StswInputBoxBaseAutomationPeer peer, int start, int end)
		{
			_peer = peer;
			_start = Math.Clamp(start, 0, Owner.Text.Length);
			_end = Math.Clamp(end, _start, Owner.Text.Length);
		}

		private StswInputBoxBase Owner => _peer.InputOwner;

		public void AddToSelection() => Select();
		public ITextRangeProvider Clone() => new InputTextRange(_peer, _start, _end);

		public bool Compare(ITextRangeProvider range)
			=> range is InputTextRange other && ReferenceEquals(other._peer, _peer) && other._start == _start && other._end == _end;

		public int CompareEndpoints(TextPatternRangeEndpoint endpoint, ITextRangeProvider targetRange, TextPatternRangeEndpoint targetEndpoint)
		{
			if (targetRange is not InputTextRange other || !ReferenceEquals(other._peer, _peer))
				throw new ArgumentException("The range belongs to another text provider.", nameof(targetRange));
			return GetEndpoint(endpoint).CompareTo(other.GetEndpoint(targetEndpoint));
		}

		public void ExpandToEnclosingUnit(TextUnit unit)
		{
			if (unit == TextUnit.Document)
			{
				_start = 0;
				_end = Owner.Text.Length;
				return;
			}

			if (unit is TextUnit.Line or TextUnit.Paragraph)
			{
				var line = Owner.GetLineIndexFromCharacterIndex(_start);
				_start = Owner.GetCharacterIndexFromLineIndex(line);
				_end = Math.Min(Owner.Text.Length, _start + Owner.GetLineLength(line) + 1);
				return;
			}

			if (unit == TextUnit.Word)
			{
				var text = Owner.Text;
				while (_start > 0 && !char.IsWhiteSpace(text[_start - 1])) _start--;
				_end = _start;
				while (_end < text.Length && !char.IsWhiteSpace(text[_end])) _end++;
				return;
			}

			_end = Math.Min(Owner.Text.Length, _start + 1);
		}

		public ITextRangeProvider? FindAttribute(int attributeId, object value, bool backward) => null;

		public ITextRangeProvider? FindText(string text, bool backward, bool ignoreCase)
		{
			if (string.IsNullOrEmpty(text))
				return null;
			var comparison = ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;
			var rangeText = Owner.Text.Substring(_start, _end - _start);
			var offset = backward ? rangeText.LastIndexOf(text, comparison) : rangeText.IndexOf(text, comparison);
			return offset < 0 ? null : new InputTextRange(_peer, _start + offset, _start + offset + text.Length);
		}

		public object GetAttributeValue(int attributeId) => AutomationElement.NotSupported;

		public double[] GetBoundingRectangles()
		{
			if (_start == _end)
				return [];
			var rectangles = new List<double>();
			var firstLine = Owner.GetLineIndexFromCharacterIndex(_start);
			var lastLine = Owner.GetLineIndexFromCharacterIndex(_end);
			for (var lineIndex = firstLine; lineIndex <= lastLine; lineIndex++)
			{
				var lineStart = Owner.GetCharacterIndexFromLineIndex(lineIndex);
				var start = Math.Max(_start, lineStart);
				var end = Math.Min(_end, lineStart + Owner.GetLineLength(lineIndex));
				var first = Owner.GetRectFromCharacterIndex(start, trailingEdge: false);
				var last = Owner.GetRectFromCharacterIndex(end, trailingEdge: false);
				var topLeft = Owner.PointToScreen(new Point(Math.Min(first.Left, last.Left), first.Top));
				rectangles.Add(topLeft.X);
				rectangles.Add(topLeft.Y);
				rectangles.Add(Math.Max(1, Math.Abs(last.Left - first.Left)));
				rectangles.Add(Math.Max(first.Height, last.Height));
			}
			return rectangles.ToArray();
		}

		public IRawElementProviderSimple[] GetChildren() => [];
		public IRawElementProviderSimple GetEnclosingElement() => _peer.Provider;

		public string GetText(int maxLength)
		{
			var length = _end - _start;
			if (maxLength >= 0)
				length = Math.Min(length, maxLength);
			return Owner.IsAutomationValueProtected ? string.Empty : Owner.Text.Substring(_start, length);
		}

		public int Move(TextUnit unit, int count)
		{
			var moved = MoveEndpointByUnit(TextPatternRangeEndpoint.Start, unit, count);
			_end = _start;
			return moved;
		}

		public void MoveEndpointByRange(TextPatternRangeEndpoint endpoint, ITextRangeProvider targetRange, TextPatternRangeEndpoint targetEndpoint)
		{
			if (targetRange is not InputTextRange other || !ReferenceEquals(other._peer, _peer))
				throw new ArgumentException("The range belongs to another text provider.", nameof(targetRange));
			SetEndpoint(endpoint, other.GetEndpoint(targetEndpoint));
		}

		public int MoveEndpointByUnit(TextPatternRangeEndpoint endpoint, TextUnit unit, int count)
		{
			if (count == 0)
				return 0;
			var original = GetEndpoint(endpoint);
			var target = original;
			if (unit == TextUnit.Document)
				target = count < 0 ? 0 : Owner.Text.Length;
			else if (unit is TextUnit.Line or TextUnit.Paragraph)
			{
				var line = Owner.GetLineIndexFromCharacterIndex(original);
				var targetLine = Math.Clamp(line + count, 0, Owner.LineCount - 1);
				target = Owner.GetCharacterIndexFromLineIndex(targetLine);
			}
			else
				target = Math.Clamp(original + count, 0, Owner.Text.Length);
			SetEndpoint(endpoint, target);
			return target == original ? 0 : Math.Sign(target - original) * Math.Min(Math.Abs(count), Math.Abs(target - original));
		}

		public void RemoveFromSelection()
		{
			if (Owner.SelectionStart == _start && Owner.SelectionStart + Owner.SelectionLength == _end)
				Owner.Select(_start, 0);
		}

		public void ScrollIntoView(bool alignToTop)
		{
			var line = Owner.GetLineIndexFromCharacterIndex(_start);
			Owner.ScrollToLine(line);
		}

		public void Select() => Owner.Select(_start, _end - _start);

		private int GetEndpoint(TextPatternRangeEndpoint endpoint) => endpoint == TextPatternRangeEndpoint.Start ? _start : _end;

		private void SetEndpoint(TextPatternRangeEndpoint endpoint, int value)
		{
			value = Math.Clamp(value, 0, Owner.Text.Length);
			if (endpoint == TextPatternRangeEndpoint.Start)
			{
				_start = value;
				if (_start > _end) _end = _start;
			}
			else
			{
				_end = value;
				if (_end < _start) _start = _end;
			}
		}
	}
}
