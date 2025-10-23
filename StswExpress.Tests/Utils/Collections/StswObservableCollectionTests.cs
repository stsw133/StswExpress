using System.ComponentModel;

namespace StswExpress.Commons.Tests.Utils.Collections;
public class StswObservableCollectionTests
{
    private class TestItem : IStswTrackableItem
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private StswItemState _itemState;
        public StswItemState ItemState
        {
            get => _itemState;
            set
            {
                if (_itemState != value)
                {
                    _itemState = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ItemState)));
                }
            }
        }
        public bool? ShowDetails { get; set; }
        public string? Name { get; set; }
        public void SetProperty(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [Fact]
    public void Constructor_InitializesWithTrackItemsTrue()
    {
        var items = new[] { new TestItem(), new TestItem() };
        var collection = new StswObservableCollection<TestItem>(items, true);

        Assert.Equal(items.Length, collection.Count);
        Assert.True(collection.TrackItems);
        Assert.Equal(items.Length, collection.CountUnchanged);
    }

    [Fact]
    public void Constructor_InitializesWithTrackItemsFalse()
    {
        var items = new[] { new TestItem(), new TestItem() };
        var collection = new StswObservableCollection<TestItem>(items, false);

        Assert.Equal(items.Length, collection.Count);
        Assert.False(collection.TrackItems);
        Assert.Equal(items.Length, collection.CountUnchanged);
    }

    [Fact]
    public void AddRange_AddsItemsAndTracksState()
    {
        var collection = new StswObservableCollection<TestItem>();
        var items = new[] { new TestItem(), new TestItem() };

        collection.AddRange(items);

        Assert.Equal(2, collection.Count);
        Assert.Equal(2, collection.CountAdded);
        Assert.All(collection.AddedItems, i => Assert.Equal(StswItemState.Added, i.ItemState));
    }

    [Fact]
    public void AddRangeFast_AddsItemsWithState()
    {
        var collection = new StswObservableCollection<TestItem>();
        var items = new[] { new TestItem(), new TestItem() };

        collection.AddRangeFast(items, StswItemState.Deleted);

        Assert.Equal(2, collection.Count);
        Assert.Equal(2, collection.CountDeleted);
        Assert.All(collection.DeletedItems, i => Assert.Equal(StswItemState.Deleted, i.ItemState));
    }

    [Fact]
    public void AcceptChanges_ResetsStatesAndClearsTrackingLists()
    {
        var collection = new StswObservableCollection<TestItem>();
        var item = new TestItem { ItemState = StswItemState.Added };
        collection.Add(item);

        collection.AcceptChanges();

        Assert.Equal(StswItemState.Unchanged, item.ItemState);
        Assert.Empty(collection.AddedItems);
        Assert.Empty(collection.ModifiedItems);
        Assert.Empty(collection.DeletedItems);
        Assert.Equal(collection.Count, collection.CountUnchanged);
    }

    [Fact]
    public void RemoveItem_TracksDeletedState()
    {
        var item = new TestItem();
        var collection = new StswObservableCollection<TestItem>(new[] { item });

        collection.Remove(item);

        Assert.Equal(StswItemState.Deleted, item.ItemState);
        Assert.Contains(item, collection.DeletedItems);
        Assert.Equal(1, collection.CountDeleted);
    }

    [Fact]
    public void RemoveItem_AddedItem_RemovesFromAddedList()
    {
        var item = new TestItem { ItemState = StswItemState.Added };
        var collection = new StswObservableCollection<TestItem>(new[] { item });

        collection.Remove(item);

        Assert.DoesNotContain(item, collection.AddedItems);
        Assert.Equal(0, collection.CountAdded);
    }

    [Fact]
    public void SetItem_ReplacesAndTracksState()
    {
        var item1 = new TestItem();
        var item2 = new TestItem();
        var collection = new StswObservableCollection<TestItem>([item1])
        {
            [0] = item2
        };

        Assert.Equal(item2, collection[0]);
        Assert.Equal(StswItemState.Added, item2.ItemState);
        Assert.Contains(item2, collection.AddedItems);
    }

    [Fact]
    public void ClearItems_RemovesAllAndClearsTrackingLists()
    {
        var items = new[] { new TestItem(), new TestItem() };
        var collection = new StswObservableCollection<TestItem>(items);

        collection.Clear();

        Assert.Empty(collection);
        Assert.Empty(collection.AddedItems);
        Assert.Empty(collection.ModifiedItems);
        Assert.Empty(collection.DeletedItems);
    }

    [Fact]
    public void DeleteItems_MarksAllAsDeleted()
    {
        var items = new[] { new TestItem(), new TestItem() };
        var collection = new StswObservableCollection<TestItem>(items);

        collection.DeleteItems();

        Assert.All(items, i => Assert.Equal(StswItemState.Deleted, i.ItemState));
        Assert.Equal(items.Length, collection.CountDeleted);
    }

    [Fact]
    public void TrackItems_Toggle_RewiresSubscriptionsAndTrackingLists()
    {
        var item = new TestItem();
        var collection = new StswObservableCollection<TestItem>(new[] { item });

        collection.TrackItems = false;
        Assert.False(collection.TrackItems);
        Assert.Empty(collection.AddedItems);

        collection.TrackItems = true;
        Assert.True(collection.TrackItems);
        Assert.Equal(collection.Count, collection.CountUnchanged);
    }

    [Fact]
    public void OnItemPropertyChanged_IgnoredProperty_DoesNotMarkModified()
    {
        var item = new TestItem();
        var collection = new StswObservableCollection<TestItem>(new[] { item });

        item.SetProperty(nameof(IStswTrackableItem.ShowDetails));
        Assert.Equal(StswItemState.Unchanged, item.ItemState);
    }

    [Fact]
    public void OnItemPropertyChanged_NonIgnoredProperty_MarksModified()
    {
        var item = new TestItem();
        var collection = new StswObservableCollection<TestItem>(new[] { item });

        item.SetProperty("Name");
        Assert.Equal(StswItemState.Modified, item.ItemState);
        Assert.Contains(item, collection.ModifiedItems);
    }

    [Fact]
    public void UnchangedItems_ReturnsCorrectItems()
    {
        var item1 = new TestItem { ItemState = StswItemState.Unchanged };
        var item2 = new TestItem { ItemState = StswItemState.Added };
        var collection = new StswObservableCollection<TestItem>(new[] { item1, item2 });

        var unchanged = collection.UnchangedItems.ToList();
        Assert.Single(unchanged);
        Assert.Contains(item1, unchanged);
    }
}
