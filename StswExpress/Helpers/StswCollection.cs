using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;

namespace StswExpress;

/// <summary>
/// An extension of <see cref="ObservableCollection{T}"/> that adds tracking of changes to the collection items' states.
/// </summary>
public class StswCollection<T> : ObservableCollection<T>, INotifyPropertyChanged where T : IStswCollectionItem
{
    public StswCollection() : base()
    {

    }
    public StswCollection(IEnumerable<T> items) : base(items)
    {
        foreach (var item in items)
        {
            _itemStates[item] = item.ItemState = DataRowState.Unchanged;
            item.PropertyChanged += Item_PropertyChanged;
        }
    }
    public StswCollection(IList<T> items) : base(items)
    {
        foreach (var item in items)
        {
            _itemStates[item] = item.ItemState = DataRowState.Unchanged;
            item.PropertyChanged += Item_PropertyChanged;
        }
    }

    /// <summary>
    /// Clears the collection and marks all items as "Deleted" if it was not previously "Added".
    /// </summary>
    protected override void ClearItems()
    {
        foreach (var item in this)
        {
            if (item.ItemState != DataRowState.Added)
            {
                _itemStates[item] = item.ItemState = DataRowState.Deleted;
                item.PropertyChanged -= Item_PropertyChanged;
            }
            else
            {
                item.PropertyChanged -= Item_PropertyChanged;
                _itemStates.Remove(item);
            }
        }

        base.ClearItems();

        OnPropertyChanged(nameof(Added));
        OnPropertyChanged(nameof(Modified));
        OnPropertyChanged(nameof(Deleted));
        OnPropertyChanged(nameof(Count));
    }

    /// <summary>
    /// Inserts an item into the collection at the specified index and marks it as "Added".
    /// </summary>
    protected override void InsertItem(int index, T item)
    {
        base.InsertItem(index, item);

        _itemStates[item] = item.ItemState = DataRowState.Added;
        item.PropertyChanged += Item_PropertyChanged;

        OnPropertyChanged(nameof(Added));
        OnPropertyChanged(nameof(Modified));
        OnPropertyChanged(nameof(Deleted));
        OnPropertyChanged(nameof(Count));
    }

    /// <summary>
    /// Removes the item at the specified index from the collection and marks it as "Deleted" if it was not previously "Added".
    /// </summary>
    protected override void RemoveItem(int index)
    {
        var item = this[index];

        if (item.ItemState != DataRowState.Added)
        {
            _itemStates[item] = item.ItemState = DataRowState.Deleted;
            item.PropertyChanged -= Item_PropertyChanged;
        }
        else
        {
            item.PropertyChanged -= Item_PropertyChanged;
            _itemStates.Remove(item);
        }

        base.RemoveItem(index);

        OnPropertyChanged(nameof(Added));
        OnPropertyChanged(nameof(Modified));
        OnPropertyChanged(nameof(Deleted));
        OnPropertyChanged(nameof(Count));
    }

    /// <summary>
    /// Replaces the item at the specified index with the given item and marks the old item as "Deleted" and the new item as "Modified".
    /// </summary>
    protected override void SetItem(int index, T item)
    {
        var oldItem = this[index];

        _itemStates[oldItem] = oldItem.ItemState = DataRowState.Deleted;
        oldItem.PropertyChanged -= Item_PropertyChanged;

        base.SetItem(index, item);

        _itemStates[item] = item.ItemState = DataRowState.Modified;
        item.PropertyChanged += Item_PropertyChanged;

        OnPropertyChanged(nameof(Added));
        OnPropertyChanged(nameof(Modified));
        OnPropertyChanged(nameof(Deleted));
        OnPropertyChanged(nameof(Count));
    }

    /// <summary>
    /// Handles the PropertyChanged event of the collection item and updates its state when a property other than "ItemState" or "ShowDetails" changes.
    /// </summary>
    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var item = (T?)sender;

        if (e.PropertyName.In(nameof(item.ItemState), nameof(item.ShowDetails)))
            return;

        if (item?.ItemState == DataRowState.Unchanged)
        {
            _itemStates[item] = item.ItemState = DataRowState.Modified;

            OnPropertyChanged(nameof(Added));
            OnPropertyChanged(nameof(Modified));
            OnPropertyChanged(nameof(Deleted));
            OnPropertyChanged(nameof(Count));
        }
    }

    /// <summary>
    /// Gets the state of a specific collection item.
    /// </summary>
    public DataRowState GetStateOfItem(T item)
    {
        if (_itemStates.TryGetValue(item, out var state))
            return state;

        return DataRowState.Detached;
    }
    private readonly Dictionary<T, DataRowState> _itemStates = new();

    /// <summary>
    /// Gets a list of collection items that match the specified DataRowState.
    /// </summary>
    public List<T> GetItemsByState(DataRowState state) => _itemStates.Where(x => x.Value == state).Select(x => x.Key).ToList();
    public int Added => GetItemsByState(DataRowState.Added).Count;
    public int Modified => GetItemsByState(DataRowState.Modified).Count;
    public int Deleted => GetItemsByState(DataRowState.Deleted).Count;



    /// Notify the view that the ItemStates property has changed
    public new event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

/// <summary>
/// Provides properties for tracking the state and error message of collection items.
/// </summary>
public interface IStswCollectionItem : INotifyPropertyChanged
{
    /// <summary>
    /// Gets or sets the error message associated with the collection item.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the state of the collection item.
    /// </summary>
    public DataRowState ItemState { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to show details for the collection item.
    /// </summary>
    public bool? ShowDetails { get; set; }
}

/// <summary>
/// Provides a way to store and display pairs of display and value objects for use in combo boxes.
/// </summary>
public class StswComboItem
{
    /// <summary>
    /// Gets or sets the display text for the combo box item.
    /// </summary>
    public object? Display { get; set; }

    /// <summary>
    /// Gets or sets the value associated with the combo box item.
    /// </summary>
    public object? Value { get; set; }
}
