using System.Globalization;
using System.Windows;
using System.Windows.Automation.Provider;
using System.Windows.Automation.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using StswExpress.Wpf;

namespace StswExpress.Wpf.Tests.Controls.Input;

public class StswInputBoxBaseTests
{
	[StaFact]
	public void Text_NormalizesEveryLineEnding_AndUpdatesLogicalLineApi()
	{
		var input = new TestInput { Text = "first\r\n\rthird\nfourth" };

		Assert.Equal("first\n\nthird\nfourth", input.Text);
		Assert.Equal(4, input.LineCount);
		Assert.Equal(0, input.GetCharacterIndexFromLineIndex(0));
		Assert.Equal(6, input.GetCharacterIndexFromLineIndex(1));
		Assert.Equal(string.Empty, input.GetLineText(1));
		Assert.Equal("third", input.GetLineText(2));
		Assert.Equal(2, input.GetLineIndexFromCharacterIndex(8));
	}

	[StaFact]
	public void NormalizeInsertedText_RespectsMultilineAndTabModes()
	{
		var input = new TestInput();

		Assert.Equal("a b c  ", input.Normalize("a\r\nb\rc\n\t", isPaste: true));
		Assert.Equal("abc", input.Normalize("a\r\nb\nc", isPaste: false));

		input.AcceptsReturn = true;
		input.AcceptsTab = true;
		Assert.Equal("a\nb\nc\n\t", input.Normalize("a\r\nb\rc\n\t", isPaste: true));
	}

	[StaFact]
	public void SelectedText_ReplacesMultilineSelection_AsSingleUndoUnit()
	{
		var input = new TestInput { AcceptsReturn = true, Text = "one\ntwo\nthree" };
		input.Select(4, 3);

		input.SelectedText = "second\nline";

		Assert.Equal("one\nsecond\nline\nthree", input.Text);
		Assert.True(input.CanUndo);
		input.Undo();
		Assert.Equal("one\ntwo\nthree", input.Text);
		input.Redo();
		Assert.Equal("one\nsecond\nline\nthree", input.Text);
	}

	[StaFact]
	public void Selection_SnapsToUnicodeTextElementBoundaries()
	{
		var input = new TestInput { Text = "A👩‍💻e\u0301Z" };

		input.Select(2, 1);

		Assert.Equal("👩‍💻", input.SelectedText);
		Assert.Equal(input.SelectedText.Length, input.SelectionLength);
	}

	[StaFact]
	public void MinLinesAndMaxLines_CoerceToAConsistentRange()
	{
		var input = new TestInput { MinLines = 0, MaxLines = 2 };
		Assert.Equal(1, input.MinLines);

		input.MinLines = 4;
		Assert.Equal(2, input.MinLines);
		input.MaxLines = 1;
		Assert.Equal(2, input.MaxLines);
		input.MaxLines = 0;
		input.MinLines = 4;
		Assert.Equal(4, input.MinLines);
	}

	[StaFact]
	public void MeasureAndScrollInfo_ExposeVerticalExtentAndClampOffsets()
	{
		var input = new TestInput
		{
			AcceptsReturn = true,
			Text = string.Join('\n', Enumerable.Range(1, 20).Select(x => $"line {x}"))
		};
		input.Measure(new Size(160, 50));
		input.Arrange(new Rect(0, 0, 160, 50));
		_ = input.GetRectFromCharacterIndex(input.Text.Length, trailingEdge: false);

		Assert.True(input.ExtentHeight > input.ViewportHeight);
		input.ScrollToVerticalOffset(double.MaxValue / 2);
		Assert.Equal(input.ScrollableHeight, input.VerticalOffset);
		input.ScrollToHome();
		Assert.Equal(0, input.VerticalOffset);
	}

	[StaFact]
	public void Wrapping_DisablesHorizontalOffset()
	{
		var input = new TestInput { TextWrapping = TextWrapping.Wrap, Text = new string('x', 100) };
		input.Measure(new Size(80, 60));
		input.Arrange(new Rect(0, 0, 80, 60));
		_ = input.GetCharacterIndexFromPoint(new Point(40, 40), snapToText: true);

		input.ScrollToHorizontalOffset(100);

		Assert.Equal(0, input.HorizontalOffset);
		Assert.True(input.ExtentHeight > 0);
	}

