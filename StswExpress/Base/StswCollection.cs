using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;

namespace StswExpress;

public delegate void ListedItemPropertyChangedEventHandler(IList sourceList, object item, PropertyChangedEventArgs e);
public class StswCollection<T> : ObservableCollection<T> where T : INotifyPropertyChanged
{
    private readonly Dictionary<T, DataRowState> _itemStates = new();

    public StswCollection() : base() { }

    public StswCollection(IEnumerable<T> items) : base(items)
    {
        foreach (var item in items)
        {
            _itemStates[item] = DataRowState.Unchanged;
            item.PropertyChanged += Item_PropertyChanged;
        }
    }

    public StswCollection(IList<T> items) : base(items)
    {
        foreach (var item in items)
        {
            _itemStates[item] = DataRowState.Unchanged;
            item.PropertyChanged += Item_PropertyChanged;
        }
    }

    /// ClearItems
    protected override void ClearItems()
    {
        foreach (var item in this)
        {
            _itemStates[item] = DataRowState.Deleted;
            item.PropertyChanged -= Item_PropertyChanged;
        }

        base.ClearItems();
    }

    /// InsertItem
    protected override void InsertItem(int index, T item)
    {
        base.InsertItem(index, item);

        _itemStates[item] = DataRowState.Added;
        item.PropertyChanged += Item_PropertyChanged;
    }

    /// RemoveItem
    protected override void RemoveItem(int index)
    {
        var item = this[index];

        _itemStates[item] = DataRowState.Deleted;
        item.PropertyChanged -= Item_PropertyChanged;

        base.RemoveItem(index);
    }

    /// SetItem
    protected override void SetItem(int index, T item)
    {
        var oldItem = this[index];

        _itemStates[oldItem] = DataRowState.Deleted;
        oldItem.PropertyChanged -= Item_PropertyChanged;

        base.SetItem(index, item);

        _itemStates[item] = DataRowState.Modified;
        item.PropertyChanged += Item_PropertyChanged;
    }

    /// Item_PropertyChanged
    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var item = (T?)sender;

        if (_itemStates[item] == DataRowState.Unchanged)
            _itemStates[item] = DataRowState.Modified;
    }

    /// GetState
    public DataRowState GetStateOfItem(T item)
    {
        if (_itemStates.TryGetValue(item, out var state))
            return state;

        return DataRowState.Detached;
    }

    /// GetItemsByState
    public List<T> GetItemsByState(DataRowState state) => _itemStates.Where(x => x.Value == state).Select(x => x.Key).ToList();
}