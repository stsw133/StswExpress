using StswExpress.Commons;
using System.ComponentModel;

namespace StswExpress.Tests.Utils.Helpers;
public class StswCollectionViewWrapperTests
{
    private class TestItem : StswObservableObject, IStswCollectionItem
    {
        public StswItemState ItemState { get; set; }
        public bool? ShowDetails { get; set; }
        public event PropertyChangedEventHandler? PropertyChanged;
    }

    [Fact]
    public void Constructor_Default_CreatesEmptyCollection()
    {
        var wrapper = new StswCollectionViewWrapper<TestItem>();
        Assert.NotNull(wrapper.Items);
        Assert.Empty(wrapper.Items);
        Assert.NotNull(wrapper.Source);
        Assert.NotNull(wrapper.View);
    }

    [Fact]
    public void Constructor_WithItems_InitializesCollection()
    {
        var items = new List<TestItem> { new(), new() };
        var wrapper = new StswCollectionViewWrapper<TestItem>(items);

        Assert.Equal(2, wrapper.Items.Count);
        Assert.True(wrapper.Items.All(i => items.Contains(i)));
        Assert.NotNull(wrapper.Source);
        Assert.NotNull(wrapper.View);
    }

    [Fact]
    public void Constructor_NullItems_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new StswCollectionViewWrapper<TestItem>(null!));
    }

    [Fact]
    public void ReplaceWith_ReplacesItems()
    {
        var initial = new List<TestItem> { new(), new() };
        var wrapper = new StswCollectionViewWrapper<TestItem>(initial);

        var newItems = new List<TestItem> { new(), new(), new() };
        wrapper.ReplaceWith(newItems);

        Assert.Equal(3, wrapper.Items.Count);
        Assert.True(wrapper.Items.All(i => newItems.Contains(i)));
    }

    [Fact]
    public void View_ReflectsItems()
    {
        var items = new List<TestItem> { new(), new() };
        var wrapper = new StswCollectionViewWrapper<TestItem>(items);

        var viewItems = wrapper.View.Cast<TestItem>().ToList();
        Assert.Equal(items.Count, viewItems.Count);
        Assert.True(viewItems.All(i => items.Contains(i)));
    }
}