	[StaFact]
	public void AutomationPeer_ProvidesValueAndSelection()
	{
		var input = new TestInput { Text = "alpha\nbeta" };
		input.Select(2, 4);
		var peer = new StswInputBoxBaseAutomationPeer(input);
		var valueProvider = Assert.IsAssignableFrom<IValueProvider>(peer);
		var textProvider = Assert.IsAssignableFrom<ITextProvider>(peer);

		Assert.Equal(input.Text, valueProvider.Value);
		Assert.False(valueProvider.IsReadOnly);
		Assert.Equal("pha\n", textProvider.GetSelection().Single().GetText(-1));

		valueProvider.SetValue("new\r\nvalue");
		Assert.Equal("new\nvalue", input.Text);
	}

	[StaFact]
	public void RoutedEvents_AreRaisedForTextAndSelectionChanges()
	{
		var input = new TestInput();
		var textChanges = 0;
		var selectionChanges = 0;
		input.TextChanged += (_, _) => textChanges++;
		input.SelectionChanged += (_, _) => selectionChanges++;

		input.Text = "abc\ndef";
		input.Select(1, 3);

		Assert.Equal(1, textChanges);
		Assert.Equal(1, selectionChanges);
	}

	[StaFact]
	public void ProductionTemplate_UsesInputBoxViewAsTheOnlyScrollInfoClient()
	{
		var text = string.Join('\n', Enumerable.Range(1, 24).Select(index =>
			$"Line {index}: {new string('x', 100)}"));
		var (window, input, host) = ShowProductionControl(text);

		try
		{
			var scrollInfo = Assert.IsAssignableFrom<IScrollInfo>(host.Content);
			var horizontalBar = Assert.IsType<StswScrollBar>(host.Template.FindName("PART_HorizontalScrollBar", host));
			var verticalBar = Assert.IsType<StswScrollBar>(host.Template.FindName("PART_VerticalScrollBar", host));
			var presenter = Assert.IsType<ScrollContentPresenter>(host.Template.FindName("PART_ScrollContentPresenter", host));
			Assert.True(host.CanContentScroll);
			Assert.Equal(StswDynamicVisibilityMode.Off, host.DynamicMode);
			Assert.Same(host, scrollInfo.ScrollOwner);
			Assert.False(input is IScrollInfo);
			Assert.Equal(input.Padding, host.Padding);
			Assert.Equal(host.Padding, presenter.Margin);
			Assert.True(scrollInfo.CanHorizontallyScroll);
			Assert.True(scrollInfo.CanVerticallyScroll);
			Assert.Equal(Visibility.Visible, horizontalBar.Visibility);
			Assert.Equal(Visibility.Visible, verticalBar.Visibility);
			Assert.Null(input.FocusVisualStyle);
			Assert.False(StswFocusVisual.GetAssign(input));
			Assert.Equal("diagnostic icon", Assert.IsType<ContentPresenter>(input.Template.FindName("OPT_Icon", input)).Content);
			Assert.IsType<ItemsControl>(input.Template.FindName("OPT_SubControls", input));
			Assert.IsType<StswSubError>(input.Template.FindName("OPT_Error", input));
			Assert.IsType<StswBorder>(input.Template.FindName("OPT_MainBorder", input));
			Assert.Null(input.Template.FindName("OPT_Placeholder", input));
			Assert.True(input.ViewportWidth > 0);
			Assert.True(input.ViewportHeight > 0);
			Assert.True(input.ExtentWidth > input.ViewportWidth);
			Assert.True(input.ExtentHeight > input.ViewportHeight);
			Assert.Equal(input.ExtentWidth - input.ViewportWidth, input.ScrollableWidth, 6);
			Assert.Equal(input.ExtentHeight - input.ViewportHeight, input.ScrollableHeight, 6);
			Assert.Equal(Visibility.Visible, host.ComputedHorizontalScrollBarVisibility);
			Assert.Equal(Visibility.Visible, host.ComputedVerticalScrollBarVisibility);
		}
		finally
		{
			window.Close();
		}
	}

