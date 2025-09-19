namespace StswExpress.Commons.Tests.Utils.Stores;
public class DummyModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class DummyStore : StswStoreBase<DummyModel>
{
    public DummyStore(Func<Task<IEnumerable<DummyModel>>>? fetchFunc = null)
        : base(fetchFunc ?? (() => Task.FromResult<IEnumerable<DummyModel>>(new List<DummyModel>
        {
            new DummyModel { Id = 1, Name = "A" },
            new DummyModel { Id = 2, Name = "B" }
        })))
    { }
}

public class StswStoreBaseTests
{
    [Fact]
    public async Task Load_PopulatesItems_FromFetchFunc()
    {
        var store = new DummyStore();
        await store.Load();
        Assert.Collection(store.Items,
            item => Assert.Equal("A", item.Name),
            item => Assert.Equal("B", item.Name));
    }

    [Fact]
    public void Add_AddsItem_AndRaisesEvent()
    {
        var store = new DummyStore();
        bool eventRaised = false;
        store.StoreChanged += (_, args) => eventRaised = true;
        var model = new DummyModel { Id = 3, Name = "C" };
        store.Add(model);
        Assert.Contains(model, store.Items);
        Assert.True(eventRaised);
    }

    [Fact]
    public void Remove_RemovesItem_AndRaisesEvent()
    {
        var store = new DummyStore();
        var model = new DummyModel { Id = 4, Name = "D" };
        store.Add(model);
        bool eventRaised = false;
        store.StoreChanged += (_, args) => eventRaised = true;
        store.Remove(model);
        Assert.DoesNotContain(model, store.Items);
        Assert.True(eventRaised);
    }

    [Fact]
    public void AddRange_AddsItems_AndRaisesEvent()
    {
        var store = new DummyStore();
        var models = new List<DummyModel>
        {
            new DummyModel { Id = 5, Name = "E" },
            new DummyModel { Id = 6, Name = "F" }
        };
        bool eventRaised = false;
        store.StoreChanged += (_, args) => eventRaised = true;
        store.AddRange(models);
        Assert.Contains(models[0], store.Items);
        Assert.Contains(models[1], store.Items);
        Assert.True(eventRaised);
    }

    [Fact]
    public void Clear_RemovesAllItems_AndRaisesEvent()
    {
        var store = new DummyStore();
        store.Add(new DummyModel { Id = 7, Name = "G" });
        bool eventRaised = false;
        store.StoreChanged += (_, args) => eventRaised = true;
        store.Clear();
        Assert.Empty(store.Items);
        Assert.True(eventRaised);
    }

    [Fact]
    public void Refresh_ClearsAndReloadsItems()
    {
        var store = new DummyStore();
        store.Add(new DummyModel { Id = 8, Name = "H" });
        store.Refresh();
        Assert.Collection(store.Items,
            item => Assert.Equal("A", item.Name),
            item => Assert.Equal("B", item.Name));
    }

    [Fact]
    public void DeferRefresh_DefersEventUntilDisposed()
    {
        var store = new DummyStore();
        int eventCount = 0;
        store.StoreChanged += (_, args) => eventCount++;
        using (store.DeferRefresh())
        {
            store.Add(new DummyModel { Id = 9, Name = "I" });
            store.Add(new DummyModel { Id = 10, Name = "J" });
            Assert.Equal(0, eventCount);
        }
        Assert.Equal(1, eventCount);
    }

    //[Fact]
    //public async Task FetchItemsFromDatabaseAsync_ReturnsItems()
    //{
    //    var store = new DummyStore();
    //    var items = await store.FetchItemsFromDatabaseAsync();
    //    Assert.Collection(items,
    //        item => Assert.Equal("A", item.Name),
    //        item => Assert.Equal("B", item.Name));
    //}
}
