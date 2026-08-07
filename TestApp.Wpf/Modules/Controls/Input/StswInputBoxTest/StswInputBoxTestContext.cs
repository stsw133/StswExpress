using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TestApp.Wpf;

public partial class StswInputBoxTestContext : ControlsContext
{
	public override void SetDefaults()
	{
		base.SetDefaults();

		IsReadOnly = (bool?)ThisControlSetters.FirstOrDefault(x => x.Property.Name.Equals(nameof(IsReadOnly)))?.Value ?? false;
	}

	[StswCommand] void SetShortSingleLine() => SetSample("Short editable text");

	[StswCommand]
	void SetLongSingleLine() => SetSample(
		"A deliberately long single line used to verify horizontal extent, the horizontal scrollbar thumb, Shift plus mouse wheel and caret auto-scrolling. " +
		string.Join(' ', Enumerable.Repeat("0123456789", 18)));

	[StswCommand]
	void SetMultilineWithEmptyLines() => SetSample(
		"First logical line\n\nThird line after an empty line\n\n\nSixth logical line\nSeventh line\nEighth line\nNinth line\nTenth line");

	[StswCommand]
	void SetMultilineWithLongLines() => SetSample(
		string.Join('\n', Enumerable.Range(1, 48).Select(index =>
			$"Line {index:00}: {string.Join(' ', Enumerable.Repeat("a very long segment for horizontal scrolling", 5))}")));

	[StswCommand]
	void SetUnicode() => SetSample(
		"Unicode and grapheme clusters\nEmoji: 👩‍💻 🚀 🧑🏽‍🔬\nComposed: é; Polish AltGr: ąćęłńóśźż\n日本語 한국어 Ελληνικά");

	[StswCommand]
	void SetRtl()
	{
		FlowDirection = FlowDirection.RightToLeft;
		Text = "مرحبا بالعالم\nهذا سطر عربي طويل لاختبار التمرير الأفقي والعمودي داخل عنصر الإدخال\nשלום עולם\nRTL with 123 and mixed Latin text";
	}

	private void SetSample(string text)
	{
		FlowDirection = FlowDirection.LeftToRight;
		Text = text;
	}

	[StswObservableProperty] bool _icon = true;
	[StswObservableProperty] bool _subControls = true;
	[StswObservableProperty] string _text = "Short editable text";
	[StswObservableProperty] bool _isReadOnly;
	[StswObservableProperty] bool _acceptsReturn = false;
	[StswObservableProperty] bool _acceptsTab = false;
	[StswObservableProperty] TextWrapping _textWrapping = TextWrapping.NoWrap;
	[StswObservableProperty] ScrollBarVisibility _horizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
	[StswObservableProperty] ScrollBarVisibility _verticalScrollBarVisibility = ScrollBarVisibility.Hidden;
	[StswObservableProperty] int _minLines = 1;
	[StswObservableProperty] int _maxLines = int.MaxValue;
	[StswObservableProperty] CharacterCasing _characterCasing = CharacterCasing.Normal;
	[StswObservableProperty] bool _isTextDragDropEnabled = true;
	[StswObservableProperty] bool _isInactiveSelectionHighlightEnabled = false;
	[StswObservableProperty] FlowDirection _flowDirection = FlowDirection.LeftToRight;
}