	[StaFact]
	public void ProductionTemplate_NaturalSizeAllowsStandardWpfAlignment()
	{
		var (window, container, input) = ShowNaturallySizedProductionControl("Short editable text");

		try
		{
			input.HorizontalAlignment = HorizontalAlignment.Left;
			input.VerticalAlignment = VerticalAlignment.Top;
			FlushLayout(window);
			var naturalSize = input.RenderSize;
			var expectedHeight = MeasureLineHeight(input)
				+ input.Padding.Top + input.Padding.Bottom
				+ input.BorderThickness.Top + input.BorderThickness.Bottom;

			Assert.InRange(naturalSize.Height, expectedHeight - 1, expectedHeight + 1);
			Assert.InRange(naturalSize.Width, 1, container.ActualWidth - 1);
			AssertPosition(input, container, 0, 0);

			input.TextWrapping = TextWrapping.Wrap;
			FlushLayout(window);
			Assert.Equal(naturalSize.Width, input.ActualWidth, 6);
			input.TextWrapping = TextWrapping.NoWrap;

			input.HorizontalAlignment = HorizontalAlignment.Center;
			FlushLayout(window);
			Assert.Equal(naturalSize.Width, input.ActualWidth, 6);
			AssertPosition(input, container, (container.ActualWidth - naturalSize.Width) / 2, 0);

			input.HorizontalAlignment = HorizontalAlignment.Right;
			FlushLayout(window);
			Assert.Equal(naturalSize.Width, input.ActualWidth, 6);
			AssertPosition(input, container, container.ActualWidth - naturalSize.Width, 0);

			input.HorizontalAlignment = HorizontalAlignment.Stretch;
			FlushLayout(window);
			Assert.Equal(container.ActualWidth, input.ActualWidth, 6);

			input.HorizontalAlignment = HorizontalAlignment.Left;
			input.VerticalAlignment = VerticalAlignment.Center;
			FlushLayout(window);
			Assert.Equal(naturalSize.Height, input.ActualHeight, 6);
			AssertPosition(input, container, 0, (container.ActualHeight - naturalSize.Height) / 2);

			input.VerticalAlignment = VerticalAlignment.Bottom;
			FlushLayout(window);
			Assert.Equal(naturalSize.Height, input.ActualHeight, 6);
			AssertPosition(input, container, 0, container.ActualHeight - naturalSize.Height);

			input.VerticalAlignment = VerticalAlignment.Stretch;
			FlushLayout(window);
			Assert.Equal(container.ActualHeight, input.ActualHeight, 6);
		}
		finally
		{
			window.Close();
		}
	}

	[StaFact]
	public void ProductionTemplate_MinLinesAndMaxLinesControlNaturalHeightAndScrolling()
	{
		var (window, _, input) = ShowNaturallySizedProductionControl("Short editable text");

		try
		{
			var lineHeight = MeasureLineHeight(input);
			var heights = new List<double>();
			foreach (var minLines in new[] { 1, 2, 3, 5 })
			{
				input.MinLines = minLines;
				FlushLayout(window);
				heights.Add(input.ActualHeight);
			}

			for (var index = 1; index < heights.Count; index++)
				Assert.Equal(lineHeight * (new[] { 1, 2, 3, 5 }[index] - 1), heights[index] - heights[0], 5);

			input.AcceptsReturn = true;
			input.MinLines = 1;
			input.MaxLines = 5;
			input.Text = "Short editable text";
			FlushLayout(window);
			var heightWithMaxLines = input.ActualHeight;
			input.Text = string.Join('\n', Enumerable.Range(1, 20).Select(line => $"Line {line}"));
			FlushLayout(window);

			var expectedMaximumHeight = heights[0] + (4 * lineHeight);
			Assert.Equal(heightWithMaxLines, input.ActualHeight, 6);
			Assert.InRange(input.ActualHeight, expectedMaximumHeight - 1, expectedMaximumHeight + 1);
			Assert.True(input.ExtentHeight > input.ViewportHeight);
			Assert.True(input.ScrollableHeight > 0);
			var host = Assert.IsType<StswScrollView>(input.Template.FindName("PART_ContentHost", input));
			Assert.Equal(Visibility.Visible, host.ComputedVerticalScrollBarVisibility);
		}
		finally
		{
			window.Close();
		}
	}

