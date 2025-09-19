using System.Collections.Specialized;

namespace StswExpress.Commons.Tests.Utils.Collections;
public class StswObservableDictionaryTests
{
    [Fact]
    public void Add_AddsItemAndRaisesEvents()
    {
        var dict = new StswObservableDictionary<string, int>();
        bool propertyChanged = false, collectionChanged = false;
        dict.PropertyChanged += (_, e) => { if (e.PropertyName == "Count") propertyChanged = true; };
        dict.CollectionChanged += (_, e) => { if (e.Action == NotifyCollectionChangedAction.Add) collectionChanged = true; };

        dict.Add("a", 1);

        Assert.True(dict.ContainsKey("a"));
        Assert.Equal(1, dict["a"]);
        Assert.True(propertyChanged);
        Assert.True(collectionChanged);
    }

    [Fact]
    public void IndexerGet_AutoAddOnGetTrue_AddsDefaultValue()
    {
        var dict = new StswObservableDictionary<string, int> { AutoAddOnGet = true };
        bool collectionChanged = false;
        dict.CollectionChanged += (_, e) => { if (e.Action == NotifyCollectionChangedAction.Add) collectionChanged = true; };

        var value = dict["missing"];

        Assert.Equal(0, value);
        Assert.True(dict.ContainsKey("missing"));
        Assert.True(collectionChanged);
    }

    [Fact]
    public void IndexerGet_AutoAddOnGetFalse_Throws()
    {
        var dict = new StswObservableDictionary<string, int> { AutoAddOnGet = false };
        Assert.Throws<KeyNotFoundException>(() => _ = dict["missing"]);
    }

    [Fact]
    public void IndexerSet_UpdatesValueAndRaisesReplace()
    {
        var dict = new StswObservableDictionary<string, int>();
        dict.Add("a", 1);
        bool replaced = false;
        dict.CollectionChanged += (_, e) => { if (e.Action == NotifyCollectionChangedAction.Replace) replaced = true; };

        dict["a"] = 2;

        Assert.Equal(2, dict["a"]);
        Assert.True(replaced);
    }

    [Fact]
    public void Remove_RemovesItemAndRaisesEvents()
    {
        var dict = new StswObservableDictionary<string, int>();
        dict.Add("a", 1);
        bool propertyChanged = false, collectionChanged = false;
        dict.PropertyChanged += (_, e) => { if (e.PropertyName == "Count") propertyChanged = true; };
        dict.CollectionChanged += (_, e) => { if (e.Action == NotifyCollectionChangedAction.Remove) collectionChanged = true; };

        var removed = dict.Remove("a");

        Assert.True(removed);
        Assert.False(dict.ContainsKey("a"));
        Assert.True(propertyChanged);
        Assert.True(collectionChanged);
    }

    [Fact]
    public void Clear_RemovesAllAndRaisesReset()
    {
        var dict = new StswObservableDictionary<string, int>();
        dict.Add("a", 1);
        dict.Add("b", 2);
        bool reset = false;
        dict.CollectionChanged += (_, e) => { if (e.Action == NotifyCollectionChangedAction.Reset) reset = true; };

        dict.Clear();

        Assert.Empty(dict);
        Assert.True(reset);
    }

    [Fact]
    public void AddRange_AddsItemsAndRaisesAdd()
    {
        var dict = new StswObservableDictionary<string, int>();
        bool add = false;
        dict.CollectionChanged += (_, e) => { if (e.Action == NotifyCollectionChangedAction.Add) add = true; };

        dict.AddRange(new Dictionary<string, int> { { "a", 1 }, { "b", 2 } });

        Assert.Equal(2, dict.Count);
        Assert.True(dict.ContainsKey("a"));
        Assert.True(dict.ContainsKey("b"));
        Assert.True(add);
    }

    [Fact]
    public void TryGetValue_ReturnsCorrectValue()
    {
        var dict = new StswObservableDictionary<string, int>();
        dict.Add("a", 1);

        var found = dict.TryGetValue("a", out var value);

        Assert.True(found);
        Assert.Equal(1, value);
    }

    [Fact]
    public void ContainsKey_ReturnsTrueIfExists()
    {
        var dict = new StswObservableDictionary<string, int>();
        dict.Add("a", 1);

        Assert.True(dict.ContainsKey("a"));
        Assert.False(dict.ContainsKey("b"));
    }

    [Fact]
    public void IsReadOnly_IsFalse()
    {
        var dict = new StswObservableDictionary<string, int>();
        Assert.False(dict.IsReadOnly);
    }

    [Fact]
    public void KeysAndValues_ExposeUnderlyingCollections()
    {
        var dict = new StswObservableDictionary<string, int>();
        dict.Add("a", 1);
        dict.Add("b", 2);

        Assert.Contains("a", dict.Keys);
        Assert.Contains("b", dict.Keys);
        Assert.Contains(1, dict.Values);
        Assert.Contains(2, dict.Values);
    }

    [Fact]
    public void Enumerator_EnumeratesAllItems()
    {
        var dict = new StswObservableDictionary<string, int>();
        dict.Add("a", 1);
        dict.Add("b", 2);

        var items = dict.ToList();

        Assert.Contains(new KeyValuePair<string, int>("a", 1), items);
        Assert.Contains(new KeyValuePair<string, int>("b", 2), items);
    }

    [Fact]
    public void Remove_KeyValuePair_RemovesItem()
    {
        var dict = new StswObservableDictionary<string, int>();
        dict.Add("a", 1);

        var removed = dict.Remove(new KeyValuePair<string, int>("a", 1));

        Assert.True(removed);
        Assert.False(dict.ContainsKey("a"));
    }

    [Fact]
    public void CopyTo_CopiesItemsToArray()
    {
        var dict = new StswObservableDictionary<string, int>();
        dict.Add("a", 1);
        dict.Add("b", 2);
        var array = new KeyValuePair<string, int>[2];

        dict.CopyTo(array, 0);

        Assert.Contains(new KeyValuePair<string, int>("a", 1), array);
        Assert.Contains(new KeyValuePair<string, int>("b", 2), array);
    }

    [Fact]
    public void Constructor_WithComparer_Works()
    {
        var dict = new StswObservableDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        dict.Add("A", 1);

        Assert.True(dict.ContainsKey("a"));
    }

    [Fact]
    public void Constructor_WithCapacityAndComparer_Works()
    {
        var dict = new StswObservableDictionary<string, int>(10, StringComparer.OrdinalIgnoreCase);
        dict.Add("A", 1);

        Assert.True(dict.ContainsKey("a"));
    }

    [Fact]
    public void Constructor_WithDictionary_Works()
    {
        var source = new Dictionary<string, int> { { "a", 1 } };
        var dict = new StswObservableDictionary<string, int>(source);

        Assert.True(dict.ContainsKey("a"));
        Assert.Equal(1, dict["a"]);
    }

    [Fact]
    public void Insert_AddThrowsIfKeyExists()
    {
        var dict = new StswObservableDictionary<string, int>();
        dict.Add("a", 1);

        Assert.Throws<ArgumentException>(() => dict.Add("a", 2));
    }

    [Fact]
    public void Contains_KeyValuePair_ReturnsTrueIfExists()
    {
        var dict = new StswObservableDictionary<string, int>();
        dict.Add("a", 1);

        Assert.True(dict.Contains(new KeyValuePair<string, int>("a", 1)));
        Assert.False(dict.Contains(new KeyValuePair<string, int>("b", 2)));
    }
}
