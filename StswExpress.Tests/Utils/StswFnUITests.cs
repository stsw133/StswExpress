using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace StswExpress.Tests.Utils;
public class StswFnUITests
{
    [Fact]
    public void AppNameAndVersion_ReturnsExpectedFormat()
    {
        var result = StswFnUI.AppNameAndVersion;
        Assert.False(string.IsNullOrWhiteSpace(result));
    }

    [Fact]
    public void ColorFromHsl_And_ColorToHsl_RoundTrip()
    {
        var color = StswFnUI.ColorFromHsl(255, 120, 0.5, 0.5);
        StswFnUI.ColorToHsl(color, out var h, out var s, out var l);
        Assert.InRange(h, 0, 360);
        Assert.InRange(s, 0, 1);
        Assert.InRange(l, 0, 1);
    }

    [Fact]
    public void ColorFromHsv_And_ColorToHsv_RoundTrip()
    {
        var color = StswFnUI.ColorFromHsv(255, 240, 0.5, 0.5);
        StswFnUI.ColorToHsv(color, out var h, out var s, out var v);
        Assert.InRange(h, 0, 360);
        Assert.InRange(s, 0, 1);
        Assert.InRange(v, 0, 1);
    }

    [Fact]
    public void GenerateColor_ReturnsDeterministicColor()
    {
        var color1 = StswFnUI.GenerateColor("test", 1);
        var color2 = StswFnUI.GenerateColor("test", 1);
        Assert.Equal(color1, color2);
    }

    [Fact]
    public void CompareModels_ReturnsCorrectDictionary()
    {
        var obj1 = new DummyModel { Value = 1, Name = "A" };
        var obj2 = new DummyModel { Value = 1, Name = "B" };
        var result = StswFnUI.CompareModels(obj1, obj2);
        Assert.True(result.ContainsKey(nameof(DummyModel.Value)));
        Assert.True(result.ContainsKey(nameof(DummyModel.Name)));
        Assert.True(result[nameof(DummyModel.Value)]);
        Assert.False(result[nameof(DummyModel.Name)]);
    }

    [Fact]
    public void BytesToBitmapImage_ReturnsNullForEmpty()
    {
        Assert.Null(StswFnUI.BytesToBitmapImage(null));
        Assert.Null(StswFnUI.BytesToBitmapImage(Array.Empty<byte>()));
    }

    [StaFact]
    public void FindDependencyProperty_ReturnsNullForNonexistent()
    {
        var obj = new DummyModel();
        var result = StswFnUI.FindDependencyProperty(obj, "Nonexistent");
        Assert.Null(result);
    }

    [StaFact]
    public void FindLogicalAncestor_ReturnsNullIfNoAncestor()
    {
        var btn = new Button();
        var ancestor = StswFnUI.FindLogicalAncestor<Window>(btn);
        Assert.Null(ancestor);
    }

    [StaFact]
    public void FindLogicalChild_ReturnsNullIfNoChild()
    {
        var grid = new Grid();
        var child = StswFnUI.FindLogicalChild<Button>(grid);
        Assert.Null(child);
    }

    [StaFact]
    public void FindVisualAncestor_ReturnsNullIfNoAncestor()
    {
        var btn = new Button();
        var ancestor = StswFnUI.FindVisualAncestor<Window>(btn);
        Assert.Null(ancestor);
    }

    [StaFact]
    public void FindVisualChild_ReturnsNullIfNoChild()
    {
        var grid = new Grid();
        var child = StswFnUI.FindVisualChild<Button>(grid);
        Assert.Null(child);
    }

    [StaFact]
    public void GetParent_ReturnsNullForNullInput()
    {
        Assert.Null(StswFnUI.GetParent(null));
    }

    [StaFact]
    public void RemoveFromParent_RemovesElementFromPanel()
    {
        var panel = new StackPanel();
        var btn = new Button();
        panel.Children.Add(btn);
        StswFnUI.RemoveFromParent(btn);
        Assert.DoesNotContain(btn, panel.Children.Cast<UIElement>());
    }

    [Fact]
    public void GetWindowsTheme_ReturnsLightOrDark()
    {
        var theme = StswFnUI.GetWindowsTheme();
        Assert.Contains(theme, new[] { "Light", "Dark" });
    }

    [StaFact]
    public void IsChildOfPopup_ReturnsFalseForNonChild()
    {
        var popup = new Popup();
        var btn = new Button();
        Assert.False(StswFnUI.IsChildOfPopup(popup, btn));
    }

    [Fact]
    public void True_And_False_StaticFields_AreCorrect()
    {
        Assert.True(StswFnUI.True);
        Assert.False(StswFnUI.False);
    }

    [Fact]
    public void CurrentDate_And_CurrentDateTime_AreCorrect()
    {
        Assert.Equal(DateTime.Today, StswFnUI.CurrentDate);
        Assert.True((DateTime.Now - StswFnUI.CurrentDateTime).TotalSeconds < 2);
    }

    private class DummyModel
    {
        public int Value { get; set; }
        public string? Name { get; set; }
    }
}