	[StaFact]
	public void ProductionTemplate_MaxLinesZeroKeepsMinLinesHeightAndScrollsOverflow()
	{
		var (window, _, input) = ShowNaturallySizedProductionControl("Short editable text");

		try
		{
			input.AcceptsReturn = true;
			input.HorizontalAlignment = HorizontalAlignment.Stretch;
			input.MinLines = 1;
			FlushLayout(window);
			var expectedHeight = input.ActualHeight;

			input.Text = "First line\nSecond line";
			FlushLayout(window);
			Assert.Equal(expectedHeight, input.ActualHeight, 6);

			input.Text = string.Join('\n', Enumerable.Range(1, 20).Select(line => $"Line {line}"));
			FlushLayout(window);

			var host = Assert.IsType<StswScrollView>(input.Template.FindName("PART_ContentHost", input));
			Assert.Equal(expectedHeight, input.ActualHeight, 6);
			Assert.True(input.ExtentHeight > input.ViewportHeight);
			Assert.True(input.ScrollableHeight > 0);
			Assert.Equal(Visibility.Visible, host.ComputedVerticalScrollBarVisibility);

			input.ScrollToEnd();
			Assert.True(input.VerticalOffset > 0);
			input.Text = "Short again";
			FlushLayout(window);

			Assert.Equal(expectedHeight, input.ActualHeight, 6);
			Assert.Equal(0, input.ScrollableHeight);
			Assert.Equal(0, input.VerticalOffset);
			Assert.Equal(Visibility.Collapsed, host.ComputedVerticalScrollBarVisibility);
		}
		finally
		{
			window.Close();
		}
	}

	[StaFact]
	public void ProductionTemplate_RightToLeftTextIsUnmirroredAndRightAligned()
	{
		var (window, _, input) = ShowNaturallySizedProductionControl("F");

		try
		{
			input.FlowDirection = FlowDirection.RightToLeft;
			input.Foreground = Brushes.Red;
			input.Background = Brushes.Black;
			input.BorderBrush = Brushes.Black;
			input.FontSize = 40;
			input.HorizontalAlignment = HorizontalAlignment.Stretch;
			FlushLayout(window);
			var host = Assert.IsType<StswScrollView>(input.Template.FindName("PART_ContentHost", input));
			var contentView = Assert.IsAssignableFrom<FrameworkElement>(host.Content);
			Assert.Equal(FlowDirection.LeftToRight, contentView.FlowDirection);

			var firstCharacter = input.GetRectFromCharacterIndex(0, trailingEdge: false);
			var lastCharacter = input.GetRectFromCharacterIndex(input.Text.Length, trailingEdge: false);
			Assert.True(firstCharacter.X > lastCharacter.X);

			var width = (int)Math.Ceiling(input.ActualWidth);
			var height = (int)Math.Ceiling(input.ActualHeight);
			var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
			bitmap.Render(input);
			var pixels = new byte[width * height * 4];
			bitmap.CopyPixels(pixels, width * 4, 0);

			var redPixels = 0;
			var leftmostRedPixel = width;
			var rightmostRedPixel = 0;
			for (var offset = 0; offset < pixels.Length; offset += 4)
			{
				if (pixels[offset + 2] > 100 && pixels[offset + 2] > pixels[offset + 1] * 2)
				{
					redPixels++;
					leftmostRedPixel = Math.Min(leftmostRedPixel, offset / 4 % width);
					rightmostRedPixel = Math.Max(rightmostRedPixel, offset / 4 % width);
				}
			}
			Assert.True(redPixels > 0, "The RTL text did not render inside the control viewport.");
			Assert.True(leftmostRedPixel > width / 2, "The RTL text was rendered on the left side of the viewport.");

			var glyphMiddle = (leftmostRedPixel + rightmostRedPixel) / 2;
			var redPixelsOnLeft = 0;
			var redPixelsOnRight = 0;
			for (var offset = 0; offset < pixels.Length; offset += 4)
			{
				if (pixels[offset + 2] <= 100 || pixels[offset + 2] <= pixels[offset + 1] * 2)
					continue;
				if (offset / 4 % width <= glyphMiddle)
					redPixelsOnLeft++;
				else
					redPixelsOnRight++;
			}
			Assert.True(redPixelsOnLeft > redPixelsOnRight, "The asymmetric 'F' glyph was horizontally mirrored.");
		}
		finally
		{
			window.Close();
		}
	}

