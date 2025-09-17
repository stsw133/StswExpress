/*
using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Moq;
using StswExpress.Commons;

namespace StswExpress.Tests;
public class StswCommandsTests
{
    private class SelectionItem : IStswSelectionItem
    {
        public bool IsSelected { get; set; }
        public bool CustomProperty { get; set; }
    }

    private class SelectionBoxMock : StswSelectionBox
    {
        public SelectionBoxMock(IEnumerable items)
        {
            ItemsSource = items;
            UpdateTextCommand = new Mock<ICommand>().Object;
        }
    }

    [Fact]
    public void Clear_Execute_Clears_IList()
    {
        var list = new ArrayList { 1, 2, 3 };
        var args = new ExecutedRoutedEventArgs() { Command = StswCommands.Clear, Parameter = list };
        typeof(StswCommands).GetMethod("Clear_Execute", BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, new object[] { list, args });
        Assert.Empty(list);
    }

    [Fact]
    public void Clear_Execute_Clears_ItemsControl_Items()
    {
        var control = new ListBox();
        control.Items.Add("A");
        var args = new ExecutedRoutedEventArgs(StswCommands.Clear, control);
        typeof(StswCommands).GetMethod("Clear_Execute", BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, new object[] { control, args });
        Assert.Empty(control.Items);
    }

    [Fact]
    public void Clear_Execute_Clears_ItemsControl_ItemsSource()
    {
        var srcList = new ObservableCollection<string> { "A", "B" };
        var control = new ListBox { ItemsSource = srcList };
        var args = new ExecutedRoutedEventArgs(StswCommands.Clear, control);
        typeof(StswCommands).GetMethod("Clear_Execute", BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, new object[] { control, args });
        Assert.Empty(srcList);
    }

    [Fact]
    public void Clear_Execute_Clears_StswPasswordBox()
    {
        var pwd = new StswPasswordBox { Password = "secret" };
        var args = new ExecutedRoutedEventArgs(StswCommands.Clear, pwd);
        typeof(StswCommands).GetMethod("Clear_Execute", BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, new object[] { pwd, args });
        Assert.Null(pwd.Password);
    }

    [Fact]
    public void Clear_Execute_Clears_TextBox()
    {
        var tb = new TextBox { Text = "abc" };
        var args = new ExecutedRoutedEventArgs(StswCommands.Clear, tb);
        typeof(StswCommands).GetMethod("Clear_Execute", BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, new object[] { tb, args });
        Assert.Equal(string.Empty, tb.Text);
    }

    [Fact]
    public void Deselect_Execute_Selector()
    {
        var selector = new ListBox();
        selector.Items.Add("A");
        selector.SelectedIndex = 0;
        var args = new ExecutedRoutedEventArgs(StswCommands.Deselect, selector);
        typeof(StswCommands).GetMethod("Deselect_Execute", BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, new object[] { selector, args });
        Assert.Equal(-1, selector.SelectedIndex);
    }

    [Fact]
    public void Deselect_Execute_StswSelectionBox()
    {
        var items = new[] { new SelectionItem { IsSelected = true }, new SelectionItem { IsSelected = false } };
        var box = new SelectionBoxMock(items);
        var args = new ExecutedRoutedEventArgs(StswCommands.Deselect, box);
        typeof(StswCommands).GetMethod("Deselect_Execute", BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, new object[] { box, args });
        Assert.All(items, i => Assert.False(i.IsSelected));
    }

    [Fact]
    public void DeselectAll_Execute_Deselects_All()
    {
        var items = new[] { new SelectionItem { IsSelected = true }, new SelectionItem { IsSelected = true } };
        var box = new SelectionBoxMock(items);
        var args = new ExecutedRoutedEventArgs(StswCommands.DeselectAll, null);
        typeof(StswCommands).GetMethod("DeselectAll_Execute", BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, new object[] { box, args });
        Assert.All(items, i => Assert.False(i.IsSelected));
    }

    [Fact]
    public void SelectAll_Execute_Selects_All()
    {
        var items = new[] { new SelectionItem { IsSelected = false }, new SelectionItem { IsSelected = false } };
        var box = new SelectionBoxMock(items);
        var args = new ExecutedRoutedEventArgs(StswCommands.SelectAll, null);
        typeof(StswCommands).GetMethod("SelectAll_Execute", BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, new object[] { box, args });
        Assert.All(items, i => Assert.True(i.IsSelected));
    }

    [Fact]
    public void SetPropertyForSelected_Execute_Sets_Property()
    {
        var items = new[] { new SelectionItem { IsSelected = true }, new SelectionItem { IsSelected = false } };
        var box = new SelectionBoxMock(items);
        var toggle = new ToggleButton { IsChecked = true };
        typeof(StswFnUI).GetMethod("FindVisualAncestor")?.Invoke(null, new object[] { toggle }); // Just to ensure method exists
        // Patch FindVisualAncestor to return our box
        StswFnUI.FindVisualAncestorFunc = _ => box;
        var args = new ExecutedRoutedEventArgs(StswCommands.SetPropertyForSelected, "CustomProperty");
        typeof(StswCommands).GetMethod("SetPropertyForSelected_Execute", BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, new object[] { toggle, args });
        Assert.True(items[0].CustomProperty);
        Assert.False(items[1].CustomProperty);
        StswFnUI.FindVisualAncestorFunc = null; // cleanup
    }

    [Fact]
    public void TryGetSelectionItems_Returns_Items_From_Param()
    {
        var items = new[] { new SelectionItem(), new SelectionItem() };
        var method = typeof(StswCommands).GetMethod("TryGetSelectionItems", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
        var parameters = new object[] { null, items, null! };
        var result = (bool)method.Invoke(null, parameters);
        var outItems = (IEnumerable<IStswSelectionItem>)parameters[2];
        Assert.True(result);
        Assert.Equal(items, outItems.ToArray());
    }

    [Fact]
    public void TryGetSelectionItems_Returns_Items_From_ItemsControl()
    {
        var items = new[] { new SelectionItem(), new SelectionItem() };
        var control = new SelectionBoxMock(items);
        var method = typeof(StswCommands).GetMethod("TryGetSelectionItems", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
        var parameters = new object[] { control, null, null! };
        var result = (bool)method.Invoke(null, parameters);
        var outItems = (IEnumerable<IStswSelectionItem>)parameters[2];
        Assert.True(result);
        Assert.Equal(items, outItems.ToArray());
    }
}
*/