	[StaFact]
	public void ProductionTemplate_StretchedViewportRendersEveryVisibleLineWithoutClipping()
	{
		var text = "First visible line\nSecond visible line\nThird visible line";
		var (window, container, input) = ShowNaturallySizedProductionControl(text);

		try
		{
			input.AcceptsReturn = true;
			input.Foreground = Brushes.Red;
			input.Background = Brushes.Black;
			input.BorderBrush = Brushes.Black;
			input.HorizontalAlignment = HorizontalAlignment.Stretch;
			input.VerticalAlignment = VerticalAlignment.Stretch;
			FlushLayout(window);

			var host = Assert.IsType<StswScrollView>(input.Template.FindName("PART_ContentHost", input));
			var contentView = Assert.IsAssignableFrom<FrameworkElement>(host.Content);
			Assert.Equal(container.ActualHeight, input.ActualHeight, 6);
			Assert.True(contentView.ActualHeight > 3 * MeasureLineHeight(input));

			var width = (int)Math.Ceiling(input.ActualWidth);
			var height = (int)Math.Ceiling(input.ActualHeight);
			var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
			bitmap.Render(input);
			var pixels = new byte[width * height * 4];
			bitmap.CopyPixels(pixels, width * 4, 0);

			foreach (var lineStart in new[] { 0, text.IndexOf('\n') + 1, text.LastIndexOf('\n') + 1 })
			{
				var lineRect = input.GetRectFromCharacterIndex(lineStart, trailingEdge: false);
				var firstRow = Math.Clamp((int)Math.Floor(lineRect.Top), 0, height - 1);
				var lastRow = Math.Clamp((int)Math.Ceiling(lineRect.Bottom), firstRow, height - 1);
				var redPixels = 0;
				for (var y = firstRow; y <= lastRow; y++)
				{
					for (var x = 0; x < width; x++)
					{
						var offset = (y * width + x) * 4;
						if (pixels[offset + 2] > 100 && pixels[offset + 2] > pixels[offset + 1] * 2)
							redPixels++;
					}
				}
				Assert.True(redPixels > 0, $"The visible line starting at index {lineStart} was clipped.");
			}
		}
		finally
		{
			window.Close();
		}
	}

	[StaFact]
	public void ProductionTemplate_AutoHorizontalScrollbarNeverClipsTheOnlyViewportLine()
	{
		var (window, _, input) = ShowNaturallySizedProductionControl("Short line");

		try
		{
			input.AcceptsReturn = true;
			var lineHeight = MeasureLineHeight(input);
			var expectedHeight = input.ActualHeight;

			input.Text = $"{new string('W', 200)}\nSecond line";
			FlushLayout(window);

			var host = Assert.IsType<StswScrollView>(input.Template.FindName("PART_ContentHost", input));
			Assert.Equal(expectedHeight, input.ActualHeight, 6);
			Assert.Equal(Visibility.Visible, host.ComputedHorizontalScrollBarVisibility);
			Assert.True(input.ViewportHeight >= lineHeight - 0.5,
				$"The horizontal scrollbar reduced the {input.ViewportHeight:F2}px viewport below one {lineHeight:F2}px text line.");
		}
		finally
		{
			window.Close();
		}
	}

	[StaFact]
	public void ProductionTemplate_ScrollCommandsWheelCaretAndWrappingUpdateOffsets()
	{
		var text = string.Join('\n', Enumerable.Range(1, 30).Select(index =>
			$"Line {index}: {new string((char)('a' + index % 20), 120)}"));
		var (window, input, host) = ShowProductionControl(text);

		try
		{
			var scrollInfo = Assert.IsAssignableFrom<IScrollInfo>(host.Content);
			var horizontalBar = Assert.IsType<StswScrollBar>(host.Template.FindName("PART_HorizontalScrollBar", host));
			var verticalBar = Assert.IsType<StswScrollBar>(host.Template.FindName("PART_VerticalScrollBar", host));

			Assert.Equal(input.ScrollableWidth, horizontalBar.Maximum, 6);
			Assert.Equal(input.ScrollableHeight, verticalBar.Maximum, 6);
			Assert.Equal(input.ViewportWidth, horizontalBar.ViewportSize, 6);
			Assert.Equal(input.ViewportHeight, verticalBar.ViewportSize, 6);

			ScrollBar.ScrollToHorizontalOffsetCommand.Execute(60d, horizontalBar);
			ScrollBar.ScrollToVerticalOffsetCommand.Execute(40d, verticalBar);
			FlushLayout(window);
			Assert.True(input.HorizontalOffset > 0);
			Assert.True(input.VerticalOffset > 0);

			var verticalBeforeWheel = input.VerticalOffset;
			var wheelEvent = new MouseWheelEventArgs(Mouse.PrimaryDevice, Environment.TickCount, -120)
			{
				RoutedEvent = Mouse.MouseWheelEvent,
				Source = host
			};
			host.RaiseEvent(wheelEvent);
			Assert.True(input.VerticalOffset > verticalBeforeWheel);

			var horizontalBeforeWheel = input.HorizontalOffset;
			scrollInfo.MouseWheelRight();
			Assert.True(input.HorizontalOffset > horizontalBeforeWheel);

			input.ScrollToHome();
			Assert.Equal(0, input.HorizontalOffset);
			Assert.Equal(0, input.VerticalOffset);
			scrollInfo.MakeVisible((Visual)host.Content, new Rect(input.ExtentWidth - 5, input.ExtentHeight - 5, 5, 5));
			Assert.True(input.HorizontalOffset > 0);
			Assert.True(input.VerticalOffset > 0);
			input.ScrollToHome();

			scrollInfo.PageDown();
			Assert.True(input.VerticalOffset > 0);
			input.ScrollToLine(20);
			Assert.True(input.VerticalOffset > 0);
			input.ScrollToEnd();
			Assert.Equal(input.ScrollableWidth, input.HorizontalOffset);
			Assert.Equal(input.ScrollableHeight, input.VerticalOffset);

			input.ScrollToHome();
			input.CaretIndex = input.Text.Length;
			Assert.True(input.HorizontalOffset > 0);
			Assert.True(input.VerticalOffset > 0);

			input.TextWrapping = TextWrapping.Wrap;
			FlushLayout(window);
			Assert.Equal(0, input.HorizontalOffset);
			Assert.Equal(0, input.ScrollableWidth);
			Assert.Equal(input.ViewportWidth, input.ExtentWidth);
			Assert.Equal(Visibility.Collapsed, host.ComputedHorizontalScrollBarVisibility);
		}
		finally
		{
			window.Close();
		}
	}

	[StaFact]
	public void ReplacingTemplate_DetachesTheOldScrollOwnerAndContent()
	{
		var (window, input, oldHost) = ShowProductionControl(new string('x', 300));
		var oldScrollInfo = Assert.IsAssignableFrom<IScrollInfo>(oldHost.Content);

		try
		{
			var factory = new FrameworkElementFactory(typeof(StswScrollView), "PART_ContentHost");
			factory.SetValue(ScrollViewer.CanContentScrollProperty, true);
			var replacement = new ControlTemplate(typeof(StswInputBoxTest)) { VisualTree = factory };

			input.Template = replacement;
			input.ApplyTemplate();
			FlushLayout(window);

			var newHost = Assert.IsType<StswScrollView>(input.Template.FindName("PART_ContentHost", input));
			var newScrollInfo = Assert.IsAssignableFrom<IScrollInfo>(newHost.Content);
			Assert.NotSame(oldHost, newHost);
			Assert.Null(oldHost.Content);
			Assert.NotSame(oldHost, oldScrollInfo.ScrollOwner);
			Assert.Same(oldScrollInfo, newScrollInfo);
			Assert.Same(newHost, newScrollInfo.ScrollOwner);
			Assert.True(input.ViewportWidth > 0);
			Assert.True(input.ViewportHeight > 0);
		}
		finally
		{
			window.Close();
		}
	}

	private static (Window Window, StswInputBoxTest Input, StswScrollView Host) ShowProductionControl(string text)
	{
		EnsureApplicationResources();
		var resources = new ResourceDictionary
		{
			Source = new Uri("/StswExpress.Wpf;component/Controls/Input/StswInputBoxTest.xaml", UriKind.Relative)
		};
		var input = new StswInputBoxTest();
		input.Resources["StswFocusVisual.Static.Border"] = Brushes.Black;
		input.Style = Assert.IsType<Style>(resources[typeof(StswInputBoxTest)]);
		input.SetCurrentValue(StswInputBoxBase.AcceptsReturnProperty, true);
		input.SetCurrentValue(FrameworkElement.HeightProperty, 140d);
		input.SetCurrentValue(StswInputBoxBase.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
		input.SetCurrentValue(StswInputBoxBase.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
		input.SetCurrentValue(StswInputBoxBase.TextWrappingProperty, TextWrapping.NoWrap);
		input.SetCurrentValue(StswInputBoxBase.TextProperty, text);
		input.SetCurrentValue(StswInputBoxTest.IconProperty, "diagnostic icon");
		var window = new Window
		{
			Content = input,
			Width = 340,
			Height = 190,
			ShowInTaskbar = false,
			WindowStyle = WindowStyle.None,
			Left = -10000,
			Top = -10000
		};
		window.Resources["StswFocusVisual.Static.Border"] = Brushes.Black;

		window.Show();
		FlushLayout(window);
		input.ApplyTemplate();
		FlushLayout(window);
		var host = Assert.IsType<StswScrollView>(input.Template.FindName("PART_ContentHost", input));
		return (window, input, host);
	}

	private static (Window Window, Grid Container, StswInputBoxTest Input) ShowNaturallySizedProductionControl(string text)
	{
		EnsureApplicationResources();
		var resources = new ResourceDictionary
		{
			Source = new Uri("/StswExpress.Wpf;component/Controls/Input/StswInputBoxTest.xaml", UriKind.Relative)
		};
		var input = new StswInputBoxTest
		{
			Style = Assert.IsType<Style>(resources[typeof(StswInputBoxTest)]),
			Text = text,
			MinLines = 1,
			MaxLines = 0,
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Top,
			HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
			VerticalScrollBarVisibility = ScrollBarVisibility.Auto
		};
		input.Resources["StswFocusVisual.Static.Border"] = Brushes.Black;
		var container = new Grid { Width = 600, Height = 300 };
		container.Children.Add(input);
		var window = new Window
		{
			Content = container,
			SizeToContent = SizeToContent.WidthAndHeight,
			ShowInTaskbar = false,
			WindowStyle = WindowStyle.None,
			Left = -10000,
			Top = -10000
		};
		window.Resources["StswFocusVisual.Static.Border"] = Brushes.Black;

		window.Show();
		FlushLayout(window);
		input.ApplyTemplate();
		FlushLayout(window);
		return (window, container, input);
	}

	private static double MeasureLineHeight(Control control)
	{
		var typeface = new Typeface(control.FontFamily, control.FontStyle, control.FontWeight, control.FontStretch);
		return new FormattedText(
			" ",
			CultureInfo.CurrentUICulture,
			control.FlowDirection,
			typeface,
			control.FontSize,
			control.Foreground,
			VisualTreeHelper.GetDpi(control).PixelsPerDip).Height;
	}

	private static void AssertPosition(FrameworkElement element, UIElement ancestor, double expectedX, double expectedY)
	{
		var position = element.TranslatePoint(new Point(), ancestor);
		Assert.Equal(expectedX, position.X, 5);
		Assert.Equal(expectedY, position.Y, 5);
	}

	private static void EnsureApplicationResources()
	{
		var application = Application.Current ?? new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
		application.Resources["StswFocusVisual.Static.Border"] = Brushes.Black;
	}

	private static void FlushLayout(Window window)
	{
		window.UpdateLayout();
		window.Dispatcher.Invoke(() => { }, DispatcherPriority.Loaded);
		window.UpdateLayout();
	}

	private sealed class TestInput : StswInputBoxBase
	{
		public string Normalize(string text, bool isPaste) => NormalizeInsertedText(text, isPaste);
	}
}
